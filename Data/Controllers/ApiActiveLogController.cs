using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LNF.Repository;
using LNF.Repository.Data;

namespace Data.Controllers
{
    [Route("api/activelog")]
    public class ApiActiveLogController : ApiController
    {
        public ActiveLogModel Post([FromBody] ActiveLogModel model)
        {
            ActiveLog alog = DA.Current.Single<ActiveLog>(model.LogID);

            if (alog == null) return null;

            alog.EnableDate = model.EnableDate;
            DA.Current.SaveOrUpdate(alog);

            return new ActiveLogModel() { LogID = alog.LogID, EnableDate = alog.EnableDate };
        }
    }

    public class ActiveLogModel
    {
        public int LogID { get; set; }
        public DateTime EnableDate { get; set; }
    }
}
