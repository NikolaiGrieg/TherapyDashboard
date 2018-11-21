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
            Patient pat = Patient.createSimulated();
            pat.Measurement1Path = "../../Data/Sample_MADRS.csv";

            //PatientRepository.createPatient();
            PatientRepository repo = new PatientRepository();
            //var bill = repo.Patients.AsQueryable().First();

            /*
            var forms = new List<NumericForm>();
            forms.Add(new NumericForm
            {
                name = "MADRS",
                colNames = new List<string>()
                {
                    "A", "B", "C"
                },
                row = new List<int>()
                {
                    1, 2, 3
                }

            });
            */
            repo.Patients.InsertOne(new Patient() {
                name = "Bill",
                
            });
            


            //TODO see if data can be gotten out of the db
            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/Data/sample_json_1m_1d.json"))
            {
                string json = @r.ReadToEnd();

                /*
                NumericForm form = new NumericForm();
                JObject jobject = JObject.Parse(json);
                JObject first = (JObject)jobject["2010-12-01"];

                var bill = repo.Patients.AsQueryable().First();
                bill.NumericForms = new List<NumericForm>();
                form.entry = new List<JObject>();
                form.entry.Add(first);

                bill.NumericForms.Add(form);

                /*
                var filter = Builders<Patient>.Filter.Eq(s => s.id, bill.id);
                var result = repo.Patients.ReplaceOne(filter, bill);
                */
                var document = BsonSerializer.Deserialize<BsonDocument>(json);

                repo.Forms.InsertOne(document);
            }

            /*
            repo.Patients.DeleteOne("{_id : " + bill.id + "}");
            repo.Patients.InsertOne(bill);

            /*
            var entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            foreach(var entry in entries)
            {
                form.entry = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Value);
                break;
            }
            */



            return View(pat);
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