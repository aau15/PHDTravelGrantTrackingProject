using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Data;

namespace SupervisorNew1.Security
{
    public class AuthenticationService
    {
        public static string SignIn(string userName, string pwd, bool createPersistentCookie)
        {

     //       if (String.IsNullOrEmpty(userName))
      //          throw new ArgumentException("Value cannot be null or empty.", "userName");

            if (userName == "s1234" && pwd == "password") // mock login
            {

              //  Membership.ValidateUser(userName, pwd);
                FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
                return "Sophia"; //static for now

            }
            else if(userName == "t1234" && pwd == "password")
            {
                FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
                return "Larry";
            }
            else
            {
                return null;
            }
        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}