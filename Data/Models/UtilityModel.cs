using LNF;
using LNF.CommonTools;
using LNF.Control;
using LNF.DataAccess;
using LNF.Impl.Repository.Control;
using LNF.Impl.Repository.Data;
using LNF.Impl.Repository.Scheduler;
using LNF.Util.AutoEnd;
using LNF.Web.Mvc;
using LNF.Web.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Data.Models
{
    public class UtilityModel : BaseModel
    {
        private Point _point;

        public int Record { get; set; }
        public string TableName { get; set; }
        public ActiveLog ActiveLog { get; set; }
        public DateTime EnableDate { get; set; }
        public string Command { get; set; }
        public DateTime? Period { get; set; }
        public int ReservationID { get; set; }
        public IEnumerable<AutoEndProblem> AutoEndProblems { get; set; }

        public IEnumerable<SelectListItem> GetYearSelectItems()
        {
            return Utility.GetYears(2003).Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString(), Selected = DateTime.Now.AddMonths(-1).Year == x });
        }

        public IEnumerable<SelectListItem> GetMonthSelectItems()
        {
            for (int x = 0; x < 12; x++)
            {
                int m = x + 1;
                yield return new SelectListItem() { Text = new DateTime(DateTime.Now.Year, m, 1).ToString("MMMM"), Value = m.ToString(), Selected = DateTime.Now.AddMonths(-1).Month == m };
            }
        }

        public override SubMenu GetSubMenu()
        {
            return base.GetSubMenu()
                .Clear()
                .Add(new SubMenu.MenuItem() { LinkText = "Utility", ActionName = "Index", ControllerName = "Utility" })
                .Add(new SubMenu.MenuItem() { LinkText = "Control", ActionName = "Control", ControllerName = "Utility" })
                .Add(new SubMenu.MenuItem() { LinkText = "Fees", ActionName = "Fees", ControllerName = "Utility" })
                .Add(new SubMenu.MenuItem() { LinkText = "ActiveLog", ActionName = "ActiveLog", ControllerName = "Utility" })
                .Add(new SubMenu.MenuItem() { LinkText = "Billing Checks", ActionName = "BillingChecks", ControllerName = "Utility" });
        }

        public ActionInstance[] GetInstances()
        {
            return DataSession.Query<ActionInstance>().ToArray();
        }

        private Point GetPoint(int pointId)
        {
            if (_point == null)
                _point = DataSession.Query<Point>().First(x => x.PointID == pointId);
            return _point;
        }

        public PointState GetPointState(int pointId)
        {
            var point = GetPoint(pointId);
            var block = point.Block;
            var resp = ServiceProvider.Current.Control.GetBlockState(block.BlockID);
            var blockState = resp.BlockState;
            var result = blockState.Points.First(x => x.PointID == point.PointID);
            return result;
        }

        public string GetPointName(int pointId)
        {
            var point = GetPoint(pointId);
            return point.Name;
        }

        public void SetPointState(int actionId, bool state, ActionType action = ActionType.Interlock, int duration = 0)
        {
            uint d = duration > 0 ? (uint)duration : 0;
            var inst = Provider.Control.GetActionInstance(action, actionId);

            if (inst == null)
                throw new Exception(string.Format("Cannot find the ActionInstance for ActionID = {0}, Action = {1}", actionId, action));

            var point = inst.GetPoint();

            Provider.Control.SetPointState(point.PointID, state, d);
        }

        public ActionInstance GetActionInstance(int pointId, ActionType actionType = ActionType.Interlock)
        {
            string actionName = Enum.GetName(typeof(ActionType), actionType);
            return DataSession.Query<ActionInstance>().First(x => x.Point == pointId && x.ActionName == actionName);
        }

        public Block GetBlock(int pointId)
        {
            Point p = DataSession.Single<Point>(pointId);
            return p.Block;
        }

        public ResourceState[] GetResources()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ResourceID", typeof(int));
            dt.Columns.Add("ResourceName", typeof(string));
            dt.Columns.Add("BuildingName", typeof(string));
            dt.Columns.Add("LabName", typeof(string));
            dt.Columns.Add("ProcessTechName", typeof(string));

            ResourceInfo[] query = DataSession.Query<ResourceInfo>().Where(x => x.ResourceIsActive).ToArray();
            foreach (var r in query)
                dt.Rows.Add(r.ResourceID, r.ResourceName, r.BuildingName, r.LabName, r.ProcessTechName);

            WagoInterlock.AllToolStatus(dt);

            ResourceState[] result = dt.AsEnumerable().Select(x => new ResourceState()
            {
                ResourceID = x.Field<int>("ResourceID"),
                ResourceName = x.Field<string>("ResourceName"),
                BuildingName = x.Field<string>("BuildingName"),
                LabName = x.Field<string>("LabName"),
                ProcessTechName = x.Field<string>("ProcessTechName"),
                PointID = x.Field<int>("PointID"),
                InterlockStatus = x.Field<string>("InterlockStatus"),
                InterlockState = x.Field<bool>("InterlockState"),
                InterlockError = x.Field<bool>("InterlockError"),
                IsInterlocked = x.Field<bool>("IsInterlocked")
            }).ToArray();

            return result;
        }

        public FeeItem[] GetFeeItems()
        {
            //List<FeeItem> list = new List<FeeItem>();
            //FeeItem item = new FeeItem();
            throw new NotImplementedException();
        }

        public ClientAccount GetClientAccount(int clientAccountId)
        {
            return DataSession.Single<ClientAccount>(clientAccountId);
        }

        public UserInfoItem[] GetUserInfoItems(int currentUserClientId)
        {
            List<UserInfoItem> list = new List<UserInfoItem>();

            string tableName = !string.IsNullOrEmpty(TableName) ? TableName.ToLower() : "client";

            int id = Record != 0 ? Record : currentUserClientId;

            switch (tableName)
            {
                case "client":
                    var client = DataSession.Single<Client>(id);
                    list.Add(new UserInfoItem() { Label = "Client", Text = client.DisplayName });
                    break;
                case "account":
                    var acct = DataSession.Single<Account>(id);
                    list.Add(new UserInfoItem() { Label = "Account", Text = GetAccountName(acct) });
                    break;
                case "org":
                    var org = DataSession.Single<Org>(id);
                    list.Add(new UserInfoItem() { Label = "Org", Text = org.OrgName });
                    break;
                case "clientaccount":
                    var ca = DataSession.Single<ClientAccount>(id);
                    list.Add(new UserInfoItem() { Label = "Client", Text = ca.ClientOrg.Client.DisplayName });
                    list.Add(new UserInfoItem() { Label = "Account", Text = GetAccountName(ca.Account) });
                    list.Add(new UserInfoItem() { Label = "Org", Text = ca.ClientOrg.Org.OrgName });
                    break;
                case "clientorg":
                    var co = DataSession.Single<ClientOrg>(id);
                    list.Add(new UserInfoItem() { Label = "Client", Text = co.Client.DisplayName });
                    list.Add(new UserInfoItem() { Label = "Org", Text = co.Org.OrgName });
                    break;
                case "clientmanager":
                    var cm = DataSession.Single<ClientManager>(id);
                    list.Add(new UserInfoItem() { Label = "Client", Text = cm.ClientOrg.Client.DisplayName });
                    list.Add(new UserInfoItem() { Label = "Manager", Text = cm.ManagerOrg.Client.DisplayName });
                    list.Add(new UserInfoItem() { Label = "Org", Text = cm.ClientOrg.Org.OrgName });
                    break;
            }
            return list.ToArray();
        }

        private string GetAccountName(Account acct)
        {
            string result = acct.Name;
            string shortCode = acct.ShortCode.Trim();
            if (!string.IsNullOrEmpty(shortCode))
                result += " [" + shortCode + "]";
            return result;
        }

        public ActiveLog[] GetLastActiveLogs(int currentUserClientId)
        {
            List<ActiveLog> list = new List<ActiveLog>();

            string tableName = !string.IsNullOrEmpty(TableName) ? TableName.ToLower() : "client";
            int id = Record != 0 ? Record : currentUserClientId;

            var current = DataSession.Query<ActiveLog>().Where(x => x.TableName.ToLower() == tableName && x.Record == id).OrderByDescending(x => x.LogID).FirstOrDefault();

            list.Add(current);

            switch (tableName)
            {
                case "clientaccount":
                    var ca = DataSession.Single<ClientAccount>(Record);
                    list.Add(DataSession.Query<ActiveLog>().Where(x => x.TableName == "ClientOrg" && x.Record == ca.ClientOrg.ClientOrgID).OrderByDescending(x => x.LogID).FirstOrDefault());
                    list.Add(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Client" && x.Record == ca.ClientOrg.Client.ClientID).OrderByDescending(x => x.LogID).FirstOrDefault());
                    list.Add(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Account" && x.Record == ca.Account.AccountID).OrderByDescending(x => x.LogID).FirstOrDefault());
                    list.Add(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Org" && x.Record == ca.ClientOrg.Org.OrgID).OrderByDescending(x => x.LogID).FirstOrDefault());
                    break;
                case "clientorg":
                    var co = DataSession.Single<ClientOrg>(Record);
                    list.Add(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Client" && x.Record == co.Client.ClientID).OrderByDescending(x => x.LogID).FirstOrDefault());
                    list.Add(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Org" && x.Record == co.Org.OrgID).OrderByDescending(x => x.LogID).FirstOrDefault());
                    break;
                case "clientmanager":
                    var cm = DataSession.Single<ClientManager>(Record);
                    list.Add(CopyActiveLog(DataSession.Query<ActiveLog>().Where(x => x.TableName == "ClientOrg" && x.Record == cm.ClientOrg.ClientOrgID).OrderByDescending(x => x.LogID).FirstOrDefault(), "{0} (User)"));
                    list.Add(CopyActiveLog(DataSession.Query<ActiveLog>().Where(x => x.TableName == "ClientOrg" && x.Record == cm.ManagerOrg.ClientOrgID).OrderByDescending(x => x.LogID).FirstOrDefault(), "{0} (Manager)"));
                    list.Add(CopyActiveLog(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Client" && x.Record == cm.ClientOrg.Client.ClientID).OrderByDescending(x => x.LogID).FirstOrDefault(), "{0} (User)"));
                    list.Add(CopyActiveLog(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Client" && x.Record == cm.ManagerOrg.Client.ClientID).OrderByDescending(x => x.LogID).FirstOrDefault(), "{0} (Manager)"));
                    list.Add(CopyActiveLog(DataSession.Query<ActiveLog>().Where(x => x.TableName == "Org" && x.Record == cm.ClientOrg.Org.OrgID).OrderByDescending(x => x.LogID).FirstOrDefault(), "{0} (Both)"));
                    break;
            }

            return list.ToArray();
        }

        private ActiveLog CopyActiveLog(ActiveLog alog, string format)
        {
            return new ActiveLog()
            {
                LogID = alog.LogID,
                TableName = string.Format(format, alog.TableName),
                Record = alog.Record,
                EnableDate = alog.EnableDate,
                DisableDate = alog.DisableDate
            };
        }
    }

    public class UserInfoItem
    {
        public string Label { get; set; }
        public string Text { get; set; }
    }
}