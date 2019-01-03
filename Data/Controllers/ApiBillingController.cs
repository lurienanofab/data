using Data.Models.Api;
using LNF.CommonTools;
using LNF.Logging;
using LNF.Models;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Data.Controllers
{
    public class ApiBillingController : ApiController
    {
        [MultiAuthorize(AuthorizeMethod.Basic | AuthorizeMethod.Forms)]
        public IList<ToolBilling> GetToolBilling(DateTime period, int clientId = 0, int limit = 0)
        {
            IList<ToolBilling> query = DA.Current.Query<ToolBilling>().Where(x => x.Period == period).ToList();
            IList<ToolBilling> result = query.Where(x => x.ClientID == (clientId == 0 ? x.ClientID : clientId)).ToList();
            if (limit == 0)
                return result;
            else
                return result.Take(limit).ToList();
        }

        [MultiAuthorize(AuthorizeMethod.Basic | AuthorizeMethod.Forms)]
        public BillingResult Post([FromBody] BillingModel model, bool delete = true)
        {
            HttpContext.Current.Server.ScriptTimeout = 1800;

            if (model == null)
                throw new ArgumentNullException("model");

            DateTime start = DateTime.Now;

            int clientId = (model.ClientID == 0) ? -1 : model.ClientID;

            bool success = true;

            int count;
            string message = string.Empty;
            string subject = "Data.Controllers.ApiBillingController.Post";
            LogMessageLevel level = LogMessageLevel.Info;


            ProcessResult result = null;

            if (model.StartPeriod != DateTime.MinValue)
            {
                if (model.EndPeriod == DateTime.MinValue)
                    model.EndPeriod = model.StartPeriod.AddMonths(1);

                switch (model.Command)
                {
                    case "tool-data-clean":
                        result = new WriteToolDataCleanProcess(model.StartPeriod, model.EndPeriod, model.ClientID).Start();
                        message += result.LogText;
                        break;
                    case "room-data-clean":
                        result = new WriteRoomDataCleanProcess(model.StartPeriod, model.EndPeriod, model.ClientID).Start();
                        message += result.LogText;
                        break;
                    case "store-data-clean":
                        result = new WriteStoreDataCleanProcess(model.StartPeriod, model.EndPeriod, model.ClientID).Start();
                        message += result.LogText;
                        break;
                    case "tool-data":
                        result = new WriteToolDataProcess(model.StartPeriod, model.ClientID, model.ResourceID).Start();
                        message += result.LogText;
                        break;
                    case "room-data":
                        result = new WriteRoomDataProcess(model.StartPeriod, model.ClientID, model.RoomID).Start();
                        message += result.LogText;
                        break;
                    case "store-data":
                        result = new WriteStoreDataProcess(model.StartPeriod, model.ClientID, model.ItemID).Start();
                        message += result.LogText;
                        break;
                    case "tool-billing-step1":
                        result = BillingDataProcessStep1.PopulateToolBilling(model.StartPeriod, model.ClientID, model.IsTemp);
                        message += result.LogText;
                        break;
                    case "room-billing-step1":
                        result = BillingDataProcessStep1.PopulateRoomBilling(model.StartPeriod, model.ClientID, model.IsTemp);
                        message += result.LogText;
                        break;
                    case "store-billing-step1":
                        result = BillingDataProcessStep1.PopulateStoreBilling(model.StartPeriod, model.IsTemp);
                        message += result.LogText;
                        break;
                    case "tool-billing-step2":
                        count = BillingDataProcessStep2.PopulateToolBillingByAccount(model.StartPeriod, model.ClientID);
                        message += $"Tool Step2 By Account: count = {count}";
                        count = BillingDataProcessStep2.PopulateToolBillingByToolOrg(model.StartPeriod, model.ClientID);
                        message += $"{Environment.NewLine}Tool Step2 By Tool Org: count = {count}";
                        break;
                    case "room-billing-step2":
                        count = BillingDataProcessStep2.PopulateRoomBillingByAccount(model.StartPeriod, model.ClientID);
                        message += $"Room Step2 By Account: count = {count}";
                        count = BillingDataProcessStep2.PopulateRoomBillingByRoomOrg(model.StartPeriod, model.ClientID);
                        message += $"{Environment.NewLine}Room Step2 By Room Org: count = {count}";
                        break;
                    case "store-billing-step2":
                        count = BillingDataProcessStep2.PopulateStoreBillingByAccount(model.StartPeriod, model.ClientID);
                        message += $"Store Step2 By Account: count = {count}";
                        count = BillingDataProcessStep2.PopulateStoreBillingByItemOrg(model.StartPeriod, model.ClientID);
                        message += $"{Environment.NewLine}Store Step2 By Item Org: count = {count}";
                        break;
                    case "tool-billing-step3":
                        count = BillingDataProcessStep3.PopulateToolBillingByOrg(model.StartPeriod, clientId);
                        message += $"Tool Step3 By Org: count = {count}";
                        break;
                    case "room-billing-step3":
                        count = BillingDataProcessStep3.PopulateRoomBillingByOrg(model.StartPeriod, clientId);
                        message += $"Room Step3 By Org: count = {count}";
                        break;
                    case "store-billing-step3":
                        count = BillingDataProcessStep3.PopulateStoreBillingByOrg(model.StartPeriod);
                        message += $"Store Step3 By Org: count = {count}";
                        break;
                    case "subsidy-billing-step4":
                        result = BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(model.StartPeriod, clientId);
                        message += result.LogText;
                        break;
                    case "subsidy-distribution":
                        BillingDataProcessStep4Subsidy.DistributeSubsidyMoneyEvenly(model.StartPeriod, model.ClientID);
                        message += "Subsidy distribution: complete";
                        break;
                    case "finalize-data-tables":
                        result = DataTableManager.Finalize(model.StartPeriod);
                        message += result.LogText;
                        break;
                    default:
                        success = false;
                        message += $"Unknown command: {model.Command}";
                        level = LogMessageLevel.Error;
                        break;
                }
            }
            else
            {
                success = false;
                message += $"Missing parameter: StartPeriod [{model.StartPeriod:yyyy-MM-dd HH:mm:ss}]";
                level = LogMessageLevel.Error;
            }

            Log.Write(level, subject, message, null);

            DateTime end = DateTime.Now;

            return new BillingResult()
            {
                Success = success,
                Command = model.Command,
                Description = subject,
                StartDate = start,
                EndDate = end,
                TimeTaken = (end - start).TotalSeconds,
                LogText = message
            };
        }
    }
}
