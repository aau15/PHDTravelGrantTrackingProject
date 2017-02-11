using SupervisorNew1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupervisorNew1.Domain
{

    // class which used as the container for the publishing of navigation bar
    public class Data
    {
        public IEnumerable<Navbar> navbarItems()
        {
            var menu = new List<Navbar>();
            menu.Add(new Navbar { Id = 1, nameOption = "Dashboard", controller = "Home", action = "Index", imageClass = "fa fa-dashboard fa-fw", status = true, isParent = false, parentId = 0 });       
            menu.Add(new Navbar { Id = 5, nameOption = "All Applications", controller = "Home", action = "Tables", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 2, nameOption = "New Applications", controller = "Home", action = "NewApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 3, nameOption = "Pending Applications", controller = "Home", action = "PendingApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 19, nameOption = "Approved Applications", controller = "Home", action = "ApprovedApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 19, nameOption = "Rejected Applications", controller = "Home", action = "RejectedApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });


     
            return menu.ToList();
        }
    }
}