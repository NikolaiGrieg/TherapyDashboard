using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using TherapyDashboard.DataBase;
using TherapyDashboard.Models;

namespace TherapyDashboard.Services
{
    public class FHIRRepository
    {
        FhirClient client;

        public FHIRRepository()
        {
            client = new FhirClient("http://localhost:8080/hapi/baseDstu3");
        }

        public List<QuestionnaireResponse> getQRsAfterDateTime(DateTime dt, long patID)
        {
            List<QuestionnaireResponse> QRs = new List<QuestionnaireResponse>();
            string dtString = dt.ToString("o"); //XML string

            Bundle results = client.Search<QuestionnaireResponse>(new string[] {
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


        public Dictionary<long, string> getSummaries(List<Patient> patients)
        {
            DBCache cache = new DBCache();

            //get all patient IDs
            
            List<long> oldIds = new List<long>();
            foreach (var pat in patients)
            {
                oldIds.Add(Int32.Parse(pat.Id));
            }

            //get cached data
            Dictionary<long, List<QuestionnaireResponse>> patientData = cache.getAllPatientResources(oldIds); //could be empty

            List<long> newIds = new List<long>();
            foreach (var id in patientData.Keys)
            {
                newIds.Add(id);
            }

            //query new, update cache
            //create list of new elements
            List<Patient> newPats = new List<Patient>();
            foreach (var pat in patients)
            {
                if (!newIds.Contains(Int32.Parse(pat.Id)))
                {
                    newPats.Add(pat);
                }
            }

            //get all QRs for new patients
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

            PatientAnalytics calc = new PatientAnalytics(); //TODO refactor -> reverse this coupling
            Dictionary<long, string> summaries = new Dictionary<long, string>();

            //calculate summaries
            foreach (var kvp in patientData) 
            {
                summaries[kvp.Key] = calc.calculateSummary(kvp);
            }

            return summaries;
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
            DateTime lastDate = DateTime.Parse(lastQR.Authored); //TODO this might throw error

            List<QuestionnaireResponse> newQRs = getQRsAfterDateTime(lastDate, patId);

            return newQRs;
        }

        private List<QuestionnaireResponse> getQRByPatientId(long id)
        {
            List<QuestionnaireResponse> QRs = new List<QuestionnaireResponse>();

            //TODO only get new ones when db is implemented
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
    }
}