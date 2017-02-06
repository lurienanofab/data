using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LNF.Repository;
using LNF.Repository.Data;

namespace Data.Controllers
{
    public class ApiOrgDepartmentController : ApiController
    {
        public OrgDepartmentModel[] Get(int orgId)
        {
            var query = DA.Current.Query<Department>().Where(x => x.Org.OrgID == orgId);
            return query.OrderBy(x => x.Org.OrgID).ThenBy(x => x.DepartmentName).Select(GetModel).ToArray();
        }

        public OrgDepartmentModel Post([FromBody] OrgDepartmentModel model, int orgId)
        {
            Department entity = null;

            if (model.DepartmentID > 0)
            {
                //update existing
                entity = DA.Current.Single<Department>(model.DepartmentID);
                entity.DepartmentName = model.DepartmentName;
            }
            else
            {
                //add new
                entity = new Department()
                {
                    DepartmentName = model.DepartmentName,
                    Org = DA.Current.Single<LNF.Repository.Data.Org>(orgId)
                };
            }

            DA.Current.SaveOrUpdate(entity);
            return GetModel(entity);
        }

        public void Delete(int departmentId)
        {
            var entity = DA.Current.Single<Department>(departmentId);
            DA.Current.Delete(new Department[] { entity });
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
