using Data.Models;
using LNF;
using LNF.Data;
using LNF.Models.Data;
using LNF.Repository.Data;
using LNF.Scripting;
using LNF.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Data.Controllers
{
    public class FeedController : Controller
    {
        private IScriptingService ScriptingService { get; }

        public FeedController(IScriptingService scriptingService)
        {
            ScriptingService = scriptingService;
        }

        // feed/reports/define/{alias}
        public ActionResult Configuration(string alias, string callback = null)
        {
            string content = string.Empty;

            if (string.IsNullOrEmpty(alias))
                content = ServiceProvider.Current.Serialization.Json.SerializeObject(new { error = true, errorMessage = "Missing parameter: alias" });
            else
            {
                string filePath = Path.Combine(Server.MapPath("~/Content/json"), string.Format("{0}.json", alias));

                if (System.IO.File.Exists(filePath))
                    content = System.IO.File.ReadAllText(filePath);
                else
                    content = ServiceProvider.Current.Serialization.Json.SerializeObject(new { error = true, errorMessage = string.Format("Cannot find report configuration: {0}.json", alias) });
            }

            if (!string.IsNullOrEmpty(callback))
                content = callback + "(" + content + ")";

            return Content(content, "application/json");
        }

        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult List(FeedModel model)
        {
            ViewBag.UsePills = true;
            model.ViewInactive = ViewInactive();
            model.CurrentPage = "Feed";
            model.CurrentSubMenuItem = "list";
            return View(model);
        }

        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Console(FeedModel model)
        {
            model.CurrentPage = "Feed";
            model.CurrentSubMenuItem = "console";

            if (!string.IsNullOrEmpty(model.Alias))
            {
                //this means we are editing
                DataFeed feed = model.GetFeed();
                if (feed != null)
                {
                    model.Guid = feed.FeedGUID.ToString();
                    model.Name = feed.FeedName;
                    model.Description = feed.FeedDescription;
                    model.Link = feed.FeedLink;
                    model.Private = feed.Private;
                    model.Active = feed.Active;
                    model.FeedType = feed.FeedType;
                    model.Query = feed.FeedQuery;
                }
            }
            else
            {
                model.Active = true;
            }

            return View(model);
        }

        public ActionResult Reports(FeedModel model)
        {
            model.CurrentPage = "Feed";
            model.CurrentSubMenuItem = "reports";
            return View(model);
        }

        [LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Delete(FeedModel model)
        {
            model.DeleteFeed();
            return RedirectToAction("Index", new { Alias = "", Format = "" });
        }

        [HttpPost, LNFAuthorize]
        public ActionResult Ajax(FeedModel model)
        {
            if (model.Command == "run-script")
            {
                string query = string.Empty;
                string error = string.Empty;
                string buffer = string.Empty;
                string html = string.Empty;
                Hashtable data = new Hashtable();

                if (model.FeedType == DataFeedType.Python)
                    query = model.Query;
                else
                    query = string.Format("data(sqlquery(\"{0}\"))", model.Query.Replace("\n", " ").Replace("\"", @"\""").Trim());

                Result result = ScriptingService.Run(query, Parameters.Create());

                if (result.Exception != null)
                    error = result.Exception.Message;

                buffer = result.Buffer.ToString();
                html = result.Html.ToString();
                if (result.DataSet.Count > 0)
                {
                    foreach (KeyValuePair<string, LNF.Scripting.Data> kvp in result.DataSet)
                        data.Add(kvp.Key, new { Headers = kvp.Value.GetHeaders().Select(x => x.DisplayText).ToArray(), Items = kvp.Value.GetItems() });
                }

                return Json(new { Success = true, Message = "", Error = error, Buffer = buffer, Html = html, Data = data });
            }
            else
                return Json(new { Success = false, Message = "Invalid command" });
        }

        [AllowAnonymous]
        public ActionResult Index(FeedModel model)
        {
            if (string.IsNullOrEmpty(model.Alias))
                return RedirectToAction("List");

            DataFeed feed = model.GetFeed();
            ContentResult result = new ContentResult();
            try
            {
                if (feed == null)
                    throw new Exception("Could not find feed: " + model.Alias);
                else if (feed.Deleted)
                    throw new Exception("Could not find feed: " + model.Alias);
                else if (!feed.Active && !DataFeedUtility.CanViewInactiveFeeds())
                    throw new Exception("Could not find feed: " + model.Alias);
                else
                {
                    switch (model.Format)
                    {
                        case "xml":
                            result.Content = DataFeedUtility.XmlFeedContent(feed, model.Key, Parameters.Create());
                            result.ContentType = "text/xml";
                            break;
                        case "rss":
                            result.Content = DataFeedUtility.RssFeedContent(feed, model.Key, Parameters.Create());
                            result.ContentType = "application/rss+xml";
                            break;
                        case "html":
                            return View("Feed", model);
                        case "table":
                            result.Content = DataFeedUtility.HtmlFeedContent(feed, model.Key, model.Format, Parameters.Create());
                            result.ContentType = "text/html";
                            break;
                        case "json":
                        case "jsonp":
                        case "datatables":
                            result.Content = DataFeedUtility.JsonFeedContent(feed, model.Key, model.Format, Parameters.Create());
                            if (model.Format == "jsonp" && !string.IsNullOrEmpty(model.Callback))
                                result.Content = model.Callback + "(" + result.Content + ")";
                            result.ContentType = "application/json";
                            break;
                        case "ical":
                            result.Content = DataFeedUtility.IcalFeedContent(feed, model.Key, Parameters.Create());
                            result.ContentType = "text/calendar";
                            Response.Charset = string.Empty;
                            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}-{1}.ics", feed.FeedAlias, DateTime.Now.ToString("yyyyMMddHHmmss")));
                            break;
                        default: //csv
                            result.Content = DataFeedUtility.CsvFeedContent(feed, model.Key, Parameters.Create());
                            result.ContentType = "text/csv";
                            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}-{1}.csv", feed.FeedAlias, DateTime.Now.ToString("yyyyMMddHHmmss")));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //model.App = "Content";
                model.ErrorMessage = ex.Message;
                switch (model.Format)
                {
                    case "xml":
                        result.Content = string.Format("<data name=\"LNF Feed Error\"><table name=\"default\"><row><Error>{0}</Error></row></table></data>", ex.Message);
                        result.ContentType = "text/xml";
                        break;
                    case "rss":
                        result.Content = string.Format("<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"><channel><title>LNF Feed Error</title><item><title>Error</title><description>{0}</description></item></channel></rss>", ex.Message);
                        result.ContentType = "application/rss+xml";
                        break;
                    case "json":
                    case "jsonp":
                    case "datatables":
                        result.Content = "{\"Error\":\"" + ex.Message + "\"}";
                        if (model.Format == "jsonp" && !string.IsNullOrEmpty(model.Callback))
                            result.Content = model.Callback + "(" + result.Content + ")";
                        result.ContentType = "application/json";
                        break;
                    case "ical":
                        result.Content = string.Format("Error: {0}", ex.Message);
                        result.ContentType = "text/plain";
                        break;
                    case "csv":
                        result.Content = "Error" + Environment.NewLine + ex.Message;
                        result.ContentType = "text/csv";
                        Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}-{1}.csv", "lnf-feed-error", DateTime.Now.ToString("yyyyMMddHHmmss")));
                        break;
                    default:
                        return View("Launcher", model);
                }
            }

            return result;
        }

        [LNFAuthorize]
        public ActionResult ViewHtml(FeedModel model)
        {
            //model.App = "view";
            model.CurrentPage = "Feed";
            model.CurrentSubMenuItem = "list";

            DataFeed feed = model.GetFeed();
            if (feed != null)
            {
                if (feed.FeedType == DataFeedType.Python)
                {
                    try
                    {
                        Result result = ScriptingService.Run(feed.FeedQuery, Parameters.Create());
                        if (result.Exception != null)
                            throw result.Exception;

                        model.Title = result.Title;
                        model.Html = new HtmlString(result.Html.ToString());
                    }
                    catch (Exception ex)
                    {
                        //model.App = "ViewHtml";
                        model.ErrorMessage = ex.Message;
                        return View("Launcher", model);
                    }
                }
                else
                    model.ErrorMessage = "Feed type does not support HTML viewing";
            }
            else
                model.ErrorMessage = "Could not find feed: " + model.Alias;

            return View(model);
        }

        [LNFAuthorize]
        public ActionResult Save(FeedModel model)
        {
            model.SaveFeed();
            if (string.IsNullOrEmpty(model.ErrorMessage))
                return RedirectToAction("List");
            else
            {
                model.CurrentPage = "Feed";
                model.CurrentSubMenuItem = "console";
                return View("Console", model);
            }
        }

        private bool ViewInactive()
        {
            if (bool.TryParse(Request.QueryString["inactive"], out bool result))
                return result;
            else
                return false;
        }
    }
}
