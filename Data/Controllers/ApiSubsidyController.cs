using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.CommonTools;
using LNF.Models.Billing.Process;

namespace Data.Controllers
{
    public class ApiSubsidyController : ApiController
    {
        public SubsidyModel[] Get(DateTime period, int clientId = 0, bool calculate = false)
        {
            if (calculate) CalculateSubsidy(period, clientId);

            IEnumerable<TieredSubsidyBilling> query;
            if (clientId == 0)
                query = DA.Current.Query<TieredSubsidyBilling>().Where(x => x.Period == period);
            else
                query = DA.Current.Query<TieredSubsidyBilling>().Where(x => x.Client.ClientID == clientId && x.Period == period);

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
            var step4 = new BillingDataProcessStep4Subsidy(new BillingProcessStep4Command
            {
                Command = "subsidy",
                Period = period,
                ClientID = clientId
            });

            BillingDataProcessStep2.PopulateRoomBillingByAccount(period, clientId);
            BillingDataProcessStep2.PopulateToolBillingByAccount(period, clientId);
            step4.PopulateSubsidyBilling();
            BillingDataProcessStep2.PopulateRoomBillingByAccount(period, clientId);
            BillingDataProcessStep2.PopulateRoomBillingByRoomOrg(period, clientId);
            BillingDataProcessStep2.PopulateToolBillingByAccount(period, clientId);
            BillingDataProcessStep2.PopulateToolBillingByToolOrg(period, clientId);
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
