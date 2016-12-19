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
            string sql = "select name ,istutor from supervisor where supervisorid = '" + userName + "' and pwd = '" + pwd + "';";
            DataTable dt = db.RunProcReturn(sql, "table").Tables[0];
            
            if(dt.Rows.Count ==0)
            {
                return null;
            }
            else
            {
                FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
                return new Tuple<string, int>(dt.Rows[0]["name"].ToString(), Convert.ToInt32(dt.Rows[0]["istutor"]));
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