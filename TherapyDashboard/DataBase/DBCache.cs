using Hl7.Fhir.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;

namespace TherapyDashboard.DataBase
{
    public class DBCache
    {
        IMongoCollection<MetaData> collection;

        //init connection
        public DBCache()
        {
            MongoClient client = new MongoClient();
            var db = client.GetDatabase("Dashboard");
            collection = db.GetCollection<MetaData>("MetaData");
        }

        //implement saving metadata
        public void saveMetaData(Dictionary<int, List<QuestionnaireResponse>> QRDict, Dictionary<int, string> summaries)
        {
            //create metadata objects
            
            foreach (var kvp in QRDict) //iterate over patients
            {
                int id = kvp.Key;
                List<QuestionnaireResponse> QRs = kvp.Value;
                string summary = summaries[id];

                MetaData metaData = new MetaData(id, DateTime.Now, summary);
                collection.InsertOne(metaData);
            }

        }

        //implement checking metadata

        //possibly another place, get new ones based on metadata
    }
}