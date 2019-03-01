using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;

namespace TherapyDashboard.DataBase
{
    public class LastCheckedRepository
    {
        IMongoCollection<LastCheckedMap> collection;

        public LastCheckedRepository()
        {
            MongoClient client = new MongoClient();
            var db = client.GetDatabase("Dashboard");
            collection = db.GetCollection<LastCheckedMap>("LastChecked");

        }

        //TODO test this
        public void upsertSingleEntry(long therapistID, string patientID, DateTime ts)
        {
            //find if exists
            var filter = Builders<LastCheckedMap>.Filter.Eq(x => x.therapistID, therapistID);
            var map = collection.Find(filter).FirstOrDefault();

            //insert
            if(map != null && map.patientMap != null)
            {
                Dictionary<string, DateTime> patientMap = map.patientMap;
                patientMap[patientID] = ts;

                collection.UpdateOne(filter,
                    Builders<LastCheckedMap>.Update.Set("patientMap", patientMap)
                    );

            }
            else
            {
                LastCheckedMap entry = new LastCheckedMap();
                entry.therapistID = therapistID;
                entry.patientMap = new Dictionary<string, DateTime>();
                entry.patientMap[patientID] = ts;
                collection.InsertOne(entry);
            }
        }

        public void upsertMap(long therapistID, Dictionary<string, DateTime> patientMap)
        {
            //find if exists
            var filter = Builders<LastCheckedMap>.Filter.Eq(x => x.therapistID, therapistID);
            var map = collection.Find(filter).FirstOrDefault();

            //insert
            if (map != null)
            {
                collection.UpdateOne(filter,
                    Builders<LastCheckedMap>.Update.Set("patientMap", patientMap)
                    );
            }
            else
            {
                LastCheckedMap entry = new LastCheckedMap();
                entry.therapistID = therapistID;
                entry.patientMap = patientMap;
                collection.InsertOne(entry);
            }
        }

        public LastCheckedMap getMapByTherapistId(long therapistID)
        {
            var filter = Builders<LastCheckedMap>.Filter.Eq(x => x.therapistID, therapistID);
            var map = collection.Find(filter).FirstOrDefault();
            return map;
        }
    }
}