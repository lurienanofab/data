using Data.Controllers.Api;
using LNF;
using LNF.Impl.Repository.Data;
using System.Linq;
using System.Web.Http;

namespace Data.Controllers
{
    [Route("api/org/department")]
    public class OrgDepartmentController : DataApiController
    {
        public OrgDepartmentController(IProvider provider) : base(provider) { }

        public OrgDepartmentModel[] Get(int orgId)
        {
            var query = DataSession.Query<Department>().Where(x => x.Org.OrgID == orgId);
            return query.OrderBy(x => x.Org.OrgID).ThenBy(x => x.DepartmentName).Select(GetModel).ToArray();
        }

        public OrgDepartmentModel Post([FromBody] OrgDepartmentModel model, int orgId)
        {
            Department entity;

            if (model.DepartmentID > 0)
            {
                //update existing
                entity = DataSession.Single<Department>(model.DepartmentID);
                entity.DepartmentName = model.DepartmentName;
            }
            else
            {
                //add new
                entity = new Department()
                {
                    DepartmentName = model.DepartmentName,
                    Org = DataSession.Single<Org>(orgId)
                };
            }

            DataSession.SaveOrUpdate(entity);
            return GetModel(entity);
        }

        public void Delete(int departmentId)
        {
            var entity = DataSession.Single<Department>(departmentId);
            DataSession.Delete(new Department[] { entity });
        }

        private OrgDepartmentModel GetModel(Department entity)
        {
            if (entity == null)
                return null;

            return new OrgDepartmentModel()
            {
                DepartmentID = entity.DepartmentID,
                DepartmentName = entity.DepartmentName,
                OrgID = entity.Org.OrgID
            };
        }
    }

    public class OrgDepartmentModel
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int OrgID { get; set; }
    }
}
