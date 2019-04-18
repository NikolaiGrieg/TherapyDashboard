using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Services.WarningFunctions
{
    public class AbsSuicidalMADRSWarningFunc :IWarningFunction
    {
        private int threshold;
        private string qid;

        public AbsSuicidalMADRSWarningFunc(int _threshold, string _questionnaireID)
        {
            this.threshold = _threshold;
            this.qid = "Questionnaire/" + _questionnaireID; //fhir canonical uri
        }
        public List<string> calculateWarning(List<QuestionnaireResponse> QRs)
        {
            //find QRs matching "qid" string
            List<QuestionnaireResponse> matchingQRs = new List<QuestionnaireResponse>();
            if(QRs != null)
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
            
            if(matchingQRs.Count > 0)
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

            List<string> triggeredCategories = new List<string>();
            if (lastQR != null)
            {
                
                foreach (var item in lastQR.Item)
                {
                    if (item.Text == "Suicidal thoughts")
                    {
                        float value = (float)Int32.Parse(item.Answer[0].Value.ToString());
                        if (value >= threshold)
                        {
                            triggeredCategories.Add("Suicidal thoughts " + value.ToString());
                            return triggeredCategories;
                        }
                    }
                }
            }

            return triggeredCategories;
        }
    }
}