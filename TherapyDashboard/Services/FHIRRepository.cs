using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using TherapyDashboard.DataBase;
using TherapyDashboard.Models;
using TherapyDashboard.Services.AggregationFunctions;
using TherapyDashboard.Services.FlagFunctions;
using TherapyDashboard.Services.WarningFunctions;
using TherapyDashboard.ViewModels;

namespace TherapyDashboard.Services
{
    public class FHIRRepository //TODO refactor duplicate code
    {
        FhirClient client;
        DBCache cache;
        PatientAnalytics calc;
        Dictionary<long, List<QuestionnaireResponse>> patientData; //TODO consider other ways to handle this
        FHIRObservationHandler obsHandler;

        FHIRQRHandler QRHandler;

        public FHIRRepository()
        {
            client = new FhirClient("http://localhost:8080/hapi/baseDstu3");
            cache = new DBCache();
            calc = new PatientAnalytics();
            obsHandler = new FHIRObservationHandler(client, cache, calc);
            QRHandler = new FHIRQRHandler(client, cache);
        }

        public List<Observation> getCachedObservationsForPatient(long patId)
        {
            return obsHandler.getCachedObservationsForPatient(patId);
        }

        public DetailViewModel getDetailViewModel(long id)
        {
            return cache.loadDetailViewModel(id);
        }

        public MasterViewModel loadCache()
        {
            return cache.loadViewModel();
        }

        public void updateCachedDetailViewByPatientId(long id)
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

            model.patientID = id;
            cache.cacheDetailViewModel(model);
        }

        /// <summary>
        /// Reads the lastCheckedMap for the given therapist from DB and returns it. 
        /// Requires an existing global model to exist before calling this, 
        /// and will do nothing if this is not the case (should only happen if database is completely fresh).
        /// </summary>
        public void updateTherapistState(long therapistID)
        {
            var model = loadCache();
            if (model != null)
            {
                var LCHandler = new LastCheckedHandler();
                var lastCheckedMap = LCHandler.readPatientMap(therapistID); //0 is therapistID, replace with ID when authentication is impl
                model.lastCheckedMap = lastCheckedMap.patientMap;
                cache.cacheMasterViewModel(model);
            }
            
        }

        /// <summary>
        /// Refreshes Resources from FHIR server and preforms all necessary calculations for master view. 
        /// Result is stored as MasterViewModel in db. 
        /// Operates independent of logged in therapist.
        /// Should preferably be called by a task scheduler on the server as this is a long running process.
        /// </summary>
        public void updateGlobalState()
        {
            List<Patient> patients = getAllPatients();

            updateResources(patients); 

            //declare calculation functions
            IAggregationFunction aggFunc = new SumDeltaThresholdSingleQRFunc(1, "42220");
            IFlagFunction flagFunc = new MaxDeltaFlagFunc();
            IWarningFunction warningFunc = new DeltaThresholdWarningFunc(1);

            Dictionary<long, string> summaries = getSummaries(aggFunc);
            Dictionary<long, List<string>> flags = getFlags(flagFunc); //todo handle multiple flags
            Dictionary<long, List<string>> warnings = getWarnings(warningFunc);

            //convert dictionaries to strings, and add to model, TODO extract this to method
            MasterViewModel model = new MasterViewModel();
            model.summaries = new Dictionary<string, string>();
            foreach (var kvp in summaries)
            {
                model.summaries[kvp.Key.ToString()] = kvp.Value;
            }

            model.flags = new Dictionary<string, List<string>>();
            foreach (var kvp in flags)
            {
                model.flags[kvp.Key.ToString()] = kvp.Value;
            }

            model.warnings = new Dictionary<string, List<string>>();
            if (warnings != null)
            {
                foreach (var kvp in warnings)
                {
                    model.warnings[kvp.Key.ToString()] = kvp.Value;
                }
            }


            model.patientNames = new Dictionary<string, string>();
            foreach (var pat in patients)
            {
                model.patientNames[pat.Id] = pat.Name[0].Given.FirstOrDefault() + " " + pat.Name[0].Family;
            }

            cache.cacheMasterViewModel(model);

            foreach (var pat in patients)
            {
                long patID = Int32.Parse(pat.Id);
                updateCachedDetailViewByPatientId(patID);
            }
        }

        public List<Observation> getAllObservationsByPatId(long id)
        {
            return obsHandler.getAllObservationsByPatId(id);
        }

        public List<QuestionnaireResponse> getAllQRsByPatId(long id)
        {
            return QRHandler.getCachedQRsForPatient(id);
        }
        

        public List<Patient> getAllPatients()
        {
            List<Patient> patients = new List<Patient>();

            Bundle results = client.Search<Patient>(); //todo error handling 
            while (results != null)
            {
                foreach (var entry in results.Entry)
                {
                    Patient pat = (Patient)entry.Resource;
                    patients.Add(pat);
                }

                results = client.Continue(results);
            }

            return patients;
        }

