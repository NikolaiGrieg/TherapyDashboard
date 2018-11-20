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

        public string Measurement1Path{ get; set; }

        public static Random random = null;


        public static Patient createSimulated()
        {
            if (random == null)
            {
                random = new Random();
            }

            int num = random.Next(0, 10);
            Patient pat = new Patient() {
                name = "Guy Simulated" + num,
                id = num
            };

            return pat;
        }
    }
}