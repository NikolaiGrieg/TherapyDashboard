using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TherapyDashboard.Services
{
    interface IUrgencyScore
    {
        int calculateUrgency(List<QuestionnaireResponse> QRs);
    }
}
