using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FileUpLoad.Models
{
    /// <summary>
    /// 包含基本表单信息
    /// 可继承进行扩展
    /// </summary>
    public abstract class FormFileInput
    {

        /// <summary>
        /// Form表单中的文件列表
        /// </summary>
        internal List<FormFileInfo> formFiles { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public FormFileInput()
        {
            var reqFiles = HttpContext.Current.Request.Files;
            if (reqFiles.Count>0)
            {
                formFiles = new List<FormFileInfo>();
                for(var i = 0;i<reqFiles.Count;i++)
                {
                    if (!string.IsNullOrEmpty(reqFiles[i].FileName))
                    {
                        formFiles.Add(new FormFileInfo(reqFiles[i])
                        {
                            Key = reqFiles.Keys[i]
                        });
                    }
                    
                }
            }
        }
        
        /// <summary>
        /// 根据KEY【name属性】设置文件新保存路径&新名称
        /// </summary>
        /// <param name="key"></param>
        /// <param name="path">新路径可为空</param>
        /// <param name="name">新名称可为空</param>
        public void SetFileSavePathAndName(string key,string path,string name)
        {
            var formFile = formFiles?.Find(con => con.Key == key);
            if (path != null && formFile != null)
                formFile.SavePath = path;
            if (formFile != null)
            {
                if (path != null)
                    formFile.SavePath = path;
                if (name != null)
                    formFile.SaveName = name;
            }
        }

        /// <summary>
        /// 根据KEY【name属性】设置文件新保存路径
        /// </summary>
        /// <param name="key"></param>
        /// <param name="path">新路径可为空</param>
        public void SetFileSavePathAndName(string key, string path)
        {
            SetFileSavePathAndName(key, path, null);
        }

        /// <summary>
        /// 保存表单中所有文件
        /// </summary>
        public string[,] SaveFormFiles()
        {
            string[,] reData = new string[formFiles.Count, 2];
            for(var i = 0;i<formFiles.Count;i++)
            {
                reData[i, 0] = formFiles[i].Key;
                reData[i,1] = formFiles[i].SaveAs();
            }
            return reData;
        }
    }

    public class FormFileInfo
    {
        /// <summary>
        /// 臨時編號
        /// </summary>
        public string UpLoadId { get; set; }

        /// <summary>
        /// 文件塊數
        /// </summary>
        public int BlockTotal { get; set; }

        /// <summary>
        /// 當前塊數
        /// </summary>
        public int BlockIndex { get; set; }



        /// <summary>
        /// 单个文件數據
        /// </summary>
        private HttpPostedFile _fileData { get; set; }
        public FormFileInfo(HttpPostedFile fileData)
        {
            _fileData = fileData;
            SaveName = _fileData.FileName;
        }
        /// <summary>
        /// 和Form表单中的name属性对应
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 另存为文件名称
        /// 默认为原文件名
        /// </summary>
        public string SaveName { get; set; }

        /// <summary>
        /// 文件保存路径
        /// </summary>
        /// 默认路径为程式运行目录下UpLoad
        public string SavePath { get; set; } = Environment.CurrentDirectory + @"\UpLoad\";

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <returns>返回文件保存完整路径</returns>
        public string SaveAs()
        {
            //检查文件夹是否存在，不存在则创建
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            var fileFullPath = SavePath + SaveName;
            _fileData.SaveAs(fileFullPath);
            return fileFullPath;
        }

    }
}