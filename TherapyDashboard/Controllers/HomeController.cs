using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TherapyDashboard.ViewModels;
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

            //TODO remove these when fully integrated with FHIR
            /*
            MongoDBConnection repo = new MongoDBConnection();
            //var bill = repo.Patients.AsQueryable().First();

            MultiPatientViewModel model = new MultiPatientViewModel();

            List<Patient> patients = MongoRepository.getAllPatients();
            model.patients = patients;
            */

            //fetch all patients from FHIR
            FHIRRepository repo = new FHIRRepository();

            //Get list of QRs for each patient
            List<List<QuestionnaireResponse>> QRs = repo.getAllQRs(); //indexed by patient ID order


            //TODO calculate QR deltas for each patient
            PatientAnalytics calc = new PatientAnalytics();
            List<string> summaries = new List<string>();

            foreach (var QR in QRs)
            {
                string summary = calc.calculateSummary(QR);
                summaries.Add(summary);
            }
            

            //TODO persist calculations in DB

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

        [Route("fhir-app")]
        public ActionResult FHIRView()
        {

            return View();//RedirectToAction("Index");
        }

        [Route("fhir-app/launch")]
        public ActionResult FHIRLaunch()
        {

            return View();
        }
    }
}