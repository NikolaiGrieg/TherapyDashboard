using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;

namespace TherapyDashboard.DataBase
{
    public class PatientRepository
    {

        private IMongoDatabase db;

        public PatientRepository()
        {
            MongoClient client = new MongoClient();
            this.db = client.GetDatabase("Dashboard");
            var collection = db.GetCollection<Patient>("Patient");
        }

        public IMongoCollection<Patient> Patients
        {
            get
            {
                return db.GetCollection<Patient>("Patient");
            }
        }



    }
}