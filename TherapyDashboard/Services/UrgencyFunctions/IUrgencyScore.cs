using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TherapyDashboard.Services
{
    public interface IUrgencyScoreFunction
    {
        int calculateUrgency(List<QuestionnaireResponse> QRs);
    }
}
