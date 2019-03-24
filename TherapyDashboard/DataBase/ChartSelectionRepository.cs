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

        public void upsertSingleEntry(long therapistID, string patientID, string chartName, string chartType)
        {
            //find if exists
            var filter = Builders<ChartSelection>.Filter.Eq(x => x.therapistID, therapistID);
            var charts = collection.Find(filter).FirstOrDefault();

            //insert
            if (charts != null && charts.chartMap != null)
            {
                Dictionary<string, Dictionary<string, string>> map = charts.chartMap;
                if (map.Keys.Contains(patientID)) //has settings for patient
                {
                    if (!map[patientID].Keys.Contains(chartName))
                    {
                        map[patientID][chartName] = chartType;
                    }
                    
                }
                else
                {
                    map[patientID] = new Dictionary<string, string>();
                    map[patientID][chartName] = chartType;
                }

                collection.UpdateOne(filter,
                    Builders<ChartSelection>.Update.Set("chartMap", map)
                    );

            }
            else
            {
                ChartSelection entry = new ChartSelection();
                entry.therapistID = therapistID;
                entry.chartMap = new Dictionary<string, Dictionary<string, string>>();
                entry.chartMap[patientID] = new Dictionary<string, string>();
                entry.chartMap[patientID][chartName] = chartType;
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
                Dictionary<string, Dictionary<string, string>> map = charts.chartMap;
                if (map.Keys.Contains(patientID)) //has settings for patient
                {
                    if (map[patientID].Keys.Contains(chartName))
                    {
                        map[patientID].Remove(chartName);

                        collection.UpdateOne(filter,
                        Builders<ChartSelection>.Update.Set("chartMap", map)
                        );
                    }
                }
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