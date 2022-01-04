using LNF;
using LNF.Data;
using LNF.Repository;
using LNF.Web.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Data.Controllers
{
    public class SettingsController : DataController
    {
        public SettingsController(IProvider provider) : base(provider) { }

        [HttpGet, Route("settings"), LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, Route("settings/ajax"), LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Ajax()
        {
            var items = DataSession.Query<LNF.Impl.Repository.Data.GlobalSettings>();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Route("settings/ajax"), LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Ajax(LNF.Impl.Repository.Data.GlobalSettings model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.SettingName))
                    throw new Exception("Setting name is required.");

                if (model.SettingID == 0)
                {
                    // create new
                    var existing = DataSession.Query<LNF.Impl.Repository.Data.GlobalSettings>().FirstOrDefault(x => x.SettingName == model.SettingName);

                    if (existing != null)
                        throw new Exception($"A setting with name '{model.SettingName}' already exists.");

                    DataSession.Insert(new LNF.Impl.Repository.Data.GlobalSettings
                    {
                        SettingName = model.SettingName,
                        SettingValue = model.SettingValue
                    });
                }
                else
                {
                    var gs = DataSession.Single<LNF.Impl.Repository.Data.GlobalSettings>(model.SettingID);

                    if (gs == null)
                        throw new Exception($"Cannot find global setting with SettingID = {model.SettingID}");

                    var existing = DataSession.Query<LNF.Impl.Repository.Data.GlobalSettings>().FirstOrDefault(x => x.SettingName == model.SettingName && x.SettingID != model.SettingID);

                    if (existing != null)
                        throw new Exception($"A setting with name '{model.SettingName}' already exists.");

                    gs.SettingName = model.SettingName;
                    gs.SettingValue = model.SettingValue;

                    DataSession.SaveOrUpdate(gs);
                }

                var items = DataSession.Query<LNF.Impl.Repository.Data.GlobalSettings>();

                return Json(items);
            }
            catch (Exception ex)
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 500;
                return Json(new
                {
                    Message = "An error has occurred.",
                    ExceptionMessage = ex.Message,
                    ExceptionType = ex.GetType().ToString(),
                    ex.StackTrace
                });
            }
        }

        [HttpDelete, Route("settings/ajax"), LNFAuthorize(ClientPrivilege.Administrator)]
        public ActionResult Ajax(int id)
        {
            var gs = DataSession.Single<LNF.Impl.Repository.Data.GlobalSettings>(id);

            if (gs != null)
                DataSession.Delete(gs);

            var items = DataSession.Query<LNF.Impl.Repository.Data.GlobalSettings>();

            return Json(items, JsonRequestBehavior.AllowGet);
        }
    }
}
