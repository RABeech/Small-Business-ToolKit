using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _540GPWorkingBuild.Models;

namespace _540GPWorkingBuild.Controllers
{
    public class EmployeesController : Controller
    {
        private MusciToolkitDBEntities db = new MusciToolkitDBEntities();

        // GET: Employees
        public ActionResult Index(string option, string search)
        {

            var Employees = db.Employees.Include(e => e.Address).Include(e => e.EmployeeRole);
            List<Employee> EmpList = db.Employees.Include(e => e.Address).ToList();

            if (option == "EmployeeID")
            {
                return View(db.Employees.Where(i => i.EmployeeID.ToString() == search || search == null).ToList());
            }
            else if (option == "EmployeePhone")
            {
                return View(db.Employees.Where(i => i.PhoneNum.ToString() == search || search == null).ToList());
            }
            else if (option == "EmpFirstName")
            {
                return View(db.Employees.Where(i => i.FirstName.ToString() == search || search == null).ToList());
            }
            else if (option == "EmpLastName")
            {
                return View(db.Employees.Where(i => i.LastName.ToString() == search || search == null).ToList());
            }
            else
            {
                return View(Employees.ToList());
            }
        }

        public ActionResult LowPermission()
        {
            return View();
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {

            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress");
            ViewBag.RoleID = new SelectList(db.EmployeeRoles, "RoleID", "EmpRole");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmployeeID,FirstName,LastName,HireDate,RoleID,PhoneNum,AddressID,Email,Password,Active")] Employee employee)
        {

            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            if (ModelState.IsValid)
            {
                employee.Active = 1;
                db.Employees.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress", employee.AddressID);
            ViewBag.RoleID = new SelectList(db.EmployeeRoles, "RoleID", "EmpRole", employee.RoleID);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {

            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress", employee.AddressID);
            ViewBag.RoleID = new SelectList(db.EmployeeRoles, "RoleID", "EmpRole", employee.RoleID);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmployeeID,FirstName,LastName,HireDate,RoleID,PhoneNum,AddressID,Email,Password,Active")] Employee employee)
        {

            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress", employee.AddressID);
            ViewBag.RoleID = new SelectList(db.EmployeeRoles, "RoleID", "EmpRole", employee.RoleID);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int? id)
        {

            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employees.Find(id);
            employee.Active = 0;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
