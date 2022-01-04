using LNF;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;
using LNF.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models
{
    public class ServiceModel : BaseModel
    {
        public DateTime LogStartDate { get; set; }
        public DateTime LogEndDate { get; set; }
        public DateTime PurgeLogDate { get; set; }
        public string Task { get; set; }

        public void InitLog()
        {
            if (LogStartDate == default)
                LogStartDate = DateTime.Now.Date;
            if (LogEndDate == default)
                LogEndDate = DateTime.Now.Date;
            if (PurgeLogDate == default)
                PurgeLogDate = DateTime.Now.Date;
        }

        public override SubMenu GetSubMenu()
        {
            return base.GetSubMenu()
                .Add(new SubMenu.MenuItem() { LinkText = "Services", ActionName = "Index", ControllerName = "Service" });
        }

        public IList<ServiceLog> GetLogEntries(string service)
        {
            DateTime sd = LogStartDate;
            DateTime ed = LogEndDate.AddDays(1);
            IList<ServiceLog> result = DataSession.Query<ServiceLog>().Where(x => x.ServiceName == service && x.LogDateTime >= sd && x.LogDateTime < ed).OrderBy(x => x.LogDateTime).ToList();
            return result;
        }

        public GenericResult HandleCommand()
        {
            if (Task == "interlock-test")
                return HandleInterlockTest();

            try
            {
                var req = new WorkerRequest { Command = "RunTask", Args = new[] { Task } };
                string message = Provider.Worker.Execute(req);
                return new GenericResult
                {
                    Success = true,
                    Message = message,
                    Data = $"task-{Task}"
                };
            }
            catch(Exception ex)
            {
                return new GenericResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public GenericResult HandleInterlockTest()
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request != null)
                    {
                        if (int.TryParse(HttpContext.Current.Request.QueryString["id"], out int id))
                        {
                            string state = HttpContext.Current.Request.QueryString["state"];
                            if (!string.IsNullOrEmpty(state))
                            {
                                LNF.CommonTools.WagoInterlock.ToggleInterlock(id, state == "on", 0);
                                return new GenericResult() { Success = true, Message = string.Format("{0}: {1}", id, state) };
                            }
                            else
                                return new GenericResult() { Success = false, Message = "Missing parameter: state" };
                        }
                        else
                            return new GenericResult() { Success = false, Message = "Missing parameter: id" };
                    }
                    else
                        return new GenericResult() { Success = false, Message = "HttpContext.Current.Request is null" };
                }
                else
                    return new GenericResult() { Success = false, Message = "HttpContext.Current is null" };
            }
            catch (Exception ex)
            {
                return new GenericResult() { Success = false, Message = ex.ToString() };
            }
        }
    }
}