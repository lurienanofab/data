using LNF.Impl.Billing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Data.Tests.CommonTools
{
    [TestClass]
    public class BillingDataProcessTests
    {
        [TestMethod]
        public void Step4Subsidy_CanPopulateSubsidyBilling()
        {
            //Dehui Zhang    => 2838
            //James Ricker   => 1419
            //David Pellinen => 189

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();

                var step4 = new BillingDataProcessStep4Subsidy(new Step4Config
                {
                    Connection = conn,
                    ClientID = 1419,
                    Period = DateTime.Parse("2015-09-01"),
                    Context = "Data.Tests.CommonTools.BillingDataProcessTests.Step4Subsidy_CanPopulateSubsidyBilling"
                });

                step4.PopulateSubsidyBilling();

                conn.Close();
            }
        }

        [TestMethod]
        public void Step1_CanPopulateRoomBilling()
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();

                var step1 = new BillingDataProcessStep1(new Step1Config
                {
                    Connection = conn,
                    Period = DateTime.Parse("2015-09-01"),
                    ClientID = 2838,
                    IsTemp = false,
                    Now = DateTime.Now,
                    Context = "Data.Tests.CommonTools.BillingDataProcessTests.Step1_CanPopulateRoomBilling"
                });

                step1.PopulateRoomBilling();

                conn.Close();
            }
        }
    }
}
