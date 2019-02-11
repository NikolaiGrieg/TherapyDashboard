using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.ViewModels
{
    public class DetailViewModel
    {
        public List<string> QRs { get; set; }
        public List<string> observations { get; set; }
        public string patient { get; set; }
    }
}