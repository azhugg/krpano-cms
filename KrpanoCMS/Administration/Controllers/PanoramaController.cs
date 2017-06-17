﻿using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KrpanoCMS.Rename;
using Microsoft.AspNet.Identity;

namespace KrpanoCMS.Administration.Controllers
{
    public class PanoramaController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Index()
        {
            return View(db.Panorama.ToList().Where(item => item.UserId == User.Identity.GetUserId()).OrderByDescending(item => item.AddedOn));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Panorama panorama, HttpPostedFileBase photo)
        {
            if (photo != null)
            {
                panorama.PictureUrl = photo.FileName;
            }

            panorama.UserId = User.Identity.GetUserId();
            panorama.AddedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Panorama.Add(panorama);
                db.SaveChanges();

                string extension = Path.GetExtension(photo.FileName);
                FileUploader.Upload(photo, panorama.Id + extension);

                return RedirectToAction("CreatePano", new { id = panorama.Id, userId = panorama.UserId });
            }

            return View(panorama);
        }


        public ActionResult CreatePano(int id, string userId)
        {
            ViewBag.PanoId = id;

            return View();
        }

        public JsonResult ExecuteCommandCreatePano(int id, string type, int hfov, int vfov)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            var krpanoPath = System.Web.HttpContext.Current.Server.MapPath(@"/Krpano");
            var krpanoImgPath = System.Web.HttpContext.Current.Server.MapPath(@"~/Documents/Panoramas/" + id + ".jpg");

            var panoramaConfig = @" -config=templates\custom.config -panotype=" + type;
            if (type == "cylinder" || type == "sphere")
            {
                panoramaConfig += @" -hfov=" + hfov;
            }
            if (type == "sphere")
            {
                panoramaConfig += @" -vfov=" + vfov;
            }

            cmd.StandardInput.WriteLine(@"cd """ + krpanoPath + @"""");
            cmd.StandardInput.WriteLine(@"start krpanotools64.exe makepano"
                + panoramaConfig
                + @" """ + krpanoImgPath + @"""");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Debug.WriteLine(cmd.StandardOutput.ReadToEnd());

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Panorama panorama = db.Panorama.Find(id);
            if (panorama == null)
            {
                return HttpNotFound();
            }
            return View(panorama);
        }

        [HttpPost]
        public ActionResult Edit(Panorama panorama)
        {
            panorama.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.Entry(panorama).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(panorama);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Panorama panorama = db.Panorama.Find(id);
            if (panorama == null)
            {
                return HttpNotFound();
            }
            return View(panorama);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Panorama panorama = db.Panorama.Find(id);
            if (panorama == null)
            {
                return HttpNotFound();
            }

            return View(panorama);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Panorama panorama = db.Panorama.Find(id);

            db.Panorama.Remove(panorama);
            db.SaveChanges();

            var rootFolderPath = System.Web.HttpContext.Current.Server.MapPath(@"/Documents/Panoramas/");

            Directory.Delete(rootFolderPath + id, true);

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
