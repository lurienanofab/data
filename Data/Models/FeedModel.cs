using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using LNF;
using LNF.Data;
using LNF.Scripting;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;

namespace Data.Models
{
    public class FeedModel : BaseModel
    {
        public string Command { get; set; }
        public string Alias { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public bool Private { get; set; }
        public bool Active { get; set; }
        public string Format { get; set; }
        public string Callback { get; set; }
        public string Query { get; set; }
        public string Title { get; set; }
        public string Key { get; set; }
        public IHtmlString Html { get; set; }
        public DataFeedType FeedType { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public bool ViewInactive { get; set; }
        //public override string App { get; set; }

        public DataFeed[] GetFeeds()
        {
            if (ViewInactive)
                return DA.Current.Query<DataFeed>().Where(x => !x.Deleted).OrderBy(x => x.FeedName).ToArray();
            else
                return DA.Current.Query<DataFeed>().Where(x => x.Active && !x.Deleted).OrderBy(x => x.FeedName).ToArray();
        }

        public DataFeed GetFeed()
        {
            return DA.Current.Query<DataFeed>().FirstOrDefault(x => x.FeedAlias == Alias);
        }

        public DataFeed GetFeedByGuid()
        {
            DataFeed result = DA.Current.Query<DataFeed>().FirstOrDefault(x => x.FeedGUID == new Guid(Guid));
            return result;
        }

        public string GetRowClass(DataFeed item)
        {
            return item.Active ? "active-feed" : "inactive-feed";
        }

        public IHtmlString GetLinks(DataFeed item)
        {
            string result = string.Empty;
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/{0}/csv", item.FeedAlias)) + "\" style=\"margin-right: 20px;\">csv</a>";
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/{0}/xml", item.FeedAlias)) + "\" style=\"margin-right: 20px;\">xml</a>";
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/{0}/rss", item.FeedAlias)) + "\" style=\"margin-right: 20px;\">rss</a>";
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/{0}/html", item.FeedAlias)) + "\" style=\"margin-right: 20px;\">html</a>";
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/{0}/json", item.FeedAlias)) + "\" style=\"margin-right: 20px;\">json</a>";
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/{0}/ical", item.FeedAlias)) + "\" style=\"margin-right: 20px;\">ical</a>";
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/console/{0}", item.FeedAlias)) + "\" style=\"margin-right: 10px;\"><img src=\"//ssel-apps.eecs.umich.edu/static/images/edit.png\" alt=\"edit\" /></a>";
            result += "<a href=\"" + VirtualPathUtility.ToAbsolute(string.Format("~/feed/delete/{0}", item.FeedAlias)) + "\" style=\"margin-right: 10px;\"><img src=\"//ssel-apps.eecs.umich.edu/static/images/delete.png\" alt=\"delete\" /></a>";
            return new HtmlString(result);
        }

        public string GetActiveState(DataFeed item)
        {
            return item.Active ? "is:active" : "isnot:active";
        }

        public string GetPrivateState(DataFeed item)
        {
            return item.Private ? "is:private" : "isnot:private";
        }

        public IHtmlString GetFeedAlias(DataFeed item)
        {
            if (item.Private)
                return new HtmlString("<img src=\"//ssel-apps.eecs.umich.edu/static/images/locked.png\" alt=\"Locked\" /> " + item.FeedAlias);
            else
                return new HtmlString(item.FeedAlias);
        }

        public IHtmlString GetError()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
                return new HtmlString(string.Empty);
            else
                return new HtmlString(string.Format("<div style=\"color: #FF0000; margin-bottom: 10px;\">{0}</div>", ErrorMessage));
        }

        public void DeleteFeed()
        {
            DataFeedUtility.DeleteFeed(GetFeed());
        }

        public override SubMenu GetSubMenu()
        {
            return base.GetSubMenu().Clear()
                .Add(new SubMenu.MenuItem() { LinkText = "List", ActionName = "List", ControllerName = "Feed" })
                .Add(new SubMenu.MenuItem() { LinkText = "Console", ActionName = "Console", ControllerName = "Feed" })
                .Add(new SubMenu.MenuItem() { LinkText = "Reports", ActionName = "Reports", ControllerName = "Feed" });
        }

        public DataSet ExecuteQuery(DataFeed feed)
        {
            return DataFeedUtility.ExecuteQuery(feed, Parameters.Create());
        }

        public DataTable[] GetTables(DataSet ds)
        {
            return DataFeedUtility.GetTables(ds, Key).ToArray();
        }

        public bool SaveFeed()
        {
            try
            {
                ErrorMessage = string.Empty;

                DataFeed feed;
                DataFeed existing = GetFeed();

                if (string.IsNullOrEmpty(Guid))
                {
                    //add new feed
                    if (existing != null)
                        throw new Exception("Alias '" + Alias + "' is already in use");

                    feed = new DataFeed
                    {
                        FeedGUID = System.Guid.NewGuid(),
                        FeedAlias = Alias,
                        FeedName = Name,
                        FeedDescription = Description,
                        FeedLink = Link,
                        Private = Private,
                        Active = Active,
                        FeedType = FeedType,
                        FeedQuery = Query
                    };

                    DA.Current.SaveOrUpdate(feed);
                }
                else
                {
                    //update existing feed
                    feed = GetFeedByGuid();

                    if (feed == null)
                        throw new Exception($"Could not find feed using GUID: {Guid}");

                    if (existing != null && existing.FeedGUID != feed.FeedGUID)
                    {
                        Alias = feed.FeedAlias;
                        throw new Exception($"Alias '{existing.FeedAlias}' is already in use.");
                    }

                    if (string.IsNullOrEmpty(Alias))
                    {
                        Alias = feed.FeedAlias;
                        throw new Exception("Alias is required.");
                    }

                    if (string.IsNullOrEmpty(Name))
                        throw new Exception("Name is required.");
                        
                    feed.FeedAlias = Alias;
                    feed.FeedName = Name;
                    feed.FeedDescription = Description;
                    feed.FeedLink = Link;
                    feed.Private = Private;
                    feed.Active = Active;
                    feed.FeedType = FeedType;
                    feed.FeedQuery = Query;

                    DA.Current.SaveOrUpdate(feed);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        public IEnumerable<ReportItem> GetReports()
        {
            var path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Content\\json");

            foreach (var f in Directory.GetFiles(path, "*.json"))
            {
                string alias = Path.GetFileNameWithoutExtension(f);
                string json = File.ReadAllText(f);
                var def = ServiceProvider.Current.Serialization.Json.DeserializeAnonymous(json, new { title = "" });
                yield return new ReportItem() { Alias = alias, Title = def.title };
            }
        }
    }
}