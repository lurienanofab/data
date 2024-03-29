﻿using LNF;
using LNF.Data;
using LNF.DataAccess;
using LNF.Impl;
using LNF.Impl.Repository.Data;
using System.Collections.Generic;
using System.Linq;

namespace Data.Models.Api
{
    public enum ApiOption
    {
        Client,
        Account
    }

    public static class ApiUtility
    {
        public static IProvider Provider => Startup.WebApp.Context.GetInstance<IProvider>();
        public static ISession DataSession => Provider.DataAccess.Session;

        #region ##### Manager ###########################################################

        public static ClientModel[] GetCurrentManagers(ClientModel model)
        {
            ClientOrg co = DataSession.Single<ClientOrg>(model.ClientOrgID);
            var query = DataSession.Query<ClientManager>().Where(x => x.ClientOrg == co && x.Active).Select(x => x.ManagerOrg);
            var current = query.CreateModels();
            return current.Select(CreateClientModel).OrderBy(x => x.DisplayName).ToArray();
        }

        public static ClientModel[] GetCurrentManagers(AccountModel model)
        {
            var query = DataSession.Query<ClientAccount>().Where(x => x.Account.AccountID == model.AccountID && x.Manager && x.Active).Select(x => x.ClientOrg);
            var current = query.CreateModels();
            return current.Select(CreateClientModel).OrderBy(x => x.DisplayName).ToArray();
        }

        public static ClientModel[] GetAvailableManagers(ClientModel model)
        {
            ClientOrg co = DataSession.Single<ClientOrg>(model.ClientOrgID);
            ClientOrg[] current = DataSession.Query<ClientManager>().Where(x => x.ClientOrg == co && x.Active).Select(x => x.ManagerOrg).ToArray();
            IEnumerable<IClient> all = Provider.Data.Client.SelectOrgManagers(co.Org.OrgID);
            ClientModel[] result = all.Where(a => !current.Select(c => c.ClientOrgID).Contains(a.ClientOrgID)).Select(x => CreateClientModel(x)).OrderBy(x => x.DisplayName).ToArray();
            return result;
        }

        public static ClientModel[] GetAllManagers(ClientModel model)
        {
            IEnumerable<IClient> all = Provider.Data.Client.SelectOrgManagers(model.OrgID);
            ClientModel[] result = all.Select(x => CreateClientModel(x)).OrderBy(x => x.DisplayName).ToArray();
            return result;
        }

        public static ClientModel CreateClientModel(IClient c)
        {
            return new ClientModel()
            {
                ClientID = c.ClientID,
                ClientOrgID = c.ClientOrgID,
                OrgID = c.OrgID,
                DisplayName = c.DisplayName,
                OrgName = c.OrgName,
                Email = c.Email,
                Phone = c.Phone
            };
        }

        #endregion

        #region ##### Account ###########################################################

        public static AccountModel[] GetCurrentAccounts(ClientModel model)
        {
            ClientOrg co = DataSession.Single<ClientOrg>(model.ClientOrgID);
            IList<ClientAccount> query = DataSession.Query<ClientAccount>().Where(x => x.ClientOrg == co && x.Active).ToList();
            return query.Select(x => x.Account).Select(x => CreateAccountModel(x)).OrderBy(x => x.AccountName).ToArray();
        }

        public static AccountModel[] GetAvailableAccounts(ClientModel model)
        {
            ClientModel[] managers = ApiUtility.GetCurrentManagers(model);
            AccountModel[] current = ApiUtility.GetCurrentAccounts(model);
            int[] ids = managers.Select(x => x.ClientOrgID).ToArray();
            IList<ClientAccount> query = DataSession.Query<ClientAccount>().Where(x => ids.Contains(x.ClientOrg.ClientOrgID) && x.Active && x.Manager && x.Account.Active).ToList();
            ids = current.Select(x => x.AccountID).ToArray();
            return query
                .Where(x => !ids.Contains(x.Account.AccountID))
                .Select(x => CreateAccountModel(x.Account))
                .Distinct(new AccountModelComparer())
                .OrderBy(x => x.AccountName)
                .ToArray();
        }

        public static AccountModel[] GetAllAccounts(ClientModel model)
        {
            if (model != null)
            {
                int[] clientOrgs = DataSession.Query<ClientOrg>().Where(x => x.Active && x.Client.ClientID == model.ClientID).Select(x => x.ClientOrgID).ToArray();
                int[] accts = DataSession.Query<ClientAccount>().Where(x => x.Active && clientOrgs.Contains(x.ClientOrg.ClientOrgID)).Select(x => x.Account.AccountID).ToArray();
                return DataSession.Query<Account>()
                    .Where(x => x.Active && accts.Contains(x.AccountID))
                    .Select(x => CreateAccountModel(x)).OrderBy(x => x.AccountName)
                    .ToArray();
            }
            else
            {
                return DataSession.Query<Account>()
                    .Where(x => x.Active)
                    .Select(x => CreateAccountModel(x)).OrderBy(x => x.AccountName)
                    .ToArray();
            }
        }

        public static AccountModel CreateAccountModel(Account acct)
        {
            return new AccountModel()
            {
                AccountID = acct.AccountID,
                AccountName = acct.Name,
                ShortCode = acct.ShortCode
            };
        }

        #endregion

        #region ##### Address ###########################################################

        public static AddressModel[] GetCurrentAddresses(string type, int id)
        {
            switch (type)
            {
                case "client":
                    ClientOrg co = DataSession.Single<ClientOrg>(id);
                    if (co != null)
                    {
                        IList<Address> query = DataSession.Query<Address>().Where(x => x.AddressID == co.ClientAddressID).ToList();
                        return query.Select(x => CreateAddressModel(type, x)).OrderBy(x => x.State).ThenBy(x => x.StreetAddress1).ToArray();
                    }
                    break;
            }

            return new AddressModel[] { };
        }

        public static AddressModel CreateAddressModel(string type, Address addr)
        {
            return new AddressModel()
            {
                AddressID = addr.AddressID,
                AddressType = type,
                City = addr.City,
                Country = addr.Country,
                Attention = addr.InternalAddress,
                State = addr.State,
                StreetAddress1 = addr.StrAddress1,
                StreetAddress2 = addr.StrAddress2,
                Zip = addr.Zip
            };
        }

        #endregion
    }
}