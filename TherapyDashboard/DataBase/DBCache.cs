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
        IMongoCollection<MasterViewModel> masterViewModels; //TODO map to therapist
        IMongoCollection<DetailViewModel> detailViewModels;


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
            masterViewModels = db.GetCollection<MasterViewModel>("ViewModels");
            detailViewModels = db.GetCollection<DetailViewModel>("DetailViewModels");
        }

        public MasterViewModel loadViewModel()
        {
            return masterViewModels.Find(x => true).FirstOrDefault();//TODO therapistID here
        }

        public DetailViewModel loadDetailViewModel(long id) //TODO in theory this should take a therapistID too... maybe revisit this idea
        {
            return detailViewModels.Find(x => x.patientID == id).FirstOrDefault();
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
        public void cacheMasterViewModel(MasterViewModel model)
        {
            var prev = masterViewModels.Find(x => x.id == model.id).FirstOrDefault();
            if (prev != null)
            {
                masterViewModels.ReplaceOne(x => x.id == model.id, model);
            }
            else
            {
                masterViewModels.InsertOne(model);
            }
        }

        public void cacheDetailViewModel(DetailViewModel model)
        {
            var prev = detailViewModels.Find(x => x.patientID == model.patientID).FirstOrDefault();
            if (prev != null)
            {
                model.id = prev.id;
                detailViewModels.ReplaceOne(x => x.patientID == model.patientID, model);
            }
            else
            {
                detailViewModels.InsertOne(model);
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