using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        MongoRepository cache;
        PatientAnalytics calc;
        Dictionary<long, List<QuestionnaireResponse>> patientData; //TODO consider other ways to handle this
        FHIRObservationHandler obsHandler;
        ChartSelectionRepository chartSelectionRepo;
        Logger log;

        FHIRQRHandler QRHandler;

        public FHIRRepository()
        {
            client = new FhirClient("http://localhost:8080/hapi/baseDstu3");
            cache = new MongoRepository();
            calc = new PatientAnalytics();
            obsHandler = new FHIRObservationHandler(client, cache, calc);
            QRHandler = new FHIRQRHandler(client, cache);
            chartSelectionRepo = new ChartSelectionRepository();
            log = new Logger();
        }

        public List<Observation> getCachedObservationsForPatient(long patId)
        {
            return obsHandler.getCachedObservationsForPatient(patId);
        }

        public DetailViewModel getDetailViewModel(long id)
        {
            FullPatientData dataModel = cache.loadPatientDataModel(id);
            DetailViewModel model = new DetailViewModel();

            if(dataModel != null)
            {
                model.observations = dataModel.observations;
                model.patient = dataModel.patient;
                model.patientID = dataModel.patientID;
                model.QRs = dataModel.QRs;
                model.questionnaireMap = dataModel.questionnaireMap;
            }
            

            //load selected charts
            ChartSelection charts = chartSelectionRepo.getChartsByTherapistId(0); //0 is therapistID, replace with ID when authentication is impl
            if (charts != null && charts.chartMap.Keys.Contains(id.ToString())) //saved settings exist
            {
                model.selectedCharts = charts.chartMap[id.ToString()];
            }
            else
            {
                model.selectedCharts = new Dictionary<string, string>();
            }
            
            return model;
        }

        public MasterViewModel loadCache()
        {
            return cache.loadViewModel();
        }

        public void updateCachedPatientDataModelById(long id)
        {
            FHIRRepository repo = new FHIRRepository();
            FhirJsonSerializer serializer = new FhirJsonSerializer();

            FullPatientData model = new FullPatientData();

            //add patient details
            Patient patient = repo.getPatientById(id);
            if (patient != null)
            {
                string patJson = serializer.SerializeToString(patient);
                model.patient = patJson;
            }

            //add QRs
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<QuestionnaireResponse> QRs = repo.getAllQRsByPatId(id); //< 1 sec
            stopwatch.Stop();
            log.logTimeSpan("getAllQRsByPatID(" + id.ToString() +")_from_cache", stopwatch.Elapsed, QRs.Count);
            
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
            stopwatch.Restart();
            List<Observation> observations = repo.getAllObservationsByPatId(id); // ~2 sec
            stopwatch.Stop();
            log.logTimeSpan("getAllObservationsByPatId(" + id.ToString() + ")_from_server", stopwatch.Elapsed, observations.Count);
            
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
            stopwatch.Restart();
            Dictionary<string, string> qMap = repo.getQMap(id); //~2 sec
            stopwatch.Stop();
            log.logTimeSpan("getQMap(" + id.ToString() + ")", stopwatch.Elapsed);
            
            model.questionnaireMap = qMap;
            model.patientID = id;

            cache.cacheFullPatientData(model); //update charts independently //runs in < 0.01 sec
            
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
        public void updateGlobalState(bool patientViews)
        {
            List<Patient> patients = getAllPatients();
            Stopwatch stopwatchUpdate = new Stopwatch();
            stopwatchUpdate.Start();
            updateAllQRs(patients); //3m 09 sec
            stopwatchUpdate.Stop();
            log.logTimeSpan("updateAllQRs()", stopwatchUpdate.Elapsed);


            //declare calculation functions
            IAggregationFunction aggFunc = new SumDeltaThresholdSingleQRFunc(1, "83153");
            IFlagFunction flagFunc = new MaxDeltaFlagFunc(2, "83153");
            IWarningFunction warningFunc = new AbsSuicidalMADRSWarningFunc(4, "83153");

            Stopwatch stopwatchCalc = new Stopwatch();
            stopwatchCalc.Start();

            Dictionary<long, string> summaries = getSummaries(aggFunc);
            Dictionary<long, List<string>> flags = getFlags(flagFunc); //todo handle multiple flags
            Dictionary<long, List<string>> warnings = getWarnings(warningFunc);
            Dictionary<long, DateTime> earliestDates = getEarliestDates();


            stopwatchCalc.Stop();
            log.logTimeSpan("calculate_generic_functions", stopwatchCalc.Elapsed);

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


            model.earliestQRDate = new Dictionary<string, DateTime>();
            foreach (var kvp in earliestDates)
            {
                model.earliestQRDate[kvp.Key.ToString()] = kvp.Value; //TODO check if format is fine
            }

            cache.cacheMasterViewModel(model);


            //Detail views
            if (patientViews)
            {
                foreach (var pat in patients)
                {
                    long patID = Int32.Parse(pat.Id);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    updateCachedPatientDataModelById(patID);
                    stopwatch.Stop();
                    log.logTimeSpan("updateCachedPatientDataModelById(" + patID.ToString() + ")", stopwatch.Elapsed);
                    
                }
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
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

            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            log.logTimeSpan("getAllPatients()_from_server", ts, patients.Count);

            return patients;
        }

        private Dictionary<long, DateTime> getEarliestDates()
        {
            Dictionary<long, DateTime> dates = new Dictionary<long, DateTime>();
            foreach (var kvp in patientData)
            {
                List<QuestionnaireResponse> QRs = kvp.Value;
                DateTime lastQR = DateTime.Parse(QRs[0].Authored);

                foreach (var QR in QRs)
                {
                    DateTime date = DateTime.Parse(QR.Authored);
                    if (lastQR == null || lastQR > date)
                    {
                        lastQR = DateTime.Parse(QR.Authored);
                    }
                }
                dates[kvp.Key] = lastQR;
            }
            return dates;
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
            var QRs = QRHandler.getCachedQRsForPatient(patID); 

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

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            

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

            stopwatch.Stop();
            log.logTimeSpan("get_questionnaires_for_patient(" + patID.ToString() + ")_from_server", stopwatch.Elapsed, questionnaires.Count);

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

        public void updateAllQRs(List<Patient> patients)
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Dictionary<long, List<QuestionnaireResponse>> newQRs = assembleQRDict(patients, newIds);

            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            log.logTimeSpan("assembleQRDict()_all_patients", ts);

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
        private Dictionary<long, List<QuestionnaireResponse>> assembleQRDict(List<Patient> patients, List<long> newIds)
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