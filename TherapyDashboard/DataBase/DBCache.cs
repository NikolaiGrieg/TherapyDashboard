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
using TherapyDashboard.Services.AggregationFunctions;
using TherapyDashboard.Services.FlagFunctions;
using TherapyDashboard.ViewModels;

namespace TherapyDashboard.DataBase
{
    public class DBCache
    {

        IMongoCollection<PatientData> collection;
        IMongoCollection<MasterViewModel> viewModels; //TODO map to therapist

        //TODO create functionality for persisting viewmodel, and load it on index call
        //with replacement of the viewmodel on update resources

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
            viewModels = db.GetCollection<MasterViewModel>("ViewModels");
        }

        public MasterViewModel loadViewModel()
        {
            return viewModels.Find(x => true).FirstOrDefault();//TODO therapistID here
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

        /*
        public Dictionary<long, List<QuestionnaireResponse>> loadCache()
        {
            registerCMs();

            Dictionary<long, List<QuestionnaireResponse>> mds = new Dictionary<long, List<QuestionnaireResponse>>();

            List<PatientData> pds = collection.Find(x => true).ToList();

            foreach (var pat in pds)
            {
                mds[pat.fhirID] = pat.QRs;
            }
            return mds;

        }
        */

        public PatientData getPatientDataById(long fhirID)
        {
            registerCMs();
            var filter = Builders<PatientData>.Filter.Eq(x => x.fhirID, fhirID);
            var md = collection.Find(filter).FirstOrDefault();

            return md;
        }

        private void insertSinglePatientQRs(long id, List<QuestionnaireResponse> QRs)
        {
            registerCMs();

            //TODO check if id exists before inserting
            PatientData data = new PatientData(id, QRs, null);
            collection.InsertOne(data);
        }

        private void insertSinglePatientObservations(long id, List<Observation> observations)
        {
            registerCMs();

            //TODO check if id exists before inserting
            var filter = Builders<PatientData>.Filter.Eq(x => x.fhirID, id);
            PatientData pd;
            //PatientData toInsert = new PatientData(id, null, observations);
            try
            {
                pd = collection.Find(filter).FirstOrDefault();
                if (pd.observations == null)
                {
                    pd.observations = observations;
                }
                else
                {
                    List<Observation> oldObservations = pd.observations;
                    List<Observation> newObservations = new List<Observation>();
                    foreach (var obs in observations)
                    {
                        if (!oldObservations.Any(x => x.Id == obs.Id))
                        {
                            newObservations.Add(obs);
                        }
                    }
                    pd.observations = oldObservations.Concat(newObservations).ToList();
                }

                collection.ReplaceOne(filter, pd);
            }
            catch (Exception e)
            {
                //no patient with that Id
                pd = new PatientData(id, null, observations);
                collection.InsertOne(pd);
            }
        }

        //TODO support more than 1 therapist
        public void cacheModel(MasterViewModel model)
        {
            var prev = viewModels.Find(x => x.id == model.id).FirstOrDefault();
            if (prev != null)
            {
                viewModels.ReplaceOne(x => x.id == model.id, model);
            }
            else
            {
                viewModels.InsertOne(model);
            }
        }

        public void insertNewQRs(Dictionary<long, List<QuestionnaireResponse>> newQRs)
        {
            registerCMs();
            foreach (var kvp in newQRs)
            {
                if (kvp.Value.Any()) //QRlist not empty
                {
                    insertSinglePatientQRs(kvp.Key, kvp.Value);
                }
                
            }
        }
    }
}