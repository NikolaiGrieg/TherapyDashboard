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
        public void saveMetaData(List<List<QuestionnaireResponse>> QRs, List<string> summaries)
        {

        }

        //implement checking metadata

        //possibly another place, get new ones based on metadata
    }
}