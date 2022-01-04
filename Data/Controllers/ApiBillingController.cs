using Data.Controllers.Api;
using Data.Models.Api;
using LNF;
using LNF.CommonTools;
using LNF.Impl.Billing;
using LNF.Impl.Repository.Billing;
using LNF.Logging;
using LNF.WebApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Data.Controllers
{
    public class ApiBillingController : DataApiController
    {
        public ApiBillingController(IProvider provider) : base(provider) { }

        [ApiAuthentication, Route("api/billing/tool")]
        public IList<ToolBilling> GetToolBilling(DateTime period, int clientId = 0, int limit = 0)
        {
            IList<ToolBilling> query = DataSession.Query<ToolBilling>().Where(x => x.Period == period).ToList();
            IList<ToolBilling> result = query.Where(x => x.ClientID == (clientId == 0 ? x.ClientID : clientId)).ToList();
            if (limit == 0)
                return result;
            else
                return result.Take(limit).ToList();
        }

        [ApiAuthentication, Route("api/billing")]
        public BillingResult Post([FromBody] BillingModel model)
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

            ProcessResult result;

            string context = "Data.Controllers.ApiBillingController.Post";

            if (model.StartPeriod != DateTime.MinValue)
            {
                if (model.EndPeriod == DateTime.MinValue)
                    model.EndPeriod = model.StartPeriod.AddMonths(1);

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
                {
                    conn.Open();

                    BillingDataProcessStep1 step1 = new BillingDataProcessStep1(new Step1Config { Connection = conn, ClientID = clientId, IsTemp = model.IsTemp, Period = model.StartPeriod, Now = DateTime.Now, Context = context });
                    BillingDataProcessStep2 step2;
                    BillingDataProcessStep3 step3;
                    BillingDataProcessStep4Subsidy step4;

                    switch (model.Command)
                    {
                        case "tool-data-clean":
                            result = new WriteToolDataCleanProcess(new WriteToolDataCleanConfig { Connection = conn, StartDate = model.StartPeriod, EndDate = model.EndPeriod, ClientID = clientId, Context = context }).Start();
                            message += result.LogText;
                            break;
                        case "room-data-clean":
                            result = new WriteRoomDataCleanProcess(new WriteRoomDataCleanConfig { Connection = conn, StartDate = model.StartPeriod, EndDate = model.EndPeriod, ClientID = clientId, RoomID = model.RoomID, Context = context }).Start();
                            message += result.LogText;
                            break;
                        case "store-data-clean":
                            result = new WriteStoreDataCleanProcess(new WriteStoreDataCleanConfig { Connection = conn, StartDate = model.StartPeriod, EndDate = model.EndPeriod, ClientID = clientId, Context = context }).Start();
                            message += result.LogText;
                            break;
                        case "tool-data":
                            result = new WriteToolDataProcess(new WriteToolDataConfig { Connection = conn, Period = model.StartPeriod, ClientID = clientId, ResourceID = model.ResourceID, Context = context }).Start();
                            message += result.LogText;
                            break;
                        case "room-data":
                            result = new WriteRoomDataProcess(new WriteRoomDataConfig { Connection = conn, Period = model.StartPeriod, ClientID = clientId, RoomID = model.RoomID, Context = context }).Start();
                            message += result.LogText;
                            break;
                        case "store-data":
                            result = new WriteStoreDataProcess(new WriteStoreDataConfig { Connection = conn, Period = model.StartPeriod, ClientID = clientId, ItemID = model.ItemID, Context = context }).Start();
                            message += result.LogText;
                            break;
                        case "tool-billing-step1":
                            result = step1.PopulateToolBilling();
                            message += result.LogText;
                            break;
                        case "room-billing-step1":
                            result = step1.PopulateRoomBilling();
                            message += result.LogText;
                            break;
                        case "store-billing-step1":
                            result = step1.PopulateStoreBilling();
                            message += result.LogText;
                            break;
                        case "tool-billing-step2":
                            step2 = new BillingDataProcessStep2(conn);
                            count = step2.PopulateToolBillingByAccount(model.StartPeriod, clientId);
                            message += $"Tool Step2 By Account: count = {count}";
                            count = step2.PopulateToolBillingByToolOrg(model.StartPeriod, clientId);
                            message += $"{Environment.NewLine}Tool Step2 By Tool Org: count = {count}";
                            break;
                        case "room-billing-step2":
                            step2 = new BillingDataProcessStep2(conn);
                            count = step2.PopulateRoomBillingByAccount(model.StartPeriod, clientId);
                            message += $"Room Step2 By Account: count = {count}";
                            count = step2.PopulateRoomBillingByRoomOrg(model.StartPeriod, clientId);
                            message += $"{Environment.NewLine}Room Step2 By Room Org: count = {count}";
                            break;
                        case "store-billing-step2":
                            step2 = new BillingDataProcessStep2(conn);
                            count = step2.PopulateStoreBillingByAccount(model.StartPeriod, clientId);
                            message += $"Store Step2 By Account: count = {count}";
                            count = step2.PopulateStoreBillingByItemOrg(model.StartPeriod, clientId);
                            message += $"{Environment.NewLine}Store Step2 By Item Org: count = {count}";
                            break;
                        case "tool-billing-step3":
                            step3 = new BillingDataProcessStep3(conn);
                            count = step3.PopulateToolBillingByOrg(model.StartPeriod, clientId);
                            message += $"Tool Step3 By Org: count = {count}";
                            break;
                        case "room-billing-step3":
                            step3 = new BillingDataProcessStep3(conn);
                            count = step3.PopulateRoomBillingByOrg(model.StartPeriod, clientId);
                            message += $"Room Step3 By Org: count = {count}";
                            break;
                        case "store-billing-step3":
                            step3 = new BillingDataProcessStep3(conn);
                            count = step3.PopulateStoreBillingByOrg(model.StartPeriod);
                            message += $"Store Step3 By Org: count = {count}";
                            break;
                        case "subsidy-billing-step4":
                            step4 = new BillingDataProcessStep4Subsidy(new Step4Config { Connection = conn, Period = model.StartPeriod, ClientID = clientId, Context = context });
                            result = step4.PopulateSubsidyBilling();
                            message += result.LogText;
                            break;
                        case "subsidy-distribution":
                            step4 = new BillingDataProcessStep4Subsidy(new Step4Config { Connection = conn, Period = model.StartPeriod, ClientID = clientId, Context = context });
                            step4.DistributeSubsidyMoneyEvenly();
                            message += "Subsidy distribution: complete";
                            break;
                        case "finalize-data-tables":
                            result = DataTableManager.Create(Provider).Finalize(model.StartPeriod);
                            message += result.LogText;
                            break;
                        default:
                            success = false;
                            message += $"Unknown command: {model.Command}";
                            level = LogMessageLevel.Error;
                            break;
                    }
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
