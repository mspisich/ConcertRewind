using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ConcertRewind.Models;
using System.Globalization;

namespace ConcertRewind.Controllers
{
    public class ConcertDBsController : Controller
    {
        public ConcertDBEntities db = new ConcertDBEntities();

        // GET: ConcertDBs
        public ActionResult Index()
        {
            List<ConcertDB> SortedList = db.ConcertDBs.ToList();
            SortedList = SortedList.OrderByDescending(o => o.Times_Searched).Take(10).ToList();
            
            return View(SortedList);
        }

        // GET: ConcertDBs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConcertDB concertDB = db.ConcertDBs.Find(id);
            if (concertDB == null)
            {
                return HttpNotFound();
            }
            return View(concertDB);
        }

        // GET: ConcertDBs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ConcertDBs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Artist_Name,Times_Searched")] ConcertDB concertDB)
        {
            if (ModelState.IsValid)
            {
                db.ConcertDBs.Add(concertDB);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(concertDB);
        }

        // GET: ConcertDBs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConcertDB concertDB = db.ConcertDBs.Find(id);
            if (concertDB == null)
            {
                return HttpNotFound();
            }
            return View(concertDB);
        }

        // POST: ConcertDBs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Artist_Name,Times_Searched")] ConcertDB concertDB)
        {
            if (ModelState.IsValid)
            {
                db.Entry(concertDB).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(concertDB);
        }

        // GET: ConcertDBs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConcertDB concertDB = db.ConcertDBs.Find(id);
            if (concertDB == null)
            {
                return HttpNotFound();
            }
            return View(concertDB);
        }

        // POST: ConcertDBs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ConcertDB concertDB = db.ConcertDBs.Find(id);
            db.ConcertDBs.Remove(concertDB);
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
