using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;

namespace TherapyDashboard.DataBase
{
    public class MongoDBConnection
    {
        /*
        private IMongoDatabase db;

        public MongoDBConnection()
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

        public IMongoCollection<BsonDocument> Forms
        {
            get
            {
                return db.GetCollection<BsonDocument>("Form");
            }
        }
        */


    }
}