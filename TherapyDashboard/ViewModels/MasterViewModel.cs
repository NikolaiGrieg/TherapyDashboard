using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.ViewModels
{
    public class MasterViewModel
    {
        public Dictionary<string, string> flags;
        public Dictionary<string, string> summaries { get; set; }
        public Dictionary<string, string> patientNames { get; set; }
        
    }
}