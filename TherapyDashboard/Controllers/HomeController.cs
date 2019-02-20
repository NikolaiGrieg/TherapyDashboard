﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualBasic.FileIO;
using TherapyDashboard.DataBase;
using MongoDB.Driver;
using TherapyDashboard.Services;
using Hl7.Fhir.Model;
using TherapyDashboard.ViewModels;
using TherapyDashboard.Services.AggregationFunctions;
using TherapyDashboard.Services.FlagFunctions;

namespace TherapyDashboard.Controllers
{
    public class HomeController : Controller
    {
        //[Authorize]
        public ActionResult Index()
        {
            FHIRRepository repo = new FHIRRepository();
            List<Patient> patients = repo.getAllPatients();

            repo.updateResources(patients); //TODO consider better options, as an exception is called if this line isnt executed before calculations

            //declare calculation functions
            IAggregationFunction aggFunc = new SumCompareThresholdFunc(1);
            IFlagFunction flagFunc = new MaxDeltaFlagFunc();

            Dictionary<long, string> summaries = repo.getSummaries(aggFunc);
            Dictionary<long, string> flags = repo.getFlags(flagFunc);

            //convert dictionaries to strings, and add to model
            MasterViewModel model = new MasterViewModel();
            model.summaries = new Dictionary<string, string>();
            foreach (var kvp in summaries)
            {
                model.summaries[kvp.Key.ToString()] = kvp.Value;
            }

            model.flags = new Dictionary<string, string>();
            foreach (var kvp in flags)
            {
                model.flags[kvp.Key.ToString()] = kvp.Value;
            }

            model.patientNames = new Dictionary<string, string>();
            foreach (var pat in patients)
            {
                model.patientNames[pat.Id] = pat.Name[0].Given.FirstOrDefault() + " " + pat.Name[0].Family;
            }

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