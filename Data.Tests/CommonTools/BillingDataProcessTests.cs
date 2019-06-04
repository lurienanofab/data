using LNF;
using LNF.CommonTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Data.Tests.CommonTools
{
    [TestClass]
    public class BillingDataProcessTests
    {
        [TestMethod]
        public void Step4Subsidy_CanPopulateSubsidyBilling()
        {
            //Dehui Zhang   => 2838
            //James Ricker  => 1419

            //BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(DateTime.Parse("2015-09-01"), 2838);

            var step4 = new BillingDataProcessStep4Subsidy(new LNF.Models.Billing.Process.BillingProcessStep4Command
            {
                ClientID = 1419,
                Command = "subsidy",
                Period = DateTime.Parse("2015-09-01")
            });

            step4.PopulateSubsidyBilling();
        }

        [TestMethod]
        public void Step1_CanPopulateRoomBilling()
        {
            var step1 = new BillingDataProcessStep1(DateTime.Now, ServiceProvider.Current);
            step1.PopulateRoomBilling(DateTime.Parse("2015-09-01"), 2838, false);
        }

        [TestMethod]
        public void Step1_CanPopulatToolBilling()
        {
            //David Pellinen => 189
            var step1 = new BillingDataProcessStep1(DateTime.Now, ServiceProvider.Current);
            step1.PopulateToolBilling(DateTime.Parse("2015-05-01"), 189, false);
        }
    }
}
