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
            List<QuestionnaireResponse> QRs = repo.getAllQRsByPatId(id);
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

            //observations
            List<Observation> observations = repo.getAllObservationsByPatId(id);
            if (observations != null)
            {
                List<string> observationList = new List<string>();
                foreach (var obs in observations)
                {
                    string json = serializer.SerializeToString(obs);
                    observationList.Add(json);
                }
                model.observations = observationList;
            }

            //questionnaireMap - <name, id>
            Dictionary<string, string> qMap = repo.getQMap(id);
            model.questionnaireMap = qMap;

            //update LastUpdated
            var LCHandler = new LastCheckedHandler();
            LCHandler.updatePatientChecked(0, id); //0 is therapistID, replace with ID when authentication is impl


            return View(model);
        }

    }
}