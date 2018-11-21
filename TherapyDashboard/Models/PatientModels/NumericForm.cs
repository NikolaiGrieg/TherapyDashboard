using Newtonsoft.Json.Linq;
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
        public List<JObject> entry { get; set; }
    }
}