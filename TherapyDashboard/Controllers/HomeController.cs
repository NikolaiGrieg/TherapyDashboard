using System;
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
using TherapyDashboard.Services.WarningFunctions;

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
            IAggregationFunction aggFunc = new SumDeltaThresholdSingleQRFunc(1, "42220");
            IFlagFunction flagFunc = new MaxDeltaFlagFunc();
            IWarningFunction warningFunc = new DeltaThresholdWarningFunc(1);

            Dictionary<long, string> summaries = repo.getSummaries(aggFunc);
            Dictionary<long, List<string>> flags = repo.getFlags(flagFunc); //todo handle multiple flags
            Dictionary<long, List<string>> warnings = repo.getWarnings(warningFunc);

            //convert dictionaries to strings, and add to model, TODO extract this to method
            MasterViewModel model = new MasterViewModel();
            model.summaries = new Dictionary<string, string>();
            foreach (var kvp in summaries)
            {
                model.summaries[kvp.Key.ToString()] = kvp.Value;
            }

            model.flags = new Dictionary<string, List<string>>();
            foreach (var kvp in flags)
            {
                model.flags[kvp.Key.ToString()] = kvp.Value;
            }

            model.warnings = new Dictionary<string, List<string>>();
            if (warnings != null)
            {
                foreach (var kvp in warnings)
                {
                    model.warnings[kvp.Key.ToString()] = kvp.Value;
                }
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