using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Services.AggregationFunctions;

namespace TherapyDashboard.Services
{
    //TODO maybe static?
    public class PatientAnalytics
    {
        
        public string calculateSummary(KeyValuePair<long, List<QuestionnaireResponse>> kvp, IAggregationFunction func)
        {
            //TODO get aggregation settings/formula from DB
            //for now use (sum delta last and second last > x)

            var QRs = kvp.Value;
            if (QRs == null || !QRs.Any())
            {
                return "no forms";
            }
            string summary = func.aggregate(QRs);
            return summary;
            
        }

        public string calculateFlags()
        {
            return null;
        }
    }
}