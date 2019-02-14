using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TherapyDashboard.Services.FlagFunctions
{
    public interface IFlagFunction
    {
        string calculateFlag(List<QuestionnaireResponse> QRs);
    }
}
