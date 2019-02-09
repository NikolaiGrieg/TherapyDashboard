using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models
{
    public class MetaData
    {
        public long fhirID { get; set; }
        public DateTime lastUpdate { get; set; }
        public string lastSummary { get; set; }

        public MetaData(long _fhirID, DateTime _lastUpdate, string _lastSummary)
        {
            this.fhirID = _fhirID;
            this.lastUpdate = _lastUpdate;
            this.lastSummary = _lastSummary;
        }
    }
}