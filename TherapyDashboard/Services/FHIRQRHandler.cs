using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Logger log;

        public FHIRQRHandler(FhirClient client, MongoRepository cache)
        {
            this.client = client;
            this.cache = cache;
            log = new Logger();
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

            List<QuestionnaireResponse> newQRs = getQRByPatientId(patId);

            return newQRs;
        }


        public List<QuestionnaireResponse> getQRByPatientId(long id)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


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
            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            log.logTimeSpan("getQRByPatientId("+ id.ToString() + ")", ts);

            return QRs;
        }

    }
}