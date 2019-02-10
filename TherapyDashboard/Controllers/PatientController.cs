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


namespace TherapyDashboard.Controllers
{
    //[Authorize]
    public class PatientController : Controller
    {
        // GET: Patient
        /*
        [Route("Patient/{id}")]
        public ActionResult SinglePatientView(int id)
        {
            //return Content("patient " + id);
            
            return View();
        }
        */

        [Route("Patient/v2")]
        public ActionResult DetailView()
        {
            
            return View();
        }

    }
}