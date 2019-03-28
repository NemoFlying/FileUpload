using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
namespace FileUpLoad.Models
{
    public class FormInput2
    {
        /// <summary>
        /// 臨時編號
        /// </summary>
        public string UpLoadId { get; set; }

        /// <summary>
        /// 文件名稱
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件塊數
        /// </summary>
        public int TotalBlock { get; set; }

        /// <summary>
        /// 當前塊數
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 當前塊數據
        /// </summary>
        public HttpPostedFile file
        {
            get { return HttpContext.Current.Request.Files[0]; }
        }

        public FileBlockSaveOutput SaveBlock()
        {
            var result = new FileBlockSaveOutput()
            {
                UpLoadId = UpLoadId
            };
            if (string.IsNullOrWhiteSpace(UpLoadId)&&Index>1)
            {
                return result;
            }

            if(Index==1)
            {
                UpLoadId = Guid.NewGuid().ToString().Replace("-", "");
                result.UpLoadId = UpLoadId;
            }

            var blockName = $"{UpLoadId}_{Index}.block";

            var blockPath = @"E:\UploadTest\Temp\";

            var blockFullPath = Path.Combine(blockPath, blockName);
            if(File.Exists(blockFullPath))
            {
                //塊已經上傳，
                return result;
            }
            file.SaveAs(blockFullPath);
            //塊合併
            if(Directory.GetFiles(blockPath).Count().Equals(TotalBlock))
            {
                var fileFullPath = @"E:\UploadTest\" + FileName;
                using (var fs = new FileStream(fileFullPath, FileMode.Create))
                {
                    for (var i = 1; i <= TotalBlock; i++)
                    {
                        var path = Path.Combine(blockPath, $"{UpLoadId}_{i}.block");
                        var bytes = File.ReadAllBytes(path);
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    Directory.Delete(blockPath, true);
                }
                result.Finished = true;
            }
            return result;

        }


    }
}