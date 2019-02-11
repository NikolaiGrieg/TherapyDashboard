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
            //TODO use cached resources her aswell
            FHIRRepository repo = new FHIRRepository();
            FhirJsonSerializer serializer = new FhirJsonSerializer();

            DetailViewModel model = new DetailViewModel();

            //add patient details
            Patient patient = repo.getPatientById(id);
            if (patient != null)
            {
                string patJson = serializer.SerializeToString(patient);
                model.patient = patJson;
            }
            
            //add QRs
            List<QuestionnaireResponse> QRs = repo.getCachedQRsForPatient(id);
            if (QRs != null)
            {
                List<string> QRJsonList = new List<string>();
                foreach (var QR in QRs)
                {
                    string json = serializer.SerializeToString(QR);
                    QRJsonList.Add(json);
                }
                model.QRs = QRJsonList;  
            }



            return View(model);
        }

    }
}