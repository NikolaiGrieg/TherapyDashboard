using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace TherapyDashboard.Services.WarningFunctions
{
    public class DeltaThresholdWarningFunc : IWarningFunction
    {
        private int threshold;

        public DeltaThresholdWarningFunc(int _threshold)
        {
            this.threshold = _threshold;
        }
        public List<string> calculateWarning(List<QuestionnaireResponse> QRs)
        {
            throw new NotImplementedException();
        }
    }
}