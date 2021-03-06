﻿using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Services.AggregationFunctions;
using TherapyDashboard.Services.FlagFunctions;
using TherapyDashboard.Services.WarningFunctions;

namespace TherapyDashboard.Services
{
    //TODO maybe static?
    public class PatientAnalytics
    {
        
        public string calculateSummary(KeyValuePair<long, List<QuestionnaireResponse>> kvp, IAggregationFunction func)
        {
            //TODO get aggregation settings/formula from DB
            //for now use (sum delta last and second last > x)

            var QRs = kvp.Value;
            if (QRs == null || !QRs.Any())
            {
                return "no forms";
            }
            string summary = func.aggregate(QRs).ToString();
            return summary;
            
        }

        public List<string> calculateFlags(KeyValuePair<long, List<QuestionnaireResponse>> kvp, IFlagFunction func)
        {
            var QRs = kvp.Value;
            if (QRs == null || !QRs.Any())
            {
                return new List<string>(new string[] { "no forms" });
            }
            List<string> flag = func.calculateFlag(QRs);
            return flag;
        }

        public List<string> calculateWarnings(KeyValuePair<long, List<QuestionnaireResponse>> kvp, IWarningFunction func)
        {
            var QRs = kvp.Value;
            if (QRs == null || !QRs.Any())
            {
                return new List<string>();
            }
            List<string> warning = func.calculateWarning(QRs);
            return warning;
        }

        internal int calculateUrgency(KeyValuePair<long, List<QuestionnaireResponse>> kvp, IUrgencyScoreFunction func)
        {
            var QRs = kvp.Value;
            if (QRs == null || !QRs.Any())
            {
                return -1;
            }
            int urgency = func.calculateUrgency(QRs);
            return urgency;
        }
    }
}