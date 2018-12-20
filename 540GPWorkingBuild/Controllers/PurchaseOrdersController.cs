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
    public class PurchaseOrdersController : Controller
    {

        private MusciToolkitDBEntities db = new MusciToolkitDBEntities();

        // Bridge between PurchaseOrder and PurchaseOrderItems
        public class poWithItems
        {
            public IEnumerable<PurchaseOrderItem> itemList { get; private set; }
            public PurchaseOrder p { get; private set; }

            // Pass a specific Po and entirety of DB's PurchaseOrderItems
            public poWithItems(PurchaseOrder x, IEnumerable<PurchaseOrderItem> y)
            {
                p = x;
                itemList = y;
            }
        }

        // Get a poWithItems object based on a given purchase order ID and db instance
        public poWithItems getOrderWithItems(int givenID, MusciToolkitDBEntities dbInstance)
        {

            var ansPO = db.PurchaseOrders.SingleOrDefault(x => x.PurchaseOrderID == givenID);
            var ansList = db.PurchaseOrderItems.Where(x => x.PurchaseOrderID == givenID);
            // Declare total line price for each PurchaseOrderItem in the list.. Shoutout to Luke!!
            foreach (var each in ansList)
            {
                double currLineCost = (double)each.Quantity * (double)each.Inventory.NetPrice;
                each.totalPrice = currLineCost;
            }
            db.SaveChanges();
            var ans = new poWithItems(ansPO, ansList);
            poTotalSet(ans);
            return ans;

        }

        // Calculates and sets the total price
        // for a purchase order given a
        // poWithItems object
        public void poTotalSet(poWithItems x)
        {
            var allItems = x.itemList;
            double ans = 0;
            foreach (var line in allItems)
            {
                double lineTotal = line.Quantity * (double)line.Inventory.NetPrice;
                ans += lineTotal;
            }
            x.p.totalPrice = ans;
            return;
        }




        // GET: PurchaseOrders (show all purchase orders)
        public ActionResult Index()
        {
            if (Session["UserRole"] != null)
            {
                return View(db.PurchaseOrders.ToList());
            }
            else
            {
                return RedirectToAction("LowPermission");
            }
        }


        // Landing page
        public ActionResult PurchaseOrderHome()
        {
            if (Session["UserRole"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("LowPermission");
            }
        }

        // GET: PurchaseOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(id);
            if (purchaseOrder == null)
            {
                return HttpNotFound();
            }
            var x = getOrderWithItems((int)id, db);
            Session["currPo"] = x.p.PurchaseOrderID.ToString();
            return View(x);
        }

        // GET: PurchaseOrders/Create
        public ActionResult Create()
        {
            PurchaseOrder p = new PurchaseOrder();

            return Create(p);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PurchaseOrderID,OrderDate")] PurchaseOrder purchaseOrder)
        {
            if (ModelState.IsValid)
            {
                purchaseOrder.OrderDate = DateTime.Now;
                purchaseOrder.isReceived = false;
                db.PurchaseOrders.Add(purchaseOrder);
                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = purchaseOrder.PurchaseOrderID });
        }

        // GET: PurchaseOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(id);
            if (purchaseOrder == null)
            {
                return HttpNotFound();
            }
            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PurchaseOrderID,OrderDate")] PurchaseOrder purchaseOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(purchaseOrder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(id);
            if (purchaseOrder == null)
            {
                return HttpNotFound();
            }
            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(id);
            db.PurchaseOrders.Remove(purchaseOrder);
            db.SaveChanges();
            return RedirectToAction("Index");
        }





        // RECEIVE PURCHASE ORDER
        public ActionResult Receive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            poWithItems entireOrder = getOrderWithItems((int)id, db);
            if (entireOrder == null)
            {
                return HttpNotFound();
            }

            foreach (var line in entireOrder.itemList)
            {
                line.Inventory.Quantity += line.Quantity;
                // Set receieved to 'true'
                line.Received = 1;
            }
            db.SaveChanges();
            return RedirectToAction("Details", new { id = entireOrder.p.PurchaseOrderID });
        }


        // CANCEL A PURCHASE ORDER (bring it to a state of never having existed)
        public ActionResult Cancel(int? id)
        {
            // Must be Manager or Admin to cancel a purchase order
            if (!(Session["UserRole"].Equals("Manager") || Session["UserRole"].Equals("Admin")))
            {
                return RedirectToAction("LowPermission");
            }
            poWithItems entireOrder = getOrderWithItems((int)id, db);
            foreach (var item in entireOrder.itemList)
            {
                db.PurchaseOrderItems.Remove(item);
            }
            db.PurchaseOrders.Remove(entireOrder.p);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Returns total price of a purchase order
        // given a purchase order ID
        public double getTotalPrice(int id)
        {
            poWithItems x = getOrderWithItems(id, db);
            return x.p.totalPrice;
        }

        // Returns a string status OPEN or CLOSED
        // depending on the state of a PO(id)
        public string getStatus(int id)
        {
            poWithItems x = getOrderWithItems(id, db);
            if (!(x.itemList.Any()))
            {
                return "OPEN";
            }
            if (x.itemList.First().Received == 0)
            {
                return "OPEN";
            }
            return "CLOSED";
        }

        // Method to control search functionality
        public ActionResult Search(string option, string search)
        {
            Session["searchdebug"] = search;
            List<poWithItems> poListComplete = new List<poWithItems>();
            List<PurchaseOrder> poList = db.PurchaseOrders.ToList();
            foreach (var item in poList)
            {
                int currID = item.PurchaseOrderID;
                poWithItems currItem = getOrderWithItems(currID, db);
                poListComplete.Add(currItem);
            }
            //if a user choose the radio button option as Subject  
            if (option == "ID")
            {
                try
                {
                    //Index action method will return a view with a student records based on what a user specify the value in textbox  
                    return View(poListComplete.Where(x => x.p.PurchaseOrderID == Int32.Parse(search) || search == null).ToList());
                }
                catch
                {
                    return View(new List<poWithItems>());
                }
            }
            else if (option == "Date")
            {
                return View(poListComplete.Where(x => x.p.dateStr.Equals(search) || search == null).ToList());
            }
            else if (option == "Status")
            {
                return View(poListComplete.Where(x => getStatus(x.p.PurchaseOrderID).Equals(search) || search == null).ToList());
            }
            else
            {
                return View(new List<poWithItems>());
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
