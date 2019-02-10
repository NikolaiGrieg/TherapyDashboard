using Hl7.Fhir.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TherapyDashboard.Models
{
    [BsonIgnoreExtraElements] //doesnt seem to cascade
    public class PatientData
    {
        public ObjectId id { get; set; }
        public long fhirID { get; set; }
        public List<QuestionnaireResponse> QRs { get; set; }


        public PatientData(long _fhirID, List<QuestionnaireResponse> _QRs)
        {
            this.fhirID = _fhirID;
            this.QRs = _QRs;
        }
    }
}