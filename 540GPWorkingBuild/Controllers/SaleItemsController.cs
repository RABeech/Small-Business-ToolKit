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
    public class SaleItemsController : Controller
    {
        private MusciToolkitDBEntities db = new MusciToolkitDBEntities();

        // GET: SaleItems
        public ActionResult Index()
        {
            var saleItems = db.SaleItems.Include(s => s.Inventory).Include(s => s.Sale);
            return View(saleItems.ToList());
        }

        // GET: SaleItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SaleItem saleItem = db.SaleItems.Find(id);
            if (saleItem == null)
            {
                return HttpNotFound();
            }
            return View(saleItem);
        }

        // GET: SaleItems/Create
        public ActionResult Create()
        {
            var allSaleItems = db.SaleItems.ToList();

            ViewBag.ProductID = new SelectList(db.Inventories, "ProductID", "ProductID");
            ViewBag.SaleID = new SelectList(db.Sales, "SaleID", "SaleID");

            // Calculates the totals specifically for the View.
            foreach (SaleItem saleItem in allSaleItems)
            {
                saleItem.TotalSIPrice += saleItem.Quantity * (double)saleItem.Inventory.SalePrice;
                saleItem.TotalSI += saleItem.Quantity;
                saleItem.Sale.TotalSalePrice += saleItem.TotalSIPrice;
                saleItem.Sale.TotalSaleItems += saleItem.TotalSI;
            }

            return View(allSaleItems); // Return SaleItems list as a parameter for the View
        }


        // POST: SaleItems/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(SaleItem saleItem)
        {
            if (ModelState.IsValid)
            {
                SaleItem SI; // Create new SaleItem and set it equal to the SaleItem that was passed in
                SI = saleItem;
                Inventory inv; // Get information from Inventory table
                inv = db.Inventories.Find(SI.ProductID); // Set Inventory information equal to the SaleItem ProductID
                SI.Inventory = inv;
                Sale s; // Get information from Sale table
                s = db.Sales.Find(Int32.Parse(Session["Current SaleID"].ToString())); // Set s equal to the Current SaleID
                SI.Sale = s;
                SI.Returned = 0; // Returned = 0 because a transaction is being performed. (Not returning)
                SI.SaleID = s.SaleID; // added
                SI.TotalSI += SI.Quantity; // Total of items added at one time
                SI.Inventory.Quantity -= SI.Quantity;
                SI.TotalSIPrice += SI.Quantity * (double)inv.SalePrice; // Total sale price of item specified
                                                                        //SI.Sale.TotalSaleItems += SI.TotalSI;
                                                                        //SI.Sale.TotalSalePrice += SI.TotalSIPrice;
                db.SaleItems.Add(SI);
                db.SaveChanges();
                return RedirectToAction("Create", new { id = s.SaleID.ToString() }); // URL ID equal to the Current SaleID
            }

            ViewBag.ProductID = new SelectList(db.Inventories, "ProductID", "ProductID", saleItem.ProductID);
            ViewBag.SaleID = new SelectList(db.Sales, "SaleID", "SaleID", saleItem.SaleID);
            return View(saleItem);
        }

        public ActionResult CheckOut()
        {
            var allSaleItems = db.SaleItems.ToList();
            // Calculates totals specifically for this View
            foreach (SaleItem saleItem in allSaleItems)
            {
                saleItem.TotalSIPrice += saleItem.Quantity * (double)saleItem.Inventory.SalePrice;
                saleItem.TotalSI += saleItem.Quantity;
                saleItem.Sale.TotalSalePrice += saleItem.TotalSIPrice;
                saleItem.Sale.TotalSaleItems += saleItem.TotalSI;
            }
            return View(allSaleItems);
        }

        // Return a specific item given a SaleItem ID
        public ActionResult Return(int? id)
        {
            Session["SOReturnItem"] = (int)id; // Stores SaleItem ID to find the desired item to be returned
            Session["SOReturnError"] = "";
            return RedirectToAction("ReturnItem");
        }

        // Contains text box to enter desired Quantity
        public ActionResult ReturnItem()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProcessReturn()
        {
            bool xisvalid = true;
            int quantityToReturn = 0;
            try
            {
                quantityToReturn = Int32.Parse(Request["qty"].ToString()); // Declared inside ReturnItem view. User inputs this number into the text box.
            }
            catch
            {
                xisvalid = false;
            }
            SaleItem SI = db.SaleItems.Find(Int32.Parse(Session["SOReturnItem"].ToString())); // Find the SaleItem ID that contains the desired item to be returned
            if (SI == null)
            {
                xisvalid = false;
            }
            else
            {
                int q = SI.Quantity;
                // Make sure quantity that is input is valid (not 0 or greater than number of items purchased)
                if ((quantityToReturn < 1) || (quantityToReturn > q))
                {
                    xisvalid = false;
                }
            }
            if (xisvalid)
            {
                SI.Quantity -= quantityToReturn; // Subtract quantity returned from previous quantity
                SI.Inventory.Quantity += quantityToReturn;
                SI.TotalSI -= SI.Quantity; // Subtract total items from the sale from previous quantity
                if (SI.Quantity == 0)
                {
                    SI.Returned = 1; // No more items in list, so take away option to return any more items
                }
                db.SaveChanges();
                Session["SOReturnError"] = "";
                return RedirectToAction("Details", "Sales", new { id = SI.Sale.SaleID });
            }
            else
            {
                Session["SOReturnError"] = "Invalid return value";
                return RedirectToAction("ReturnItem");
            }
        }

        public ActionResult Cancel(int? id)
        {
            var x = db.SaleItems.Find((int)id);
            x.Inventory.Quantity += x.Quantity;
            db.SaleItems.Remove(x);
            db.SaveChanges();
            return RedirectToAction("Create", new { id = Session["Current SaleID"].ToString() });
        }

        // GET: SaleItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SaleItem saleItem = db.SaleItems.Find(id);
            if (saleItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductID = new SelectList(db.Inventories, "ProductID", "ProductID", saleItem.ProductID);
            ViewBag.SaleID = new SelectList(db.Sales, "SaleID", "SaleID", saleItem.SaleID);
            return View(saleItem);
        }

        // POST: SaleItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SaleItemID,ProductID,Quantity,Returned,SaleID")] SaleItem saleItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(saleItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductID = new SelectList(db.Inventories, "ProductID", "ProductID", saleItem.ProductID);
            ViewBag.SaleID = new SelectList(db.Sales, "SaleID", "SaleID", saleItem.SaleID);
            return View(saleItem);
        }

        // GET: SaleItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SaleItem saleItem = db.SaleItems.Find(id);
            if (saleItem == null)
            {
                return HttpNotFound();
            }
            return View(saleItem);
        }

        // POST: SaleItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SaleItem saleItem = db.SaleItems.Find(id);
            db.SaleItems.Remove(saleItem);
            db.SaveChanges();
            return RedirectToAction("Create");
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
