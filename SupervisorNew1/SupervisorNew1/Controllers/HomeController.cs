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
using System.Globalization;
using ICSharpCode;
using Excel;
using Ionic.Zip;


namespace SupervisorNew1.Controllers
{

   
    public class HomeController : Controller
    {
         

        //controller for loading of index page(home page)
        //deals with retrieving data from the database for notification panel and funds usage chart
        public ActionResult Index()
        {
            RefreshSession();

            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login"); 
            }

            LoginData lg = (LoginData)Session["LoginData"];
            string sql = "";
            string sqlAppSize = "";
            string sqlNewMsg = "";
            string sqlNewDoc = "";

            //use different queries for different user role
            //get funds usage and various category metrics from database 
            if(lg.role == 1)
            {
                sql = "select student.name, student.studentid, traveldate, month(traveldate) as mon, year(traveldate) as y, amountreceived from trip join student on trip.studentid = student.studentid join supervisor on student.supervisorid = supervisor.supervisorid where isfunddispatched = 1 AND student.supervisorid = '" + lg.username + "' order by traveldate asc;";
            }
            else if(lg.role == 2)
            {
                sql = "select supervisor.name as supname, student.supervisorid, student.name, student.studentid, traveldate, month(traveldate) as mon,year(traveldate) as y, amountreceived from trip join student on trip.studentid = student.studentid join supervisor on student.supervisorid = supervisor.supervisorid where isfunddispatched = 1 AND student.tutorid = '" + lg.username + "' order by traveldate asc;";
                
            }
            else if (lg.role ==3)
            {
                sql = "select supervisor.name as supname, student.supervisorid, student.name, student.studentid, traveldate, month(traveldate) as mon,year(traveldate) as y, amountreceived from trip join student on trip.studentid = student.studentid join supervisor on student.supervisorid = supervisor.supervisorid where isfunddispatched = 1 order by traveldate asc;";
            }
            
            //get size of new application for supervisor and admin, size of pending application for tutor
            if(lg.role == 1 )
            {
                sqlAppSize = "select count(tripid) as size from trip join student on trip.studentid = student.studentid where status = 1 and supervisorid  ='" + lg.username + "' ;";
            }
            else if(lg.role == 2)
            {
                sqlAppSize = "select count(tripid) as size from trip join student on trip.studentid = student.studentid where status = 2 and tutorid = '" + lg.username + "';";
            }
            else if(lg.role == 3)
            {
                sqlAppSize = "select count(tripid) as size from trip join student on trip.studentid = student.studentid where status = 1 ";
            }
          

            //get size of unread message 
            if(lg.role == 1 )
            {
                sqlNewMsg = "select count(messageid) as size from message where supervisorid ='"+ lg.username+"' and seenbysup = 0";
            }
            else if(lg.role == 2)
            {
                sqlNewMsg = "select count(messageid) as size from message where tutorid = '" + lg.username + "' and seenbytut =0";
            }
            
            //get size of un-downloaded document
            if(lg.role == 1)
            {
                sqlNewDoc = "select count(docid) as size from documents join trip on documents.tripid = trip.tripid join student on trip.studentid = student.studentid where student.supervisorid ='" + lg.username + "' and documents.statusSup = 0";
            }
            else if (lg.role == 2)
            {
                sqlNewDoc = "select count(docid) as size from documents join trip on documents.tripid = trip.tripid join student on trip.studentid = student.studentid where student.tutorid ='" + lg.username + "' and documents.statusTut = 0";
            }
         
          

