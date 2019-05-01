using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace TherapyDashboard.Services.UrgencyFunctions
{
    public class MadrsSumUrgencyScore : IUrgencyScore
    {

        private string qid;

        public MadrsSumUrgencyScore(string _questionnaireID)
        {
            this.qid = "Questionnaire/" + _questionnaireID; //fhir canonical uri
        }

        public int calculateUrgency(List<QuestionnaireResponse> QRs)
        {
            //find QRs matching "qid" string
            List<QuestionnaireResponse> matchingQRs = new List<QuestionnaireResponse>();
            if (QRs != null)
            {
                foreach (var QR in QRs)
                {
                    if (QR.Questionnaire.Reference == qid)
                    {
                        matchingQRs.Add(QR);
                    }
                }
            }


            //find latest and second latest of matched QRs
            QuestionnaireResponse lastQR = null;

            if (matchingQRs.Count > 0)
            {
                foreach (var QR in matchingQRs)
                {
                    DateTime date = DateTime.Parse(QR.Authored);
                    if (lastQR == null || DateTime.Parse(lastQR.Authored) < date)
                    {
                        lastQR = QR;
                    }
                }
            }

            int sum = 0;
            if (lastQR != null)
            {
                foreach (var item in lastQR.Item)
                {
                    int value = Int32.Parse(item.Answer[0].Value.ToString());
                    sum += value;
                }
            }
            return sum;
        }
    }
}