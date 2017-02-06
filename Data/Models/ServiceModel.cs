using LNF;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;
using OnlineServices.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

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
            if (LogStartDate == default(DateTime))
                LogStartDate = DateTime.Now.Date;
            if (LogEndDate == default(DateTime))
                LogEndDate = DateTime.Now.Date;
            if (PurgeLogDate == default(DateTime))
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
            IList<ServiceLog> result = DA.Current.Query<ServiceLog>().Where(x => x.ServiceName == service && x.LogDateTime >= sd && x.LogDateTime < ed).OrderBy(x => x.LogDateTime).ToList();
            return result;
        }

        public async Task<GenericResult> HandleCommand()
        {
            if (Task == "interlock-test")
                return HandleInterlockTest();

            var cookieValue = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value;
            var opt = new ApiClientOptions()
            {
                AccessToken = cookieValue,
                Host = new Uri(ConfigurationManager.AppSettings["ApiHost"]),
                TokenType = "Forms"
            };

            using (var ssc = await ApiProvider.NewSchedulerServiceClient())
            {
                string command = "task-" + Task;
                GenericResult result = new GenericResult();
                bool taskResult = false;

                switch (command)
                {
                    case "task-5min":
                        taskResult = await ssc.RunFiveMinuteTask();
                        break;
                    case "task-daily":
                        taskResult = await ssc.RunDailyTask();
                        break;
                    case "task-monthly":
                        taskResult = await ssc.RunMonthlyTask();
                        break;
                    default:
                        result.Success = false;
                        result.Message = "Invalid Command";
                        result.Data = command;
                        return result;
                }

                if (taskResult)
                {
                    result.Success = true;
                    result.Message = command;
                }
                else
                {
                    result.Success = false;
                    result.Message = "Service did not respond.";
                    result.Data = command;
                }

                return result;
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
                        int id;
                        if (int.TryParse(HttpContext.Current.Request.QueryString["id"], out id))
                        {
                            string state = HttpContext.Current.Request.QueryString["state"];
                            if (!string.IsNullOrEmpty(state))
                            {
                                LNF.CommonTools.WagoInterlock.ToggleInterlock(id, state == "on", 0).Wait();
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