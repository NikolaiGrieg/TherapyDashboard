using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.DataBase;
using TherapyDashboard.Models;

namespace TherapyDashboard.Services
{
    public class FHIRQRHandler
    {
        FhirClient client;
        MongoRepository cache;

        public FHIRQRHandler(FhirClient client, MongoRepository cache)
        {
            this.client = client;
            this.cache = cache;
        }

        /*
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
        */

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

        public List<QuestionnaireResponse> getNewestQRs(long patId, Dictionary<long, List<QuestionnaireResponse>> patientData)
        {
            var oldQRs = patientData[patId]; //if this is empty, the resource server doesn't have QRs for the patient

            if (!oldQRs.Any())
            {
                return null;
            }

            //get newest QR, currently assuming order holds, TODO test this
            //QuestionnaireResponse lastQR = oldQRs[oldQRs.Count - 1];
            //DateTime lastDate = DateTime.Parse(lastQR.Authored);

            List<QuestionnaireResponse> newQRs = getQRByPatientId(patId);

            return newQRs;
        }


        public List<QuestionnaireResponse> getQRByPatientId(long id)
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

    }
}