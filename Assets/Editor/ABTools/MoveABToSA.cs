using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

    /// <summary>
    /// 移动选中的AB包资源到StreamingAssets中
    /// </summary>
    public class MoveABToSA
    {
        //[MenuItem("AB包工具/移动所选中的资源到StreamingAssets中")]
        public static void MoveABToStreamingAssets()
        {
            //通过编辑器Selection类中的方法 获取在Project窗口中选中的资源
            Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
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
    }
