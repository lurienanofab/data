using Data.Controllers.Api;
using LNF;
using LNF.Impl.Repository.Data;
using System;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/activelog")]
    public class ApiActiveLogController : DataApiController
    {
        public ApiActiveLogController(IProvider provider) : base(provider) { }

        public ActiveLogModel Post([FromBody] ActiveLogModel model)
        {
            ActiveLog alog = DataSession.Single<ActiveLog>(model.LogID);

            if (alog == null) return null;

            alog.EnableDate = model.EnableDate;
            DataSession.SaveOrUpdate(alog);

            return new ActiveLogModel() { LogID = alog.LogID, EnableDate = alog.EnableDate };
        }
    }

    public class ActiveLogModel
    {
        public int LogID { get; set; }
        public DateTime EnableDate { get; set; }
    }
}
