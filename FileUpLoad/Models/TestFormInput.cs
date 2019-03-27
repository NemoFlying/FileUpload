using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUpLoad.Models
{
    public class TestFormInput : FormFileInput
    {
        public string Param1 { get; set; }

        public TestFormInput()
        {
            //配置文件保存路径
            base.SetFileSavePathAndName("file1", @"D:\UploadTest\Img\");
            base.SetFileSavePathAndName("file2", @"D:\UploadTest\文件");
        }

        /// <summary>
        /// 保存表单文件信息
        /// </summary>
        public string[,] FormFilesSave()
        {
            return base.SaveFormFiles();
        }
    }
}