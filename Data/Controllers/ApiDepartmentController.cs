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
    public class ApiDepartmentController : ApiController
    {
        public DepartmentModel[] Get(int orgId = 0)
        {
            IQueryable<Department> query = null;

            if (orgId == 0)
                query = DA.Current.Query<Department>();
            else
                query = DA.Current.Query<Department>().Where(x => x.Org.OrgID == orgId);

            return query.OrderBy(x => x.Org.OrgID).ThenBy(x => x.DepartmentName).Select(GetModel).ToArray();
        }

        public DepartmentModel[] Post([FromBody] DepartmentModel model)
        {
            if (model.DepartmentID > 0)
            {
                //update existing
                var entity = DA.Current.Single<Department>(model.DepartmentID);
                entity.DepartmentName = model.DepartmentName;
                DA.Current.SaveOrUpdate(entity);
                return new DepartmentModel[] { GetModel(entity) };
            }
            else
            {
                //add new
                var entity = new Department()
                {
                    DepartmentName = model.DepartmentName,
                    Org = DA.Current.Single<Org>(model.OrgID)
                };
                DA.Current.SaveOrUpdate(entity);
                return Get(model.OrgID);
            }
        }

        public DepartmentModel[] Delete(int id)
        {
            var entity = DA.Current.Single<Department>(id);
            var orgId = entity.Org.OrgID;
            DA.Current.Delete(new Department[] { entity });
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
