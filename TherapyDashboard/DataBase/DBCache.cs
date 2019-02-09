using Hl7.Fhir.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;
using TherapyDashboard.Services;

namespace TherapyDashboard.DataBase
{
    public class DBCache
    {

        //TODO replace MetaData class with inherited FHIR QRs


        IMongoCollection<MetaData> collection;
        FHIRRepository repo;

        //init connection
        public DBCache()
        {
            MongoClient client = new MongoClient();
            var db = client.GetDatabase("Dashboard");
            collection = db.GetCollection<MetaData>("MetaData");

            repo = new FHIRRepository();
        }

        /// <summary>
        /// Can return nulls as part of list, should return list of size n
        /// </summary>
        /// <param name="patientIDs"></param>
        /// <returns></returns>
        public List<MetaData> getMetaData(List<int> patientIDs)
        {
            List<MetaData> mds = new List<MetaData>();
            foreach (var id in patientIDs)
            {
                
                mds.Add(getMetaDataByPatientId(id));
            }
            return mds;
        }

        public MetaData getMetaDataByPatientId(long fhirID)
        {
            MongoDBConnection db = new MongoDBConnection();
            var filter = Builders<MetaData>.Filter.Eq(x => x.fhirID, fhirID);
            var md = collection.Find(filter).FirstOrDefault();
            return md;
        }

        private void insertSingleMetaData(int id, string summary)
        {
            MetaData metaData = new MetaData(id, DateTime.Now, summary);
            collection.InsertOne(metaData);
        }

        //possibly another place, get new ones based on metadata
    }
}