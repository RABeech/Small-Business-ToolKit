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
    public class InventoriesController : Controller
    {
        private MusciToolkitDBEntities db = new MusciToolkitDBEntities();

        // GET: Inventories
        public ActionResult Index()
        {
            if (Session["UserRole"] == null)
            {
                return RedirectToAction("LowPermission");
            }
            var inventories = db.Inventories.Include(i => i.Category);
            return View(inventories.ToList());
        }

        // GET: Inventories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = db.Inventories.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // GET: Inventories/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Category1");
            return View();
        }

        // POST: Inventories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,Name,Description,NetPrice,SalePrice,Quantity,CategoryID,Active")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.Active = 1;
                db.Inventories.Add(inventory);
                db.SaveChanges();
                return RedirectToAction("Products");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Category1", inventory.CategoryID);
            return View(inventory);
        }

        // GET: Inventories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventory inventory = db.Inventories.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Category1", inventory.CategoryID);
            return View(inventory);
        }

        // POST: Inventories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,Name,Description,NetPrice,SalePrice,Quantity,CategoryID,Active")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Category1", inventory.CategoryID);
            return View(inventory);
        }

        // GET: Inventories/Delete/5
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
            Inventory inventory = db.Inventories.Find(id);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // POST: Inventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            Inventory inventory = db.Inventories.Find(id);
            inventory.Active = 0;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // Home page for product section
        public ActionResult Products()
        {
            if (Session["UserRole"] == null)
            {
                return RedirectToAction("LowPermission");
            }
            else
            {
                return View();
            }

        }

        // Display all active products
        public ActionResult ViewProducts()
        {
            var inventories = db.Inventories.Include(i => i.Category).Where(x => x.Active == 1);
            return View(inventories.ToList());
        }



        // Method to control search functionality
        public ActionResult Search(string option, string search)
        {
            List<Inventory> invList = db.Inventories.ToList();
            //if a user choose the radio button option as Subject  
            if (option == "ID")
            {
                try
                {
                    // Doesn't discriminate against active
                    return View(invList.Where(x => x.ProductID == Int32.Parse(search) || search == null).ToList());
                }
                catch
                {
                    return View(new List<Inventory>());
                }
            }
            else if (option == "Name")
            {
                return View(invList.Where(x => x.Name.Contains(search) && x.Active == 1 || search == null).ToList());
            }
            else if (option == "Description")
            {
                return View(invList.Where(x => x.Description.Contains(search) && x.Active == 1 || search == null).ToList());
            }
            else if (option == "Category")
            {
                return View(invList.Where(x => x.Category.Category1.Equals(search) && x.Active == 1 || search == null).ToList());
            }
            else
            {
                // Return empty list / no search results
                return View(new List<Inventory>());
            }
        }

        public ActionResult LowPermission()
        {
            return View();
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
