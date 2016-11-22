using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupervisorNew1.Security
{
    [Serializable]
    public class LoginData
    {
        public string username { get; set; }
        public int role { get; set; }
        public string name { get; set; }

        public LoginData(string username, int role, string name)
        {
            this.username = username;
            this.role = role; //0 = student; 1 = supervisor; 2= tutor; 3 = admin;
            this.name = name;
        }

        
    }
}