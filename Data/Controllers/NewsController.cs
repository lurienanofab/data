using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Models;

namespace Data.Controllers
{
    public class NewsController : Controller
    {
        [Route("news")]
        public ActionResult Index(NewsModel model)
        {
            return View(model);
        }

        [Route("news/list")]
        public ActionResult List(NewsModel model)
        {
            return View(model);
        }

        [Route("news/edit/{NewsID}")]
        public ActionResult Edit(NewsModel model)
        {
            return View(model);
        }
    }
}
