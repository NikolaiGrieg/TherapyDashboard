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

        public void upsertSingleEntry(long therapistID, string chartName)
        {
            //find if exists
            var filter = Builders<ChartSelection>.Filter.Eq(x => x.therapistID, therapistID);
            var charts = collection.Find(filter).FirstOrDefault();

            //insert
            if (charts != null && charts.chartNames != null)
            {
                List<string> chartList = charts.chartNames;
                chartList.Add(chartName);

                collection.UpdateOne(filter,
                    Builders<ChartSelection>.Update.Set("chartNames", chartList)
                    );

            }
            else
            {
                ChartSelection entry = new ChartSelection();
                entry.therapistID = therapistID;
                entry.chartNames = new List<string>();
                entry.chartNames.Add(chartName);
                collection.InsertOne(entry);
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