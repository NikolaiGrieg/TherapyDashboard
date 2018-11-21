using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models
{
    public class Patient
    {
        
        public ObjectId id { get; set; }
        public int tempRand { get; set; }
        public string name { get; set; }
        public ICollection<PatientFilledForms> forms { get; set; }


        public string Measurement1Path{ get; set; } //TODO remove eventually

        //public static Random random = null;

        
        public static Patient createSimulated()
        {
            Patient pat = new Patient() {
                name = "Guy Simulated" + 0
            };

            return pat;
        }
    }
}