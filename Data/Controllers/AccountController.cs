using Data.Models;
using LNF;
using LNF.Data;
using LNF.Impl;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using LNF.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Data.Controllers
{
    [LNFAuthorize(ClientPrivilege.Administrator)]
    public class AccountController : DataController
    {
        public AccountController(IProvider provider) : base(provider) { }

        [Route("account/{orgId?}")]
        public ActionResult Index(int orgId = 0)
        {
            var model = new AccountModel();

            var allOrgs = DataSession.Query<Org>().OrderBy(x => x.OrgName).ToList();
            var activeOrgs = allOrgs.Where(x => x.Active).OrderBy(x => x.OrgName).ToList();
            var currentOrg = allOrgs.FirstOrDefault(x => x.OrgID == orgId);

            if (currentOrg == null)
                currentOrg = allOrgs.First(x => x.PrimaryOrg); // throw an error if there isn't one

            model.ActiveOrgs = activeOrgs.CreateModels<IOrg>();
            model.CurrentOrg = currentOrg.CreateModel<IOrg>();

            var activeAccounts = DataSession.Query<Account>().Where(x => x.Active && x.Org.OrgID == currentOrg.OrgID).OrderBy(x => x.Name).ToList();
            model.ActiveAccounts = activeAccounts.CreateModels<IAccount>();

            model.IsChartFieldOrg = AccountChartFields.IsChartFieldOrg(currentOrg.CreateModel<IOrg>());

            return View(model);
        }

        [Route("account/edit/{orgId}/{accountId}")]
        public ActionResult Edit(int orgId, int accountId)
        {
            var currentOrg = DataSession.Single<Org>(orgId);

            if (currentOrg == null)
                return RedirectToAction("Index", new { orgId });

            var model = new AccountEditModel
            {
                AccountID = accountId,
                CurrentOrg = currentOrg
            };

            var fundingSources = DataSession.Query<FundingSource>().OrderBy(x => x.FundingSourceName).ToList();
            model.FundingSources = fundingSources;

            var technicalFields = DataSession.Query<TechnicalField>().OrderBy(x => x.TechnicalFieldName).ToList();
            model.TechnicalFields = technicalFields;

            var specialTopics = DataSession.Query<SpecialTopic>().OrderBy(x => x.SpecialTopicName).ToList();
            model.SpecialTopics = specialTopics;

            Account acct = null;

            if (accountId > 0)
            {
                acct = DataSession.Single<Account>(accountId);

                if (acct == null && accountId > 0)
                    return RedirectToAction("Index", new { orgId });

                if (acct.Org.OrgID != orgId)
                    return RedirectToAction("Index", new { orgId });

                model.AccountName = acct.Name;
            }

            AccountEdit acctEdit;

            IAccount a = acct.CreateModel<IAccount>();
            IOrg o = currentOrg.CreateModel<IOrg>();

            if (Session["AccountEdit"] == null)
                InitAccountEdit(a, o);
            else
            {
                acctEdit = (AccountEdit)Session["AccountEdit"];

                if (acctEdit.OrgID != currentOrg.OrgID || acctEdit.AccountID != accountId)
                    InitAccountEdit(a, o);
            }

            acctEdit = (AccountEdit)Session["AccountEdit"];

            model.AvailableManagers = AccountEditUtility.GetAvailableManagers(acctEdit);

            model.IsChartFieldOrg = AccountChartFields.IsChartFieldOrg(o);

            return View(model);
        }

        [Route("account/delete/{orgId}/{accountId}")]
        public ActionResult Delete(int orgId, int accountId)
        {
            var acct = DataSession.Single<Account>(accountId);

            if (acct != null)
            {
                // disable ClientAccounts
                var clientAccounts = DataSession.Query<ClientAccount>().Where(x => x.Account == acct && x.Active).ToList();
                foreach (var ca in clientAccounts)
                {
                    // disable access to this account
                    Provider.Data.ActiveLog.Disable(ca);

                    // check that all clients still have another account
                    var co = ca.ClientOrg;
                    var c = co.Client;

                    throw new Exception("need to do this");
                    // check if client has any active accounts
                    //bool hasActiveAcct = false;
                    //hasActiveAcct = DataUtility.HasActiveAccount(clientDataRow, dsAccount.Tables["ClientOrg"], dsAccount.Tables["ClientAccount"]);

                    //if (!hasActiveAcct && c != null)
                    //clientDataRow["EnableAccess"] = false;
                }

                Provider.Data.ActiveLog.Disable(acct);
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
                    acct = DataSession.Single<Account>(acctEdit.AccountID);
                else
                {
                    acct = new Account { Org = DataSession.Single<Org>(orgId) };
                    insert = true;
                }

                if (acct != null)
                {
                    acct.Name = acctEdit.AccountName;
                    acct.Number = AccountEditUtility.GetAccountNumber(acctEdit);
                    acct.ShortCode = AccountEditUtility.GetShortCode(acctEdit);
                    acct.FundingSourceID = acctEdit.FundingSourceID;
                    acct.TechnicalFieldID = acctEdit.TechnicalFieldID;
                    acct.SpecialTopicID = acctEdit.SpecialTopicID;
                    acct.AccountType = DataSession.Single<AccountType>(acctEdit.AccountTypeID);
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
                                addr = DataSession.Single<Address>(kvp.Value.AddressID);

                            addr.InternalAddress = kvp.Value.Attention;
                            addr.StrAddress1 = kvp.Value.AddressLine1;
                            addr.StrAddress2 = kvp.Value.AddressLine2;
                            addr.City = kvp.Value.City;
                            addr.State = kvp.Value.State;
                            addr.Zip = kvp.Value.Zip;
                            addr.Country = kvp.Value.Country;

                            if (insertAddr)
                                DataSession.Insert(addr);

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
                                    DataSession.Delete(DataSession.Single<Address>(acct.BillAddressID));
                                acct.BillAddressID = 0;
                            }
                            if (kvp.Key == "shipping")
                            {
                                if (acct.ShipAddressID > 0)
                                    DataSession.Delete(DataSession.Single<Address>(acct.ShipAddressID));
                                acct.ShipAddressID = 0;
                            }
                        }
                    }

                    if (insert)
                    {
                        DataSession.Insert(acct);
                        Provider.Data.ActiveLog.Enable(acct);
                    }

                    // handle managers
                    var currentManagers = AccountEditUtility.GetManagerEdits(acct.AccountID).ToList();

                    foreach (var mgr in acctEdit.Managers)
                    {
                        if (!currentManagers.Any(x => x.ClientOrgID == mgr.ClientOrgID))
                        {
                            // adding a new manager

                            // check for an existing ClientAccount to make a manager and reactivate if needed
                            ClientAccount ca;

                            ca = DataSession.Query<ClientAccount>().FirstOrDefault(x => x.ClientOrg.ClientOrgID == mgr.ClientOrgID && x.Account == acct);

                            if (ca != null)
                            {
                                ca.Manager = true;
                                if (!ca.Active)
                                    Provider.Data.ActiveLog.Enable(ca);
                            }
                            else
                            {
                                ca = new ClientAccount()
                                {
                                    ClientOrg = DataSession.Single<ClientOrg>(mgr.ClientOrgID),
                                    Account = acct,
                                    Manager = true,
                                    IsDefault = false
                                };

                                DataSession.Insert(ca);

                                Provider.Data.ActiveLog.Enable(ca);
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

                            ClientAccount ca = DataSession.Query<ClientAccount>()
                                .FirstOrDefault(x => x.ClientOrg.ClientOrgID == mgr.ClientOrgID && x.Account == acct);

                            if (ca != null)
                            {
                                RemoveManager(ca);
                                currentManagers.Remove(mgr);
                            }
                        }
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

        private void InitAccountEdit(IAccount acct, IOrg org)
        {
            AccountEdit acctEdit = new AccountEdit();

            // null means adding a new account

            if (acct != null)
            {
                acctEdit.OrgID = acct.OrgID;
                acctEdit.AccountID = acct.AccountID;
                acctEdit.AccountName = acct.AccountName;
                acctEdit.AccountNumber = acct.AccountNumber;
                acctEdit.AccountTypeID = acct.AccountTypeID;
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

                acctEdit.Addresses = new Dictionary<string, AddressEdit>
                {
                    { "billing", AccountEditUtility.GetAddressEdit(acct.BillAddressID) },
                    { "shipping", AccountEditUtility.GetAddressEdit(acct.ShipAddressID) }
                };

                if (AccountChartFields.IsChartFieldOrg(org))
                {
                    var cf = ServiceProvider.Current.Data.Account.GetChartFields(acct);
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
                acctEdit.Addresses = new Dictionary<string, AddressEdit>
                {
                    { "billing", AccountEditUtility.GetAddressEdit(org.DefBillAddressID) },
                    { "shipping", AccountEditUtility.GetAddressEdit(org.DefShipAddressID) }
                };
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

            var managed = DataSession.Query<ClientManager>().Where(x => x.ManagerOrg == mgr.ClientOrg && x.Active).ToList();

            // get all the active ClientAccounts for this account, this is a mix of managers and non-managers
            var clientAccounts = DataSession.Query<ClientAccount>().Where(x => x.Account == mgr.Account && x.Active).ToList();

            foreach (var mcm in managed) // for all clients serviced by this manager
            {
                // get an active ClientAccount for the user managed by this manager if available
                var ca = clientAccounts.FirstOrDefault(x => x.ClientOrg == mcm.ClientOrg);

                if (ca != null) // if the client has access to this account
                {
                    disableAcct = true;
                    var managers = DataSession.Query<ClientManager>().Where(x => x.ClientOrg == mcm.ClientOrg && x.Active).ToList();
                    foreach (var ccm in managers) // for all of the client's managers
                    {
                        if (clientAccounts.Any(x => x.Active && x.Manager && x.ClientOrg == ccm.ManagerOrg)) // another of this clients managers is managing this account
                            disableAcct = false;
                    }

                    if (disableAcct)
                        Provider.Data.ActiveLog.Disable(ca);
                }
            }

        }
    }
}
