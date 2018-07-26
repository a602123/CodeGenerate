using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CodeGenerate
{
    public class CodeGeneratorHelper
    {

        #region client

        /// <summary>
        /// 生成ControllerClass
        /// </summary>
        /// <param name="className"></param>
        public static void SetControllerClass(string className, string primary_Key_Here)
        {
           /* string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Client\Mvc\ControllerClass\MainTemplate.txt";
            var templateContent = Read(appServiceIntercafeClassDirectory);

            templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
                                             .Replace("{{Namespace_Relative_Full_Here}}", className)
                                             .Replace("{{Entity_Name_Plural_Here}}", className)
                                             .Replace("{{Entity_Name_Here}}", className)
                                             .Replace("{{Permission_Name_Here}}", $"Pages_Administration_{className}")
                                             .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
                                             .Replace("{{Primary_Key_Here}}", primary_Key_Here)
                                             .Replace("{{Project_Name_Here}}", Configuration.Controller_Base_Class)
                                             .Replace("{{entity_Name_Plural_Here}}", GetFirstToLowerStr(className))
                                             ;
            Write(Configuration.Web_Mvc_Directory + "Areas\\Admin\\Controllers\\", className + "Controller.cs", templateContent);*/
        }

        public static void ReplaceGenerate(string namespacestr)
        {
            DirectoryInfo i = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Template\");
            doReadAndWrite(i,namespacestr);
            
        }
        static void  doReadAndWrite(DirectoryInfo i, string namespacestr)
        {
            var files = i.GetFiles("*.cs");
            foreach (var item in files)
            {
                var savefilename = item.Name.Split('.')[0] + ".cs";
                var templateContent = Read(item.FullName);
                templateContent = templateContent.Replace("#%namespace%#", namespacestr);
                Write(i.FullName.Replace("Template", "ResultFile")+@"\", savefilename, templateContent);
            }
            foreach(var item in i.GetDirectories())
            {
              
                doReadAndWrite(item, namespacestr);
            }
        }

        #endregion

            #region 文件读取
        public static string Read(string path)
        {
            using (StreamReader sr = new StreamReader(path,Encoding.Default))
            {
                StringBuilder sb = new StringBuilder();

                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line.ToString());
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">文件保存路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="templateContent">模板内容</param>
        public static void Write(string filePath, string fileName, string templateContent)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            using (FileStream fs = new FileStream(filePath + fileName, FileMode.Create))
            {
                //获得字节数组
                byte[] data = Encoding.Default.GetBytes(templateContent);
                //开始写入
                fs.Write(data, 0, data.Length);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">文件保存路径</param>
        /// <param name="templateContent">模板内容</param>
        public static void Write(string filePath, string templateContent)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                //获得字节数组
                byte[] data = Encoding.Default.GetBytes(templateContent);
                //开始写入
                fs.Write(data, 0, data.Length);
            }

        }
        #endregion

        #region 首字母小写
        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetFirstToLowerStr(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Length > 1)
                {
                    return char.ToLower(str[0]) + str.Substring(1);
                }
                return char.ToLower(str[0]).ToString();
            }
            return null;
        }
        #endregion
    }
}
