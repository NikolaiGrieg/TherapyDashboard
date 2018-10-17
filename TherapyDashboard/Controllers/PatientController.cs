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

        /*
        [Route("Patient/Py")]
        public ActionResult Py()
        {
            string url = "http://127.0.0.1:5000/";
            var client = new RestClient(url);

            var request = new RestRequest("MADRS", Method.GET);
            IRestResponse response = client.Execute(request);
            var content = response.Content;


            return Content(content);
        }
        */

        /*
        public ActionResult Single(int id)
        {
            return Content("patient " + id);
        }
        */
    }
}