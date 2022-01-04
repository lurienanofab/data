using Data.Models;
using LNF;
using LNF.Data;
using LNF.Impl.Repository.Data;
using LNF.Scripting;
using LNF.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Data.Controllers
{
    public class FeedController : DataController
    {
        private readonly IScriptEngine _engine;

        public FeedController(IProvider provider) : base(provider)
        {
            _engine = new PythonScriptService();
        }

        [Route("feed/reports/{alias}/cfg")]
        public ActionResult Configuration(string alias, string callback = null)
        {
            string content;

            if (string.IsNullOrEmpty(alias))
                content = Provider.Utility.Serialization.Json.SerializeObject(new { error = true, errorMessage = "Missing parameter: alias" });
            else
            {
                string filePath = Path.Combine(Server.MapPath("~/Content/json"), string.Format("{0}.json", alias));

                if (System.IO.File.Exists(filePath))
                    content = System.IO.File.ReadAllText(filePath);
                else
                    content = Provider.Utility.Serialization.Json.SerializeObject(new { error = true, errorMessage = string.Format("Cannot find report configuration: {0}.json", alias) });
            }

            if (!string.IsNullOrEmpty(callback))
                content = callback + "(" + content + ")";

            return Content(content, "application/json");
        }

        [HttpGet, Route("feed/list"), LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult List(FeedModel model)
        {
            ViewBag.UsePills = true;
            model.Provider = Provider;
            model.ViewInactive = ViewInactive();
            model.CurrentPage = "Feed";
            model.CurrentSubMenuItem = "list";
            return View(model);
        }

        [HttpGet, Route("feed/console/{alias?}"), LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Console(FeedModel model)
        {
            model.Provider = Provider;
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
                    model.DefaultParameters = feed.DefaultParameters;
                    model.Message = GetAndRemoveSessionValue("SaveFeedMessage");
                    model.ErrorMessage = GetAndRemoveSessionValue("SaveFeedError");
                }
            }
            else
            {
                model.Active = true;
            }

            return View(model);
        }

        [HttpPost, Route("feed/console/save"), LNFAuthorize]
        public ActionResult Save(FeedModel model)
        {
            model.Provider = Provider;

            var result = model.SaveFeed();

            Session.Remove("SaveFeedMessage");
            Session.Remove("SaveFeedError");

            if (result)
                Session["SaveFeedMessage"] = "Saved OK!";
            else
                Session["SaveFeedError"] = model.ErrorMessage;

            return RedirectToAction("Console", new { model.Alias });
        }

        [Route("feed/reports/{alias}")]
        public ActionResult Reports(FeedModel model)
        {
            model.Provider = Provider;
            model.CurrentPage = "Feed";
            model.CurrentSubMenuItem = "reports";
            return View(model);
        }

        [Route("feed/delete/{alias}"), LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Delete(string alias)
        {
            var model = new FeedModel() { Provider = Provider, Alias = alias, CurrentUser = CurrentUser };
            model.DeleteFeed();
            return RedirectToAction("Index", new { Alias = "", Format = "" });
        }

        [HttpPost, Route("feed/ajax"), LNFAuthorize]
        public ActionResult Ajax(FeedModel model)
        {
            model.Provider = Provider;

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

                var dict = new Dictionary<object, object>();
                DataFeedItem.ApplyDefaultParameters(model.DefaultParameters, dict);

                var parameters = ScriptParameters.Create(dict);

                var result = _engine.Run(query, parameters);

                if (result.Exception != null)
                    error = result.Exception.Message;

                buffer = result.Buffer.ToString();
                html = result.Html.ToString();
                if (result.DataSet.Count > 0)
                {
                    foreach (KeyValuePair<string, ScriptData> kvp in result.DataSet)
                        data.Add(kvp.Key, new { Headers = kvp.Value.GetHeaders().Select(x => x.DisplayText).ToArray(), Items = kvp.Value.GetItems() });
                }

                return Json(new { Success = true, Message = "", Error = error, Buffer = buffer, Html = html, Data = data, Parameters = parameters.ToString() });
            }
            else
                return Json(new { Success = false, Message = "Invalid command" });
        }

        [AllowAnonymous, Route("feed/{alias?}/{format?}/{key?}")]
        public ActionResult Index(string alias, string format, string key, string callback = null)
        {
            var model = new FeedModel
            {
                Provider = Provider,
                CurrentUser = CurrentUser,
                Alias = alias,
                Format = format,
                Key = key,
                Callback = callback
            };

            if (string.IsNullOrEmpty(model.Alias))
                return RedirectToAction("List");

            ContentResult result = new ContentResult();

            var parameters = new Dictionary<object, object>();
            Merge(parameters, Request.QueryString);

            var feedResult = ServiceProvider.Current.Data.Feed.GetDataFeedResult(alias, key, parameters);

            try
            {
                var util = new DataFeedUtility(ServiceProvider.Current);

                if (feedResult == null)
                    throw new Exception("Could not find feed: " + model.Alias);
                else if (feedResult.Deleted)
                    throw new Exception("Could not find feed: " + model.Alias);
                else if (!feedResult.Active && !DataFeedItem.CanViewInactiveFeeds(CurrentUser))
                    throw new Exception("Could not find feed: " + model.Alias);
                else
                {
                    switch (model.Format)
                    {
                        case "xml":
                            result.Content = util.XmlFeedContent(feedResult);
                            result.ContentType = "text/xml";
                            break;
                        case "rss":
                            result.Content = util.RssFeedContent(feedResult, model.Key, Request.Url, Request.Path);
                            result.ContentType = "application/rss+xml";
                            break;
                        case "html":
                            return View("Feed", model);
                        case "table":
                            result.Content = util.HtmlFeedContent(feedResult, model.Format);
                            result.ContentType = "text/html";
                            break;
                        case "json":
                        case "jsonp":
                        case "datatables":
                            result.Content = util.JsonFeedContent(feedResult, model.Key, model.Format);
                            if (model.Format == "jsonp" && !string.IsNullOrEmpty(model.Callback))
                                result.Content = model.Callback + "(" + result.Content + ")";
                            result.ContentType = "application/json";
                            break;
                        case "ical":
                            result.Content = util.IcalFeedContent(feedResult, model.Key, Request.ServerVariables["LOCAL_ADDR"]);
                            result.ContentType = "text/calendar";
                            Response.Charset = string.Empty;
                            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}-{1}.ics", feedResult.Alias, DateTime.Now.ToString("yyyyMMddHHmmss")));
                            break;
                        default: //csv
                            result.Content = util.CsvFeedContent(feedResult, model.Key);
                            result.ContentType = "text/csv";
                            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}-{1}.csv", feedResult.Alias, DateTime.Now.ToString("yyyyMMddHHmmss")));
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

        [LNFAuthorize, Route("view/{alias}")]
        public ActionResult ViewHtml(FeedModel model)
        {
            //model.App = "view";
            model.Provider = Provider;
            model.CurrentPage = "Feed";
            model.CurrentSubMenuItem = "list";

            DataFeed feed = model.GetFeed();
            if (feed != null)
            {
                if (feed.FeedType == DataFeedType.Python)
                {
                    try
                    {
                        var result = _engine.Run(feed.FeedQuery, GetParameters(feed));
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

        private bool ViewInactive()
        {
            if (bool.TryParse(Request.QueryString["inactive"], out bool result))
                return result;
            else
                return false;
        }

        private string GetAndRemoveSessionValue(string key)
        {
            string result = string.Empty;

            if (Session[key] != null)
            {
                result = Session[key].ToString();
                Session.Remove(key);
            }

            return result;
        }

        private ScriptParameters GetParameters(DataFeed feed)
        {
            var dict = new Dictionary<object, object>();
            Merge(dict, Request.QueryString);
            Merge(dict, Request.Form);
            feed.ApplyDefaultParameters(dict);

            var result = ScriptParameters.Create(dict);

            return result;
        }

        private void Merge(IDictionary<object, object> dict, NameValueCollection nvc)
        {
            foreach (var key in nvc.AllKeys)
            {
                if (dict.ContainsKey(key))
                    dict[key] = nvc[key];
                else
                    dict.Add(key, nvc[key]);
            }
        }
    }
}
