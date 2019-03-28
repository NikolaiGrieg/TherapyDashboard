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
    public class FHIRObservationHandler
    {
        FhirClient client;
        MongoRepository cache;
        PatientAnalytics calc;

        public FHIRObservationHandler(FhirClient client, MongoRepository cache, PatientAnalytics calc)
        {
            this.client = client;
            this.cache = cache;
            this.calc = calc;
        }

        //TODO unused?
        public List<Observation> getCachedObservationsForPatient(long patId)
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
                "authored=gt" + dtString //gt = strictly greater
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