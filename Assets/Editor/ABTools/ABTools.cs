using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

    public class ABTools : EditorWindow
    {
        private int curSelIndex = 0;
        private string[] targetStrs = new string[] { "PC", "IOS", "Android" };
        private string serverIP = "FTP://127.0.0.1";

        [MenuItem("AB包工具/打开工具窗口")]
        private static void OpenWindow()
        {
            //获取一个ABTools编辑器窗口对象
            ABTools window = EditorWindow.GetWindowWithRect(typeof(ABTools), new Rect(0, 0, 380, 250)) as ABTools;
            window.Show();
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 150, 15), "平台选择");
            curSelIndex = GUI.Toolbar(new Rect(10, 30, 250, 20), curSelIndex, targetStrs);
            //资源服务器IP地址设置
            GUI.Label(new Rect(10, 60, 150, 15), "资源服务器地址");
            serverIP = GUI.TextField(new Rect(10, 80, 150, 20), serverIP);
            //创建按钮
            if (GUI.Button(new Rect(10, 110, 100, 40), "创建对比文件"))
            {
                CreateABCompareFile();
            }
            if (GUI.Button(new Rect(115, 110, 250, 40), "移动所选资源到StreamingAssets中"))
            {
                MoveABToStreamingAssets();
            }
            if (GUI.Button(new Rect(10, 160, 355, 40), "上传AB包和对比文件"))
            {
                UploadAllABFile();
            }

        }

        /// <summary>
        /// 创建对比文件
        /// </summary>
        private void CreateABCompareFile()
        {
            //根据所选平台 将其保存在指定平台文件夹
            DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetStrs[curSelIndex] + "/");
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

            File.WriteAllText(Application.dataPath + "/ArtRes/AB/" + targetStrs[curSelIndex] + "/ABCompareInfo.txt", abCompareInfo);
            AssetDatabase.Refresh();    //刷新编辑器
        }
        /// <summary>
        /// 获取资源MD5码
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetMD5(string filePath)
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

        /// <summary>
        /// 移动所选资源到StreamingAssets中
        /// </summary>
        private void MoveABToStreamingAssets()
        {
            //通过编辑器Selection类中的方法 获取在Project窗口中选中的资源
            UnityEngine.Object[] selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            if (selectedAssets.Length == 0)
            {
                return;
            }

            string abCompareInfo = "";  //用于拼接本地默认AB包资源信息的字符串

            foreach (var asset in selectedAssets)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                string fileName = assetPath.Substring(assetPath.LastIndexOf('/'));

                if (fileName.LastIndexOf('.') != -1)
                {
                    //如果有后缀，则不处理该文件
                    continue;
                }

                AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);    //拷贝资源

                //获取拷贝到StreamingAssets文件夹中的文件的全部信息
                FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName);
                abCompareInfo += fileInfo.Name + " " + fileInfo.Length + " " + CreateABCompare.GetMD5(fileInfo.FullName);
                abCompareInfo += "|";
            }
            abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);   //去掉最后一个符号
            File.WriteAllText(Application.streamingAssetsPath + "/ABCompareInfo.txt", abCompareInfo);

            AssetDatabase.Refresh();
        }


        /// <summary>
        /// 上传AB包和对比文件
        /// </summary>
        private void UploadAllABFile()
        {
            DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetStrs[curSelIndex] + "/");
            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                if (string.IsNullOrEmpty(file.Extension) || file.Extension == ".txt")
                {
                    //没有文件后缀或者后缀为txt(因为该文件夹中 只有对比文件的格式才是txt，所以可以使用.txt判断)
                    FtpUploadFile(file.FullName, file.Name);
                }
            }
        }

        /// <summary>
        /// FTP上传文件
        /// </summary>
        /// <param name="filePath">文件所处文件路径</param>
        /// <param name="fileName">文件名</param>
        private async void FtpUploadFile(string filePath, string fileName)
        {
            await Task.Run(() =>
            {
                try
                {
                    //创建一个FTP连接 用于上传
                    FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AB/" + targetStrs[curSelIndex] + "/" + fileName)) as FtpWebRequest;
                    //设置通信凭证
                    NetworkCredential n = new NetworkCredential("luoying", "1234");
                    req.Credentials = n;
                    //其他设置
                    //  设置代理为空
                    req.Proxy = null;
                    //  请求完毕后 是否关闭控制连接
                    req.KeepAlive = false;
                    //  操作命令 - 上传
                    req.Method = WebRequestMethods.Ftp.UploadFile;
                    //  指定传输类型
                    req.UseBinary = true;
                    //上传文件

                    //获取ftp的流对象
                    Stream uploadStream = req.GetRequestStream();
                    //读取文件信息 写入该流对象
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        byte[] bytes = new byte[2048];  //一点一点地上传内容
                                                        //返回值 代表读取了多少个字节
                        int contentLength = fs.Read(bytes, 0, bytes.Length);

                        //循环上传文件中的内容
                        while (contentLength > 0)
                        {
                            uploadStream.Write(bytes, 0, contentLength);
                            //写完后再读取
                            contentLength = fs.Read(bytes, 0, bytes.Length);
                        }

                        //循环完毕 证明上传结束
                        fs.Close();
                        uploadStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("文件上传失败：" + ex.Message);
                }

                Debug.Log(fileName + "文件上传成功");
            });
        }
    }
