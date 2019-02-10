using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;
using TherapyDashboard.Services;

namespace TherapyDashboard.DataBase
{
    public class DBCache
    {

        IMongoCollection<PatientData> collection;
        FHIRRepository repo;

        public DBCache()
        {

            BsonClassMap.RegisterClassMap<Hl7.Fhir.Model.Integer>();
            ConventionRegistry.Register(
                "Ignore null values",
                new ConventionPack
                {
                    new IgnoreIfDefaultConvention(true)
                },
                t => true);
            
            MongoClient client = new MongoClient();
            var db = client.GetDatabase("Dashboard");
            collection = db.GetCollection<PatientData>("PatientData");

            repo = new FHIRRepository();
        }

        public Dictionary<long, List<QuestionnaireResponse>> getAllPatientResources(List<long> patientIDs)
        {
            Dictionary<long, List<QuestionnaireResponse>> mds = new Dictionary<long, List<QuestionnaireResponse>>();
            foreach (var id in patientIDs)
            {
                var patData = getPatientDataById(id);
                if (patData != null)
                {
                    var QRs = patData.QRs;
                    mds[id] = QRs;
                }
            }
            return mds;
        }

        public PatientData getPatientDataById(long fhirID)
        {
            /*
            BsonClassMap.RegisterClassMap<QuestionnaireResponse>();
            BsonClassMap.RegisterClassMap<QuestionnaireResponse.AnswerComponent>();
            BsonClassMap.RegisterClassMap<QuestionnaireResponse.ItemComponent>();
            */
            //BsonClassMap.RegisterClassMap<PatientData>();
            

            var filter = Builders<PatientData>.Filter.Eq(x => x.fhirID, fhirID);
            var md = collection.Find(filter).FirstOrDefault();

            return md;
        }

        private void insertSinglePatientData(long id, List<QuestionnaireResponse> QRs)
        {
            PatientData data = new PatientData(id, QRs);
            var QR = QRs.FirstOrDefault();
            //QR.Meta
            //BsonClassMap.RegisterClassMap<PatientData>();
            

            string json = QRs.FirstOrDefault().ToJson();


            collection.InsertOne(data);
            //PatientData after = collection.Find<PatientData>(x => true).FirstOrDefault();
        }

        public void insertNewQRs(Dictionary<long, List<QuestionnaireResponse>> newQRs)
        {
            foreach (var kvp in newQRs)
            {
                if (kvp.Value.Any()) //QRlist not empty
                {
                    insertSinglePatientData(kvp.Key, kvp.Value);
                }
                
            }
        }
    }
}