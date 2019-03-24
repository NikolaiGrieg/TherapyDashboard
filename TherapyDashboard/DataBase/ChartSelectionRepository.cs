using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.Models;

namespace TherapyDashboard.DataBase
{
    public class ChartSelectionRepository
    {
        IMongoCollection<ChartSelection> collection;

        public ChartSelectionRepository()
        {
            MongoClient client = new MongoClient();
            var db = client.GetDatabase("Dashboard");
            collection = db.GetCollection<ChartSelection>("ChartSelectionModel");
        }

        public void upsertSingleEntry(long therapistID, string patientID, string chartName)
        {
            //find if exists
            var filter = Builders<ChartSelection>.Filter.Eq(x => x.therapistID, therapistID);
            var charts = collection.Find(filter).FirstOrDefault();

            //insert
            if (charts != null && charts.chartMap != null)
            {
                Dictionary<string, List<string>> map = charts.chartMap;
                if (map.Keys.Contains(patientID)) //has settings for patient
                {
                    if (!map[patientID].Contains(chartName))
                    {
                        map[patientID].Add(chartName);
                    }
                    
                }
                else
                {
                    map[patientID] = new List<string>();
                    map[patientID].Add(chartName);
                }

                collection.UpdateOne(filter,
                    Builders<ChartSelection>.Update.Set("chartMap", map)
                    );

            }
            else
            {
                ChartSelection entry = new ChartSelection();
                entry.therapistID = therapistID;
                entry.chartMap = new Dictionary<string, List<string>>();
                entry.chartMap[patientID] = new List<string>();
                entry.chartMap[patientID].Add(chartName);
                collection.InsertOne(entry);
            }
        }

        public void removeSingleEntry(long therapistID, string patientID, string chartName)
        {
            //find if exists
            var filter = Builders<ChartSelection>.Filter.Eq(x => x.therapistID, therapistID);
            var charts = collection.Find(filter).FirstOrDefault();

            if (charts != null && charts.chartMap != null)
            {
                Dictionary<string, List<string>> map = charts.chartMap;
                if (map.Keys.Contains(patientID)) //has settings for patient
                {
                    if (map[patientID].Contains(chartName))
                    {
                        map[patientID].Remove(chartName);
                    }
                }
                else
                {
                    map[patientID] = new List<string>();
                    map[patientID].Add(chartName);
                }

                collection.UpdateOne(filter,
                    Builders<ChartSelection>.Update.Set("chartMap", map)
                    );

            }
        }

        public ChartSelection getChartsByTherapistId(long therapistID)
        {
            var filter = Builders<ChartSelection>.Filter.Eq(x => x.therapistID, therapistID);
            var charts = collection.Find(filter).FirstOrDefault();
            return charts;
        }
    }
}