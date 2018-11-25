using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static void addFormToPatient(string patName)
        {
            //PatientRepository.createPatient();
            MongoDBConnection db = new MongoDBConnection();
            var filter = Builders<Patient>.Filter.Eq(x => x.name, patName);
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
                    db.Patients.ReplaceOne(
                        item => item.id == pat.id,
                        pat
                        );
                }

            }

            //to find documents with a given date: db.Form.find({"somedate" : { $exists : true} }).pretty()
        }

        public static List<BsonDocument> getPatientForms(string patName)
        {
            //see https://www.mongodb.com/blog/post/6-rules-of-thumb-for-mongodb-schema-design-part-1 for queries/joins

            MongoDBConnection db = new MongoDBConnection();
            var filter = Builders<Patient>.Filter.Eq(x => x.name, patName);
            var pat = db.Patients.Find(filter).FirstOrDefault();

            var formFilter = Builders<BsonDocument>.Filter.In<ObjectId>("_id", pat.NumericForms);
            var forms = db.Forms.Find(formFilter).ToList();
            return forms;
        }

        public static string getPatientFormsSingle(string patName)
        {
            MongoDBConnection db = new MongoDBConnection();
            var filter = Builders<Patient>.Filter.Eq(x => x.name, patName);
            var pat = db.Patients.Find(filter).FirstOrDefault();

            var formFilter = Builders<BsonDocument>.Filter.In<ObjectId>("_id", pat.NumericForms);
            var forms = db.Forms.Find(formFilter).ToList();
            var mainDocument = forms[0];
            for (int i = 1; i < forms.Count; i++)
            {
                mainDocument.Merge(forms[i]);
            }

            var jsonWriterSettings = new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict };
            string json = mainDocument.ToJson(jsonWriterSettings);


            return json;
        }
    }
}