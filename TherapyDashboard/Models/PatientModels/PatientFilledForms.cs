using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models.PatientModels;

namespace TherapyDashboard.Models
{
    public class PatientFilledForms
    {
        public int id { get; set; }
        public List<NumericForm> numericForms { get; set; }
        public List<UnstructuredForm> unstructuredForms { get; set; }
        //TODO login info etc goes here
    }
}