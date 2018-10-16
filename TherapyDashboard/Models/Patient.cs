using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models
{
    public class Patient
    {
        public int id { get; set; }
        public string name { get; set; }
        public string[] forms { get; set; }
        public string[] background { get; set; }

    }
}