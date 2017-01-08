using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupervisorNew1.Models
{
    public class AllPreviews
    {
        public List<ApplicationPreview> previewList { get; set; }
        public int role;
        public int operation;
        // 1 == all application 2 == new application 3 == pending application
        // 4 == approved application 5 == rejected application 6 == new message
        // 7 == new document
    }
    public class ApplicationPreview
    {
        public string SupervisorName;
        public string tripID;
        public string studentName;
        public string conferenceName;
        public string cityCountry;
        public string travelDate;
        public string endDate;
        public string cost;
        public string status;
        public string currency;
    }
}