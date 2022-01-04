using Data.Controllers.Api;
using LNF;
using LNF.Impl.Repository.Data;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    public class ApiDepartmentController : DataApiController
    {
        public ApiDepartmentController(IProvider provider) : base(provider) { }

        public DepartmentModel[] Get(int orgId = 0)
        {
            IQueryable<Department> query = null;

            if (orgId == 0)
                query = DataSession.Query<Department>();
            else
                query = DataSession.Query<Department>().Where(x => x.Org.OrgID == orgId);

            return query.OrderBy(x => x.Org.OrgID).ThenBy(x => x.DepartmentName).Select(GetModel).ToArray();
        }

        public DepartmentModel[] Post([FromBody] DepartmentModel model)
        {
            if (model.DepartmentID > 0)
            {
                //update existing
                var entity = DataSession.Single<Department>(model.DepartmentID);
                entity.DepartmentName = model.DepartmentName;
                DataSession.SaveOrUpdate(entity);
                return new DepartmentModel[] { GetModel(entity) };
            }
            else
            {
                //add new
                var entity = new Department()
                {
                    DepartmentName = model.DepartmentName,
                    Org = DataSession.Single<Org>(model.OrgID)
                };
                DataSession.SaveOrUpdate(entity);
                return Get(model.OrgID);
            }
        }

        public DepartmentModel[] Delete(int id)
        {
            var entity = DataSession.Single<Department>(id);
            var orgId = entity.Org.OrgID;
            DataSession.Delete(new Department[] { entity });
            return Get(orgId);
        }

        private DepartmentModel GetModel(Department entity)
        {
            return new DepartmentModel()
            {
                DepartmentID = entity.DepartmentID,
                DepartmentName = entity.DepartmentName,
                OrgID = entity.Org.OrgID
            };
        }
    }

    public class DepartmentModel
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int OrgID { get; set; }
    }
}
