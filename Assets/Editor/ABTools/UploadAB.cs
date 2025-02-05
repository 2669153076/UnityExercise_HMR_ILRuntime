using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net;
using System;
using System.Threading.Tasks;

    public class UploadAB
    {
        //[MenuItem("AB包工具/上传AB包和对比文件")]
        private static void UploadAllABFile()
        {
            DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/PC");
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
        private async static void FtpUploadFile(string filePath, string fileName)
        {
            await Task.Run(() =>
            {
                try
                {
                    //创建一个FTP连接 用于上传
                    FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AB/PC/" + fileName)) as FtpWebRequest;
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
