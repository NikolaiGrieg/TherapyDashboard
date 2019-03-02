using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
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
        Dictionary<long, List<QuestionnaireResponse>> patientData;
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

        public MasterViewModel loadCache()
        {
            return cache.loadViewModel();
        }

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

            //lastchecked
            var LCHandler = new LastCheckedHandler();
            var lastCheckedMap = LCHandler.readPatientMap(0); //0 is therapistID, replace with ID when authentication is impl
            model.lastCheckedMap = lastCheckedMap.patientMap;

            cache.cacheModel(model);
        }

        public List<Observation> getAllObservationsByPatId(long id)
        {
            return obsHandler.getAllObservationsByPatId(id);
        }

        public List<QuestionnaireResponse> getAllQRsByPatId(long id)
        {
            return QRHandler.getCachedQRsForPatient(id);
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <returns>List of FHIR Patient objects</returns>
        public List<Patient> getAllPatients()
        {
            //TODO limit max
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

            //asseble map
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