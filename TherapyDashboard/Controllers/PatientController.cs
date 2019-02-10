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

        [Route("Patient/{id}")]
        public ActionResult DetailView(int id)
        {
            //TODO use cached resources her aswell
            return View();
        }

    }
}