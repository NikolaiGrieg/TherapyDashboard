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

        public SumDeltaThresholdSingleQRFunc(float _deltaThreshold, string _questionnaireID)
        {
            this.threshold = _deltaThreshold;
            this.qid = "Questionnaire/" + _questionnaireID; //fhir canonical uri
        }

        public SummaryRepresentation aggregate(List<QuestionnaireResponse> QRs)
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
                    return SummaryRepresentation.declining;
                }
                else if ((delta >= -threshold) && (delta <= threshold))
                {
                    return SummaryRepresentation.steady;
                }
                else if (delta > threshold)
                {
                    return SummaryRepresentation.improving;
                }
                return SummaryRepresentation.error;
            }
            else
            {
                return SummaryRepresentation.blank;
            }
            
        }
    }
}