using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models
{
    public class NumericForm
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<int> row { get; set; }
        public List<string> colNames { get; set; }
    }
}