using LNF;
using LNF.Billing.Reports;
using LNF.PhysicalAccess;
using System;
using System.Web.Http;

namespace Data.Controllers.Api
{
    public class UtilityController : DataApiController
    {
        public UtilityController(IProvider provider) : base(provider) { }

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

            var result = Provider.Billing.Report.SendFinancialManagerReport(options);

            return result.TotalEmailsSent;
        }

        [HttpPost, Route("utility/api/email/card-expiration")]
        public int SendCardExpirationEmails()
        {
            var result = Provider.Billing.Report.SendCardExpirationReport();
            return result.TotalEmailsSent;
        }

        [HttpPost, Route("utility/api/email/user-apportionment")]
        public int SendApportionmentEmails([FromBody] UserApportionmentReportOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            var result = Provider.Billing.Report.SendUserApportionmentReport(options);

            return result.TotalEmailsSent;
        }
    }
}
