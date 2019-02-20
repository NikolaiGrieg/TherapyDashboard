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

namespace TherapyDashboard.Services
{
    public class FHIRRepository //TODO refactor duplicate code
    {
        FhirClient client;
        DBCache cache;
        PatientAnalytics calc;
        Dictionary<long, List<QuestionnaireResponse>> patientData;

        public FHIRRepository()
        {
            client = new FhirClient("http://localhost:8080/hapi/baseDstu3");
            cache = new DBCache();
            calc = new PatientAnalytics();
        }

        private List<QuestionnaireResponse> getQRsAfterDateTime(DateTime dt, long patID)
        {
            List<QuestionnaireResponse> QRs = new List<QuestionnaireResponse>();
            string dtString = dt.ToString("o"); //XML string

            Bundle results = client.Search<QuestionnaireResponse>(new string[] { //TODO handle errors
                "subject=Patient/" + patID,
                "authored=gt" + dtString
            });

            while (results != null)
            {
                foreach (var entry in results.Entry)
                {
                    QuestionnaireResponse QR = (QuestionnaireResponse)entry.Resource;
                    QRs.Add(QR);
                }

                results = client.Continue(results);
            }
            return QRs;
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

            //get all QRs for new patients
            List<Patient> newPats = new List<Patient>();
            foreach (var pat in patients)
            {
                if (!newIds.Contains(Int32.Parse(pat.Id)))
                {
                    newPats.Add(pat);
                }
            }

            Dictionary<long, List<QuestionnaireResponse>> newQRs = new Dictionary<long, List<QuestionnaireResponse>>();
            foreach (var pat in newPats)
            {
                long patID = Int32.Parse(pat.Id);
                var newQRList = getQRByPatientId(patID);
                newQRs[patID] = newQRList;
                patientData[patID] = newQRList;
            }

            //get new QRs for old patients, TODO refactor extract method
            Dictionary<long, List<QuestionnaireResponse>> newQRsOldPatients = new Dictionary<long, List<QuestionnaireResponse>>();
            foreach (var kvp in patientData)
            {
                long patId = kvp.Key;
                List<QuestionnaireResponse> newQRList = getNewestQRs(patId, patientData);
                if (newQRList != null && newQRList.Any())
                {
                    newQRs[patId] = newQRList;
                    newQRsOldPatients[patId] = newQRList;
                }
            }

            foreach (var kvp in newQRsOldPatients)
            {
                patientData[kvp.Key] = kvp.Value;
            }


            //insert new elements in cache
            cache.insertNewQRs(newQRs);


            //calculate summaries
            //IAggregationFunction aggFunc = new SumCompareThresholdFunc(1); //TODO extract
            //Dictionary<long, string> summaries = calculateSummaries(patientData, aggFunc);

            //IFlagFunction flagFunc = new MaxDeltaFlagFunc();
            //Dictionary<long, string> flags = calculateFlags(patientData, flagFunc);

            //return summaries;
        }

        public Dictionary<long, string> getFlags(IFlagFunction flagFunc)
        {
            Dictionary<long, string> flags = new Dictionary<long, string>();
            foreach (var kvp in patientData)
            {
                flags[kvp.Key] = calc.calculateFlags(kvp, flagFunc);
            }
            return flags;
        }

        public Dictionary<long, string> getSummaries(IAggregationFunction aggFunc)
        {
            calc = new PatientAnalytics();
            Dictionary<long, string> summaries = new Dictionary<long, string>();
            foreach (var kvp in patientData)
            {
                summaries[kvp.Key] = calc.calculateSummary(kvp, aggFunc);
            }
            return summaries;
        }

        public List<QuestionnaireResponse> getCachedQRsForPatient(long patId)
        {
            PatientData pat = cache.getPatientDataById(patId);
            if (pat != null)
            {
                List<QuestionnaireResponse> QRs = pat.QRs;
                return QRs;
            }
            else
            {
                return null;
            }
        }

        private List<QuestionnaireResponse> getNewestQRs(long patId, Dictionary<long, List<QuestionnaireResponse>> patientData)
        {
            var oldQRs = patientData[patId]; //if this is empty, the resource server doesn't have QRs for the patient

            if (!oldQRs.Any())
            {
                return null;
            }

            //get newest QR, currently assuming order holds, TODO test this
            QuestionnaireResponse lastQR = oldQRs[oldQRs.Count - 1];
            DateTime lastDate = DateTime.Parse(lastQR.Authored);

            List<QuestionnaireResponse> newQRs = getQRsAfterDateTime(lastDate, patId);

            return newQRs;
        }


        private List<QuestionnaireResponse> getQRByPatientId(long id)
        {
            List<QuestionnaireResponse> QRs = new List<QuestionnaireResponse>();

            Bundle results = client.Search<QuestionnaireResponse>(new string[] { "subject=Patient/" + id });

            while (results != null)
            {
                foreach (var entry in results.Entry)
                {
                    QuestionnaireResponse QR = (QuestionnaireResponse)entry.Resource;
                    QRs.Add(QR);
                }

                results = client.Continue(results);
            }
            return QRs;
        }


        public List<Observation> getCachedObservationssForPatient(long patId)
        {
            PatientData pat = cache.getPatientDataById(patId);
            if (pat != null)
            {
                List<Observation> observations = pat.observations;
                return observations;
            }
            else
            {
                return null;
            }
        }


        public List<Observation> getAllObservationsByPatId(long id)
        {
            List<Observation> oldObservations = getCachedObservationssForPatient(id);
            if (oldObservations != null)
            {

            }

            List<Observation> newObservations = new List<Observation>();
            Bundle results = client.Search<Observation>(new string[] { "subject=Patient/" + id });

            while (results != null)
            {
                foreach (var entry in results.Entry)
                {
                    Observation obs = (Observation)entry.Resource;
                    newObservations.Add(obs);
                }

                results = client.Continue(results);
            }
            return newObservations;
        }

        private List<Observation> getNewestQRs(long patId, Dictionary<long, List<Observation>> observations)
        {
            var oldObservations = observations[patId]; //if this is empty, the resource server doesn't have QRs for the patient

            if (!oldObservations.Any())
            {
                return null;
            }

            //get newest QR, currently assuming order holds, TODO test this
            Observation lastObs = oldObservations[oldObservations.Count - 1];
            DateTime lastDate = DateTime.Parse(lastObs.Meta.LastUpdated.ToString());//could maybe do this without the string cast

            List<Observation> newObs = getObservationsAfterDateTime(lastDate, patId);

            return newObs;
        }

        private List<Observation> getObservationsAfterDateTime(DateTime dt, long patID)
        {
            List<Observation> observations = new List<Observation>();
            string dtString = dt.ToString("o"); //XML string

            Bundle results = client.Search<Observation>(new string[] { //TODO handle errors
                "subject=Patient/" + patID,
                "authored=gt" + dtString
            });

            while (results != null)
            {
                foreach (var entry in results.Entry)
                {
                    Observation obs = (Observation)entry.Resource;
                    observations.Add(obs);
                }

                results = client.Continue(results);
            }
            return observations;
        }
    }
}