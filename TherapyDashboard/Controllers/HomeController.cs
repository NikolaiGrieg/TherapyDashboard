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

namespace TherapyDashboard.Controllers
{
    public class HomeController : Controller
    {
        //[Authorize]
        public ActionResult Index()
        {
            FHIRRepository repo = new FHIRRepository();
            
            Dictionary<int, string> summaries = repo.getSummaries();
            

            return View();
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

        /*
        [Route("fhir-app")]
        public ActionResult FHIRView()
        {

            return View();
        }

        [Route("fhir-app/launch")]
        public ActionResult FHIRLaunch()
        {

            return View();
        }
        */
    }
}