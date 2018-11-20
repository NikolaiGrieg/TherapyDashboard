using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.DBContexts;
using TherapyDashboard.Models;

namespace TherapyDashboard.DataBase
{
    public class PatientRepository
    {
        public static void createPatient()
        {
            using (var ctx = new DashboardContext())
            {
                var pat = new Patient() { name = "Bill" };

                ctx.Patients.Add(pat);
                ctx.SaveChanges();
            }
        }
    }
}