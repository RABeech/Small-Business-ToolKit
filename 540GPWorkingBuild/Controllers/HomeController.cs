using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace _540GPWorkingBuild.Controllers
{

    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TestView()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ManagerMenu()
        {
            return View();
        }

        public ActionResult EmployeeMenu()
        {
            return View();
        }

        public ActionResult AdminView()
        {
            return View();
        }
                     
        public ActionResult Login()
        {
            Session["EmpLoginError"] = "";
            Session["PassLoginError"] = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Models.Employee e)
        {
            if (ModelState.IsValid)
            {
                using (Models.MusciToolkitDBEntities db = new Models.MusciToolkitDBEntities())
                {
                    var obj = db.Employees.Where(a => a.EmployeeID.Equals(e.EmployeeID) && a.Password.Equals(e.Password)).FirstOrDefault();
                    if (obj != null)
                    {
                        Session["UserID"] = obj.EmployeeID.ToString();
                        Session["UserRole"] = obj.EmployeeRole.EmpRole.ToString();
                        Session["UserName"] = obj.FirstName.ToString();
                        Session["EmpLoginError"] = "";
                        Session["PassLoginError"] = "";

                        ViewBag.UserName = Session["UserName"];

                        if (Session["UserRole"].Equals("Manager"))
                        {
                            return View("ManagerMenu");
                        }
                        else if (Session["UserRole"].Equals("Admin"))
                        {
                            return View("AdminView");
                        }
                        else if (Session["UserRole"].Equals("Employee"))
                        {
                            return View("EmployeeMenu");
                        }
                        
                    }
                    else
                    {
                        Models.Employee a = db.Employees.Find(e.EmployeeID);
                        if (a == null)
                        {
                            Session["EmpLoginError"] = "Invalid employee ID";
                            Session["PassLoginError"] = "";
                        }
                        else
                        {
                            Session["PassLoginError"] = "Invalid password";
                            Session["EmpLoginError"] = "";
                        }
                    }
                }
            }

            return View(e);
        }

        public ActionResult Menu()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                Session["EmpLoginError"] = "Invalid employee ID";
                return RedirectToAction("Login");
            }
        }

        public ActionResult Logout()
        {
            Session["UserID"] = null;
            Session["UserRole"] = null;
            return View();
        }
    } // home controller

} // namespace