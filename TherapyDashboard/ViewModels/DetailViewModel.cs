using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.ViewModels
{
    public class DetailViewModel
    {
        public ObjectId id { get; set; }
        public long patientID { get; set; }
        public List<string> QRs { get; set; }
        public List<string> observations { get; set; }
        public string patient { get; set; }
        public Dictionary<string, string> questionnaireMap { get; set; }
        public Dictionary<string, string> selectedCharts { get; set; }

    }
}