using SupervisorNew1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupervisorNew1.Domain
{
    public class Data
    {
        public IEnumerable<Navbar> navbarItems()
        {
            var menu = new List<Navbar>();
            menu.Add(new Navbar { Id = 1, nameOption = "Dashboard", controller = "Home", action = "Index", imageClass = "fa fa-dashboard fa-fw", status = true, isParent = false, parentId = 0 });
       
           // menu.Add(new Navbar { Id = 5, nameOption = "Tables", controller = "Home", action = "Tables", imageClass = "fa fa-table fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 5, nameOption = "All Applications", controller = "Home", action = "Tables", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 2, nameOption = "New Applications", controller = "Home", action = "NewApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 3, nameOption = "Pending Applications", controller = "Home", action = "PendingApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 19, nameOption = "Approved Applications", controller = "Home", action = "ApprovedApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 19, nameOption = "Rejected Applications", controller = "Home", action = "RejectedApplication", imageClass = "fa fa-edit fa-fw", status = true, isParent = false, parentId = 0 });


            menu.Add(new Navbar { Id = 17, nameOption = "Import Staff", controller = "Home", action = "UploadStaff", imageClass = "fa fa-upload fa-fw", status = true, isParent = false, parentId = 0 });
            menu.Add(new Navbar { Id = 18, nameOption = "Import Student", controller = "Home", action = "UploadStudent", imageClass = "fa fa-upload fa-fw", status = true, isParent = false,  parentId = 0 });
        //    menu.Add(new Navbar { Id = 18, nameOption = "Login Page", controller = "Home", action = "Login", status = true, isParent = false, parentId = 0 });
        //    menu.Add(new Navbar { Id = 7, nameOption = "UI Elements", imageClass = "fa fa-wrench fa-fw", status = true, isParent = true, parentId = 0 });
        //    menu.Add(new Navbar { Id = 8, nameOption = "Panels and Wells", controller = "Home", action = "Panels", status = true, isParent = false, parentId = 7 });
        //    menu.Add(new Navbar { Id = 9, nameOption = "Buttons", controller = "Home", action = "Buttons", status = true, isParent = false, parentId = 7 });
        //    menu.Add(new Navbar { Id = 10, nameOption = "Notifications", controller = "Home", action = "Notifications", status = true, isParent = false, parentId = 7 });
        //    menu.Add(new Navbar { Id = 11, nameOption = "Typography", controller = "Home", action = "Typography", status = true, isParent = false, parentId = 7 });
        //    menu.Add(new Navbar { Id = 12, nameOption = "Icons", controller = "Home", action = "Icons", status = true, isParent = false, parentId = 7 });
        //    menu.Add(new Navbar { Id = 13, nameOption = "Grid", controller = "Home", action = "Grid", status = true, isParent = false, parentId = 7 });
        //    menu.Add(new Navbar { Id = 14, nameOption = "Multi-Level Dropdown", imageClass = "fa fa-sitemap fa-fw", status = true, isParent = true, parentId = 0 });
         //   menu.Add(new Navbar { Id = 15, nameOption = "Second Level Item", status = true, isParent = false, parentId = 14 });
         //   menu.Add(new Navbar { Id = 16, nameOption = "Sample Pages", imageClass = "fa fa-files-o fa-fw", status = true, isParent = true, parentId = 0 });
     
         //   menu.Add(new Navbar { Id = 2, nameOption = "Charts", imageClass = "fa fa-bar-chart-o fa-fw", status = true, isParent = true, parentId = 0 });
         //   menu.Add(new Navbar { Id = 3, nameOption = "Flot Charts", controller = "Home", action = "FlotCharts", status = true, isParent = false, parentId = 2 });
         //   menu.Add(new Navbar { Id = 4, nameOption = "Morris.js Charts", controller = "Home", action = "MorrisCharts", status = true, isParent = false, parentId = 2 });
            return menu.ToList();
        }
    }
}