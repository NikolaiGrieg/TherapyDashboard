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
        public static void createPatient(string name)
        {
            MongoDBConnection db = new MongoDBConnection();

            db.Patients.InsertOne(new Patient()
            {
                name = name

            });
        }

        public static void addFormToPatient(string patientName)
        {
            //PatientRepository.createPatient();
            MongoDBConnection db = new MongoDBConnection();
            var filter = Builders<Patient>.Filter.Eq(x => x.name, patientName);
            var pat = db.Patients.Find(filter).FirstOrDefault(); //TODO handle multiple patients w same name


            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/Data/sample_json_1m_1d.json"))
            {
                string json = @r.ReadToEnd();

                var documents = BsonSerializer.Deserialize<BsonDocument>(json);
                Dictionary<string, object> values = documents.ToDictionary();

                foreach (KeyValuePair<string, object> kvp in values)
                {
                    var entryDict = new Dictionary<string, object> {
                        { kvp.Key, kvp.Value }
                    };
                    var jsonDoc = JsonConvert.SerializeObject(entryDict);
                    var bsonDoc = BsonSerializer.Deserialize<BsonDocument>(jsonDoc);

                    //TODO make atomic maybe
                    db.Forms.InsertOne(bsonDoc);

                    var id = bsonDoc.GetElement("_id");
                    var objectID = (ObjectId)id.Value;

                    if (pat.NumericForms == null)
                    {
                        pat.NumericForms = new List<ObjectId>() {
                            objectID
                        };
                    }
                    else
                    {
                        pat.NumericForms.Add(objectID);
                    }

                    //update patient
                    //Builders<BsonDocument>.Update.AddToSet("numericForms", pat.NumericForms);

                    db.Patients.ReplaceOne(
                        item => item.id == pat.id,
                        pat
                        );

                    //see https://www.mongodb.com/blog/post/6-rules-of-thumb-for-mongodb-schema-design-part-1 for queries/joins
                }

            }

            //to find documents with a given date: db.Form.find({"somedate" : { $exists : true} }).pretty()
        }

        public BsonDocument getPatientForms(string patName)
        {
            return null;
        }
    }
}