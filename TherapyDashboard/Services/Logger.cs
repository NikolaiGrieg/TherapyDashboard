using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Services
{
    public class Logger
    {
        string path;
        public Logger()
        {
            this.path = HttpContext.Current.Server.MapPath("~/Data/Log.txt");
        }

        public void logString(string description, string content)
        {

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string datestr = unixTimestamp.ToString();

            using (StreamWriter writer = new StreamWriter(this.path, true))
            {
                try
                {
                    writer.WriteLine(datestr + ";;" + description + ";;" + content);
                }
                catch (Exception e)
                {
                    writer.WriteLine(datestr + ";;" + "error" + ";;" + e);
                }
                
            }
        }

        public void logTimeSpan(string description, TimeSpan ts)
        {
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            logString(description, elapsedTime);
        }
    }
}