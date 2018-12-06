using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;

namespace TherapyDashboard.ViewModels
{
    public class DetailViewModel
    {
        public Patient pat { get; set; }
        public Patient avgPat { get; set; }
        public string json { get; set; }
    }
}