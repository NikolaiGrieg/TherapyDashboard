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
using TherapyDashboard.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using TherapyDashboard.ViewModels;

namespace TherapyDashboard.Controllers
{
    //[Authorize]
    public class PatientController : Controller
    {

        [Route("Patient/{id}")]
        public ActionResult DetailView(long id)
        {
            FHIRRepository repo = new FHIRRepository();
            var model = repo.getDetailViewModel(id);
            if (model == null || model.patient == null)
            {
                repo.updateCachedPatientDataModelById(id); 
                model = repo.getDetailViewModel(id);
            }

            //update LastUpdated
            var LCHandler = new LastCheckedHandler();
            LCHandler.updatePatientChecked(0, id); //0 is therapistID, replace with ID when authentication is impl


            return View(model);
        }

    }
}