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
            List<QuestionnaireResponse> QRs = repo.getCachedQRsForPatient(id);
            FhirJsonSerializer serializer = new FhirJsonSerializer();
            if (QRs != null)
            {
                List<string> jsonList = new List<string>();
                foreach (var QR in QRs)
                {
                    string json = serializer.SerializeToString(QR);
                    jsonList.Add(json);
                }

                return View(jsonList);
            }
            else
            {
                return new HttpNotFoundResult("No with ID " + id +" found");
            }
        }

    }
}