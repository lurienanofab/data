using LNF;
using LNF.Models.Billing.Reports;
using OnlineServices.Api.Scheduler;
using System;
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
        public int SendFinancialManagerEmails([FromBody] FinancialManagerReportOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            var result = ServiceProvider.Current.Billing.Report.SendFinancialManagerReport(options);

            return result.TotalEmailsSent;
        }

        [HttpPost, Route("utility/api/email/card-expiration")]
        public int SendCardExpirationEmails()
        {
            var client = new SchedulerServiceClient();
            var result = client.SendExpiringCardsEmail();
            return result;
        }

        [HttpPost, Route("utility/api/email/apportionment")]
        public int SenApportionmentEmails([FromBody] UserApportionmentReportOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            var result = ServiceProvider.Current.Billing.Report.SendUserApportionmentReport(options);

            return result.TotalEmailsSent;
        }
    }
}
