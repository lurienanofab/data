using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LNF.GoogleApi;
using LNF.Web.Mvc;

namespace Data.Models
{
    public class GoogleModel : BaseModel
    {
        public UserInfo UserInfo { get; set; }
        public DriveFile[] Files { get; set; }
        public string FeedAlias { get; set; }
    }
}