using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TherapyDashboard.Services.WarningFunctions
{
    public interface IWarningFunction
    {
        List<string> calculateWarning(List<QuestionnaireResponse> QRs);
    }
}
