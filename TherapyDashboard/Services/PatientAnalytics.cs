using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Services
{
    //TODO maybe static?
    public class PatientAnalytics
    {
        //TODO private
        public string calculateSummary(KeyValuePair<int, List<QuestionnaireResponse>> kvp)
        {
            //TODO get aggregation settings/formula from DB
            //for now use (sum delta last and second last > x)

            //TODO current assumption is that last element will be latest, check if this is the case
            var QRs = kvp.Value;
            QuestionnaireResponse lastQR = QRs[QRs.Count -1];
            QuestionnaireResponse secondLastQR = QRs[QRs.Count - 2];

            //calculate sums
            int sumLatest = 0;
            int sumSecondLatest = 0;

            foreach (var item in lastQR.Item)
            {
                var valueElement = item.Answer[0].Value;
                int value = Int32.Parse(valueElement.ToString()); // getting the Value property from the Element class was problematic
                sumLatest += value;
            }

            foreach (var item in secondLastQR.Item)
            {
                var valueElement = item.Answer[0].Value;
                int value = Int32.Parse(valueElement.ToString()); // getting the Value property from the Element class was problematic
                sumSecondLatest += value;
            }

            //compare 
            float threshold = 2;
            float delta = sumSecondLatest - sumLatest;
            if (delta < -threshold) 
            {
                //sum going up => condition declining
                return "declining";
            }
            else if ((delta >= -threshold) && (delta <= threshold))
            {
                //steady
                return "steady";
            }
            else if (delta > threshold) //should cover all possibilities by this point
            {
                //improving
                return "improving";
            }
            return "error"; // error checking
        }
    }
}