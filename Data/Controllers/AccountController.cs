using Data.Models;
using LNF.Data;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Data.Controllers
{
    [LNFAuthorize(ClientPrivilege.Administrator)]
    public class AccountController : Controller
    {
        [Route("account/{orgId?}")]
        public ActionResult Index(int orgId = 0)
        {
            var model = new Models.AccountModel();

            var allOrgs = DA.Current.Query<Org>().OrderBy(x => x.OrgName).ToList();
            var activeOrgs = allOrgs.Where(x => x.Active).OrderBy(x => x.OrgName).ToList();
            var currentOrg = allOrgs.FirstOrDefault(x => x.OrgID == orgId);

            if (currentOrg == null)
                currentOrg = allOrgs.First(x => x.PrimaryOrg); // throw an error if there isn't one

            model.ActiveOrgs = activeOrgs;
            model.CurrentOrg = currentOrg;

            var activeAccounts = DA.Current.Query<Account>().Where(x => x.Active && x.Org.OrgID == currentOrg.OrgID).OrderBy(x => x.Name).ToList();
            model.ActiveAccounts = activeAccounts;

            model.IsChartFieldOrg = AccountChartFields.IsChartFieldOrg(currentOrg);

            return View(model);
        }

        [Route("account/edit/{orgId}/{accountId}")]
        public ActionResult Edit(int orgId, int accountId)
        {
            var currentOrg = DA.Current.Single<Org>(orgId);

            if (currentOrg == null)
                return RedirectToAction("Index", new { orgId });

            var model = new AccountEditModel();
            model.AccountID = accountId;
            model.CurrentOrg = currentOrg;

            var fundingSources = DA.Current.Query<FundingSource>().OrderBy(x => x.FundingSourceName).ToList();
            model.FundingSources = fundingSources;

            var technicalFields = DA.Current.Query<TechnicalField>().OrderBy(x => x.TechnicalFieldName).ToList();
            model.TechnicalFields = technicalFields;

            var specialTopics = DA.Current.Query<SpecialTopic>().OrderBy(x => x.SpecialTopicName).ToList();
            model.SpecialTopics = specialTopics;

            Account acct = null;

            if (accountId > 0)
            {
                acct = DA.Current.Single<Account>(accountId);

                if (acct == null && accountId > 0)
                    return RedirectToAction("Index", new { orgId });

                if (acct.Org.OrgID != orgId)
                    return RedirectToAction("Index", new { orgId });

                model.AccountName = acct.Name;
            }

            AccountEdit acctEdit;

            if (Session["AccountEdit"] == null)
                InitAccountEdit(acct, currentOrg);
            else
            {
                acctEdit = (AccountEdit)Session["AccountEdit"];

                if (acctEdit.OrgID != currentOrg.OrgID || acctEdit.AccountID != accountId)
                    InitAccountEdit(acct, currentOrg);
            }

            acctEdit = (AccountEdit)Session["AccountEdit"];

            model.AvailableManagers = AccountEditUtility.GetAvailableManagers(acctEdit);

            model.IsChartFieldOrg = AccountChartFields.IsChartFieldOrg(currentOrg);

            return View(model);
        }

        [Route("account/delete/{orgId}/{accountId}")]
        public ActionResult Delete(int orgId, int accountId)
        {
            var acct = DA.Current.Single<Account>(accountId);

            if (acct != null)
            {
                // disable ClientAccounts
                var clientAccounts = DA.Current.Query<ClientAccount>().Where(x => x.Account == acct && x.Active).ToList();
                foreach (var ca in clientAccounts)
                {
                    ca.Disable(); // disable access to this account

                    // check that all clients still have another account
                    var co = ca.ClientOrg;
                    var c = co.Client;
                    bool hasActiveAcct = false;
                    throw new Exception("need to do this");
                    // check if client has any active accounts
                    //hasActiveAcct = DataUtility.HasActiveAccount(clientDataRow, dsAccount.Tables["ClientOrg"], dsAccount.Tables["ClientAccount"]);

                    //if (!hasActiveAcct && c != null)
                        //clientDataRow["EnableAccess"] = false;
                }

                acct.Disable();
            }

            return RedirectToAction("Index", new { orgId });
        }

        [Route("account/update/{orgId}")]
        public ActionResult Update(int orgId)
        {
            // save all changes
            if (Session["AccountEdit"] != null)
            {
                var acctEdit = (AccountEdit)Session["AccountEdit"];

                Account acct;
                bool insert = false;

                if (acctEdit.AccountID > 0)
                    acct = DA.Current.Single<Account>(acctEdit.AccountID);
                else
                {
                    acct = new Account();
                    insert = true;
                }

                if (acct != null)
                {
                    acct.Name = acctEdit.AccountName;
                    acct.Number = acctEdit.AccountNumber;
                    acct.ShortCode = acctEdit.ShortCode;
                    acct.FundingSourceID = acctEdit.FundingSourceID;
                    acct.TechnicalFieldID = acctEdit.TechnicalFieldID;
                    acct.SpecialTopicID = acctEdit.SpecialTopicID;
                    acct.AccountType = DA.Current.Single<AccountType>(acctEdit.AccountTypeID);
                    acct.InvoiceNumber = acctEdit.InvoiceNumber;
                    acct.InvoiceLine1 = acctEdit.InvoiceLine1;
                    acct.InvoiceLine2 = acctEdit.InvoiceLine2;
                    acct.PoEndDate = acctEdit.PoEndDate;
                    acct.PoInitialFunds = acctEdit.PoInitialFunds;

                    // handle addresses
                    foreach (var kvp in acctEdit.Addresses)
                    {
                        if (kvp.Value != null)
                        {
                            Address addr;
                            bool insertAddr = false;

                            if (kvp.Value.AddressID == 0)
                            {
                                addr = new Address();
                                insertAddr = true;
                            }
                            else
                                addr = DA.Current.Single<Address>(kvp.Value.AddressID);

                            addr.InternalAddress = kvp.Value.Attention;
                            addr.StrAddress1 = kvp.Value.AddressLine1;
                            addr.StrAddress2 = kvp.Value.AddressLine2;
                            addr.City = kvp.Value.City;
                            addr.State = kvp.Value.State;
                            addr.Zip = kvp.Value.Zip;
                            addr.Country = kvp.Value.Country;

                            if (insertAddr)
                                DA.Current.Insert(addr);

                            if (kvp.Key == "billing")
                                acct.BillAddressID = addr.AddressID;

                            if (kvp.Key == "shipping")
                                acct.ShipAddressID = addr.AddressID;
                        }
                        else
                        {
                            if (kvp.Key == "billing")
                            {
                                if (acct.BillAddressID > 0)
                                    DA.Current.Delete(DA.Current.Single<Address>(acct.BillAddressID));
                                acct.BillAddressID = 0;
                            }
                            if (kvp.Key == "shipping")
                            {
                                if (acct.ShipAddressID > 0)
                                    DA.Current.Delete(DA.Current.Single<Address>(acct.ShipAddressID));
                                acct.ShipAddressID = 0;
                            }
                        }
                    }

                    // handle managers
                    var currentManagers = AccountEditUtility.GetManagerEdits(acctEdit.AccountID).ToList();

                    foreach (var mgr in acctEdit.Managers)
                    {
                        if (!currentManagers.Any(x => x.ClientOrgID == mgr.ClientOrgID))
                        {
                            // adding a new manager

                            // check for an existing ClientAccount to make a manager and reactivate if needed
                            ClientAccount ca;

                            ca = DA.Current.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == mgr.ClientOrgID && x.Account.AccountID == acctEdit.AccountID);

                            if (ca != null)
                            {
                                ca.Manager = true;

                                if (!ca.Active)
                                    ca.Enable();
                            }
                            else
                            {
                                ca = new ClientAccount()
                                {
                                    ClientOrg = DA.Current.Single<ClientOrg>(mgr.ClientOrgID),
                                    Account = DA.Current.Single<Account>(acctEdit.AccountID),
                                    Manager = true,
                                    IsDefault = false
                                };

                                DA.Current.Insert(ca);

                                ca.Enable();
                            }

                            currentManagers.Add(new AccountManagerEdit()
                            {
                                ClientOrgID = ca.ClientOrg.ClientOrgID,
                                FName = ca.ClientOrg.Client.FName,
                                LName = ca.ClientOrg.Client.LName
                            });
                        }
                    }


                    // now check for any deleted managers
                    foreach (var mgr in currentManagers.ToArray())
                    {
                        if (!acctEdit.Managers.Any(x => x.ClientOrgID == mgr.ClientOrgID))
                        {
                            // a current manager was deleted

                            ClientAccount ca = DA.Current.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == mgr.ClientOrgID && x.Account.AccountID == acctEdit.AccountID);

                            if (ca != null)
                            {
                                RemoveManager(ca);
                                currentManagers.Remove(mgr);
                            }
                        }
                    }

                    if (insert)
                    {
                        DA.Current.Insert(acct);
                        acct.Enable();
                    }
                }

                Session.Remove("AccountEdit");
            }

            return RedirectToAction("Index", new { orgId });
        }

        [Route("account/cancel/{orgId}")]
        public ActionResult Cancel(int orgId)
        {
            Session.Remove("AccountEdit");
            return RedirectToAction("Index", new { orgId });
        }

        private void InitAccountEdit(Account acct, Org org)
        {
            AccountEdit acctEdit = new AccountEdit();

            // null means adding a new account

            if (acct != null)
            {
                acctEdit.OrgID = acct.Org.OrgID;
                acctEdit.AccountID = acct.AccountID;
                acctEdit.AccountName = acct.Name;
                acctEdit.AccountNumber = acct.Number;
                acctEdit.AccountTypeID = acct.AccountType.AccountTypeID;
                acctEdit.FundingSourceID = acct.FundingSourceID;
                acctEdit.Managers = AccountEditUtility.GetManagerEdits(acct.AccountID);
                acctEdit.ShortCode = acct.ShortCode.Trim();
                acctEdit.SpecialTopicID = acct.SpecialTopicID;
                acctEdit.TechnicalFieldID = acct.TechnicalFieldID;
                acctEdit.InvoiceNumber = acct.InvoiceNumber;
                acctEdit.InvoiceLine1 = acct.InvoiceLine1;
                acctEdit.InvoiceLine2 = acct.InvoiceLine2;
                acctEdit.PoEndDate = acct.PoEndDate;
                acctEdit.PoInitialFunds = acct.PoInitialFunds;
                acctEdit.PoRemainingFunds = acct.PoRemainingFunds;

                acctEdit.Addresses = new Dictionary<string, AddressEdit>();
                acctEdit.Addresses.Add("billing", AccountEditUtility.GetAddressEdit(acct.BillAddressID));
                acctEdit.Addresses.Add("shipping", AccountEditUtility.GetAddressEdit(acct.ShipAddressID));

                if (AccountChartFields.IsChartFieldOrg(org))
                {
                    var cf = acct.GetChartFields();
                    acctEdit.ChartFields = new AccountChartFieldEdit()
                    {
                        Account = cf.Account,
                        Fund = cf.Fund,
                        Department = cf.Department,
                        Program = cf.Program,
                        Class = cf.Class,
                        Project = cf.Project,
                        ShortCode = cf.ShortCode
                    };
                }
            }
            else
            {
                acctEdit.OrgID = org.OrgID;
                acctEdit.Managers = new List<AccountManagerEdit>();
                acctEdit.Addresses = new Dictionary<string, AddressEdit>();
                acctEdit.Addresses.Add("billing", AccountEditUtility.GetAddressEdit(org.DefBillAddressID));
                acctEdit.Addresses.Add("shipping", AccountEditUtility.GetAddressEdit(org.DefShipAddressID));
                acctEdit.AccountTypeID = 1;

                if (AccountChartFields.IsChartFieldOrg(org))
                    acctEdit.ChartFields = new AccountChartFieldEdit();
            }

            Session["AccountEdit"] = acctEdit;
        }

        private void RemoveManager(ClientAccount mgr)
        {
            // remove access for manager's clients unless another manager has client and account

            mgr.Manager = false;

            // [2018-02-12 jg] Should the manager's ClientAccount be disabled, or just set Manager to false?
            // The code in sselData does not disable, but I'm not sure if this is a bug or a feature.
            // mgr.Disable();

            bool disableAcct; // need to do this because of default account

            var managed = DA.Current.Query<ClientManager>().Where(x => x.ManagerOrg == mgr.ClientOrg && x.Active).ToList();

            // get all the active ClientAccounts for this account, this is a mix of managers and non-managers
            var clientAccounts = DA.Current.Query<ClientAccount>().Where(x => x.Account == mgr.Account && x.Active).ToList();

            foreach (var mcm in managed) // for all clients serviced by this manager
            {
                // get an active ClientAccount for the user managed by this manager if available
                var ca = clientAccounts.FirstOrDefault(x => x.ClientOrg == mcm.ClientOrg);

                if (ca != null) // if the client has access to this account
                {
                    disableAcct = true;
                    var managers = DA.Current.Query<ClientManager>().Where(x => x.ClientOrg == mcm.ClientOrg && x.Active).ToList();
                    foreach (var ccm in managers) // for all of the client's managers
                    {
                        if (clientAccounts.Any(x => x.Active && x.Manager && x.ClientOrg == ccm.ManagerOrg)) // another of this clients managers is managing this account
                            disableAcct = false;
                    }

                    if (disableAcct)
                        ca.Disable();
                }
            }

        }
    }
}
