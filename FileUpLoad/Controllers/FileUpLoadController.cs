using FileUpLoad.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileUpLoad.Controllers
{
    public class FileUpLoadController : Controller
    {
        // GET: FileUplod
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public void ReciveFile1()
        {
            var form = Request.Form;
            var files = Request.Files;
            //foreach(var file in files.Keys)
            for(var i = 0;i<files.Count;i++)
            {
                var fileData = files[i];
                try
                {
                    string filePath = Server.MapPath("~/Uploads/");
                    if(!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    string fileName = Path.GetFileName(fileData.FileName);
                    string fileExtension = Path.GetExtension(fileName);
                    string saveName = Guid.NewGuid().ToString() + fileExtension;
                    fileData.SaveAs(filePath + saveName);
                }
                catch(Exception ex)
                {
                    
                }
            }
        }
        [HttpPost]
        public string ReciveFile2(TestFormInput input)
        {
            var result = input.SaveFormFiles();
            return "OK";
            //var files = input.Files;
            //var files = HttpContext.Request.Files;

            
            //foreach(var file in files.Keys)
            //for (var i = 0; i < files.Count; i++)
            //{
            //    var fileData = files[i];
            //    try
            //    {
            //        string filePath = Server.MapPath("~/Uploads/");
            //        if (!Directory.Exists(filePath))
            //        {
            //            Directory.CreateDirectory(filePath);
            //        }
            //        string fileName = Path.GetFileName(fileData.FileName);
            //        string fileExtension = Path.GetExtension(fileName);
            //        string saveName = Guid.NewGuid().ToString() + fileExtension;
            //        fileData.SaveAs(filePath + saveName);
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
        }

        [HttpPost]
        public JsonResult ReciveFile3(FormInput2 input)
        {
            var kk = input.SaveBlock();

            return Json(kk, JsonRequestBehavior.AllowGet);
        }


    }
}