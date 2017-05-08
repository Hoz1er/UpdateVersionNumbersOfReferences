using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UpdateCiteVersion
{
    class Program
    {
        private static string projectPath = "";
        private static string masterPath = "";
        private static string backupPath = "";
        private static string logPath = "";
        private static string tempFile = "";
        private static string verName = "hgx_ver";//版本号的参数名
        private static Dictionary<string, string> fileHash = new Dictionary<string, string>();
        private static List<string> failedFiles = new List<string>();

        private static List<string> fileType = new List<string>() { "html", "htm", "asp", "aspx", "master" };//需更新引用版本的文件后缀
        static void Main(string[] args)
        {
            do
            {
                do
                {
                    Console.Write("Project Path:");
                    projectPath = Console.ReadLine().Replace('/', '\\');
                } while (!Directory.Exists(projectPath));

                while (projectPath[projectPath.Length - 1] == '\\')
                {
                    projectPath = projectPath.Substring(0, projectPath.Length - 1);
                }
                // projectPath <- D:\test
                if (projectPath.LastIndexOf('\\') < 0)
                    Console.WriteLine(" Path Inputed is not Deep Enough! ");
            }
            while (projectPath.LastIndexOf('\\') < 0);

            Console.WriteLine("<--------------------Begin-------------------->");

            masterPath = projectPath.Substring(0, projectPath.LastIndexOf('\\')) + "\\UpdateCiteVersionFolder";    // acvPath <- D:\UpdateCiteVersionFolder
            logPath = masterPath + "\\Log\\Log.txt";                                                            // logPath <- D:\UpdateCiteVersionFolder\Log\Log.txt
            backupPath = masterPath + "\\Backup";                                                               // backupPath <- D:\UpdateCiteVersionFolder\Backup

            if (Directory.Exists(masterPath))
                Directory.Delete(masterPath, true);
            Directory.CreateDirectory(masterPath + "\\Log");
            Directory.CreateDirectory(backupPath);

            DirectoryInfo theFolder = new DirectoryInfo(projectPath);
            ChangeAllFile(theFolder);

            if (!string.IsNullOrWhiteSpace(tempFile) && File.Exists(tempFile))
                File.Delete(tempFile);

            if (failedFiles.Count > 0)
            {
                Console.WriteLine("[ Warning ]存在目录过深，无法替换的文件：");
                for (int i = 0; i < failedFiles.Count; i++)
                {
                    Console.WriteLine(failedFiles[i]);
                }
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("<--------------------End. Print Any Key To Exit------------>");
            Console.Read();
        }

        /// <summary>
        /// 递归调用  更新所有文件
        /// </summary>
        /// <param name="dirInfo"></param>
        private static void ChangeAllFile(DirectoryInfo dirInfo)
        {
            DirectoryInfo[] new_dirInfo = dirInfo.GetDirectories();
            //本文件夹下的文件
            FileInfo[] fileInfo = dirInfo.GetFiles();
            foreach (FileInfo i_fileInfo in fileInfo)
            {
                bool exFlag = false;
                try
                {
                    string str = i_fileInfo.FullName;
                }
                catch (PathTooLongException)
                {
                    failedFiles.Add(i_fileInfo.Directory + "/" + i_fileInfo.Name);
                    exFlag = true;
                }
                if (exFlag)
                    continue;
                string[] temp = i_fileInfo.FullName.Split('.');

                if (fileType.Contains(temp[temp.Length - 1].ToLower()))
                    ChangeVersion(i_fileInfo);
            }
            //本文件夹下的文件夹
            foreach (DirectoryInfo i_dirInfo in new_dirInfo)
                ChangeAllFile(i_dirInfo);
        }
        /// <summary>
        /// 处理文件
        /// </summary>
        /// <param name="file"></param>
        private static void ChangeVersion(FileInfo file)
        {
            string savePath = file.DirectoryName.Replace(projectPath, backupPath);
            Directory.CreateDirectory(savePath);
            file.CopyTo(savePath + "\\" + file.Name);

            string thepath = file.FullName;
            StreamReader sr = new StreamReader(thepath);

            tempFile = masterPath + '\\' + file.Name;

            StreamWriter sw = new StreamWriter(tempFile, false, new UTF8Encoding(true));
            string line = "";
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if ((line.IndexOf("<script") > -1 && line.IndexOf("src=\"") > -1) ||
                    (line.IndexOf("<link") > -1 && line.IndexOf("href=\"") > -1))
                {   //<script src="/common.js" type="text/javascript"></script>
                    //<link href="/main.css" rel = "stylesheet" />
                    line = ChangeStrVersion(line,thepath);
                }
                line += "\r\n";
                sw.Write(line);
            }
            sr.Close();
            sw.Close();

            file.Delete();
            File.Copy(tempFile, thepath);

            if (!string.IsNullOrWhiteSpace(tempFile) && File.Exists(tempFile))
                File.Delete(tempFile);
            Console.WriteLine("[ ChangeSuccess ]" + thepath.Replace(projectPath, "~"));

        }
        /// <summary>
        /// 处理引用字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ChangeStrVersion(string str,string theFile)
        {
            int beginIndex = Math.Max(str.IndexOf("src=\"") + 4, str.IndexOf("href=\"") + 5);

            string strUrl = str.Substring(beginIndex + 1);
            strUrl = strUrl.Substring(0, strUrl.IndexOf("\""));

            string strBeforeUrl = str.Substring(0, beginIndex + 1);
            string strAfterUrl = str.Substring(beginIndex + strUrl.Length + 1);
            string strBeforePar = strUrl;
            string strPar = "";
            int parIndex = strUrl.IndexOf("?");
            if (parIndex >= 0)
            {
                strBeforePar = strBeforePar.Substring(0, parIndex);
                strPar = strUrl.Substring(parIndex + 1);
            }

            #region 分析引用文件的完整路径 -> citeFile
            string citeFile = "";
            if (strBeforePar.Substring(0, 3) == "../")
            {
                string tempUrl = strBeforePar;
                theFile = theFile.Substring(0, theFile.LastIndexOf('\\') + 1);
                while (tempUrl.Substring(0, 3) == "../")
                {
                    theFile = theFile.Substring(0, theFile.LastIndexOf('\\') + 1);
                    tempUrl = tempUrl.Substring(3);
                }
                citeFile = theFile + tempUrl;
            }
            else
            {
                if (strBeforePar[0] != '/')
                    citeFile = theFile.Substring(0, theFile.LastIndexOf('\\') + 1) + strBeforePar.Replace("/", "\\");
                else
                    citeFile = projectPath + strBeforePar.Replace("/","\\");
            }
            #endregion
            
            if (!fileHash.ContainsKey(citeFile))
                fileHash.Add(citeFile, getFileHash(citeFile));

            Dictionary<string, string> p = new Dictionary<string, string>();
            p[verName] = fileHash[citeFile];

            if (!string.IsNullOrEmpty(strPar))
            {
                string[] parList = strPar.Split('&');
                for (int i = 0; i < parList.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parList[i]))
                    {
                        string[] parItem = parList[i].Split('=');
                        if (parItem.Length == 2 && parItem[0] != verName)
                            p[parItem[0]] = parItem[1];
                    }
                }
            }
            string strNewPar = "?";
            foreach (var item in p)
            {
                if (strNewPar != "?")
                    strNewPar += "&";
                strNewPar += item.Key + "=" + item.Value;
            }
            string newStr = strBeforeUrl + strBeforePar + strNewPar + strAfterUrl;

            return newStr;
        }
        /// <summary>
        /// 获取文件的hash值，并去掉‘-’
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string getFileHash(string fileName)
        {
            if (!File.Exists(fileName))
                return "";
            string hash = "";
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    var hashcode = System.Security.Cryptography.HashAlgorithm.Create();
                    hash = BitConverter.ToString(hashcode.ComputeHash(fs));
                    hash = hash.Replace("-", "");
                }
            }
            catch (Exception ex)
            {
                WriteLog("(getFileHash)" + ex.Message);
            }

            return hash;

        }
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="str"></param>
        private static void WriteLog(string str)
        {
            try
            {
                if (!File.Exists(logPath))
                    File.Create(logPath).Close();

                using (FileStream fs = new FileStream(logPath, FileMode.Append))
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.WriteLine("[ " + DateTime.Now.ToString() + " ]: " + str);
                    sw.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[ Error ] [ Cant Write Log ]");
            }
        }
    }
}
