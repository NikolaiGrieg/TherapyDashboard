using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TherapyDashboard.Services.AggregationFunctions
{
    public enum SummaryRepresentation{ improving, steady, declining, blank, error };

    public interface IAggregationFunction
    {
        SummaryRepresentation aggregate(List<QuestionnaireResponse> QRs);
    }
}
