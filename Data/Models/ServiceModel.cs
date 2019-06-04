using LNF;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;
using OnlineServices.Api.Scheduler;
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

        public GenericResult HandleCommand()
        {
            if (Task == "interlock-test")
                return HandleInterlockTest();

            var ssc = new SchedulerServiceClient();

            string command = "task-" + Task;
            GenericResult result = new GenericResult();
            bool taskResult = true;

            switch (command)
            {
                case "task-5min":
                    var fiveMinuteTaskResult = ssc.RunFiveMinuteTask();
                    break;
                case "task-daily":
                    var dailyTaskResult = ssc.RunDailyTask();
                    break;
                case "task-monthly":
                    var monthlyTaskResult = ssc.RunMonthlyTask();
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