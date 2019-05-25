using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace TherapyDashboard.Services.AggregationFunctions
{
    public class SumDeltaThresholdSingleQRFunc : IAggregationFunction
    {
        public float threshold { get; set; }
        public string qid { get; set; }

        /// <summary>
        /// Calculates the patient state by Summing the values of the latest and second
        /// latest questionnaires of a specific questionnaire, and comparing the delta to a threshold.
        /// </summary>
        /// <param name="_deltaThreshold">Threshold for comparison. Eg. threshold 2 gives "declining" if
        /// the delta is less than -2, and "improving" if delta is higher than 2</param>
        /// <param name="_questionnaireID">The FHIR id of the questionnaire to use for comparison</param>
        public SumDeltaThresholdSingleQRFunc(float _deltaThreshold, string _questionnaireID)
        {
            this.threshold = _deltaThreshold;
            this.qid = "Questionnaire/" + _questionnaireID; //fhir canonical uri
        }

        public ProgressionRepresentation aggregate(List<QuestionnaireResponse> QRs)
        {
            //find QRs matching "qid" string
            List<QuestionnaireResponse> matchingQRs = new List<QuestionnaireResponse>();
            foreach (var QR in QRs)
            {
                if (QR.Questionnaire.Reference == qid)
                {
                    matchingQRs.Add(QR);
                }
            }

            //find latest and second latest of matched QRs
            QuestionnaireResponse lastQR = null;
            QuestionnaireResponse secondLastQR = null;

            foreach (var QR in matchingQRs)
            {
                DateTime date = DateTime.Parse(QR.Authored);
                if (lastQR == null || DateTime.Parse(lastQR.Authored) < date)
                {
                    secondLastQR = lastQR;
                    lastQR = QR;
                }
            }

            //calculate sums
            int sumLatest = 0;
            int sumSecondLatest = 0;

            if (lastQR != null && secondLastQR != null)
            {
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
                    return ProgressionRepresentation.declining;
                }
                else if ((delta >= -threshold) && (delta <= threshold))
                {
                    return ProgressionRepresentation.steady;
                }
                else if (delta > threshold)
                {
                    return ProgressionRepresentation.improving;
                }
                return ProgressionRepresentation.error;
            }
            else
            {
                return ProgressionRepresentation.blank;
            }
            
        }
    }
}