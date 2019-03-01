using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models
{
    public class LastCheckedMap
    {
        public ObjectId id { get; set; }
        public long therapistID { get; set; }
        public Dictionary<string, DateTime> patientMap { get; set; }
    }
}