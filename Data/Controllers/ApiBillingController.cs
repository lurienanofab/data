using Data.Models.Api;
using LNF;
using LNF.CommonTools;
using LNF.Logging;
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
        [MultipleAuthentication(typeof(BasicAuthenticationAttribute), typeof(FormsAuthenticationAttribute))]
        public IList<ToolBilling> GetToolBilling(DateTime period, int clientId = 0, int limit = 0)
        {
            IList<ToolBilling> query = DA.Current.Query<ToolBilling>().Where(x => x.Period == period).ToList();
            IList<ToolBilling> result = query.Where(x => x.ClientID == (clientId == 0 ? x.ClientID : clientId)).ToList();
            if (limit == 0)
                return result;
            else
                return result.Take(limit).ToList();
        }

        [MultipleAuthentication(typeof(BasicAuthenticationAttribute), typeof(FormsAuthenticationAttribute))]
        public BillingResult Post([FromBody] BillingModel model, bool delete = true)
        {
            HttpContext.Current.Server.ScriptTimeout = 1800;

            if (model == null)
                throw new ArgumentNullException("model");

            DateTime start = DateTime.Now;

            int clientId = (model.ClientID == 0) ? -1 : model.ClientID;

            bool success = true;

            if (model.StartPeriod != DateTime.MinValue)
            {
                if (model.EndPeriod == DateTime.MinValue)
                    model.EndPeriod = model.StartPeriod.AddMonths(1);

                switch (model.Command)
                {
                    case "tool-data-clean":
                        WriteToolDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.ResourceID).WriteToolDataClean();
                        break;
                    case "room-data-clean":
                        WriteRoomDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.RoomID).WriteRoomDataClean(delete);
                        break;
                    case "store-data-clean":
                        WriteStoreDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.ItemID).WriteStoreDataClean();
                        break;
                    case "tool-data":
                        WriteToolDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.ResourceID).WriteToolData();
                        break;
                    case "room-data":
                        WriteRoomDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.RoomID).WriteRoomData();
                        break;
                    case "store-data":
                        WriteStoreDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.ItemID).WriteStoreData();
                        break;
                    case "tool-billing-step1":
                        BillingDataProcessStep1.PopulateToolBilling(model.StartPeriod, model.ClientID, model.IsTemp);
                        break;
                    case "room-billing-step1":
                        BillingDataProcessStep1.PopulateRoomBilling(model.StartPeriod, model.ClientID, model.IsTemp);
                        break;
                    case "store-billing-step1":
                        BillingDataProcessStep1.PopulateStoreBilling(model.StartPeriod, model.IsTemp);
                        break;
                    case "tool-billing-step2":
                        BillingDataProcessStep2.PopulateToolBillingByAccount(model.StartPeriod, model.ClientID);
                        BillingDataProcessStep2.PopulateToolBillingByToolOrg(model.StartPeriod, model.ClientID);
                        break;
                    case "room-billing-step2":
                        BillingDataProcessStep2.PopulateRoomBillingByAccount(model.StartPeriod, model.ClientID);
                        BillingDataProcessStep2.PopulateRoomBillingByRoomOrg(model.StartPeriod, model.ClientID);
                        break;
                    case "store-billing-step2":
                        BillingDataProcessStep2.PopulateStoreBillingByAccount(model.StartPeriod, model.ClientID);
                        BillingDataProcessStep2.PopulateStoreBillingByItemOrg(model.StartPeriod, model.ClientID);
                        break;
                    case "tool-billing-step3":
                        BillingDataProcessStep3.PopulateToolBillingByOrg(model.StartPeriod, clientId);
                        break;
                    case "room-billing-step3":
                        BillingDataProcessStep3.PopulateRoomBillingByOrg(model.StartPeriod, clientId);
                        break;
                    case "store-billing-step3":
                        BillingDataProcessStep3.PopulateStoreBillingByOrg(model.StartPeriod);
                        break;
                    case "subsidy-billing-step4":
                        BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(model.StartPeriod, clientId);
                        break;
                    case "subsidy-distribution":
                        BillingDataProcessStep4Subsidy.DistributeSubsidyMoneyEvenly(model.StartPeriod, model.ClientID);
                        break;
                    case "finalize-data-tables":
                        DataTableManager.Finalize(model.StartPeriod, model.EndPeriod);
                        break;
                    default:
                        success = false;
                        Log.Write(LogMessageLevel.Error, model.Command, string.Format("Unknown command: {0}", model.Command), null);
                        break;
                }
            }
            else
            {
                success = false;
                Log.Write(LogMessageLevel.Error, model.Command, string.Format("Missing parameter: StartPeriod [{0}]", model.StartPeriod), null);
            }

            DateTime end = DateTime.Now;

            return new BillingResult()
            {
                Success = success,
                Command = model.Command,
                Description = "ApiBillingController.Post",
                StartDate = start,
                EndDate = end,
                TimeTaken = (end - start).TotalSeconds,
                LogText = string.Join(Environment.NewLine, ServiceProvider.Current.Log.Current.Select(x => x.Body))
            };
        }
    }
}
