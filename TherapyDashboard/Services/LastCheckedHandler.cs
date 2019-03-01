using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TherapyDashboard.DataBase;
using TherapyDashboard.Models;

namespace TherapyDashboard.Services
{
    public class LastCheckedHandler
    {
        private LastCheckedRepository repo;

        public LastCheckedHandler()
        {
            repo = new LastCheckedRepository();
        }

        public void updatePatientChecked(long therapistID, long patientID)
        {
            repo.upsertSingleEntry(therapistID, patientID.ToString(), DateTime.Now);
        }
        
        public LastCheckedMap readPatientMap(long therapistID)
        {
            return repo.getMapByTherapistId(therapistID);
        }
    }
}