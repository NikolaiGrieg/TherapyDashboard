using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TherapyDashboard.DataBase;
using TherapyDashboard.Models;

namespace TherapyDashboard.DataBase
{
    public class MongoRepository
    {
        public static void addFormToPatient(string patientName)
        {
            //PatientRepository.createPatient();
            MongoDBConnection db = new MongoDBConnection();
            var filter = Builders<Patient>.Filter.Eq(x => x.name, patientName);
            var bill = db.Patients.Find(filter).FirstOrDefault(); //TODO handle multiple patients w same name


            //repo.Patients.InsertOne(new Patient() {
            //    name = "Bill",

            //});

            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/Data/sample_json_1m_1d.json"))
            {
                string json = @r.ReadToEnd();

                var documents = BsonSerializer.Deserialize<BsonDocument>(json);
                Dictionary<string, object> values = documents.ToDictionary();

                foreach (KeyValuePair<string, object> kvp in values)
                {
                    var entryDict = new Dictionary<string, object> {
                        { kvp.Key, kvp.Value },
                        { "PatientFK" , bill.id}
                    };
                    var jsonDoc = JsonConvert.SerializeObject(entryDict);
                    var bsonDoc = BsonSerializer.Deserialize<BsonDocument>(jsonDoc);
                    //var document = BsonSerializer.Deserialize<BsonDocument>((BsonDocument)entry.Value);
                    db.Forms.InsertOne(bsonDoc);
                }

            }

            //to find documents with a given date: db.Form.find({"somedate" : { $exists : true} }).pretty()


            //Gives each entry as an enumerable object as the value in dict
            //BsonDocument form = repo.Forms.AsQueryable().First();
            //var entries = BsonSerializer.Deserialize(form, typeof(Dictionary<string, object>));

            //TODO drop current Form collection
            //Read in entries as one form per document
            //Have date as key in form
            //add patients as FK in form
            //should then be possible to query all forms for a patient, or all forms on a date
            
        }
    }
}