using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TherapyDashboard.Models;
using TherapyDashboard.ViewModels;

namespace TherapyDashboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            MultiPatientViewModel model = new MultiPatientViewModel();
            List<Patient> patients = new List<Patient>();
            for (int i = 0; i < 20; i++)
            {
                patients.Add(Patient.createSimulated());
            }
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
    }
}