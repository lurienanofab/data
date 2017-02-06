using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models
{
    public class NewsItem
    {
        public int NewsID { get; set; }
        public int CreatedByClientID { get; set; }
        public int? UpdatedByClientID { get; set; }
        public string ImageFileName { get; set; }
        public string ImageContentType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? SortOrder { get; set; }
        public bool Ticker { get; set; }
        public bool Default { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}