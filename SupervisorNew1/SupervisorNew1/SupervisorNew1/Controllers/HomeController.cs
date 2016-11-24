using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using SupervisorNew1.Security;
using SupervisorNew1.Models;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SupervisorNew1.Controllers
{

   
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            RefreshSession();
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            LoginData lg = (LoginData)Session["LoginData"];
             string sql = "";
            if(lg.role == 1)
            {
                sql = "select student.name, student.studentid, traveldate, amountreceived from trip join student on trip.studentid = student.studentid join supervisor on student.supervisorid = supervisor.supervisorid where isfunddispatched = 1 AND student.supervisorid = '" + lg.username + "' order by traveldate asc;";
            }
            else if(lg.role == 2)
            {
                sql = "select student.name, student.studentid, traveldate, amountreceived from trip join student on trip.studentid = student.studentid join supervisor on student.supervisorid = supervisor.supervisorid where isfunddispatched = 1 AND student.tutorid = '" + lg.username + "' order by traveldate asc;";
            }
                    
          
          //  string sql = "select traveldate, amountreceived from trip where isfunddispatched = 1 and studentid = (select studentid from student where supervisorid = '" + lg.username + "') order by traveldate asc;";
          //  ApplicationDetail ad = new ApplicationDetail();

           
          //  ad.allFundedTrips = new List<FundedTrip>();

            //////////// annual funds usage///////
            try
            {
                DataTable dt = new DataBase().RunProcReturn(sql, "table").Tables[0];

                double totalFundsUsed = 0.0;
                FundsUsedPYearList flist = new FundsUsedPYearList();
            
                flist.fundsUsedPerYearList = new List<FundsUsedPerYear>();
              
                for (int i = 0; i < dt.Rows.Count;)
                {
                    FundsUsedPerYear f = new FundsUsedPerYear();
                   
                    f.year = Convert.ToDateTime(dt.Rows[i]["traveldate"].ToString()).Year.ToString();
                    string previousYear = Convert.ToDateTime(dt.Rows[i]["traveldate"].ToString()).Year.ToString();
                    string currentYear = f.year;
                    while (previousYear.Equals(currentYear))
                    {
                        double tempfunds = Convert.ToDouble(f.totalFundsUsed);
                        tempfunds += Convert.ToDouble(dt.Rows[i]["amountreceived"]);
                       
                        f.totalFundsUsed = Convert.ToInt32(tempfunds);
                        totalFundsUsed += Convert.ToDouble(dt.Rows[i]["amountreceived"]);
                     
                        
                        if(i < dt.Rows.Count-1)
                        {
                            i++;
                            currentYear = Convert.ToDateTime(dt.Rows[i]["traveldate"].ToString()).Year.ToString();
                        }
                        else
                        {
                            i++;
                            break;
                        }
                       
                        
                      
                    }

                    
                    
                  
                    flist.fundsUsedPerYearList.Add(f);
                }

                FundsUsedPerYear total = new FundsUsedPerYear();
                total.year = "Total";
                total.totalFundsUsed = Convert.ToInt32(totalFundsUsed);
                flist.fundsUsedPerYearList.Add(total);

                ///////// funds per student ///////////
                DataView dv = dt.DefaultView;
                dv.Sort = "studentid asc";
                DataTable sortedDT = dv.ToTable();
                totalFundsUsed = 0.0;
                flist.fundsUsedPerStudentList = new List<FundsUsedPerStudent>();
                string currentStudentId = "";
                FundsUsedPerStudent fs = new FundsUsedPerStudent();
                for (int i = 0; i < sortedDT.Rows.Count;i++ )
                {
                    
                    string tempStudentid = sortedDT.Rows[i]["studentid"].ToString();
                    if (tempStudentid.Equals(currentStudentId))
                    {      
                       
                        double tempfunds = Convert.ToDouble(fs.totalFundsUsed);
                        tempfunds += Convert.ToDouble(sortedDT.Rows[i]["amountreceived"]);
                        fs.totalFundsUsed = Convert.ToInt32(tempfunds);


                    }
                    else
                    {
                        currentStudentId = tempStudentid;
                        if (i != 0)
                        {
                            flist.fundsUsedPerStudentList.Add(fs);
                        }
                        fs = new FundsUsedPerStudent();
                        fs.totalFundsUsed = Convert.ToInt32(sortedDT.Rows[i]["amountreceived"]);
                        fs.studentName = sortedDT.Rows[i]["name"].ToString();

                    }



                }
                flist.fundsUsedPerStudentList.Add(fs);
                fs = new FundsUsedPerStudent();
                fs.studentName= "Total";
                fs.totalFundsUsed = flist.fundsUsedPerYearList.Last().totalFundsUsed;
                flist.fundsUsedPerStudentList.Add(fs);




                return View(flist);

            }
            catch(Exception)
            {
                return View();
            }


           
        }

        public ActionResult FlotCharts()
        {
            RefreshSession();
              if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");              
            }
            return View("FlotCharts");
        }

        public ActionResult MorrisCharts()
        {
            RefreshSession();
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("MorrisCharts");
        }

        public ActionResult Tables() // review application 
        {
            RefreshSession();
            if (Session["LoginData"] == null) //check login
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            LoginData lg = (LoginData)(Session["LoginData"]);
            string currentUserName = lg.username;//get user id 
            DataBase data = new DataBase(); //create db instance 
            string sql = "";
            if (lg.role == 1)
            {
                sql = "select tripid, status, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where student.supervisorID = '" + currentUserName + "';";
            }
            else if(lg.role ==2)
            {
                sql = "select supervisor.name as supName, status, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid ";
            }

            DataTable  dt = data.RunProcReturn(sql,"table").Tables[0]; //get the needed info related to current user

            AllPreviews aps = new AllPreviews(); //create holder for all application preview
            aps.previewList = new List<ApplicationPreview>();
            aps.role = lg.role;            
            for(int i = 0;i<dt.Rows.Count;i++) //put everything to Viewmodel
            {
                ApplicationPreview ap = new ApplicationPreview();
                ap.tripID = dt.Rows[i]["tripid"].ToString();
                ap.studentName = dt.Rows[i]["sName"].ToString();
                if (lg.role == 2)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                int status = Convert.ToInt32(dt.Rows[i]["status"]);

                if(status == 1)
                {
                    ap.status = "New";
                }
                else if(status == 2)
                {
                    ap.status = "Pending tutor's approval";
                }
                else if(status == 3)
                {
                    ap.status = "Approved by tutor";
                }
                else if (status == 4)
                {
                    ap.status = "Rejected";
                }

                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.operation = 1;
                aps.previewList.Add(ap);
            }

           // data.Close();
           // string studentName = dt.Rows[0]["name"].ToString();
            return View(aps);
        }

        public ActionResult NewApplication()
        {
            RefreshSession();
            if (Session["LoginData"] == null) //check login
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");
            }
            LoginData lg = (LoginData)(Session["LoginData"]);
            string currentUserName = lg.username;//get user id 
            DataBase data = new DataBase(); //create db instance 
            string sql = "";
            if (lg.role == 1)
            {
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 1 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 1 ";
            }

            DataTable dt = data.RunProcReturn(sql, "table").Tables[0]; //get the needed info related to current user

            AllPreviews aps = new AllPreviews(); //create holder for all application preview
            aps.previewList = new List<ApplicationPreview>();
            aps.role = lg.role;
            for (int i = 0; i < dt.Rows.Count; i++) //put everything to Viewmodel
            {
                ApplicationPreview ap = new ApplicationPreview();
                ap.tripID = dt.Rows[i]["tripid"].ToString();
                ap.studentName = dt.Rows[i]["sName"].ToString();
                if (lg.role == 2)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }
            aps.operation = 2;
            // data.Close();
            // string studentName = dt.Rows[0]["name"].ToString();
            return View("Tables",aps);
        }

        public ActionResult PendingApplication()
        {
            RefreshSession();
            if (Session["LoginData"] == null) //check login
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");
            }
            LoginData lg = (LoginData)(Session["LoginData"]);
            string currentUserName = lg.username;//get user id 
            DataBase data = new DataBase(); //create db instance 
            string sql = "";
            if (lg.role == 1)
            {
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 2 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 2 ";
            }

            DataTable dt = data.RunProcReturn(sql, "table").Tables[0]; //get the needed info related to current user

            AllPreviews aps = new AllPreviews(); //create holder for all application preview
            aps.previewList = new List<ApplicationPreview>();
            aps.role = lg.role;
            for (int i = 0; i < dt.Rows.Count; i++) //put everything to Viewmodel
            {
                ApplicationPreview ap = new ApplicationPreview();
                ap.tripID = dt.Rows[i]["tripid"].ToString();
                ap.studentName = dt.Rows[i]["sName"].ToString();
                if (lg.role == 2)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }

            // data.Close();
            // string studentName = dt.Rows[0]["name"].ToString();
            aps.operation = 2;
            return View("Tables", aps);



        }

        public ActionResult ApprovedApplication()
        {
            RefreshSession();
            if (Session["LoginData"] == null) //check login
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");
            }
            LoginData lg = (LoginData)(Session["LoginData"]);
            string currentUserName = lg.username;//get user id 
            DataBase data = new DataBase(); //create db instance 
            string sql = "";
            if (lg.role == 1)
            {
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 3 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 3 ";
            }

            DataTable dt = data.RunProcReturn(sql, "table").Tables[0]; //get the needed info related to current user

            AllPreviews aps = new AllPreviews(); //create holder for all application preview
            aps.previewList = new List<ApplicationPreview>();
            aps.role = lg.role;
            for (int i = 0; i < dt.Rows.Count; i++) //put everything to Viewmodel
            {
                ApplicationPreview ap = new ApplicationPreview();
                ap.tripID = dt.Rows[i]["tripid"].ToString();
                ap.studentName = dt.Rows[i]["sName"].ToString();
                if (lg.role == 2)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }

            // data.Close();
            // string studentName = dt.Rows[0]["name"].ToString();
            aps.operation = 2;
            return View("Tables", aps);

        }

        public ActionResult RejectedApplication()
        {
            RefreshSession();
            if (Session["LoginData"] == null) //check login
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");
            }
            LoginData lg = (LoginData)(Session["LoginData"]);
            string currentUserName = lg.username;//get user id 
            DataBase data = new DataBase(); //create db instance 
            string sql = "";
            if (lg.role == 1)
            {
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 4 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 4 ";
            }

            DataTable dt = data.RunProcReturn(sql, "table").Tables[0]; //get the needed info related to current user

            AllPreviews aps = new AllPreviews(); //create holder for all application preview
            aps.previewList = new List<ApplicationPreview>();
            aps.role = lg.role;
            for (int i = 0; i < dt.Rows.Count; i++) //put everything to Viewmodel
            {
                ApplicationPreview ap = new ApplicationPreview();
                ap.tripID = dt.Rows[i]["tripid"].ToString();
                ap.studentName = dt.Rows[i]["sName"].ToString();
                if (lg.role == 2)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }

            // data.Close();
            // string studentName = dt.Rows[0]["name"].ToString();
            aps.operation = 2;
            return View("Tables", aps);
        }

        public ActionResult Forms()
        {
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("Forms");
        }

        public ActionResult Panels()
        {
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("Panels");
        }

        public ActionResult Buttons()
        {
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("Buttons");
        }

        public ActionResult Notifications()
        {
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("Notifications");
        }

        public ActionResult Typography()
        {
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("Typography");
        }

        public ActionResult Icons()
        {
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("Icons");
        }

        public ActionResult Grid()
        {
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }
            return View("Grid");
        }
        [HttpPost]
        public ActionResult UpdateSeen(string id)
        {
            string sql = "update message set seenbysup = 1 where messageid =" + id + ";";
            DataBase data = new DataBase();
            data.RunProc(sql);
            return null;
        }

        [HttpPost]
        public ActionResult Download(string tripid)
        {

                RefreshSession();
                DataBase data = new DataBase();
                string sql = "select doctitle, docbody,doctype from documents where tripid =" + tripid + ";";
                DataTable dt = data.RunProcReturn(sql, "table").Tables[0];
                byte[] file = (byte[])(dt.Rows[0]["docbody"]);

                MemoryStream ms = new MemoryStream(file);
                //Response.ContentType = "application/" + dt.Rows[0]["doctype"].ToString();
                //Response.AddHeader("Content-Disposition",
                //               "attachment; filename=" + dt.Rows[0]["doctitle"].ToString() + ";");










                return new FileContentResult(file, "force/pdf");
              //  return File(file, System.Net.Mime.MediaTypeNames.Application.Octet, dt.Rows[0]["doctitle"].ToString() + ".pdf");
            //    return new FileStreamResult(ms, "application/pdf");
        }

        public ActionResult Blank(string id, string viewBagMsg) 
        {
            RefreshSession();
            if (Session["LoginData"] == null)
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");              
            }

            

            LoginData lg = (LoginData)Session["LoginData"];
            string sql = "";
            if(lg.role==5)
             sql = "select email, trip.studentid,trip.tripid,name, email, maxclaim,student.maxclaim12m,  monthscompleted, dofreg, feepayer, feelpaid, conferencename, conferenceurl, city, country, papertitle, author, purpose," 
            +" status, costoftrip,traveldate,enddate,submissiondate, regfeecal,regfee,transfeecal,transfee,accfeecal,accfee,mealfeecal,mealfee,otherfeecal,otherfee,scomment, from student join trip on student.studentid = trip.studentid join ecost on trip.tripid = ecost.tripid where trip.tripid ="+id+";";
            else
                sql = "select email, isfunddispatched,oktofund, maxtofund, tcomment, scomment, trip.studentid,trip.tripid,name, email, maxclaim,student.maxclaim12m,  monthscompleted, dofreg, feepayer, feelpaid, conferencename, conferenceurl, city, country, papertitle, author, purpose,"
                + " status, costoftrip,traveldate,enddate,submissiondate, regfeecal,regfee,transfeecal,transfee,accfeecal,accfee,mealfeecal,mealfee,otherfeecal,otherfee from student join trip on student.studentid = trip.studentid join ecost on trip.tripid = ecost.tripid where trip.tripid =" + id + ";";

            try
            {

                DataBase data = new DataBase(); //create db instance 
                DataTable dt = data.RunProcReturn(sql, "table").Tables[0];//get all needed data except the past travel record and put into datatable 
                string studentid = dt.Rows[0]["studentid"].ToString();
                string sqlPastTravel = "select traveldate, submissiondate,amountreceived,conferencename from trip where status = 3 and studentid ='" + studentid + "';"; //find all records which has the studentid and approved by tutor (means funds dispatched)
                DataTable dtPastTravel = data.RunProcReturn(sqlPastTravel, "table").Tables[0];
                string sqlMessage = "select messageid,messagetitle, messagebody,  sendername, timestamp, seenbysup, seenbystudent, seenbytut from message where tripid ='" + id + "' order by timestamp desc;";
                DataTable dtMessage = data.RunProcReturn(sqlMessage, "table").Tables[0];




                ApplicationDetail ad = new ApplicationDetail();
                ad.isThereDocument = Convert.ToInt32((data.RunProcReturn("select count(tripid) as c from documents where tripid =" + id + ";","table").Tables[0]).Rows[0]["c"]);
                ad.messageList = new List<Message>();
                for (int i = 0; i < dtMessage.Rows.Count; i++)
                {
                    Message m = new Message();
                    m.messageBody = dtMessage.Rows[i]["messagebody"].ToString();
                    m.messageTitle = dtMessage.Rows[i]["messagetitle"].ToString();
                    m.senderName = dtMessage.Rows[i]["sendername"].ToString();
                    m.timeStamp = dtMessage.Rows[i]["timestamp"].ToString();
                    m.messageId = dtMessage.Rows[i]["messageid"].ToString();
                    if (!m.senderName.Equals(lg.name))
                    {
                        m.isYours = 0;
                    }
                    else
                    {
                        m.isYours = 1;
                    }

                    if (Convert.ToInt32(dtMessage.Rows[i]["seenbysup"]) == 0)
                    {
                        m.seenStatus = "New";
                    }
                    else
                    {
                        m.seenStatus = "Seen";
                    }
                    ad.messageList.Add(m);
                    ad.messageCount++;
                }

                if (lg.role == 2 || lg.role == 1 && Convert.ToInt32(dt.Rows[0]["status"]) != 1)
                {
                    ad.isFundDispatched = Convert.ToInt32(dt.Rows[0]["isfunddispatched"]);
                    ad.sComment = dt.Rows[0]["scomment"].ToString();
                    if (lg.role == 1)
                    {
                        ad.role = 1;
                    }
                    else if(lg.role == 2)
                    {
                        ad.role = 2;
                    }
                  
                    if (dt.Rows[0]["oktofund"].ToString().Equals("1"))
                    {
                        ad.isSupPay = "Yes";
                        ad.supPayAmount = dt.Rows[0]["maxtofund"].ToString();
                    }
                    else
                    {
                        ad.isSupPay = "No";
                        ad.supPayAmount = "NA";
                    }
                }
                else ad.role = 1;



                ad.studentID = dt.Rows[0]["studentid"].ToString();
                ad.tripID = dt.Rows[0]["tripid"].ToString();
                ad.studentName = dt.Rows[0]["name"].ToString();
                ad.email = dt.Rows[0]["email"].ToString();
                ad.totalRemained = dt.Rows[0]["maxclaim"].ToString();
                ad.remained12M = dt.Rows[0]["maxclaim12m"].ToString();
                ad.d1stReg = dt.Rows[0]["dofreg"].ToString();
                ad.feePayer = dt.Rows[0]["feepayer"].ToString();
                ad.feeLPaid = dt.Rows[0]["feelpaid"].ToString();
                ad.conferenceName = dt.Rows[0]["conferencename"].ToString();
                ad.conferenceURL = dt.Rows[0]["conferenceurl"].ToString();
                ad.cityCountry = dt.Rows[0]["city"].ToString() + ", " + dt.Rows[0]["country"].ToString();
                ad.paperTitle = dt.Rows[0]["papertitle"].ToString();
                ad.author = dt.Rows[0]["author"].ToString();
                ad.otherReason = dt.Rows[0]["purpose"].ToString();
                ad.status = Convert.ToInt32(dt.Rows[0]["status"]);
                ad.totalFee = dt.Rows[0]["costoftrip"].ToString();
                ad.travelDate = dt.Rows[0]["traveldate"].ToString();
                ad.endDate = dt.Rows[0]["enddate"].ToString();
                ad.dOfSubmission = dt.Rows[0]["submissiondate"].ToString();
                ad.regCal = dt.Rows[0]["regfeecal"].ToString();
                ad.regFee = dt.Rows[0]["regfee"].ToString();
                ad.transCal = dt.Rows[0]["transfeecal"].ToString();
                ad.transFee = dt.Rows[0]["transfee"].ToString();
                ad.accomCal = dt.Rows[0]["accfeecal"].ToString();
                ad.accomFee = dt.Rows[0]["accfee"].ToString();
                ad.mealCal = dt.Rows[0]["mealfeecal"].ToString();
                ad.mealFee = dt.Rows[0]["mealfee"].ToString();
                ad.otherCal = dt.Rows[0]["otherfeecal"].ToString();
                ad.otherFee = dt.Rows[0]["otherfee"].ToString();
                ad.monthsComp = dt.Rows[0]["monthscompleted"].ToString();
                ad.allFundedTrips = new List<FundedTrip>();
                ad.tComment = dt.Rows[0]["tcomment"].ToString();
                ad.studentEmail = dt.Rows[0]["email"].ToString();
                double totalFundsUsed = 0.0;
                double totalFundsUsed12M = 0.0;
                for (int i = 0; i < dtPastTravel.Rows.Count; i++)
                {
                    FundedTrip ft = new FundedTrip();
                    ft.amountReceived = dtPastTravel.Rows[i]["amountreceived"].ToString();
                    totalFundsUsed += Convert.ToDouble(ft.amountReceived);

                    ft.conferenceName = dtPastTravel.Rows[i]["conferencename"].ToString();
                    ft.dClaim = dtPastTravel.Rows[i]["submissiondate"].ToString();
                    ft.dTravel = dtPastTravel.Rows[i]["traveldate"].ToString();

                    DateTime dTravel = Convert.ToDateTime(ft.dTravel);
                    if (dTravel.AddYears(1) >= (Convert.ToDateTime(ad.travelDate)))
                    {
                        totalFundsUsed12M += Convert.ToDouble(ft.amountReceived);
                    }
                    ad.allFundedTrips.Add(ft);
                }
                ad.totalClaimed = totalFundsUsed.ToString();
                ad.claimed12M = totalFundsUsed12M.ToString();
                double totalRemained = 3000 - totalFundsUsed;
                double totalRemained12M = 2000 - totalFundsUsed12M;
                ad.totalRemained = Convert.ToString(totalRemained);
                ad.remained12M = Convert.ToString(totalFundsUsed12M);
                double totalFee = Convert.ToDouble(dt.Rows[0]["costoftrip"]);
                ad.maxAllowed = Convert.ToString(Math.Max(0, Min(totalFee, 1250, totalRemained, totalRemained12M)));

                Session["ApplicationDetail"] = ad;



                if (TempData.ContainsKey("redirect"))
                {
                    
                     ViewBag.message = viewBagMsg;
                     TempData.Remove("redirect");

                    //ViewData.Clear();
                    //it means that you reload this method or you click on the link
                    //be sure to use unique key by request since TempData is use for the next request 
                }
              
                

                

                

                
                return View(ad);

            }
            catch (Exception )
            {
                return RedirectToAction("Index");
            }
        }

        //custom min method
        public static double Min(params double[] values)
        {
            return Enumerable.Min(values);
        }

        public static string randomString()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        [HttpPost] 
        public ActionResult Blank(string studentEmail, string studentname, string studentId,string tripid, string operation, string maxFundInput, string researchAccInput, string commentInput, string messageTitle, string messageBody,  FormCollection frm)
        {
            RefreshSession();
            if (Session["LoginData"] == null){
                return RedirectToAction("Login");
            }


            TempData["redirect"] = true;
            
            LoginData lg = (LoginData)Session["LoginData"];
            int applicationStatus = 1;
            int oktoFund = 0;
            if (lg.role == 1)//supervisor
            {
               
                if (frm["optionsRadiosInline"] == "option1")
                {
                    oktoFund = 1;
                }
                else if (frm["optionsRadiosInline"] == "option2")
                {
                    oktoFund = 2;
                }
            }

            if (operation.Equals("send"))
            {
                if (messageBody == "" )
                {
                    ViewBag.message = "Message can not be empty";
                }
                else
                {
                    string senderName = lg.name;
                    string sqlInsertMessage = "";
                    if (lg.role == 1)
                        sqlInsertMessage = "insert into message (sendername, studentid, tutorid, tripid, messagebody, messagetitle, timestamp,seenbysup,seenbystudent,seenbytut) values ('" +senderName +"','" + studentId + "','t1234'," + tripid + ",'" + messageBody + "','" + messageTitle + "', NOW()," + "1,0,0);";
                    else if(lg.role ==2)
                    {
                        sqlInsertMessage = "insert into message (sendername, studentid, tutorid, tripid, messagebody, messagetitle, timestamp,seenbysup,seenbystudent,seenbytut) values ('" + senderName + "','" + studentId + "','t1234'," + tripid + ",'" + messageBody + "','" + messageTitle + "', NOW()," + "0,0,1);";
                    }

                    try
                    {
                        DataBase data = new DataBase();
                        data.RunProc(sqlInsertMessage); //update the database 
                        ViewBag.message = "Message sent";
                       
                    }
                    catch (Exception)
                    {
                        ViewBag.message = "Message failed to send";
                    }
                }

                ApplicationDetail ad = (ApplicationDetail)Session["ApplicationDetail"];
                Message m = new Message();
                m.isYours = 1;
                m.messageBody = messageBody;
                m.messageTitle =messageTitle;
                m.seenStatus = "Seen";
                m.senderName = lg.name;
                m.timeStamp = System.DateTime.Now.ToString();
                ad.messageList.Insert(0, m);
                ad.messageCount++;
                Session["ApplicationDetail"] = ad;
                return RedirectToAction("Blank", new { id = tripid, viewBagMsg = "Message sent" });
            }
            else if(operation!="dispatchFund")
            {
                if(operation.Equals("download"))
                {
                    DataBase data = new DataBase();
                    string sql = "select doctitle, docbody,doctype from documents where tripid =" + tripid + ";";
                    DataTable dt = data.RunProcReturn(sql, "table").Tables[0];
                    byte[] file = (byte[])(dt.Rows[0]["docbody"]);

                    MemoryStream ms = new MemoryStream(file);
                    //Response.ContentType = "application/" + dt.Rows[0]["doctype"].ToString();
                    //Response.AddHeader("Content-Disposition",
                    //               "attachment; filename=" + dt.Rows[0]["doctitle"].ToString() + ";");









                    return File(file, "application/" + dt.Rows[0]["doctype"].ToString(), dt.Rows[0]["doctitle"].ToString() + "." + dt.Rows[0]["doctype"].ToString());
                   //  FileContentResult fcr = new FileContentResult(file, "application/pdf");
                     
                     
                }

                if(operation.Equals("reset"))
                {
                    DataBase data = new DataBase();
                    string sql = "";
                    if(lg.role == 1)
                    {
                         sql = "update trip set maxtofund =0,  researchacc='', status=1, oktofund =0, scomment ='' where tripid =" + tripid + ";";
                    }
                    else if (lg.role ==2)
                    {
                        sql = "update trip set status = 2, tcomment = '' where tripid =" + tripid + ";";
                    }
                    try
                    {
                        data.RunProc(sql);

                        ViewBag.message = "Decision reverted succesfully";
                       

                    }
                    catch (Exception)
                    {
                        ViewBag.message = "Failed to revert decision, please check your database connection";
                        
                    }
                    return RedirectToAction("Blank", new { id = tripid, viewBagMsg = ViewBag.message });
                }


                if (operation.Equals("approve"))
                {
                    if (lg.role == 2)
                    {
                        applicationStatus = 3;
                    }
                    else
                        applicationStatus = 2;
                }
                else if (operation.Equals("reject"))
                {
                    if(lg.role == 1)
                    {
                        applicationStatus = 4;
                    }
                    else if (lg.role == 2)
                    {
                        applicationStatus = 5;
                    }
                    
                }
                else if (operation.Equals("message"))
                {
                    applicationStatus = 1;
                }


            
                    if (!operation.Equals("message"))
                    {
                        if (lg.role == 1)
                        {
                            if (applicationStatus == 2 && (String.IsNullOrEmpty(maxFundInput) || String.IsNullOrEmpty(researchAccInput))) //if approve but required detail not entered
                            {
                                ViewBag.message = "Max funding column and research account column can not be empty!";
                                if (Session["ApplicationDetail"] != null)
                                {
                                    //ApplicationDetail ad = (ApplicationDetail)Session["ApplicationDetail"];
                                    return RedirectToAction("Blank", new { id = tripid, viewBagMsg = ViewBag.message });
                                }
                                else
                                    return RedirectToAction("Index");

                            }
                        }
                        
                        if ((applicationStatus == 4 || applicationStatus==5) && String.IsNullOrEmpty(commentInput)) //reject but no comment
                        {
                            ViewBag.message = "Comment can not be empty!";
                            if (Session["ApplicationDetail"] != null)
                            {
                              //  ApplicationDetail ad = (ApplicationDetail)Session["ApplicationDetail"];
                                return RedirectToAction("Blank", new { id = tripid, viewBagMsg = ViewBag.message });
                            }
                            else
                                return RedirectToAction("Index");
                        }
                        else  //approval or reject
                        {
                            DataBase data = new DataBase();
                            string sql = "";
                            if (lg.role == 1)
                            {
                                if (applicationStatus == 2 )
                                {
                                    sql = "update trip set maxtofund =" + maxFundInput + ",  researchacc='" + researchAccInput + "', status=" + applicationStatus + ", oktofund =" + oktoFund + ", scomment ='" + commentInput + "' where tripid =" + tripid + ";";
                                }
                                else if (applicationStatus == 4 )
                                {
                                    sql = "update trip set status = " + applicationStatus + ",scomment ='" + commentInput + "' where tripid =" + tripid + ";";
                                }
                            }
                            else if (lg.role == 2)
                            {
                                if (String.IsNullOrEmpty(commentInput))
                                {
                                    sql = "update trip set status = " + applicationStatus + " where tripid =" + tripid + ";";
                                }
                                else
                                {
                                    sql = "update trip set status = " + applicationStatus + ", tcomment = '" + commentInput + "' where tripid =" + tripid + ";";
                                }
                            }
                            try
                            {
                                data.RunProc(sql); //update the database 
                                ViewBag.message = "Update record successful";


                                Task.Run(() => SendEmail(studentname, tripid, studentEmail, applicationStatus.ToString()));
                                
                            }
                            catch (Exception)
                            {
                                ViewBag.message = "Failed to update record!";
                            }
                            if (Session["ApplicationDetail"] != null)
                            {
                                ApplicationDetail ad = (ApplicationDetail)Session["ApplicationDetail"];

                                if(ad.role == 1)
                                {
                                    ad.supPayAmount = maxFundInput;
                                    ad.researchAcc = researchAccInput;
                                    ad.sComment = commentInput;

                                    if (oktoFund == 1)
                                    {
                                        ad.isSupPay = "Yes";
                                        ad.status = applicationStatus;
                                       
                                    }
                                    else
                                        ad.isSupPay = "No";
                                        ad.status = applicationStatus;
                                      
                                    }
                                    
                                else if(ad.role == 2)
                                {
                                    ad.status = applicationStatus;
                                    ad.tComment = commentInput;
                                 }

                                return RedirectToAction("Blank", new { id = tripid, viewBagMsg = ViewBag.message });
                            }
                            else
                                return RedirectToAction("Index");

                        }
                       

                    }



                    return RedirectToAction("Index");
            }
            else if(operation == "dispatchFund")
            {
                DataBase data = new DataBase();
                string sql = "update trip set isfunddispatched= 1 where tripid =" + tripid + ";";
                try
                {
                    data.RunProc(sql);
                    ViewBag.message = "Record updated successful";
                    
                }
                catch(Exception)
                {
                    ViewBag.message = "Failed to update record";
                }

                if (Session["ApplicationDetail"] != null)
                {
                    ApplicationDetail ad = (ApplicationDetail)Session["ApplicationDetail"];
                    ad.isFundDispatched = 1;
                    Session["ApplicationDetail"] = ad;
                    return View(ad);
                }
                else
                    return RedirectToAction("Index");


            }
            return RedirectToAction("Index");
        }

        public ActionResult Login(string username, string password, FormCollection frm)
        {

            if (Session["LoginData"] != null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                 string name = AuthenticationService.SignIn(username, password, true);
                if (!string.IsNullOrEmpty(name))
                {
                    LoginData lg = null;
                    if (name.Equals("Sophia"))
                    {
                         lg = new LoginData(username, 1, name); //static data for now, supervisor == 1
                    }
                    else if(name.Equals("Larry"))
                    {
                         lg = new LoginData(username, 2, name); //supervisor == 2
                      
                    }
                    
                    Session["LoginData"] = lg; //login data

                    //  string role = HttpContext.Identity.AuthenticationType;

                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Login");
                }

            }

           
        }

        public ActionResult Logout()
        {
            AuthenticationService.SignOut();
            Session["LoginData"] = null;
            return RedirectToAction("Login");
        }

        public  void  SendEmail (string studentName, string tripid, string studentEmail, string msg)
        {
            string statusString = "";
            string statusMsgFront = "";
            int status = Convert.ToInt32(msg);

            if(status == 2)
            {
                statusString = "approved by your supervisor and is now pending tutor's approval.";
                statusMsgFront = "Congratulations! ";
            }
            else if (status ==3)
            {
                statusString = "approved by both your supervisor and tutor.";
                statusMsgFront = "Congratulations! ";
            }
            else if (status ==4)
            {
                statusString = "rejected by your supervisor. Please contact your supervisor for more information.";
                statusMsgFront = "Unfortunately, ";
            }
            else if (status ==5)
            {
                statusString = "rejected by your tutor. Please contact your tutor for more information.";
                statusMsgFront = "Unfortunately, ";

            }
          //  int isSuccess = 0;
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp-mail.outlook.com");

                mail.From = new MailAddress("phdtravelgrant-g3@hotmail.com");
                mail.To.Add(studentEmail);
                mail.Subject = "Status of your application has been changed";
                mail.IsBodyHtml = true;
                //mail.Body = "<font size = \"9\"><b>PHD EASY TRAVEL </b></font> <br />  Your account Password have been reset! <br />  <br />  <br /> " +
                //"<font size = \"5\">Login Details: </font> <br />" +
                //"<b>Username:</b> <i>" + username + "</i> <br />" +
                //"<b>Password:</b> <i>" + randomPass + "</i> <br /> <br />  <br />  <br />  <br />  <br />" +

                mail.Body = "<i>Dear " + studentName + ":<i> <br /><br />" +
                           statusMsgFront+ "<i>Your application #" + tripid + " has been <i>"+statusString+" <br/><br/> " +
                "<i>Remainder from PHD Travel Easy team<i><br /> <br />  <br />  <br />  <br />" +            

                "<i> This is an auto generated message</i> <br />";



                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("phdtravelgrant-g3@hotmail.com", "Phdtravel.0");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

            }
            catch (Exception)
            {
                
            }
          //  isSuccess = 1;


           


        }

        public void RefreshSession()
        {
            if (Session["LoginData"] != null)
            {
                LoginData lg = (LoginData)Session["LoginData"];
                Session.Remove("LoginData");
                Session["LoginData"] = lg;
            }
        }



    }
}