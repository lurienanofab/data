using LNF.Models.Billing.Reports;
using OnlineServices.Api.Billing;
using OnlineServices.Api.Scheduler;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Data.Controllers.Api
{
    public class UtilityController : ApiController
    {
        [Route("utility/api")]
        public string Get()
        {
            return "utility-api";
        }

        [HttpPost, Route("utility/api/email/financial-manager")]
        public async Task<int> SendFinancialManagerEmails([FromBody] FinancialManagerReportOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            using (var client = new BillingClient())
            {
                var result = await client.SendFinancialManagerReport(options);
                return result;
            }
        }

        [HttpPost, Route("utility/api/email/card-expiration")]
        public async Task<int> SendCardExpirationEmails()
        {
            using (var client = new SchedulerServiceClient())
            {
                var result = await client.SendExpiringCardsEmail();
                return result;
            }
        }
    }
}
