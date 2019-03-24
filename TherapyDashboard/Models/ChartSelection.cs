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
        public List<string> chartNames{ get; set; } //chart 
    }
}