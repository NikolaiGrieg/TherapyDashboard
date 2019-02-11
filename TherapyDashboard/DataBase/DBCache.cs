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

        public DBCache()
        {
            ConventionRegistry.Register(
                "Ignore null values",
                new ConventionPack
                {
                    new IgnoreIfDefaultConvention(true)
                },
                filter: t => t.Name.Contains("QuestionnaireResponse"));
            
            MongoClient client = new MongoClient();
            var db = client.GetDatabase("Dashboard");
            collection = db.GetCollection<PatientData>("PatientData");

        }

        private void registerCMs()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Hl7.Fhir.Model.Integer)))
            {
                BsonClassMap.RegisterClassMap<Hl7.Fhir.Model.Integer>();
            }
        }

        public Dictionary<long, List<QuestionnaireResponse>> getAllPatientResources(List<long> patientIDs)
        {
            registerCMs();

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
            registerCMs();
            var filter = Builders<PatientData>.Filter.Eq(x => x.fhirID, fhirID);
            var md = collection.Find(filter).FirstOrDefault();

            return md;
        }

        private void insertSinglePatientData(long id, List<QuestionnaireResponse> QRs)
        {
            registerCMs();
            PatientData data = new PatientData(id, QRs);
            collection.InsertOne(data);
        }

        public void insertNewQRs(Dictionary<long, List<QuestionnaireResponse>> newQRs)
        {
            registerCMs();
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