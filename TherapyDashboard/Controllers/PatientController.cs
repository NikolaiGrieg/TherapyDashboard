using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TherapyDashboard.Models;

namespace TherapyDashboard.Controllers
{
    public class PatientController : Controller
    {
        // GET: Patient
        [Route("Patient/{id}")]
        public ActionResult SinglePatientView(int id)
        {
            //return Content("patient " + id);
            Patient pat = new Patient()
            {
                name = "Test Patient",
                id = id
            };
            return View(pat);
        }

        /*
        public ActionResult Single(int id)
        {
            return Content("patient " + id);
        }
        */
    }
}