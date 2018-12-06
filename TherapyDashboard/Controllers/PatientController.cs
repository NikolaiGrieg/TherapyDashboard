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
            return View(pat);
        }


        [Route("Patient/{id}/v2")]
        public ActionResult DetailView(string id)
        {
            //return Content("patient " + id);
            DetailViewModel model = new DetailViewModel();
            
            //Patient pat = MongoRepository.getPatientByName("Magnus Danielsen");
            Patient pat = MongoRepository.getPatientById(new ObjectId(id));
            model.pat = pat;

            //MongoRepository.createPatient("Bill");
            //MongoRepository.createPatient("John");
            //MongoRepository.addFormsToPatient("Bill", "/Data/sample_json_1m_1d.json");
            //MongoRepository.addFormsToPatient("John", "/Data/sample_json_1m_1d.json");

            //all forms in db collection Form are for the same measurement
            //need to somehow tag which ones go where, and distribute them over multiple collections
            //ex "background", "summary/", "single", "unstrucured" TODO revisit these
            //Assuming only 1 "summary" is probably reasonable
            var json = MongoRepository.getPatientFormsSingle(pat.id); //TODO
            var avgPat = MongoRepository.getPatientByName("AvgPatient");
            model.json = json;
            return View(model);
        }

        [Route("Spider")]
        public ActionResult DynamicSpiderView()
        {
            Patient pat = Patient.createSimulated();
            return View(pat);
        }

    }
}