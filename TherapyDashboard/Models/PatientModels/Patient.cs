using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models.PatientModels;

namespace TherapyDashboard.Models
{
    public class Patient
    {
        
        public ObjectId id { get; set; }
        public int tempRand { get; set; }
        public string name { get; set; }
        public int module { get; set; }
        public ICollection<ObjectId> NumericForms { get; set; } //ObjectId for each form
        public ICollection<UnstructuredForm> UnstructuredForms { get; set; }


        private static Random random = null;

        
        public static Patient createSimulated()
        {
            if (random == null)
            {
                random = new Random();
            }
            int rand = random.Next(20);

            Patient pat = new Patient() {
                name = "Guy Simulated" + rand,
                tempRand = rand 
            };

            return pat;
        }
    }
}