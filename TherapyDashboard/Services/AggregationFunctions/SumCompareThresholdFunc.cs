using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace TherapyDashboard.Services.AggregationFunctions
{
    public class SumCompareThresholdFunc : IAggregationFunction
    {
        public float threshold { get; set; }

        public SumCompareThresholdFunc(float _deltaThreshold)
        {
            this.threshold = _deltaThreshold;
        }

        public string aggregate(List<QuestionnaireResponse> QRs)
        {
            //TODO current assumption is that last element will be latest, check if this is the case
            QuestionnaireResponse lastQR = QRs[QRs.Count - 1];
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
            float delta = sumSecondLatest - sumLatest;
            if (delta < -threshold)
            {
                //sum going up => condition declining
                return "declining";
            }
            else if ((delta >= -threshold) && (delta <= threshold))
            {
                return "steady";
            }
            else if (delta > threshold)
            {
                return "improving";
            }
            return "error"; // error checking
        }
    }
}