using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TherapyDashboard.Models;
using TherapyDashboard.ViewModels;
using Microsoft.VisualBasic.FileIO;
using TherapyDashboard.DataBase;
using MongoDB.Driver;

namespace TherapyDashboard.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {

            

            //init models
            MongoDBConnection repo = new MongoDBConnection();
            //var bill = repo.Patients.AsQueryable().First();

            MultiPatientViewModel model = new MultiPatientViewModel();

            List<Patient> patients = MongoRepository.getAllPatients();
            model.patients = patients;
            

            return View(model);
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
    }
}