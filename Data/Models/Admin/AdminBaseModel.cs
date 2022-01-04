using LNF.Impl.Repository.Data;
using LNF.Repository;
using LNF.Web.Mvc.UI;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Data.Models.Admin
{
    public abstract class AdminBaseModel : DataModel
    {
        public string Command { get; set; }
        protected string Message { get; set; }

        //room properties
        public int RoomID { get; set; }
        public int ParentID { get; set; }
        public string RoomName { get; set; }
        public string DisplayName { get; set; }
        public bool PassbackRoom { get; set; }
        public bool Billable { get; set; }
        public bool ApportionDailyFee { get; set; }
        public bool ApportionEntryFee { get; set; }

        //org properties
        public int OrgID { get; set; }

        //common property
        public bool Active { get; set; }
        public bool ViewInactive { get; set; }

        public override SubMenu GetSubMenu()
        {
            return base.GetSubMenu()
                .Clear()
                .Add(new SubMenu.MenuItem() { LinkText = "Client", ActionName = "Client", ControllerName = "Admin" })
                .Add(new SubMenu.MenuItem() { LinkText = "Account", ActionName = "Account", ControllerName = "Admin" })
                .Add(new SubMenu.MenuItem() { LinkText = "Org", ActionName = "Org", ControllerName = "Admin" })
                .Add(new SubMenu.MenuItem() { LinkText = "Room", ActionName = "Room", ControllerName = "Admin" });
        }

        public Room[] GetRooms(bool viewInactive)
        {
            if (!viewInactive)
                return DataSession.Query<Room>().Where(x => x.Active).OrderBy(x => x.RoomName).ToArray();
            else
                return DataSession.Query<Room>().OrderBy(x => x.RoomName).ToArray();
        }

        public SelectListItem[] GetRoomParentSelectItems()
        {
            IList<Room> rooms = DataSession.Query<Room>()
                .Where(x => x.Active && x.ParentID == null && x.RoomID != RoomID).OrderBy(x => x.RoomName)
                .ToList();

            IList<SelectListItem> result = rooms.Select(x => new SelectListItem() { Text = x.RoomName, Value = x.RoomID.ToString() }).ToList();

            result.Add(new SelectListItem() { Text = "-- Select --", Value = "0" });

            return result.ToArray();
        }

        public void LoadRoom()
        {
            Message = string.Empty;

            if (RoomID == 0)
                return;

            Room room = DataSession.Single<Room>(RoomID);
            if (room == null)
            {
                Message = string.Format("<div class=\"alert alert-danger\" role=\"alert\">Cannot find RoomID {0}</div>", RoomID);
            }
            else
            {
                ParentID = room.ParentID ?? 0;
                RoomName = room.RoomName;
                DisplayName = room.DisplayName;
                PassbackRoom = room.PassbackRoom;
                Billable = room.Billable;
                ApportionDailyFee = room.ApportionDailyFee;
                ApportionEntryFee = room.ApportionEntryFee;
                Active = room.Active;
            }
        }

        public bool SaveRoom()
        {
            if (string.IsNullOrEmpty(RoomName))
            {
                Message = string.Format("<div class=\"alert alert-danger\" role=\"alert\">Room name must not be blank</div>", RoomID);
                return false;
            }

            Room room;
            if (RoomID == 0)
                room = new Room();
            else
            {
                room = DataSession.Single<Room>(RoomID);
                if (room == null)
                {
                    Message = string.Format("<div class=\"alert alert-danger\" role=\"alert\">Cannot find RoomID {0}</div>", RoomID);
                    return false;
                }
            }

            int? parentId = null;
            if (ParentID > 0)
                parentId = ParentID;

            room.ParentID = parentId;
            room.RoomName = RoomName;
            room.DisplayName = string.IsNullOrEmpty(DisplayName) ? null : DisplayName;
            room.PassbackRoom = PassbackRoom;
            room.Billable = Billable;
            room.ApportionDailyFee = ApportionDailyFee;
            room.ApportionEntryFee = ApportionEntryFee;
            room.Active = Active;

            DataSession.SaveOrUpdate(room);

            return true;
        }

        public Account[] GetAccounts()
        {
            IQueryable<Account> query;

            if (ViewInactive)
                query = DataSession.Query<Account>().Where(x => x.Org.OrgID == OrgID);
            else
                query = DataSession.Query<Account>().Where(x => x.Org.OrgID == OrgID && x.Active);

            return query.OrderBy(x => x.Name).ToArray();
        }

        public Room[] GetRooms()
        {
            if (!ViewInactive)
                return DataSession.Query<Room>().Where(x => x.Active).OrderBy(x => x.RoomName).ToArray();
            else
                return DataSession.Query<Room>().OrderBy(x => x.RoomName).ToArray();
        }

        public Org[] GetOrgs()
        {
            if (!ViewInactive)
                return DataSession.Query<Org>().Where(x => x.Active).OrderBy(x => x.OrgName).ToArray();
            else
                return DataSession.Query<Org>().OrderBy(x => x.OrgName).ToArray();
        }

        public SelectListItem[] GetOrgSelectItems()
        {
            return GetOrgs()
                .Select(x => new SelectListItem() { Text = x.OrgName, Value = x.OrgID.ToString() })
                .ToArray();
        }

        public Org GetPrimaryOrg()
        {
            return DataSession.Query<Org>().FirstOrDefault(x => x.PrimaryOrg);
        }

        public bool IsInternalOrg()
        {
            var org = DataSession.Single<Org>(OrgID);

            if (org == null)
                return false;

            //ChargeTypeID = 5 - 'U of Michigan (US)' is currently the only one
            //The rest are External Academic [ChargeTypeID = 15] and External Non-Academic [ChargeTypeID = 25]

            return org.OrgType.ChargeType.ChargeTypeID == 5;
        }

        public Department[] GetDepartments()
        {
            return DataSession.Query<Department>()
                .Where(x => x.Org.OrgID == OrgID)
                .OrderBy(x => x.DepartmentName)
                .ToArray();
        }

        protected string GetAlert(string message, params object[] args)
        {
            string msg = string.Format(message, args);
            return string.Format("<div role=\"alert\" class=\"alert alert-danger\">{0}</div>", msg);
        }

        public abstract void Load();
        public abstract bool Save();

        public IHtmlString GetMessage()
        {
            if (string.IsNullOrEmpty(Message))
                return null;
            else
                return new HtmlString(Message);
        }
    }
}