            //////////// annual funds usage///////
            try
            {
                DataBase db = new DataBase();
                DataTable dt = db.RunProcReturn(sql, "table").Tables[0];
                
                double totalFundsUsed = 0.0;
                FundsUsedPYearList flist = new FundsUsedPYearList();
                flist.role = lg.role;            
                flist.newAppSize = Convert.ToString(db.RunProcReturn(sqlAppSize, "table").Tables[0].Rows[0]["size"]);

                try
                {
                    flist.newMsgSize = Convert.ToString(db.RunProcReturn(sqlNewMsg, "table").Tables[0].Rows[0]["size"]);
                }
                catch(Exception)
                {
                    flist.newMsgSize = "0";
                }

                try
                {
                    flist.newDocSize = Convert.ToString(db.RunProcReturn(sqlNewDoc, "table").Tables[0].Rows[0]["size"]);
                }
                catch(Exception)
                {
                    flist.newDocSize = "0";
                }

                flist.fundsUsedPerYearList = new List<FundsUsedPerYear>();
              
                // the logic is to set the first element as the current ultilized element, then traverse the list 
                // and add the funds used to the previous bulk of element as long as the year of the funds usage is the same
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



                // the logic is to set the first element as the current ultilized element, then traverse the list 
                // and add the funds used to the previous bulk of element as long as the student id is the same
                ///////// funds per student ///////////
                DataView dv = dt.DefaultView;
                dv.Sort = "studentid asc";
                DataTable sortedDT = dv.ToTable();
                totalFundsUsed = 0.0;
                flist.fundsUsedPerStudentList = new List<FundsUsedPerStudent>();
                string currentId = "";
                FundsUsedPerStudent fs = new FundsUsedPerStudent();
                
                for (int i = 0; i < sortedDT.Rows.Count;i++ )
                {
                    
                    string tempStudentid = sortedDT.Rows[i]["studentid"].ToString();
                    if (tempStudentid.Equals(currentId))
                    {      
                       
                        double tempfunds = Convert.ToDouble(fs.totalFundsUsed);
                        tempfunds += Convert.ToDouble(sortedDT.Rows[i]["amountreceived"]);
                        fs.totalFundsUsed = Convert.ToInt32(tempfunds);


                    }
                    else
                    {
                        currentId = tempStudentid;
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
                flist.studentCount = flist.fundsUsedPerStudentList.Count;
                fs = new FundsUsedPerStudent();
                fs.studentName= "Total";
                fs.totalFundsUsed = flist.fundsUsedPerYearList.Last().totalFundsUsed;
                flist.fundsUsedPerStudentList.Add(fs);



                //use list of list for this chart, first list checks the month and second list checks the student
                /////////////////// Student monthly chart/////////////////////////////////
                flist.fundsUsedPerStudentPerMonthList = new List<studentMonthlyList>();
                dv.Sort = "studentid, y, mon asc";
                sortedDT = dv.ToTable();
                string currentStudentID = "";
                string currentMonth = "";
                studentMonthlyList sMList = new studentMonthlyList();
                sMList.fStudentMonthlyList = new List<FundsUsedPerStudentMonth>();
                FundsUsedPerStudentMonth fperMonth = new FundsUsedPerStudentMonth();

                for (int i = 0; i < sortedDT.Rows.Count;i++ )
                {
                    string tempStudentid = sortedDT.Rows[i]["studentid"].ToString();
                    string tempM = Convert.ToDateTime(sortedDT.Rows[i]["traveldate"].ToString()).Month.ToString();
                    if(currentStudentID.Equals(tempStudentid))
                    {

                        if(currentMonth==tempM)
                        {
                            double tempfunds = Convert.ToDouble(fperMonth.totalFundsUsed);
                            tempfunds += Convert.ToDouble(sortedDT.Rows[i]["amountreceived"]);
                            fperMonth.totalFundsUsed = Convert.ToInt32(tempfunds);
                        }
                        else
                        {
                            currentMonth = tempM;
                            if(i!=0)
                            {
                                sMList.fStudentMonthlyList.Add(fperMonth);
                            }
                            fperMonth = new FundsUsedPerStudentMonth();
                            fperMonth.month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(tempM)).Substring(0, 3) + " " + Convert.ToDateTime(sortedDT.Rows[i]["traveldate"].ToString()).Year.ToString().Substring(2,2); 
                            fperMonth.totalFundsUsed = Convert.ToInt32(sortedDT.Rows[i]["amountreceived"]);
                        }


                    }
                    else
                    {
                        if (i != 0)
                        {
                            sMList.fStudentMonthlyList.Add(fperMonth);
                        }
                        fperMonth = new FundsUsedPerStudentMonth();
                        currentMonth = tempM;
                        //get first 3 characters of the current month
                        fperMonth.month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(tempM)).Substring(0, 3) + " " + Convert.ToDateTime(sortedDT.Rows[i]["traveldate"].ToString()).Year.ToString().Substring(2, 2); 
                        fperMonth.totalFundsUsed = Convert.ToInt32(sortedDT.Rows[i]["amountreceived"]);



                        currentStudentID = tempStudentid;
                        if(i!=0)
                        {
                            flist.fundsUsedPerStudentPerMonthList.Add(sMList);
                        }
                        sMList = new studentMonthlyList();
                        sMList.name = sortedDT.Rows[i]["name"].ToString();
                        sMList.fStudentMonthlyList = new List<FundsUsedPerStudentMonth>();
                    }
                }
                sMList.fStudentMonthlyList.Add(fperMonth);
                flist.fundsUsedPerStudentPerMonthList.Add(sMList);

                // the logic is to set the first element as the current ultilized element, then traverse the list 
                // and add the funds used to the previous bulk of element as long as the supervisor id is the same
                ///////////////////// graph by supervisor//////////////////////////////
                flist.fundsUsedPerSupList = new List<FundsUsedPerSup>();
                if(lg.role == 2 || lg.role == 3)
                {
                dv.Sort = "supervisorid asc";
                sortedDT = dv.ToTable();
                totalFundsUsed = 0.0;
                
                currentId = "";
                FundsUsedPerSup fSup = new FundsUsedPerSup();
                for (int i = 0; i < sortedDT.Rows.Count; i++)
                {

                    string tempSupid = sortedDT.Rows[i]["supervisorid"].ToString();
                    if (tempSupid.Equals(currentId))
                    {

                        double tempfunds = Convert.ToDouble(fSup.totalFundsUsed);
                        tempfunds += Convert.ToDouble(sortedDT.Rows[i]["amountreceived"]);
                        fSup.totalFundsUsed = Convert.ToInt32(tempfunds);


                    }
                    else
                    {
                        currentId = tempSupid;
                        if (i != 0)
                        {
                            flist.fundsUsedPerSupList.Add(fSup);
                        }
                        fSup = new FundsUsedPerSup();
                        fSup.totalFundsUsed = Convert.ToInt32(sortedDT.Rows[i]["amountreceived"]);
                        fSup.supervisorName = sortedDT.Rows[i]["supname"].ToString();

                    }



                }
                flist.fundsUsedPerSupList.Add(fSup);
                fSup = new FundsUsedPerSup();
                fSup.supervisorName = "Total";
                fSup.totalFundsUsed = flist.fundsUsedPerStudentList.Last().totalFundsUsed;
                flist.fundsUsedPerSupList.Add(fSup);
                return View(flist);


                }


                return View(flist);

            }
            catch(Exception)
            {
                return View();
            }


           
        }

      

       
        public ActionResult Tables() // listing of all applications 
        {
            RefreshSession();

            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
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
                sql = "select tripid, status, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,purpose"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where student.supervisorID = '" + currentUserName + "' order by submissiondate desc;";
            }
            else if(lg.role ==2 || lg.role == 3)
            {
                sql = "select supervisor.name as supName, status, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip,purpose from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid  order by submissiondate desc; ";
            }

