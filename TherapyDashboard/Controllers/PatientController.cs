using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TherapyDashboard.Models;
using RestSharp;

namespace TherapyDashboard.Controllers
{
    public class PatientController : Controller
    {
        // GET: Patient
        [Route("Patient/{id}")]
        public ActionResult SinglePatientView(int id)
        {
            //return Content("patient " + id);
            Patient pat = Patient.createSimulated();
            return View(pat);
        }

        [Route("Spider")]
        public ActionResult DynamicSpiderView()
        {
            // TODO parse data
            /*
            var csv = new List<string[]>(); // or, List<YourClass>
            var lines = System.IO.File.ReadAllLines(@"~/Data/data.csv");
            foreach (string line in lines)
                csv.Add(line.Split(',')); // or, populate YourClass          
            string json = new
                System.Web.Script.Serialization.JavaScriptSerializer().Serialize(csv);

            Patient pat = new Patient();
            pat.json = json;
            */
            return View();
        }

    }
}