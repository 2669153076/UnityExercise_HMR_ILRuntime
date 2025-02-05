using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Custom.BaseFramework
{
    public class ABUpdateMgr : SingletonAutoMono<ABUpdateMgr>
    {
        //private static ABUpdateMgr instance;
        //public static ABUpdateMgr Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            GameObject obj = new GameObject("ABUpdateMgr");
        //            instance = obj.AddComponent<ABUpdateMgr>();

        //        }
        //        return instance;
        //    }
        //}

        private string ftpUsername = "luoying";
        private string ftpPassword = "1234";
        private string serverIp = "ftp://127.0.0.1";

        private List<string> downloadList = new List<string>(); //待下载的AB包的资源名的列表
        private Dictionary<string, ABInfo> remoteABInfoDic = new Dictionary<string, ABInfo>();  //存储远端对比文件信息
        private Dictionary<string, ABInfo> localABInfoDic = new Dictionary<string, ABInfo>();   //存储本地对比文件信息


        //private void OnDestroy()
        //{
        //    instance = null;
        //}

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="overCallback">对比完成要执行的事件</param>
        /// <param name="updateInfoCallback">更新信息要执行的事件</param>
        /// <param name="updateProgressCallback">更新进度条事件，当前进度/最大进度</param>
        public void CheckUpdate(UnityAction<bool> overCallback, UnityAction<string> updateInfoCallback,UnityAction<float,float> updateProgressCallback)
        {
            //清空上次残留信息
            remoteABInfoDic.Clear();
            localABInfoDic.Clear();
            downloadList.Clear();

            //1.加载远端资源对比文件
            DownloadABCompareFile((isOver) =>
            {
                updateInfoCallback("开始更新资源");
                if (isOver)
                {
                    updateProgressCallback(1f, 5f);
                    updateInfoCallback("远端对比文件下载结束");
                    string remoteInfo = File.ReadAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt");
                    updateInfoCallback("解析远端对比文件");
                    GetABCompareFileInfo(remoteInfo, remoteABInfoDic);
                    updateInfoCallback("解析远端对比文件完成");


                    //2.加载本地资源对比文件
                    GetLocalABCompareFileInfo((isOver) =>
                    {
                        if (isOver)
                        {
                            updateInfoCallback("解析本地对比完成");
                            updateInfoCallback("开始对比");
                            updateProgressCallback(2f, 5f);
                            //3.对比 进行AB包下载
                            //1.判断哪些资源时新的然后记录之后用于下载
                            //2.判断哪些资源是需要更新的然后记录之后用于下载
                            //3.判断哪些资源需要删除
                            foreach (var abName in remoteABInfoDic.Keys)
                            {
                                if (!localABInfoDic.ContainsKey(abName))
                                {
                                    //本地对比信息中没有该文件，则将其添加进待下载列表
                                    downloadList.Add(abName);
                                }
                                else
                                {
                                    //本地信息中有该文件
                                    //判断哪些文件需要更新
                                    if (localABInfoDic[abName].md5 != remoteABInfoDic[abName].md5)
                                    {
                                        downloadList.Add(abName);
                                    }

                                    localABInfoDic.Remove(abName);
                                    //本地中剩下来的就是远端文件中没有的文件信息
                                    //表示需要将这些本地文件删除
                                }
                            }
                            updateProgressCallback(3f, 5f);
                            updateInfoCallback("对比完成");
                            updateInfoCallback("删除无用AB包文件");

                            foreach (var abName in localABInfoDic.Keys)
                            {
                                if (File.Exists(Application.persistentDataPath + "/" + abName))
                                {
                                    //删除可读写文件夹中的多余的文件
                                    File.Delete(Application.persistentDataPath + "/" + abName);
                                }
                            }
                            updateProgressCallback(4f, 5f);
                            updateInfoCallback("下载并更新AB包资源");
                            //下载待更新列表中所有AB包
                            DownloadABFile((isOver) =>
                            {
                                if (isOver)
                                {
                                    //下载完所有AB包文件后
                                    //将远端对比文件 存储在本地
                                    File.WriteAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt", remoteInfo);
                                    updateInfoCallback("下载更新完毕");
                                }
                                updateProgressCallback(5f, 5f);
                                overCallback?.Invoke(isOver);
                            }, updateInfoCallback);
                        }
                        else
                        {
                            updateProgressCallback(5f, 5f);
                            overCallback?.Invoke(false);
                        }
                    });
                }
                else
                {
                    overCallback(false);
                }
            });

        }
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="overCallback">对比完成要执行的事件</param>
        /// <param name="updateInfoCallback">更新信息要执行的事件</param>
        public void CheckUpdate(UnityAction<bool> overCallback, UnityAction<string> updateInfoCallback)
        {
            //清空上次残留信息
            remoteABInfoDic.Clear();
            localABInfoDic.Clear();
            downloadList.Clear();

            //1.加载远端资源对比文件
            DownloadABCompareFile((isOver) =>
            {
                updateInfoCallback("开始更新资源");
                if (isOver)
                {
                    updateInfoCallback("远端对比文件下载结束");
                    string remoteInfo = File.ReadAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt");
                    updateInfoCallback("解析远端对比文件");
                    GetABCompareFileInfo(remoteInfo, remoteABInfoDic);
                    updateInfoCallback("解析远端对比文件完成");


                    //2.加载本地资源对比文件
                    GetLocalABCompareFileInfo((isOver) =>
                    {
                        if (isOver)
                        {
                            updateInfoCallback("解析本地对比完成");
                            updateInfoCallback("开始对比");
                            //3.对比 进行AB包下载
                            //1.判断哪些资源时新的然后记录之后用于下载
                            //2.判断哪些资源是需要更新的然后记录之后用于下载
                            //3.判断哪些资源需要删除
                            foreach (var abName in remoteABInfoDic.Keys)
                            {
                                if (!localABInfoDic.ContainsKey(abName))
                                {
                                    //本地对比信息中没有该文件，则将其添加进待下载列表
                                    downloadList.Add(abName);
                                }
                                else
                                {
                                    //本地信息中有该文件
                                    //判断哪些文件需要更新
                                    if (localABInfoDic[abName].md5 != remoteABInfoDic[abName].md5)
                                    {
                                        downloadList.Add(abName);
                                    }

                                    localABInfoDic.Remove(abName);
                                    //本地中剩下来的就是远端文件中没有的文件信息
                                    //表示需要将这些本地文件删除
                                }
                            }
                            updateInfoCallback("对比完成");
                            updateInfoCallback("删除无用AB包文件");

                            foreach (var abName in localABInfoDic.Keys)
                            {
                                if (File.Exists(Application.persistentDataPath + "/" + abName))
                                {
                                    //删除可读写文件夹中的多余的文件
                                    File.Delete(Application.persistentDataPath + "/" + abName);
                                }
                            }
                            updateInfoCallback("下载并更新AB包资源");
                            //下载待更新列表中所有AB包
                            DownloadABFile((isOver) =>
                            {
                                if (isOver)
                                {
                                    //下载完所有AB包文件后
                                    //将远端对比文件 存储在本地
                                    File.WriteAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt", remoteInfo);
                                    updateInfoCallback("下载更新完毕");
                                }
                                overCallback?.Invoke(isOver);
                            }, updateInfoCallback);
                        }
                        else
                        {
                            overCallback?.Invoke(false);
                        }
                    });
                }
                else
                {
                    overCallback(false);
                }
            });

        }

        /// <summary>
        /// 下载AB包对比文件
        /// </summary>
        /// <param name="overCallback"></param>
        private async void DownloadABCompareFile(UnityAction<bool> overCallback)
        {
            //从资源服务器下载资源对比文件
            //  www UnityWebRequest FtpWebRequest
            Debug.Log(Application.persistentDataPath);

            int reDownloadMaxNum = 5;
            bool isOver = false;
            //string abCompareinfo = File.ReadAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt");
            string localPath = Application.persistentDataPath;

            while (!isOver && reDownloadMaxNum > 0)
            {
                await Task.Run(() =>
                {
                    isOver = DownloadFile("ABCompareInfo.txt", localPath + "/ABCompareInfo_TMP.txt");
                    //GetABCompareFileInfo(abCompareinfo, remoteABInfoDic);
                });
                reDownloadMaxNum--;
            }

            overCallback?.Invoke(isOver);
        }

        /// <summary>
        /// 获取下载下来的AB包对比文件中存储的文件信息
        /// </summary>
        /// <param name="info">资源源对比文件中的 字符串信息</param>
        /// <param name="dic">容器</param>
        private void GetABCompareFileInfo(string info, Dictionary<string, ABInfo> dic)
        {
            //获取资源对比文件中的 字符串信息 进行拆分
            //string info = File.ReadAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt");
            string[] strs = info.Split('|');
            string[] infos = null;
            foreach (string str in strs)
            {
                //Debug.Log(str);
                infos = str.Split(' ');
                //remoteABInfoDic.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));
                dic.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));
            }
        }

        /// <summary>
        /// 获取本地资源对比信息
        /// </summary>
        /// <param name="overCallback"></param>
        private void GetLocalABCompareFileInfo(UnityAction<bool> overCallback)
        {
            if (File.Exists(Application.persistentDataPath + "/ABCompareInfo.txt"))
            {
                //如果可读写文件夹存在对比文件 说明已经更新过
                StartCoroutine(GetLocalABCompareFileInfo("file:///" + Application.persistentDataPath + "/ABCompareInfo.txt", overCallback));
            }
            else if (File.Exists(Application.streamingAssetsPath + "/ABCompareInfo.txt"))
            {
                string path =
#if UNITY_ANDROID
Application.streamingAssetsPath;
#else
                    "file:///" + Application.streamingAssetsPath;
#endif
                //如果可读写文件夹不存在，而只读文件夹存在对比文件 则加载默认资源（第一次进游戏时发生）
                StartCoroutine(GetLocalABCompareFileInfo(path + "/ABCompareInfo.txt", overCallback));
            }
            else
            {
                //没有任何对比文件（资源），可以认为加载成功
                overCallback?.Invoke(true);
            }

            //StartCoroutine(GetLocalABCompareFileInfo());
        }
        private IEnumerator GetLocalABCompareFileInfo(string filePath, UnityAction<bool> overCallback)
        {
            //通过 UnityWebRequest 加载本地文件
            UnityWebRequest req = UnityWebRequest.Get(filePath);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                GetABCompareFileInfo(req.downloadHandler.text, localABInfoDic);
                overCallback?.Invoke(true);
            }
            else
            {
                overCallback?.Invoke(false);
            }
        }

        /// <summary>
        /// 异步下载AB包文件
        /// </summary>
        /// <param name="overCallback"></param>
        /// <param name="updatePro"></param>
        private async void DownloadABFile(UnityAction<bool> overCallback, UnityAction<string> updatePro)
        {
            //1.遍历字典的键 根据文件名 下载AB包到本地
            //foreach (var name in remoteABInfoDic.Keys)
            //{
            //    downloadList.Add(name);
            //}

            string localPath = Application.persistentDataPath + "/";
            bool isOver = false;
            List<string> tempList = new List<string>();
            int reDownloadMaxNum = 5;   //最大重新下载次数
            int downloadOverNum = 0;
            int downloadMaxNum = downloadList.Count;

            while (downloadList.Count > 0 && reDownloadMaxNum > 0)
            {
                for (int i = 0; i < downloadList.Count; i++)
                {
                    isOver = false;
                    await Task.Run(() =>
                    {
                        isOver = DownloadFile(downloadList[i], localPath + downloadList[i]);
                    });
                    //2.获取现在下载了多少 是否结束
                    if (isOver)
                    {
                        updatePro?.Invoke(++downloadOverNum + "/" + downloadMaxNum);
                        tempList.Add(downloadList[i]);
                    }
                }
                //将下载成功的文件从列表中移除
                for (int i = 0; i < tempList.Count; i++)
                {
                    downloadList.Remove(tempList[i]);
                }

                reDownloadMaxNum--;
            }

            overCallback?.Invoke(downloadList.Count == 0);

        }

        /// <summary>
        /// FTP下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="localFilePath">本地路径</param>
        /// <returns></returns>
        private bool DownloadFile(string fileName, string localFilePath)
        {
            try
            {
                string pInfo =
#if UNITY_IOS
"IOS";
#elif UNITY_ANDROID
"Android";
#else
                    "PC";
#endif

                FtpWebRequest req = FtpWebRequest.Create(new Uri(serverIp + "/AB/" + pInfo + "/" + fileName)) as FtpWebRequest;
                //如果有匿名账号 可以不设置凭证 但是实际开发中 建议不要使用匿名账号
                NetworkCredential n = new NetworkCredential(ftpUsername, ftpPassword);
                req.Credentials = n;
                req.Proxy = null;
                req.KeepAlive = false;
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                req.UseBinary = true;

                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                Stream downloadStream = res.GetResponseStream();

                using (FileStream fs = File.Create(localFilePath))
                {
                    byte[] bytes = new byte[2048];
                    int contentLength = downloadStream.Read(bytes, 0, bytes.Length);

                    while (contentLength > 0)
                    {
                        fs.Write(bytes, 0, contentLength);
                        contentLength = downloadStream.Read(bytes, 0, bytes.Length);
                    }

                    fs.Close();
                    downloadStream.Close();
                }
                Debug.Log(fileName + "下载成功");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("下载失败" + ex.Message);
                return false;
            }
        }


        public class ABInfo
        {
            public readonly string name;
            public readonly long size;
            public readonly string md5;

            public ABInfo(string name, string size, string md5)
            {
                this.name = name;
                this.size = long.Parse(size);
                this.md5 = md5;
            }

            public static bool operator ==(ABInfo left, ABInfo right)
            {
                if (left.md5 == right.md5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static bool operator !=(ABInfo left, ABInfo right)
            {
                return !(left == right);
            }
            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is ABInfo))
                {
                    return false;
                }
                ABInfo other = (ABInfo)obj;
                return this.md5 == other.md5;
            }
            public override int GetHashCode()
            {
                return md5?.GetHashCode() ?? 0; //如果左边的表达式为 null，则返回右边的默认值
            }
        }
    }

}
