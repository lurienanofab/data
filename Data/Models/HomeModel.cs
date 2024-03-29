﻿using LNF.DataAccess;
using LNF.Impl.Repository.Data;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Models
{
    public class HomeModel : BaseModel
    {
        public string Type { get; set; }
        public string Search { get; set; }
        public int History { get; set; }

        public override SubMenu GetSubMenu()
        {
            return base.GetSubMenu().Clear();
        }

        public string GetSearch(string type)
        {
            if (type == Type)
                return Search;
            else
                return string.Empty;
        }

        public IEnumerable<Client> ClientSearch()
        {
            IList<Client> query;

            if (string.IsNullOrEmpty(Search))
                query = DataSession.Query<Client>().ToList();
            else
            {
                query = new List<Client>();
                if (GetSingle<Client>("client", query))
                    return query.ToArray();
                else
                {
                    query = DataSession.Query<Client>().Where(x =>
                        x.ClientID.ToString() == Search
                        || x.UserName.ToLower().Contains(Search.ToLower())
                        || x.LName.ToLower().Contains(Search.ToLower())
                        || x.FName.ToLower().Contains(Search.ToLower())
                        || (x.LName + ", " + x.FName).ToLower().Contains(Search.ToLower())
                        || (x.FName + " " + x.LName).ToLower().Contains(Search.ToLower())).ToList();
                }
            }

            return query.ToArray();
        }

        public ClientOrg[] ClientOrgSearch()
        {
            IList<ClientOrg> query = new List<ClientOrg>();
            if (string.IsNullOrEmpty(Search))
                query = DataSession.Query<ClientOrg>().ToList();
            else
            {
                if (GetSingle<ClientOrg>("co", query))
                    return query.ToArray();
                else if (GetMultiple<ClientOrg>("client", query, id => x => x.Client.ClientID == id))
                    return query.ToArray();
                else if (GetMultiple<ClientOrg>("org", query, id => x => x.Org.OrgID == id))
                    return query.ToArray();
                else
                {
                    query = DataSession.Query<ClientOrg>().Where(x =>
                        x.ClientOrgID.ToString() == Search
                        || x.Client.UserName.ToLower().Contains(Search.ToLower())
                        || x.Client.LName.ToLower().Contains(Search.ToLower())
                        || x.Client.FName.ToLower().Contains(Search.ToLower())
                        || (x.Client.LName + ", " + x.Client.FName).ToLower().Contains(Search.ToLower())
                        || (x.Client.FName + " " + x.Client.LName).ToLower().Contains(Search.ToLower())
                        || x.Email.ToLower().Contains(Search.ToLower())
                        || x.Org.OrgName.ToLower().Contains(Search.ToLower())).ToList();
                }
            }

            return query.ToArray();
        }

        public ClientAccount[] ClientAccountSearch()
        {
            IList<ClientAccount> query = new List<ClientAccount>();
            if (string.IsNullOrEmpty(Search))
                query = DataSession.Query<ClientAccount>().ToList();
            else
            {
                if (GetSingle("ca", query))
                    return query.ToArray();
                else if (GetMultiple("co", query, id => x => x.ClientOrg.ClientOrgID == id))
                    return query.ToArray();
                else if (GetMultiple("acct", query, id => x => x.Account.AccountID == id))
                    return query.ToArray();
                else
                {
                    query = DataSession.Query<ClientAccount>().Where(x =>
                        x.ClientAccountID.ToString() == Search
                        || x.ClientOrg.Client.UserName.ToLower().Contains(Search.ToLower())
                        || x.ClientOrg.Client.LName.ToLower().Contains(Search.ToLower())
                        || x.ClientOrg.Client.FName.ToLower().Contains(Search.ToLower())
                        || (x.ClientOrg.Client.LName + ", " + x.ClientOrg.Client.FName).ToLower().Contains(Search.ToLower())
                        || (x.ClientOrg.Client.FName + " " + x.ClientOrg.Client.LName).ToLower().Contains(Search.ToLower())
                        || x.ClientOrg.Email.ToLower().Contains(Search.ToLower())
                        || x.Account.Org.OrgName.ToLower().Contains(Search.ToLower())
                        || x.Account.Name.ToLower().Contains(Search.ToLower())
                        || x.Account.Number.ToLower().Contains(Search.ToLower())
                        || x.Account.ShortCode.ToLower().Contains(Search.ToLower())).ToList();
                }
            }

            return query.ToArray();
        }

        public ClientManager[] ClientManagerSearch()
        {
            var query = new List<ClientManager>();
            if (string.IsNullOrEmpty(Search))
                query = DataSession.Query<ClientManager>().ToList();
            else
            {
                if (GetSingle("cm", query))
                    return query.ToArray();
                else if (GetMultiple("co", query, id => x => x.ClientOrg.ClientOrgID == id))
                    return query.ToArray();
                else if (GetMultiple("mo", query, id => x => x.ManagerOrg.ClientOrgID == id))
                    return query.ToArray();
                else
                {
                    query = DataSession.Query<ClientManager>().Where(x =>
                        x.ClientManagerID.ToString() == Search
                        || x.ClientOrg.Client.UserName.ToLower().Contains(Search.ToLower())
                        || x.ClientOrg.Client.LName.ToLower().Contains(Search.ToLower())
                        || x.ClientOrg.Client.FName.ToLower().Contains(Search.ToLower())
                        || (x.ClientOrg.Client.LName + ", " + x.ClientOrg.Client.FName).ToLower().Contains(Search.ToLower())
                        || (x.ClientOrg.Client.FName + " " + x.ClientOrg.Client.LName).ToLower().Contains(Search.ToLower())
                        || x.ClientOrg.Email.ToLower().Contains(Search.ToLower())
                        || x.ManagerOrg.Client.UserName.ToLower().Contains(Search.ToLower())
                        || x.ManagerOrg.Client.LName.ToLower().Contains(Search.ToLower())
                        || x.ManagerOrg.Client.FName.ToLower().Contains(Search.ToLower())
                        || (x.ManagerOrg.Client.LName + ", " + x.ManagerOrg.Client.FName).ToLower().Contains(Search.ToLower())
                        || (x.ManagerOrg.Client.FName + " " + x.ManagerOrg.Client.LName).ToLower().Contains(Search.ToLower())
                        || x.ManagerOrg.Email.ToLower().Contains(Search.ToLower())).ToList();
                }
            }

            return query.ToArray();
        }

        public Org[] OrgSearch()
        {
            IList<Org> query = new List<Org>();
            if (string.IsNullOrEmpty(Search))
                query = DataSession.Query<Org>().ToList();
            else
            {
                if (GetSingle("org", query))
                    return query.ToArray();
                else
                {
                    query = DataSession.Query<Org>().Where(x =>
                        x.OrgID.ToString() == Search
                        || x.OrgName.ToLower().Contains(Search.ToLower())
                        || x.OrgType.OrgTypeName.ToLower().Contains(Search.ToLower())).ToArray();
                }
            }

            return query.ToArray();
        }

        public Account[] AccountSearch()
        {
            IList<Account> query = new List<Account>();
            if (string.IsNullOrEmpty(Search))
                query = DataSession.Query<Account>().ToList();
            else
            {
                if (GetSingle("acct", query))
                    return query.ToArray();
                else if (GetMultiple("org", query, id => x => x.Org.OrgID == id))
                    return query.ToArray();
                else
                {
                    query = DataSession.Query<Account>().Where(x =>
                        x.AccountID.ToString() == Search
                        || x.Name.ToLower().Contains(Search.ToLower())
                        || x.Number.ToLower().Contains(Search.ToLower())
                        || x.ShortCode.ToLower().Contains(Search.ToLower())
                        || x.AccountType.AccountTypeName.ToLower().Contains(Search.ToLower())).ToArray();
                }
            }

            return query.ToArray();
        }

        public bool GetMultiple<T>(string prefix, IList<T> query, Func<int, Expression<Func<T, bool>>> search) where T : class, IDataItem
        {
            prefix += ":";
            if (Search.StartsWith(prefix))
            {
                if (int.TryParse(Search.Substring(prefix.Length), out int id))
                {
                    IList<T> items = DataSession.Query<T>().Where(search(id)).ToList();
                    foreach (T i in items)
                        query.Add(i);
                }

                return true;
            }

            return false;
        }

        public bool GetSingle<T>(string prefix, IList<T> query) where T : class, IDataItem
        {
            prefix += ":";
            if (Search.StartsWith(prefix))
            {
                if (int.TryParse(Search.Substring(prefix.Length), out int id))
                {
                    T item = DataSession.Single<T>(id);
                    if (item != null)
                        query.Add(item);
                }

                return true;
            }

            return false;
        }

        public string GetLink(string type, string prefix, int id)
        {
            string result = "?";
            result += "type=" + type + "&";
            result += "search=" + prefix + ":" + id.ToString();
            return result;
        }

        public string GetHistoryLink(int id)
        {
            string result = "?";
            result += "type=" + Type + "&";
            result += "search=" + Search + "&";
            result += "history=" + id.ToString();
            result += "#history";
            return result;
        }

        public bool ShowHistory()
        {
            return History > 0;
        }

        public string GetTableName()
        {
            switch (Type)
            {
                case "client":
                    return "Client";
                case "client-org":
                    return "ClientOrg";
                case "client-account":
                    return "ClientAccount";
                case "client-manager":
                    return "ClientManager";
                case "account":
                    return "Account";
                case "org":
                    return "Org";
                default:
                    return string.Empty;
            }
        }

        public ActiveLog[] GetHistory()
        {
            string tableName = GetTableName();

            if (!string.IsNullOrEmpty(tableName))
            {
                IList<ActiveLog> query = DataSession.Query<ActiveLog>().Where(x => x.TableName == tableName && x.Record == History).ToList();
                return query.ToArray();
            }

            return new ActiveLog[] { };
        }
    }
}