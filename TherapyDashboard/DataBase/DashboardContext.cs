using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;

namespace TherapyDashboard.DBContexts
{
    public class DashboardContext : DbContext
    {
        public DashboardContext() : base()
        {

        }

        public DbSet<Patient> Patients { get; set; }
        
    }
}