            DataTable  dt = data.RunProcReturn(sql,"table").Tables[0]; //get the needed info related to current user
            AllPreviews aps = new AllPreviews(); //create holder for all application previews
            aps.previewList = new List<ApplicationPreview>();
            aps.role = lg.role;            
            for(int i = 0;i<dt.Rows.Count;i++) //put everything to Viewmodel
            {
                ApplicationPreview ap = new ApplicationPreview();
                ap.tripID = dt.Rows[i]["tripid"].ToString();
                ap.studentName = dt.Rows[i]["sName"].ToString();
                if (lg.role == 2 || lg.role==3)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                int status = Convert.ToInt32(dt.Rows[i]["status"]);

                // set status based on the status flag in the database
                if(status == 1)
                {
                    ap.status = "New";
                }
                else if(status == 2)
                {
                    ap.status = "Approved by supervisor";
                }
                else if(status == 3)
                {
                    ap.status = "Approved by tutor";
                }
                else if (status == 4)
                {
                    ap.status = "Rejected by supervisor";
                }
                else if (status  == 5)
                {
                    ap.status = "Rejected by tutor";
                }
                

                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();

                // if conference name is empty, means the purpose field is used instead
                if(String.IsNullOrEmpty(ap.conferenceName)||String.IsNullOrWhiteSpace(ap.conferenceName))
                {
                    ap.conferenceName = dt.Rows[i]["purpose"].ToString();
                }

                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                if (ap.cityCountry == " ,  " || ap.cityCountry == ", ")
                {
                    ap.cityCountry = "n/a";
                }
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.operation = 1;
                aps.previewList.Add(ap);
            }
            return View(aps);
        }

        //method to load the listing of all un-downloaded document
        public ActionResult NewDocument()
        {

            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
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
                sql = "select distinct documents.tripid, trip.status, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                     + " from student join trip on student.studentid = trip.studentid join documents on trip.tripid = documents.tripid"
                     + " where documents.statusSup = 0 and student.supervisorID = '" + currentUserName + "' order by submissiondate desc;";
            }
            else if (lg.role == 2)
            {
                sql = "select distinct supervisor.name as supName, documents.tripid, trip.status, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                    + " from student join trip on student.studentid = trip.studentid join documents on trip.tripid = documents.tripid join supervisor on student.supervisorid = supervisor.supervisorid"
                    + " where documents.statusTut = 0 and student.tutorid = '" + currentUserName + "' order by submissiondate desc;";
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
                if (lg.role == 2 || lg.role == 3)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }

                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                if (String.IsNullOrEmpty(ap.conferenceName) || String.IsNullOrWhiteSpace(ap.conferenceName))
                {
                    ap.conferenceName = dt.Rows[i]["purpose"].ToString();
                }
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                if (ap.cityCountry == " ,  " || ap.cityCountry == ", ")
                {
                    ap.cityCountry = "n/a";
                }

                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }
            aps.operation = 7; 
            return View("Tables", aps);
        }

        //method to load the listing of all un-read messages
        public ActionResult NewMessage()
        {
            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
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
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where tripid in (select tripid from message where seenbysup = 0 and supervisorid = '" + currentUserName + "');";
            }
            else if (lg.role == 2 || lg.role == 3)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                     + " from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid"
                     + " where tripid in (select tripid from message where seenbytut = 0 and tutorid = '" + currentUserName + "');";
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
                if (lg.role == 2 || lg.role == 3)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                if (String.IsNullOrEmpty(ap.conferenceName) || String.IsNullOrWhiteSpace(ap.conferenceName))
                {
                    ap.conferenceName = dt.Rows[i]["purpose"].ToString();
                }
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                if (ap.cityCountry == " ,  " || ap.cityCountry == ", ")
                {
                    ap.cityCountry = "n/a";
                }
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }
            aps.operation = 6;

            return View("Tables", aps);
        }

        //method to load the listing of all new applications
        public ActionResult NewApplication()
        {
            RefreshSession();

            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
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
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 1 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2 || lg.role == 3)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip,trip.purpose from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 1 ";
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
                if (lg.role == 2 || lg.role ==3)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                if (String.IsNullOrEmpty(ap.conferenceName) || String.IsNullOrWhiteSpace(ap.conferenceName))
                {
                    ap.conferenceName = dt.Rows[i]["purpose"].ToString();
                }
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                if (ap.cityCountry == " ,  " || ap.cityCountry == ", ")
                {
                    ap.cityCountry = "n/a";
                }
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }
            aps.operation = 2;
          
            return View("Tables",aps);
        }

        //method to load the listing of all pending application (pending tutor's decision)
        public ActionResult PendingApplication()
        {
            RefreshSession();

            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
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
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 2 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2 || lg.role ==3)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip,trip.purpose from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 2 ";
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
                if (lg.role == 2 || lg.role == 3)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }
                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                if (String.IsNullOrEmpty(ap.conferenceName) || String.IsNullOrWhiteSpace(ap.conferenceName))
                {
                    ap.conferenceName = dt.Rows[i]["purpose"].ToString();
                }
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                if (ap.cityCountry == " ,  " || ap.cityCountry == ", ")
                {
                    ap.cityCountry = "n/a";
                }
                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }

          
            aps.operation = 3;
            return View("Tables", aps);
        }

        //method to load the listing of all approved application (approved by both staffs)
        public ActionResult ApprovedApplication()
        {
            RefreshSession();

            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");
            }

            LoginData lg = (LoginData)(Session["LoginData"]);
            string currentUserName = lg.username;//get user id 
            DataBase data = new DataBase(); //create db instance 
            string sql = "";
            if (lg.role == 1 )
            {
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 3 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2 || lg.role == 3)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip,trip.purpose from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 3 ";
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
                if (lg.role == 2 || lg.role == 3)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }

                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                if (String.IsNullOrEmpty(ap.conferenceName) || String.IsNullOrWhiteSpace(ap.conferenceName))
                {
                    ap.conferenceName = dt.Rows[i]["purpose"].ToString();
                }
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                if (ap.cityCountry == " ,  " || ap.cityCountry == ", ")
                {
                    ap.cityCountry = "n/a";
                }

                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }

            aps.operation = 4;
            return View("Tables", aps);

        }

        //method to load the listing of all approved application (rejected by both staffs)
        public ActionResult RejectedApplication()
        {
            RefreshSession();
            if (Session["LoginData"] == null) // check current login status , redirect to login page if user's not logged in 
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
                sql = "select tripid, student.name as sName, conferencename, city, country ,traveldate,enddate,costoftrip,trip.purpose"
                     + " from student join trip on student.studentid = trip.studentid"
                     + " where status = 4 or status = 5 and student.supervisorID = '" + currentUserName + "';";
            }
            else if (lg.role == 2)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip,trip.purpose from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 4 or status = 5 and student.tutorid = '" + currentUserName + "';";
            }
            else if (lg.role == 3)
            {
                sql = "select supervisor.name as supName, tripid, student.name as sName, conferencename, city, country, traveldate, enddate,costoftrip,trip.purpose from student join trip on student.studentid = trip.studentid join supervisor on student.supervisorid = supervisor.supervisorid where status = 4 or status = 5";
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
                if (lg.role == 2 || lg.role ==3)
                {
                    ap.SupervisorName = dt.Rows[i]["supName"].ToString();
                }

                ap.conferenceName = dt.Rows[i]["conferencename"].ToString();
                if (String.IsNullOrEmpty(ap.conferenceName) || String.IsNullOrWhiteSpace(ap.conferenceName))
                {
                    ap.conferenceName = dt.Rows[i]["purpose"].ToString();
                }
                ap.cost = dt.Rows[i]["costoftrip"].ToString();
                ap.cityCountry = dt.Rows[i]["city"].ToString() + ", " + dt.Rows[i]["country"].ToString();
                if (ap.cityCountry == " ,  " || ap.cityCountry == ", ")
                {
                    ap.cityCountry = "n/a";
                }

                ap.travelDate = Convert.ToDateTime(dt.Rows[i]["traveldate"]).ToString("MM/dd/yyyy");
                ap.endDate = Convert.ToDateTime(dt.Rows[i]["enddate"]).ToString("MM/dd/yyyy");
                aps.previewList.Add(ap);
            }

            
            aps.operation = 5;
            return View("Tables", aps);
        }


        //ajax method to update the database, changing the flag "seenBy..." to 1. only triggers if trip has un-read trip
        [HttpPost]
        public ActionResult UpdateSeen(string id)
        {
            
            if (Session["ApplicationDetail"] != null)
            {
             
                
                ApplicationDetail ad = (ApplicationDetail)Session["ApplicationDetail"];
                if (ad.isThereNewMsg == 1)
                {
                    if (ad.role == 1 || ad.role == 2)
                    {

                        string sql = "";
                        if (ad.role == 1)
                        {
                            sql = "update message set seenbysup = 1 where tripid =" + id + ";";
                        }
                        else if (ad.role == 2)
                        {
                            sql = "update message set seenbytut = 1 where tripid =" + id + ";";
                        }
                        
                        DataBase data = new DataBase();
                        try
                        {
                            data.RunProc(sql);
                        }
                        catch(Exception)
                        {
                            return null;
                        }
                        ad.isThereNewMsg = 0;
                        Session["ApplicationDetail"] = ad;
                    }
                }
            }
            return null;
        }

     
        //handles publishing of data from database before entering the application page
        public ActionResult Application(string id, string viewBagMsg) 
        {
            RefreshSession();

            if (Session["LoginData"] == null)// check current login status , redirect to login page if user's not logged in 
            {
                ViewBag.message = "Please Sign in first";
                return RedirectToAction("Login");              
            }

            LoginData lg = (LoginData)Session["LoginData"];

            // 5mins lock time 
            if(!checkTimeStamp(Convert.ToInt32(id),lg.username))
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Sorry, but it seems like someone else is making changes to this application');window.history.back();</script>");
            }

            

           
            string sql = "";
           
            sql = "select currency, email, isfunddispatched,oktofund, maxtofund, tcomment, scomment, trip.studentid,trip.tripid,student.name, email, maxclaim,student.maxclaim12m,  monthscompleted, dofreg, feepayer, feelpaid, conferencename, conferenceurl, city, country, papertitle, author, purpose,"
                + " status, costoftrip,traveldate,enddate,submissiondate, regfeecal,regfee,transfeecal,transfee,accfeecal,accfee,mealfeecal,mealfee,otherfeecal,otherfee from supervisor join student on supervisor.supervisorid = student.supervisorid join trip on student.studentid = trip.studentid join ecost on trip.tripid = ecost.tripid where trip.tripid =" + id + ";";

            try
            {

                DataBase data = new DataBase(); //create db instance 
                DataTable dt = data.RunProcReturn(sql, "table").Tables[0];//get all needed data except the past travel record and put into datatable 
                string studentid = dt.Rows[0]["studentid"].ToString();
                //past travel history
                string sqlPastTravel = "select traveldate, submissiondate,amountreceived,conferencename from trip where status = 3 and isfunddispatched = 1 and studentid ='" + studentid + "';"; //find all records which has the studentid and approved by tutor (means funds dispatched)
                DataTable dtPastTravel = data.RunProcReturn(sqlPastTravel, "table").Tables[0];
                //all messages
                string sqlMessage = "select messageid,messagetitle, messagebody,  sendername, timestamp, seenbysup, seenbystudent, seenbytut from message where tripid ='" + id + "' order by timestamp desc;";
                DataTable dtMessage = data.RunProcReturn(sqlMessage, "table").Tables[0];
                //all documents (supporting documents and travel report)
                string sqlDocument = "select purpose from documents where tripid =" + id + " order by purpose asc;";
                DataTable dtDocument = data.RunProcReturn(sqlDocument, "table").Tables[0];

                ApplicationDetail ad = new ApplicationDetail();
                    
                // check if there is supporting document and if there is travel report
                    int a = 1;
                    int b = 2;
                    //check if 1 and 2 exist in 'purpose' column
                    bool supportingDocument = dtDocument.AsEnumerable().Any(row => a == row.Field<int>("purpose"));
                    bool travelReport = dtDocument.AsEnumerable().Any(row => b == row.Field<int>("purpose"));
                    if(supportingDocument)
                    {
                        ad.isThereDocument = 1;
                       
                    }
                    else
                    {
                        ad.isThereDocument = 0;
                     
                    }

                    if(travelReport)
                    {
                      
                        ad.isThereTravelReport = 1;
                    }
                    else
                    {
                       
                        ad.isThereTravelReport = 0;
                    }

                
                  
                //add message in to view model list
                ad.messageList = new List<Message>();
                ad.isThereNewMsg = 0;
                for (int i = 0; i < dtMessage.Rows.Count; i++)
                {
                    Message m = new Message();
                    m.messageBody = dtMessage.Rows[i]["messagebody"].ToString();
                    m.messageTitle = dtMessage.Rows[i]["messagetitle"].ToString();
                    m.senderName = dtMessage.Rows[i]["sendername"].ToString();
                    DateTime ts = Convert.ToDateTime(dtMessage.Rows[i]["timestamp"].ToString());                
                    m.timeStamp = ts.ToString();
                    m.messageId = dtMessage.Rows[i]["messageid"].ToString();
                    if (!m.senderName.Equals(lg.name))
                    {
                        m.isYours = 0;
                    }
                    else
                    {
                        m.isYours = 1;
                    }

                    if (lg.role == 1)
                    {
                        if (Convert.ToInt32(dtMessage.Rows[i]["seenbysup"]) == 0)
                        {
                            m.seenStatus = "New";
                            ad.isThereNewMsg = 1;
                        }
                        else
                        {
                            m.seenStatus = "Seen";
                        }
                    }
                    else if(lg.role == 2)
                    {
                        if (Convert.ToInt32(dtMessage.Rows[i]["seenbytut"]) == 0)
                        {
                            m.seenStatus = "New";
                            ad.isThereNewMsg = 1;
                        }
                        else
                        {
                            m.seenStatus = "Seen";
                        }
                    }
                    ad.messageList.Add(m);
                    ad.messageCount++;
                }
                
                //if role is not supervisor or trip is not new application, add isfunddispatched, supervisor comment and ok for supervisor to fund into the list
                if (lg.role == 2 ||lg.role ==3 || lg.role == 1 && Convert.ToInt32(dt.Rows[0]["status"]) != 1)
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
                    else if (lg.role == 3)
                    {
                        ad.role = 3;
                    }
                  
                    if (dt.Rows[0]["oktofund"].ToString().Equals("1"))
                    {
                        ad.isSupPay = "Yes";
                        ad.supPayAmount = dt.Rows[0]["maxtofund"].ToString();
                    }
                    else
                    {
                        ad.isSupPay = "No";
                        ad.supPayAmount = "N/A";
                    }
                }
                else ad.role = 1;


                //put data in the datatable into the view model  
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
                ad.currency = dt.Rows[0]["currency"].ToString();

                //put past travel history into view model and calculate past travel funds usage
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

                //calculate remaining amount in the student account
                double totalRemained = 3000 - totalFundsUsed;
                double totalRemained12M = 2000 - totalFundsUsed12M;
                ad.totalRemained = Convert.ToString(totalRemained);
                ad.remained12M = Convert.ToString(totalRemained12M);
                double totalFee = Convert.ToDouble(dt.Rows[0]["costoftrip"]);
                //calculate maximum allowed amount according the formula provided
                ad.maxAllowed = Convert.ToString(Math.Max(0, Min(totalFee, 1250, totalRemained, totalRemained12M)));

                Session["ApplicationDetail"] = ad;


                //method to handle refresh of page (make sure refresh of page doesn't resubmit the form)
                if (TempData.ContainsKey("redirect"))
                {
                    
                     ViewBag.message = viewBagMsg;
                     TempData.Remove("redirect");
                }





                _UpdateTime(Convert.ToInt32(id),lg.username);

                
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
        //custom random string method
        public static string randomString()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        //methods which handles all postback action triggered from the application page, it reads all possible url strings which are passed from the View engine
        //and determines which action the user has performed based on the reading of 'operation' flag
        //then it performs the action to update the database and the view model accordingly.
        [HttpPost] 
        public ActionResult Application(string studentEmail, string studentname, string studentId,string tripid, string operation, string maxFundInput, string researchAccInput, string commentInput, string messageTitle, string messageBody,string maxAllowed, FormCollection frm)
        {
            RefreshSession();

            if (Session["LoginData"] == null)// check current login status , redirect to login page if user's not logged in 
            {
                return RedirectToAction("Login");
            }

             LoginData lg = (LoginData)Session["LoginData"];
            //flag which determins whether the page has been refreshed 
            TempData["redirect"] = true;
           
           
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

            //send message 
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
                    
                    //insert the detail of the message into the database, as the database is located in USA, add 8 hours as time zone offset
                    if (lg.role == 1)
                        sqlInsertMessage = "insert into message (sendername, studentid, tutorid, tripid, messagebody, messagetitle, timestamp,seenbysup,seenbystudent,seenbytut) values ('" + senderName + "','" + studentId + "','t1234'," + tripid + ",'" + messageBody + "','" + messageTitle + "', NOW() + INTERVAL 8 HOUR," + "1,0,0);";
                    else if(lg.role ==2)
                    {
                        sqlInsertMessage = "insert into message (sendername, studentid, tutorid, tripid, messagebody, messagetitle, timestamp,seenbysup,seenbystudent,seenbytut) values ('" + senderName + "','" + studentId + "','t1234'," + tripid + ",'" + messageBody + "','" + messageTitle + "', NOW() + INTERVAL 8 HOUR," + "0,0,1);";
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

                // add message to view model and update view model in the session
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
                return RedirectToAction("Application", new { id = tripid, viewBagMsg = ViewBag.message });
            }
            else if(operation!="dispatchFund")
            {
                //downloading of documents
                if (operation.Equals("download") || operation.Equals("travelReport"))
                {
                    DataBase data = new DataBase();
                    string sqlupdate = ""; 
                    string sql = "";
                    string zipName = "";
                    if (operation.Equals("download"))
                    {

                    //turn off the un-downloaded flag 
                     if(lg.role == 1)
                     {
                         sqlupdate = "update documents set statusSup = 1 where tripid =" + tripid + " and purpose = 1";
                     }
                     else if (lg.role == 2)
                     {
                         sqlupdate = "update documents set statusTut = 1 where tripid =" + tripid + " and purpose = 1";
                     }

                        
                        sql = "select doctitle, docbody,doctype from documents where tripid =" + tripid + " and purpose = 1;";
                        zipName = "SupDoc.zip";
                    }
                    else if(operation.Equals("travelReport"))
                    {
                        //turn off the un-downloaded flag 
                        if (lg.role == 1)
                        {
                            sqlupdate = "update documents set statusSup = 1 where tripid =" + tripid + " and purpose = 2";
                        }
                        else if (lg.role == 2)
                        {
                            sqlupdate = "update documents set statusTut = 1 where tripid =" + tripid + " and purpose = 2";
                        }
                       
                        sql = "select doctitle, docbody,doctype from documents where tripid =" + tripid + " and purpose = 2;";
                        zipName = "TravelReport.zip";
                    }
                    DataTable dt = data.RunProcReturn(sql, "table").Tables[0];
                    data.RunProc(sqlupdate);

                    //use memoy stream to hold the bytes data from the database, use zip to package the document to be downloaded as multiple files are allowed
                    MemoryStream ms = new MemoryStream();
                    using (var zip = new ZipFile())
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            
                            byte[] file = (byte[])(dt.Rows[i]["docbody"]);
                            zip.AddEntry((i+1)+"_"+dt.Rows[i]["doctitle"].ToString() + "." + dt.Rows[0]["doctype"].ToString(), file);              
                        }
                        zip.Save(ms);
                    }
                    

                    ms.Position = 0;
                    return File(ms, "application/zip", zipName);
                 
                }

         



                //operation which handles the reverting of decision made by user earlier
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
                    return RedirectToAction("Application", new { id = tripid, viewBagMsg = ViewBag.message });
                }

                // set action status based on the actions captured from the user
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
                            //validate user input before updating the database (empty fields)
                            if (applicationStatus == 2 && (String.IsNullOrEmpty(maxFundInput) && oktoFund == 1 || String.IsNullOrEmpty(researchAccInput) && oktoFund == 1)) //if approve but required detail not entered
                            {
                                ViewBag.message = "Amount to fund or research account must not be empty!";
                                if (Session["ApplicationDetail"] != null)
                                {
                                  
                                    return RedirectToAction("Application", new { id = tripid, viewBagMsg = ViewBag.message });
                                }
                                else
                                    return RedirectToAction("Index");
                            }

                            //validate user input before updating the database (datatype correctness)
                            double num;
                            if (!double.TryParse(maxFundInput, out num) && operation.Equals("approve") && lg.role == 1 && oktoFund ==1) //check if its a number
                            {
                                return RedirectToAction("Application", new { id = tripid, viewBagMsg = "Amount to fund must be a number" });
                            }   
                        }

                        //validate user input before updating the database (reject must comes with comment input)
                        if ((applicationStatus == 4 || applicationStatus==5) && String.IsNullOrEmpty(commentInput)) 
                        {
                            ViewBag.message = "Comment can not be empty!";
                            if (Session["ApplicationDetail"] != null)
                            {
                                return RedirectToAction("Application", new { id = tripid, viewBagMsg = ViewBag.message });
                            }
                            else
                                return RedirectToAction("Index");
                        }
                        else  //approval or reject with no invalid user input
                        {
                            DataBase data = new DataBase();
                            string sql = "";
                            if (lg.role == 1)
                            {
                                if (applicationStatus == 2 )
                                {
                                    if (oktoFund == 1)
                                    {
                                        sql = "update trip set maxtofund =" + maxFundInput + ",  researchacc='" + researchAccInput + "', status=" + applicationStatus + ", oktofund =" + oktoFund + ", scomment ='" + commentInput + "' where tripid =" + tripid + ";";
                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(commentInput))
                                        {
                                            sql = "update trip set status=" + applicationStatus + ", oktofund =" + oktoFund + " where tripid =" + tripid + ";";
                                        }
                                        else
                                        {
                                            sql = "update trip set status=" + applicationStatus + ", oktofund =" + oktoFund + ", scomment ='" + commentInput + "' where tripid =" + tripid + ";";
                                        }
                                    }
                                    ViewBag.message = "Application approved";
                                }
                                else if (applicationStatus == 4 )
                                {
                                    sql = "update trip set status = " + applicationStatus + ",scomment ='" + commentInput + "' where tripid =" + tripid + ";";
                                    ViewBag.message = "Application rejected";
                                }
                            }
                            else if (lg.role == 2)
                            {
                                if (String.IsNullOrEmpty(commentInput))
                                {
                                    sql = "update trip set status = " + applicationStatus + " where tripid =" + tripid + ";";
                                    ViewBag.message = "Application approved";
                                }
                                else
                                {
                                    sql = "update trip set status = " + applicationStatus + ", tcomment = '" + commentInput + "' where tripid =" + tripid + ";";
                                    ViewBag.message = "Application rejected";
                                }
                            }
                            try
                            {
                                if (data.RunProc(sql) == -1) //update the database fail
                                {
                                    ViewBag.message = "Failed to update record!";
                                }
                                else 
                                {
                                    //aync method to send email to respective student regarding the change of his/her application status 
                                    Task.Run(() => SendEmail(studentname, tripid, studentEmail, applicationStatus.ToString()));
                                }
                                
                                
                                
                            }
                            catch (Exception)
                            {
                                ViewBag.message = "Failed to update record!";
                            }
                            
                            //update view model
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

                                return RedirectToAction("Application", new { id = tripid, viewBagMsg = ViewBag.message });
                            }
                            else
                                return RedirectToAction("Index");

                        }
                       

                    }



                    return RedirectToAction("Index");
            }

            //handles dispatch funds button click,
            else if(operation == "dispatchFund")
            {
                DataBase data = new DataBase();
                int fundReceived = 0;
                if (String.IsNullOrEmpty(maxAllowed))
                {
                    fundReceived = 0;
                }
                fundReceived = Convert.ToInt32(maxAllowed);
                
                string sql = "update trip set isfunddispatched= 1 , amountreceived ="+fundReceived+" where tripid =" + tripid + "; ";
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


        //publish login screen
       public ActionResult Login()
        {
            return View();
        }

      

        //handles user login operation
       [HttpPost]
        public ActionResult Login(string username, string password, FormCollection frm)
        {

            if (Session["LoginData"] != null) // check current login status , redirect to login page if user's not logged in 
            {
                return RedirectToAction("Index");
            }
            else
            {
                //call authentication module to verify user's credential , then checks the return tuple for login status and user roles 
                Tuple<string,int> t = AuthenticationService.SignIn(username, password, true);
                
               
                if (t!=null)
                {
                    string name = t.Item1;
                    int istutor = t.Item2;
                    LoginData lg = null;
                    if (istutor == 0)
                    {
                         lg = new LoginData(username, 1, name); //static data for now, supervisor == 1
                    }
                    else if (istutor == 1)
                    {
                         lg = new LoginData(username, 2, name); //tutor == 2
                      
                    }
                    else if (istutor == 2)
                    {
                        lg = new LoginData(username, 3, name);//admin == 3
                    }
                    
                    Session["LoginData"] = lg; //save login data to session

                   
                    return RedirectToAction("Index");
                }
                else
                {
                
                    return RedirectToAction("Login");
                }

            }

           
        }

        //method that handles logout
        public ActionResult Logout()
        {
            AuthenticationService.SignOut();
            Session["LoginData"] = null;
            return RedirectToAction("Login");
        }

        //method to send email, get parameters from application method
        public  void  SendEmail (string studentName, string tripid, string studentEmail, string msg)
        {
            string statusString = "";
            string statusMsgFront = "";
            int status = Convert.ToInt32(msg);

            //generate email message based on status change
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
          
            //define email sending protocols and credentials
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp-mail.outlook.com");

                mail.From = new MailAddress("phdtravelgrant-g3@hotmail.com");
                mail.To.Add(studentEmail);
                mail.Subject = "Status of your application has been changed";
                mail.IsBodyHtml = true;
             

                mail.Body = "<i>Dear " + studentName + ":<i> <br /><br />" +
                           statusMsgFront+ "<i>Your application #" + tripid + " has been <i>"+statusString+" <br/><br/> " +
                "<i>Remainder from PHD Travel grant<i><br /> <br />  <br />  <br />  <br />" +            

                "<i> This is an auto generated message</i> <br />";



                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("phdtravelgrant-g3@hotmail.com", "Phdtravel.0");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

            }
            catch (Exception)
            {
                
            }
      


           


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

        //5 mins lock to enhance occurency control
        public bool checkTimeStamp(int tripid,string userid)
        {
          //  return true;
            DataBase db = new DataBase();
            return true;
            /*
            DataTable dt = db.RunProcReturn("select now() as currenttime,lastaccessed,accessid from trip where tripid=" + tripid + ";", "table").Tables[0];
            DateTime lastAccessed = Convert.ToDateTime(dt.Rows[0]["lastaccessed"]).ToUniversalTime();
            DateTime now = Convert.ToDateTime(dt.Rows[0]["currenttime"]).AddHours(8).ToUniversalTime();
            string accessid = dt.Rows[0]["accessid"].ToString();
            if (lastAccessed.AddMinutes(5) >= now)
            {
                if (userid.Equals(accessid))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return true;
            }
              */

        }

        //update the 5 mins lock if the same user access the application
        [HttpPost]
        public ActionResult UpdateTime(int tripid)
        {
            DataBase db = new DataBase();
            try
            {
                db.RunProc("update trip set lastaccessed = DATE_ADD(NOW(), INTERVAL 8 HOUR) where tripid=" + tripid + ";");
            }
            catch(Exception)
            {

            }
            return null;
        }

        //update the 5 mins lock if the same user access the application (non postback version)
        public void _UpdateTime(int tripid, string accessId)
        {
            DataBase db = new DataBase();
            try
            {
                db.RunProc("update trip set lastaccessed = DATE_ADD(NOW(), INTERVAL 8 HOUR) , accessid = '" + accessId + "' where tripid=" + tripid + ";");
            }
            catch (Exception)
            {

            }
          
        }




    }
}