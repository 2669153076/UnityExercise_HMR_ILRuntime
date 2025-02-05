using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

    /// <summary>
    /// 生成AB包对比文件
    /// </summary>

    public class CreateABCompare
    {
        //[MenuItem("AB包工具/创建对比文件")]
        public static void CreateABCompareFile()
        {
            DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/PC/");
            FileInfo[] fileInfos = directory.GetFiles();

            string abCompareInfo = "";  //用于存储信息的字符串

            foreach (FileInfo fileInfo in fileInfos)
            {
                //Debug.Log("文件名"+fileInfo.Name);
                //Debug.Log("文件路径"+fileInfo.FullName);
                //Debug.Log("文件后缀" + fileInfo.Extension);
                //Debug.Log("文件大小" + fileInfo.Length);
                if (string.IsNullOrEmpty(fileInfo.Extension))
                {
                    abCompareInfo += fileInfo.Name + " " + fileInfo.Length + " " + GetMD5(fileInfo.FullName);
                    //分隔符分开不同文件之间的信息
                    abCompareInfo += '|';
                }

            }
            //循环完毕后切除 | 符号
            abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);

            Debug.Log(abCompareInfo);

            File.WriteAllText(Application.dataPath + "/ArtRes/AB/PC/ABCompareInfo.txt", abCompareInfo);
            AssetDatabase.Refresh();    //刷新编辑器
        }

        public static string GetMD5(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] md5Infos = md5.ComputeHash(fs);

                fs.Close();

                StringBuilder sb = new StringBuilder();
                foreach (var info in md5Infos)
                {
                    sb.Append(info.ToString("x2"));
                }

                return sb.ToString();
            }
        }

    }
