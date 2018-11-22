using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TherapyDashboard.Models;
using TherapyDashboard.ViewModels;
using Microsoft.VisualBasic.FileIO;
using TherapyDashboard.DataBase;
using MongoDB.Driver;

namespace TherapyDashboard.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {

            //TODO refactor out, and apply female names too
            //parse names
            var path = AppDomain.CurrentDomain.BaseDirectory + "/Data/Names/SSB_names_male.csv";
            var names = new List<string>();
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { ";" });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    if (fields[0].Any(char.IsDigit))
                    {
                        var name = fields[0].Split(' ')[1];
                        names.Add(name);
                    }
                }
            }


            //init models
            MongoDBConnection repo = new MongoDBConnection();
            //var bill = repo.Patients.AsQueryable().First();

            MultiPatientViewModel model = new MultiPatientViewModel();
            
            List<Patient> patients = new List<Patient>();
            for (int i = 0; i < 20; i++)
            {
                var pat = Patient.createSimulated();
                var simName = names[i] + " " + names[i + 20] + "sen";
                pat.name = simName;
                patients.Add(pat);
            }
            model.patients = patients;
            

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}