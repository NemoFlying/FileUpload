using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUpLoad.Models
{
    public class FormFileUploadOuput
    {
        /// <summary>
        /// 臨時編號
        /// </summary>
        public string UpLoadId { get; set; }

        /// <summary>
        /// 當前塊數
        /// </summary>
        public int BlockIndex { get; set; }

        /// <summary>
        /// Block保存状态
        /// </summary>
        public bool BlockSaveStatus { get; set; } = false;

        /// <summary>
        /// 文件保存状态
        /// </summary>
        public bool FileSaveStatus { get; set; } = false;

        /// <summary>
        /// 文件保存错误信息
        /// </summary>
        public string FileSaveErrMsg { get; set; }


    }
}