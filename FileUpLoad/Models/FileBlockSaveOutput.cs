using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUpLoad.Models
{
    public class FileBlockSaveOutput
    {
        /// <summary>
        /// 臨時編號
        /// </summary>
        public string UpLoadId { get; set; }

        /// <summary>
        /// 是否完成上傳
        /// </summary>
        public bool Finished { get; set; }


    }
}