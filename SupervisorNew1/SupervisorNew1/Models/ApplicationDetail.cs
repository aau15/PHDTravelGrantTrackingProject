using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupervisorNew1.Models
{
    public class ApplicationDetail
    {
        public string studentID { get; set; }
        public string tripID { get; set; }
        public string studentName { get; set; }
        public string email { get; set; }
        public string d1stReg { get; set; }
        public string monthsComp { get; set; }
        public string feePayer { get; set; }
        public string feeLPaid { get; set; }
        public string conferenceName { get; set; }
        public string conferenceURL { get; set; }
        public string cityCountry { get; set; }
        public string paperTitle { get; set; }
        public string author { get; set; }
        public string otherReason { get; set; }
        public List<FundedTrip> allFundedTrips { get; set; }
        public string totalClaimed { get; set; }
        public string totalRemained { get; set; }
        public string claimed12M { get; set; }
        public string remained12M { get; set; }
        public string regFee { get; set; }
        public string regCal { get; set; }
        public string transFee { get; set; }
        public string transCal { get; set; }
        public string accomFee { get; set; }
        public string accomCal { get; set; }
        public string mealFee { get; set; }
        public string mealCal { get; set; }
        public string otherFee { get; set; }
        public string otherCal { get; set; }
        public string totalFee { get; set; }
        public string maxAllowed { get; set; }
        public string dOfSubmission { get; set; }
        public string travelDate { get; set; }
        public string endDate {get;set;}
        public List<Message> messageList { get; set; }
        public string sComment { get; set; }
        public string tComment { get; set; }
        public int role { get; set; }
        public string isSupPay { get; set; }
        public string supPayAmount { get; set; }
        public int status { get; set; }
        public int isFundDispatched { get; set; }
        public int isThereDocument { get; set; }
        public int messageCount { get; set; }
        public string researchAcc { get; set; }
        public string studentEmail { get; set; }
    }
    public class Message
    {
        public string messageId { get; set; }
        public string messageTitle { get;set;}
        public string messageBody { get;set;}
        public string senderName { get;set;}
        public string timeStamp { get;set;}
        public string seenStatus { get; set; }
        public int isYours { get; set; }
    }
    public class FundedTrip
    {
        public string dClaim { get; set; }
        public string amountReceived { get; set; }
        public string conferenceName { get; set; }
        public string dTravel { get; set; }
        public string yTravel { get; set; }
    }

    public class FundsUsedPYearList
    {
        public List<FundsUsedPerYear> fundsUsedPerYearList {get;set;}
    }


    public class FundsUsedPerYear
    {
        public string year { get; set; }
        public int totalFundsUsed { get; set; }
    }
}