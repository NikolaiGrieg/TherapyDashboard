using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models.PatientModels
{
    public class UnstructuredForm
    {
        public int id { get; set; }
        public string name { get; set; }
        public Dictionary<string, string> values { get; set; }
    }
}