        /// <summary>
        /// Used to map questionnaire IDs to their corresponding names in DetailView. 
        /// As the FHIR QRs does not contain the name of the questionnaire, 
        /// this is done to avoid having to return the Questionnaire resources to the view.
        /// </summary>
        /// <param name="patID">ID of the patient with the requested QRs</param>
        /// <returns>Dict with (name: id)</returns>
        public Dictionary<string, string> getQMap(long patID)
        {
            //get QRs
            var QRs = QRHandler.getQRByPatientId(patID); //TODO see if we can not call this twice

            //get unique QIDs
            List<string> uniqueQIDs = new List<string>();
            foreach (var QR in QRs)
            {
                string qid = QR.Questionnaire.Reference;
                if (!uniqueQIDs.Contains(qid))
                {
                    uniqueQIDs.Add(qid);
                }
            }

            //get Qs for each ID
            List<Questionnaire> questionnaires = new List<Questionnaire>();
            foreach(var qid in uniqueQIDs)
            {
                Bundle results = client.SearchById<Questionnaire>(qid);

                foreach (var entry in results.Entry) //should be one
                {
                    Questionnaire questionnaire  = (Questionnaire)entry.Resource;
                    questionnaires.Add(questionnaire);
                }
                
            }

            //assemble map
            Dictionary<string, string> qMap = new Dictionary<string, string>();
            foreach (var questionnaire in questionnaires)
            {
                string qid = questionnaire.Id;
                qMap[qid] = questionnaire.Title;
            }

            return qMap;
        }

        public Patient getPatientById(long id)
        {
            Bundle results = client.SearchById<Patient>(id.ToString());
            if (results.Entry.Any())
            {
                Patient pat = (Patient)results.Entry[0].Resource;
                return pat;
            }
            return null;
        }

        //TODO update observation resources
        public void updateResources(List<Patient> patients)
        {
            //get all patient IDs
            List<long> oldIds = new List<long>();
            foreach (var pat in patients)
            {
                oldIds.Add(Int32.Parse(pat.Id));
            }

            //get cached data
            patientData = cache.getAllPatientResources(oldIds); //could be empty -- global for this object

            List<long> newIds = new List<long>();
            foreach (var id in patientData.Keys)
            {
                newIds.Add(id);
            }

            Dictionary<long, List<QuestionnaireResponse>> newQRs = updateAllQRs(patients, newIds);

            //insert new elements in cache
            cache.insertNewQRs(newQRs);
        }

        /// <summary>
        /// Updates the QuestionnaireResponses for all patients by using the previously cached QRs, and 
        /// querying the resource server for QRs after the latest date per patient.
        /// </summary>
        /// <param name="patients">List of FHIR Patient objects</param>
        /// <param name="newIds">List of IDs of patients with no QRs cached</param>
        /// <returns>Dict mapping patientID to their list of QuestionnaireResponses</returns>
        private Dictionary<long, List<QuestionnaireResponse>> updateAllQRs(List<Patient> patients, List<long> newIds)
        {
            //get all QRs for new patients
            List<Patient> newPats = new List<Patient>();
            foreach (var pat in patients)
            {
                if (!newIds.Contains(Int32.Parse(pat.Id)))
                {
                    newPats.Add(pat);
                }
            }

            Dictionary<long, List<QuestionnaireResponse>> allQRs = new Dictionary<long, List<QuestionnaireResponse>>();
            foreach (var pat in newPats)
            {
                long patID = Int32.Parse(pat.Id);
                var newQRList = QRHandler.getQRByPatientId(patID);
                allQRs[patID] = newQRList;
                patientData[patID] = newQRList;
            }

            //get new QRs for old patients
            Dictionary<long, List<QuestionnaireResponse>> newQRsOldPatients = new Dictionary<long, List<QuestionnaireResponse>>();
            foreach (var kvp in patientData)
            {
                long patId = kvp.Key;
                List<QuestionnaireResponse> newQRList = QRHandler.getNewestQRs(patId, patientData);
                if (newQRList != null && newQRList.Any())
                {
                    allQRs[patId] = newQRList;
                    newQRsOldPatients[patId] = newQRList;
                }
            }

            //wrangle to format
            foreach (var kvp in newQRsOldPatients)
            {
                patientData[kvp.Key] = kvp.Value;
            }

            return allQRs;
        }

        private Dictionary<long, List<string>> getWarnings(IWarningFunction warningFunc)
        {
            Dictionary<long, List<string>> warnings = new Dictionary<long, List<string>>();
            foreach (var kvp in patientData)
            {
                List<string> warning = calc.calculateWarnings(kvp, warningFunc);
                if (warning.Any())
                {
                    warnings[kvp.Key] = warning;
                }
            }
            return warnings;
        }

        private Dictionary<long, List<string>> getFlags(IFlagFunction flagFunc)
        {
            Dictionary<long, List<string>> flags = new Dictionary<long, List<string>>();
            foreach (var kvp in patientData)
            {
                flags[kvp.Key] = calc.calculateFlags(kvp, flagFunc);
            }
            return flags;
        }

        private Dictionary<long, string> getSummaries(IAggregationFunction aggFunc)
        {
            calc = new PatientAnalytics();
            Dictionary<long, string> summaries = new Dictionary<long, string>();
            foreach (var kvp in patientData)
            {
                summaries[kvp.Key] = calc.calculateSummary(kvp, aggFunc);
            }
            return summaries;
        }

        

    }
}