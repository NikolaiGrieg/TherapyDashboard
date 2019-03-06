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
using System.Diagnostics;

namespace TherapyDashboard.Controllers
{
    public class HomeController : Controller
    {
        //[Authorize]
        public ActionResult Index()
        {
            FHIRRepository repo = new FHIRRepository();
            repo.updateTherapistState(0); //using 0 as default therapistID

            var model = repo.loadCache(); //TODO also do this for detailView
            if (model == null)
            {
                repo.updateGlobalState(); //expensive operation
                repo.updateTherapistState(0); //workaround for this function requiring the global state to update first if model is null

                model = repo.loadCache();
            }
            return View(model);
        }

        public ActionResult updateCache()
        {
            FHIRRepository repo = new FHIRRepository();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            repo.updateGlobalState();

            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

            return Content("Successfully updated global state in: " + elapsedTime);
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