using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LNF.CommonTools;

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
            BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(DateTime.Parse("2015-09-01"), 1419);
        }

        [TestMethod]
        public void Step1_CanPopulateRoomBilling()
        {
            BillingDataProcessStep1.PopulateRoomBilling(DateTime.Parse("2015-09-01"), 2838, false);
        }

        [TestMethod]
        public void Step1_CanPopulatToolBilling()
        {
            //David Pellinen => 189
            BillingDataProcessStep1.PopulateToolBilling(DateTime.Parse("2015-05-01"), 189, false);
        }
    }
}
