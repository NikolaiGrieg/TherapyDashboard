using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models
{
    public class ChartSelection
    {
        public ObjectId id { get; set; }
        public long therapistID { get; set; }
        public Dictionary<string, Dictionary<string, string>> chartMap{ get; set; } //patientID : {chartName : chartType}
    }
}