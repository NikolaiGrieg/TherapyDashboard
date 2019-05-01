using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace TherapyDashboard.Services.FlagFunctions
{
    public class MaxDeltaFlagFunc : IFlagFunction
    {
        private int threshold;
        private string qid;

        public MaxDeltaFlagFunc(int threshold, string qid)
        {
            this.threshold = threshold;
            this.qid = "Questionnaire/" + qid;
        }
        public List<string> calculateFlag(List<QuestionnaireResponse> QRs)
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

            if(lastQR != null && secondLastQR != null)
            {
                KeyValuePair<string, float> highestDelta = new KeyValuePair<string, float>("", int.MinValue);
                foreach (var item in secondLastQR.Item)
                {
                    float valuePrev = (float)Int32.Parse(item.Answer[0].Value.ToString()); 
                    string keyPrev = item.Text;

                    var correspondingItem = lastQR.Item.Where(x => x.Text == keyPrev).First();
                    float valueLast = (float)Int32.Parse(correspondingItem.Answer[0].Value.ToString());

                    float delta = valueLast - valuePrev; //higher is worse

                    if (delta >= threshold)
                    {
                        if (delta > highestDelta.Value)
                        {
                            highestDelta = new KeyValuePair<string, float>(keyPrev + " +" + delta + " (" + valueLast + ")", delta);
                        }
                    }
                }
                return new List<string>(new string[] { highestDelta.Key });
            }

            return new List<string>(new string[] { "" });
        }
    }
}