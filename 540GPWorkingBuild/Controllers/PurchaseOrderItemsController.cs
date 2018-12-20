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
    public class PurchaseOrderItemsController : Controller
    {
        private MusciToolkitDBEntities db = new MusciToolkitDBEntities();

        public double calcTotalCost(PurchaseOrderItem p)
        {
            return (double)p.Quantity * (double)p.Inventory.NetPrice;
        }

        public ActionResult Debug()
        {
            return View();
        }

        // GET: PurchaseOrderItems
        public ActionResult Index()
        {
            var purchaseOrderItems = db.PurchaseOrderItems.Include(p => p.Inventory).Include(p => p.PurchaseOrder);
            return View(purchaseOrderItems.ToList());
        }


        // GET: PurchaseOrderItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrderItem purchaseOrderItem = db.PurchaseOrderItems.Find(id);
            if (purchaseOrderItem == null)
            {
                return HttpNotFound();
            }
            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Create
        public ActionResult Create()
        {
            // Third param is shown in the drop down
            ViewBag.ProductID = new SelectList(db.Inventories.Where(x => x.Active == 1), "ProductID", "dropdownStr");
            ViewBag.PurchaseOrderID = new SelectList(db.PurchaseOrders, "PurchaseOrderID", "PurchaseOrderID");
            return View();
        }

        // POST: PurchaseOrderItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "POItemID,ProductID,Quantity,Received,PurchaseOrderID")] PurchaseOrderItem purchaseOrderItem)
        {
            if (ModelState.IsValid)
            {

                // Instantiate foreign key field for Inventory
                PurchaseOrderItem ans;
                ans = purchaseOrderItem;
                _540GPWorkingBuild.Models.Inventory inv;
                inv = db.Inventories.Find(Int32.Parse(Request["ProductID"].ToString()));
                ans.Inventory = inv;


                //Instantiate foreign key for PurchaseOrder
                _540GPWorkingBuild.Models.PurchaseOrder po;
                po = db.PurchaseOrders.Find(Int32.Parse(Session["currPo"].ToString()));
                ans.PurchaseOrder = po;

                // Set initial received value to zero
                ans.Received = 0;

                // Save database
                db.PurchaseOrderItems.Add(ans);
                db.SaveChanges();

                // Bounce user to the detail page for the purchase order
                return RedirectToAction("Details", "PurchaseOrders", new { id = po.PurchaseOrderID.ToString() });
            }

            ViewBag.ProductID = new SelectList(db.Inventories, "ProductID", "Name", purchaseOrderItem.ProductID);
            ViewBag.PurchaseOrderID = new SelectList(db.PurchaseOrders, "PurchaseOrderID", "PurchaseOrderID", purchaseOrderItem.PurchaseOrderID);
            return View(purchaseOrderItem);
        }

        public ActionResult Update(PurchaseOrderItem x)
        {
            return RedirectToAction("Index");
        }

        // GET: PurchaseOrderItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrderItem purchaseOrderItem = db.PurchaseOrderItems.Find(id);
            if (purchaseOrderItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductID = new SelectList(db.Inventories, "ProductID", "Name", purchaseOrderItem.ProductID);
            ViewBag.PurchaseOrderID = new SelectList(db.PurchaseOrders, "PurchaseOrderID", "PurchaseOrderID", purchaseOrderItem.PurchaseOrderID);
            return View(purchaseOrderItem);
        }

        // POST: PurchaseOrderItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "POItemID,ProductID,Quantity,Received,PurchaseOrderID")] PurchaseOrderItem purchaseOrderItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(purchaseOrderItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductID = new SelectList(db.Inventories, "ProductID", "Name", purchaseOrderItem.ProductID);
            ViewBag.PurchaseOrderID = new SelectList(db.PurchaseOrders, "PurchaseOrderID", "PurchaseOrderID", purchaseOrderItem.PurchaseOrderID);
            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrderItem purchaseOrderItem = db.PurchaseOrderItems.Find(id);
            if (purchaseOrderItem == null)
            {
                return HttpNotFound();
            }
            return View(purchaseOrderItem);
        }

        // POST: PurchaseOrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PurchaseOrderItem purchaseOrderItem = db.PurchaseOrderItems.Find(id);
            db.PurchaseOrderItems.Remove(purchaseOrderItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        /*
         * Scrapped return functionality
        // RETURN ITEM 1/3
        public ActionResult Return(int? id)
        {
            Session["POReturnItem"] = (int)id;
            Session["POReturnError"] = "";
            return RedirectToAction("ReturnItem");
        }


        // RETURN PURCHASE ORDER ITEM
        public ActionResult ReturnItem()
        {
            
            return View();
        }


        [HttpPost]
        public ActionResult ProcessReturn()
        {
            bool xisvalid = true;
            int quantityToReturn = -9999;
            try
            {
                quantityToReturn = Int32.Parse(Request["qty"].ToString());
            }
            catch
            {
                xisvalid = false;
            }
            PurchaseOrderItem p = db.PurchaseOrderItems.Find(Int32.Parse(Session["POReturnItem"].ToString()));
            if (p == null)
            {
                xisvalid = false;
            }
            else
            {
                int q = p.Quantity;
                if ((quantityToReturn > q) || (quantityToReturn < 0))
                {
                    xisvalid = false;
                }
            }
            if (xisvalid)
            {
                // Set quantity returned and adjust inventory level
                p.Inventory.Quantity -= quantityToReturn;
                p.Received -= quantityToReturn;
                db.SaveChanges();
                // Bounce back to detail page
                Session["POReturnError"] = "";
                return RedirectToAction("Details", "PurchaseOrders", new { id = Int32.Parse(Session["currPo"].ToString())});
            }
            else
            {
                Session["POReturnError"] = "Invalid return value";
                return RedirectToAction("ReturnItem");
            }
        }
        */


        // CANCEL PURCHASE ORDER ITEM (as if the item never existed)
        public ActionResult Cancel(int? id)
        {
            var x = db.PurchaseOrderItems.Find((int)id);
            db.PurchaseOrderItems.Remove(x);
            db.SaveChanges();
            return RedirectToAction("Details", "PurchaseOrders", new { id = Session["currPo"].ToString() });
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
