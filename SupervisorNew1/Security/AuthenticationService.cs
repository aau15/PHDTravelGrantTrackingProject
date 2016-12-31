using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Data;
using SupervisorNew1.Controllers;
namespace SupervisorNew1.Security
{
    public class AuthenticationService
    {
        public static Tuple<string,int> SignIn(string userName, string pwd, bool createPersistentCookie)
        {

     //       if (String.IsNullOrEmpty(userName))
      //          throw new ArgumentException("Value cannot be null or empty.", "userName");

            if(String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(pwd))
            {
                return null;

            }

            DataBase db = new DataBase();
            string sql = "select name from supervisor where istutor = 0 and supervisorid = '" + userName + "' and pwd = '" + pwd + "';";
            DataTable dt = db.RunProcReturn(sql, "table").Tables[0];
            
            if(dt.Rows.Count ==0) // if not supervisor check tutor
            {
                sql = "select name from tutor where tutorid = '" + userName + "' and pwd = '" + pwd + "';";
                dt = db.RunProcReturn(sql, "table").Tables[0];
                
                if(dt.Rows.Count ==0)
                {
                    sql = "select name from admin where adminid = '" + userName + "' and pwd = '" + pwd + "';";
                    dt = db.RunProcReturn(sql, "table").Tables[0];
                    if(dt.Rows.Count ==0)
                    {
                        return null; // username and pwd not matched
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
                        return new Tuple<string, int>(dt.Rows[0]["name"].ToString(), 2); //admin login
                    }
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
                    return new Tuple<string, int>(dt.Rows[0]["name"].ToString(), 1); //tutor login
                }
                
                
            }
            else
            {
                FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
                return new Tuple<string, int>(dt.Rows[0]["name"].ToString(), 0);
            }


            //if (userName == "s1234" && pwd == "password") // mock login
            //{

            //  //  Membership.ValidateUser(userName, pwd);
            //    FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
            //    return "Sophia"; //static for now

            //}
            //else if(userName == "t1234" && pwd == "password")
            //{
            //    FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
            //    return "Larry";
            //}
            //else
            //{
            //    return null;
            //}
        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}