using LNF.Repository;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Models
{
    public static class AccountEditUtility
    {
        public static IEnumerable<AccountManagerEdit> GetAvailableManagers(AccountEdit acctEdit)
        {
            var availableManagers = DA.Current.Query<ClientOrgInfo>()
                .Where(x => x.ClientOrgActive && x.OrgID == acctEdit.OrgID && (x.IsManager || x.IsFinManager))
                .OrderBy(x => x.LName)
                .ThenBy(x => x.FName)
                .ToList();

            // remove already added managers from available
            foreach (var mgr in acctEdit.Managers)
            {
                var item = availableManagers.FirstOrDefault(x => x.ClientOrgID == mgr.ClientOrgID);
                if (item != null)
                    availableManagers.Remove(item);
            }

            return availableManagers.Select(x => new AccountManagerEdit()
            {
                ClientOrgID = x.ClientOrgID,
                LName = x.LName,
                FName = x.FName
            }).ToList();
        }

        public static IEnumerable<AccountManagerEdit> GetManagerEdits(int accountId)
        {
            var query = DA.Current.Query<ClientAccountInfo>()
                .Where(x => x.ClientAccountActive && x.AccountID == accountId && x.Manager)
                .OrderBy(x => x.LName)
                .ThenBy(x => x.FName)
                .ToList();

            return query.Select(x => new AccountManagerEdit()
            {
                ClientOrgID = x.ClientOrgID,
                FName = x.FName,
                LName = x.LName
            }).ToList();
        }

        public static AddressEdit GetAddressEdit(int addressId)
        {
            if (addressId == 0) return null;

            var addr = DA.Current.Single<Address>(addressId);

            if (addr == null) return null;

            return new AddressEdit()
            {
                AddressID = addr.AddressID,
                AddressLine1 = addr.StrAddress1,
                AddressLine2 = addr.StrAddress2,
                Attention = addr.InternalAddress,
                City = addr.City,
                Country = addr.Country,
                State = addr.State,
                Zip = addr.Zip
            };
        }

        public static void AddAddress(AccountEdit acctEdit, string addressType, string attention, string addressLine1, string addressLine2, string city, string state, string zip, string country)
        {
            if (acctEdit.Addresses.ContainsKey(addressType))
                acctEdit.Addresses.Remove(addressType);

            acctEdit.Addresses.Add(addressType, new AddressEdit()
            {
                AddressID = 0,
                Attention = attention,
                AddressLine1 = addressLine1,
                AddressLine2 = addressLine2,
                City = city,
                State = state,
                Zip = zip,
                Country = country
            });
        }

        public static void SetProperty(AccountEdit acctEdit, string field, object value)
        {
            if (acctEdit == null)
                throw new ArgumentNullException("acctEdit");

            object target = acctEdit;

            var bits = field.Split('.');
            for (var i = 0; i < bits.Length - 1; i++)
            {
                var propertyToGet = target.GetType().GetProperty(bits[i]);

                if (propertyToGet == null)
                    throw new Exception(string.Format("Unable to find property {0}", bits[i]));

                target = propertyToGet.GetValue(target, null);
            }

            var propertyToSet = target.GetType().GetProperty(bits.Last());

            if (propertyToSet == null)
                throw new Exception(string.Format("Unable to find property {0}", bits.Last()));

            var convertedVal = Convert.ChangeType(value, propertyToSet.PropertyType);

            propertyToSet.SetValue(target, convertedVal, null);
        }

        public static string GetAccountNumber(AccountEdit acctEdit)
        {
            if (acctEdit.ChartFields != null)
            {
                return string.Format("{0}{1}{2}{3}{4}{5}", acctEdit.ChartFields.Account, acctEdit.ChartFields.Fund, acctEdit.ChartFields.Department, acctEdit.ChartFields.Program, acctEdit.ChartFields.Class, acctEdit.ChartFields.Project);
            }
            else
            {
                return acctEdit.AccountNumber;
            }
        }

        public static string GetShortCode(AccountEdit acctEdit)
        {
            if (acctEdit.ChartFields != null)
            {
                return acctEdit.ChartFields.ShortCode;
            }
            else
            {
                return acctEdit.ShortCode;
            }
        }
    }
}