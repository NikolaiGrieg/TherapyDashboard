using Hl7.Fhir.Model;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.ViewModels
{
    public class MasterViewModel
    {
        public ObjectId id { get; set; }
        public Dictionary<string, List<string>> flags;
        public Dictionary<string, List<string>> warnings;
        public Dictionary<string, string> summaries { get; set; }
        public Dictionary<string, string> patientNames { get; set; }
        public Dictionary<string, DateTime> lastCheckedMap { get; set; }
    }
}