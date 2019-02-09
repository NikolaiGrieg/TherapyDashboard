using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace TherapyDashboard.Services
{
    public class FHIRRepository
    {
        FhirClient client;

        public FHIRRepository()
        {
            client = new FhirClient("http://localhost:8080/hapi/baseDstu3");
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

        public Dictionary<int, List<QuestionnaireResponse>> getAllQRs()
        {
            List<Patient> patients = getAllPatients();

            List<int> ids = new List<int>();
            foreach (var pat in patients)
            {
                ids.Add(Int32.Parse(pat.Id));
            }
            Dictionary<int, List<QuestionnaireResponse>> allQRs = new Dictionary<int, List<QuestionnaireResponse>>();
            foreach( var id in ids)
            {
                //TODO async
                List<QuestionnaireResponse> QRs = getQRByPatientId(id);
                if (QRs.Any())
                {
                    allQRs[id] = QRs;
                }
            }

            return allQRs;
        }

        private List<QuestionnaireResponse> getQRByPatientId(int id)
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