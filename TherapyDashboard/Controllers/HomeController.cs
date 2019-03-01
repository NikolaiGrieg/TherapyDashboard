using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualBasic.FileIO;
using TherapyDashboard.DataBase;
using MongoDB.Driver;
using TherapyDashboard.Services;
using Hl7.Fhir.Model;
using TherapyDashboard.ViewModels;
using TherapyDashboard.Services.AggregationFunctions;
using TherapyDashboard.Services.FlagFunctions;
using TherapyDashboard.Services.WarningFunctions;

namespace TherapyDashboard.Controllers
{
    public class HomeController : Controller
    {
        //[Authorize]
        public ActionResult Index()
        {
            FHIRRepository repo = new FHIRRepository();
            var model = repo.loadCache(); //TODO also do this for detailView
            return View(model);
        }

        public ActionResult updateCache()
        {
            FHIRRepository repo = new FHIRRepository();
            repo.updateResources();
            return Content("Successfully updated");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}