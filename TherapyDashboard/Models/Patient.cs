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
        public string[] forms { get; set; } //TODO foreign key
        public Dictionary<string, string> background { get; set; }
        public string[] tasks { get; set; }
        public DateTime previousDelivery { get; set; }
        public DateTime nextDelivery { get; set; }
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
                id = num,
                forms = new string[] { "SPS", "SIAS", "SE - SKALA", "TIC - C", "Bakgrunn" },
                background = new Dictionary<string, string> {
                    { "Alder", "30" },
                    { "Kjønn", "Mann" },
                    { "Utdanning","Videregående skole/gymnas" },
                    { "Sivilstatus", "Enslig" },
                    { "Antall barn", "Ingen" }
                },
                tasks = new string[] { "Modul 1: Skjema 1", "Modul 1: Skjema 2", "Modul 2: Skjema 1", "Modul 2: Skjema 2", "Modul 2: Skjema 3", },
                nextDelivery = DateTime.Now
            };

            return pat;
        }
    }
}