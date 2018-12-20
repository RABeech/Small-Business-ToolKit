using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using _540GPWorkingBuild.Models;

namespace _540GPWorkingBuild.Controllers
{
    public class CustomersController : Controller
    {
        private MusciToolkitDBEntities db = new MusciToolkitDBEntities();

        public ActionResult LowPerm()
        {
            return View();
        }
        // GET: Customers
        public ActionResult Index(string option, string search)
        {
            if (Session["UserRole"] == null)
            {
                return RedirectToAction("LowPerm");
            }

            var Customers = db.Customers.Include(c => c.Address);
            List<Customer> CustList = db.Customers.Include(c => c.Address).ToList();

            if (option == "CustomerID")
            {
                return View(db.Customers.Where(i => i.CustomerID.ToString() == search || search == null).ToList());
            }
            else if (option == "CustomerPhone")
            {
                return View(db.Customers.Where(i => i.PhoneNum.ToString() == search || search == null).ToList());
            }
            else if (option == "CustFirstName")
            {
                return View(db.Customers.Where(i => i.FirstName.ToString() == search || search == null).ToList());
            }
            else if (option == "CustLastName")
            {
                return View(db.Customers.Where(i => i.LastName.ToString() == search || search == null).ToList());
            }
            else
            {
                return View(Customers.ToList());
            }

        }

        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["UserRole"] == null)
            {
                return RedirectToAction("LowPerm");
            }

            var Customers = db.Customers.Include(s => s.Sales);
            //List<CustSales> CustList = db.Customers.Include(s => s.Sales).ToList();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
            //return View(CustSales);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPerm");
            }

            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress");
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerID,FirstName,LastName,AddressID,Email,PhoneNum,Active")] Customer customer)
        {
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPerm");
            }

            if (ModelState.IsValid)
            {
                customer.Active = 1;
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress", customer.AddressID);
            return View(customer);
        }

        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPerm");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress", customer.AddressID);
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,FirstName,LastName,AddressID,Email,PhoneNum,Active")] Customer customer)
        {
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPerm");
            }

            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "StreetAddress", customer.AddressID);
            return View(customer);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPerm");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["UserRole"] == null)
            {
                return RedirectToAction("LowPerm");
            }
            Customer customer = db.Customers.Find(id);
            customer.Active = 0;
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
