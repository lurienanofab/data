using Data.Controllers.Api;
using LNF;
using LNF.Impl.Billing;
using LNF.Impl.Repository.Billing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/subsidy")]
    public class ApiSubsidyController : DataApiController
    {
        public ApiSubsidyController(IProvider provider) : base(provider) { }

        public SubsidyModel[] Get(DateTime period, int clientId = 0, bool calculate = false)
        {
            if (calculate) CalculateSubsidy(period, clientId);

            IEnumerable<TieredSubsidyBilling> query;
            if (clientId == 0)
                query = DataSession.Query<TieredSubsidyBilling>().Where(x => x.Period == period);
            else
                query = DataSession.Query<TieredSubsidyBilling>().Where(x => x.Client.ClientID == clientId && x.Period == period);

            var result = query.ToArray().Select(x => new SubsidyModel()
            {
                ClientID = clientId,
                Period = period,
                Room = x.RoomSum,
                RoomMisc = x.RoomMiscSum,
                Tool = x.ToolSum,
                ToolMisc = x.ToolMiscSum
            }).ToArray();

            return result;
        }

        private void CalculateSubsidy(DateTime period, int clientId = 0)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();

                string context = "Data.Controllers.ApiSubsidyController.CalculateSubsidy";

                var step4 = new BillingDataProcessStep4Subsidy(new Step4Config
                {
                    Connection = conn,
                    Period = period,
                    ClientID = clientId,
                    Context = context
                });

                var step2 = new BillingDataProcessStep2(conn);

                step2.PopulateRoomBillingByAccount(period, clientId);
                step2.PopulateToolBillingByAccount(period, clientId);
                step4.PopulateSubsidyBilling();
                step2.PopulateRoomBillingByAccount(period, clientId);
                step2.PopulateRoomBillingByRoomOrg(period, clientId);
                step2.PopulateToolBillingByAccount(period, clientId);
                step2.PopulateToolBillingByToolOrg(period, clientId);

                conn.Close();
            }
        }
    }

    public class SubsidyModel
    {
        public int ClientID { get; set; }
        public DateTime Period { get; set; }
        public decimal Tool { get; set; }
        public decimal ToolMisc { get; set; }
        public decimal Room { get; set; }
        public decimal RoomMisc { get; set; }
    }
}
