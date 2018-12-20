using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _540GPWorkingBuild.Models;

//****DONE gmeeks 11/29
namespace _540GPWorkingBuild.Controllers
{
    public class CategoriesController : Controller
    {
        private MusciToolkitDBEntities db = new MusciToolkitDBEntities();

        // GET: Categories
        public ActionResult Index()
        {
            if (Session["UserRole"] != null)
            {
                var activeCategories = new List<Category>();
                var allCategories = db.Categories.ToList();
                foreach (Category c in allCategories)
                {
                    if (c.Active != 0)
                    {
                        activeCategories.Add(c);
                    }
                }

                return View(activeCategories);
            }
            else
            {
                return RedirectToAction("LowPermission");
            }
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            if (Session["UserRole"] != null)
                return View(category);
            else
            {
                return RedirectToAction("LowPermission");
            }
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            if (Session["UserRole"] != null)
                return View();
            else
            {
                return RedirectToAction("LowPermission");
            }
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,Category1,Active")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.Active = 1;
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            if (Session["UserRole"] != null)
                return View(category);
            else
            {
                return RedirectToAction("LowPermission");
            }
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,Category1,Active")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                category.Active = 1;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Delete/5
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
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            if (Session["UserRole"] != null)
                return View(category);
            else
            {
                return RedirectToAction("LowPermission");
            }
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            Category category = db.Categories.Find(id);
            category.Active = 0;
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

        public ActionResult LowPermission()
        {
            return View();
        }
    }
}
