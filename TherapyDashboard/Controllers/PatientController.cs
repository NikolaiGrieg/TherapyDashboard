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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.Serialization;
using TherapyDashboard.ViewModels;

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
            DetailViewModel model = new DetailViewModel();
            Patient pat = Patient.createSimulated();
            model.pat = pat;
            pat.Measurement1Path = "../../Data/Sample_MADRS.csv";

            //MongoRepository.createPatient("Bill");
            //MongoRepository.createPatient("John");
            //MongoRepository.addFormToPatient("Bill");
            //MongoRepository.addFormToPatient("John");


            var json = MongoRepository.getPatientFormsSingle("Bill");
            model.json = json;
            return View(model);
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