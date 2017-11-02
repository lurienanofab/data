using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using LNF.Web.Mvc;

namespace Data.Models
{
    public class ClockModel : BaseModel
    {
        private List<ClockItem> items;

        public string Option { get; set; }
        public string Room { get; set; }
        public string Redirect { get; set; }
        public int Timeout { get; set; }
        public string Format { get; set; }

        public ClockModel()
        {
            items = new List<ClockItem>();
            items.Add(new ClockItem("<div class=\"analog row\" style=\"display: {0};\"><ul class=\"col-md-12 analog-clock\"><li class=\"sec\"></li><li class=\"hour\"></li><li class=\"min\"></li></ul></div>") { Type = "analog", Order = 0, Visible = false });
            items.Add(new ClockItem("<div class=\"digital row\" style=\"display: {0};\"><div class=\"col-md-12 digital-clock\"></div></div>") { Type = "digital", Order = 0, Visible = false });
        }

        public void ApplyOption()
        {
            string opt = string.IsNullOrEmpty(Option) ? "analog-digital-12" : Option; //default

            if (string.IsNullOrEmpty(Option))
            {
                //try to get from web.config
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ClockOption"]))
                    opt = ConfigurationManager.AppSettings["ClockOption"];
            }

            Option = opt;

            string[] splitter = Option.Split('-');

            foreach (var item in items)
            {
                item.Order = 0;
                item.Visible = false;
            }

            int order = 0;
            foreach (string type in splitter)
            {
                ClockItem item = items.FirstOrDefault(x => x.Type == type);
                if (item != null)
                {
                    item.Order = order;
                    item.Visible = true;
                    order++;
                }
                else
                {
                    if (type == "12" || type == "24")
                        Format = type;
                }
            }
        }

        public IEnumerable<ClockItem> GetItems()
        {
            return items.OrderBy(x => x.Order);
        }

        public string GetScreesaverRedirect()
        {
            return string.Format(ConfigurationManager.AppSettings["ScreensaverRedirect"], Timeout, Room);
        }
    }

    public class ClockItem
    {
        private string html;

        public int Order { get; set; }
        public string Type { get; set; }
        public bool Visible { get; set; }

        public ClockItem(string html)
        {
            this.html = html;
        }

        public IHtmlString GetHtml()
        {
            return new HtmlString(string.Format(html, Visible ? "block" : "none"));
        }
    }
}