using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TherapyDashboard.Models;
using RestSharp;
using TherapyDashboard.DataBase;
using MongoDB.Driver;
using MongoDB.Bson;

namespace TherapyDashboard.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        // GET: Patient
        [Route("Patient/{id}")]
        public ActionResult SinglePatientView(int id)
        {
            //return Content("patient " + id);
            Patient pat = Patient.createSimulated();
            pat.Measurement1Path = "Data/SampleComposite.csv";
            return View(pat);
        }


        [Route("Patient/{id}/v2")]
        public ActionResult DetailView(string id)
        {
            //return Content("patient " + id);
            Patient pat = Patient.createSimulated();
            pat.Measurement1Path = "../../Data/Sample_MADRS.csv";

            //PatientRepository.createPatient();
            PatientRepository repo = new PatientRepository();
            //var bill = repo.Patients.AsQueryable().First();

            /*
            var forms = new List<NumericForm>();
            forms.Add(new NumericForm
            {
                name = "MADRS",
                colNames = new List<string>()
                {
                    "A", "B", "C"
                },
                row = new List<int>()
                {
                    1, 2, 3
                }

            });

            repo.Patients.InsertOne(new Patient() {
                name = "Bill",
                NumericForms = forms
            });
            */



            return View(pat);
        }

        [Route("Spider")]
        public ActionResult DynamicSpiderView()
        {
            Patient pat = Patient.createSimulated();
            //pat.Measurement1Path = "Data/SampleComposite.csv";
            pat.Measurement1Path = "Data/Sample_MADRS.csv";
            return View(pat);
        }

    }
}