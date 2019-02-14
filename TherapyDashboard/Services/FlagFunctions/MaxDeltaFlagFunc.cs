using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace TherapyDashboard.Services.FlagFunctions
{
    public class MaxDeltaFlagFunc : IFlagFunction
    {
        public string calculateFlag(List<QuestionnaireResponse> QRs)
        {
            //TODO current assumption is that last element will be latest, check if this is the case
            QuestionnaireResponse lastQR = QRs[QRs.Count - 1];
            QuestionnaireResponse secondLastQR = QRs[QRs.Count - 2];

            KeyValuePair<string, float> highestDelta = new KeyValuePair<string, float>("", int.MinValue);
            foreach (var item in secondLastQR.Item)
            {
                float valuePrev = (float) Int32.Parse(item.Answer[0].Value.ToString()); //read float??
                string keyPrev = item.Text;

                var correspondingItem = lastQR.Item.Where(x => x.Text == keyPrev).First();
                float valueLast = (float)Int32.Parse(correspondingItem.Answer[0].Value.ToString());

                float delta = valueLast - valuePrev; //higher is worse

                if (delta > highestDelta.Value)
                {
                    highestDelta = new KeyValuePair<string, float>(keyPrev, delta);
                }
            }
            return highestDelta.Key;
        }
    }
}