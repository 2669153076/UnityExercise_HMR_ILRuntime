using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using Custom.BaseFramework;
using System.IO;

/// <summary>
/// 知识点
/// 1.AB包相关API
/// 2.单例模式
/// 3.委托——>lambad表达式
/// 4.协程
/// 5.字典
/// </summary>
public class ABMgr : SingletonAutoMono<ABMgr>
{
    //AB包管理器目的是
    //让外部更方便的进行资源加载

    //主包
    private AssetBundle abMain = null;
    //依赖包获取用的配置文件
    private AssetBundleManifest abManifest = null;

    //AB包不能重复加载 重复加载会报错
    //使用字典存储 加载过的AB包
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    private string StreamingPathURL
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    private string PersistentPathURL
    {
        get
        {
            return Application.persistentDataPath + "/";
        }
    }

    private string ABMainName
    {
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "PC";
#endif
        }
    }

    //同步加载
    public Object LoadRes(string abName, string resName)
    {
        LoadAB(abName);
        Object obj = abDic[abName].LoadAsset(resName);
        if (obj is GameObject)
        {
            return Instantiate(obj);
        }
        else
            return obj;

    }

    public Object LoadRes(string abName, string resName, System.Type type)
    {
        LoadAB(abName);
        Object obj = abDic[abName].LoadAsset(resName, type);
        if (obj is GameObject)
        {
            return Instantiate(obj);
        }
        else
            return obj;
    }

    public T LoadRes<T>(string abName, string resName) where T : Object
    {
        LoadAB(abName);
        T obj = abDic[abName].LoadAsset<T>(resName);
        if (obj is GameObject)
        {
            return Instantiate(obj);
        }
        else
            return obj;
    }
    /// <summary>
    /// 加载AB包
    /// 加载主包 和 配置文件
    /// 因为加载所有包时 都得判断 通过它才能得到依赖信息
    /// </summary>
    /// <param name="abName"></param>
    public void LoadAB(string abName)
    { string path;
        //加载主包
        if (abMain == null)
        {
            path = File.Exists(PersistentPathURL + ABMainName) ? PersistentPathURL + ABMainName : StreamingPathURL + ABMainName;
            //abMain = AssetBundle.LoadFromFile(StreamingPathURL + ABMainName);
            abMain = AssetBundle.LoadFromFile(path);
            abManifest = abMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        //加载依赖包
        string[] strs = abManifest.GetAllDependencies(abName);
        AssetBundle ab;
        for (int i = 0; i < strs.Length; i++)
        {
            if (!abDic.ContainsKey(strs[i]))
            {
                path = File.Exists(PersistentPathURL + strs[i]) ? PersistentPathURL + strs[i] : StreamingPathURL + strs[i];
                //ab = AssetBundle.LoadFromFile(StreamingPathURL + strs[i]);
                ab = AssetBundle.LoadFromFile(path);
                abDic.Add(strs[i], ab);
            }
        }

        //加载资源包
        if (!abDic.ContainsKey(abName))
        {
            path = File.Exists(PersistentPathURL + abName) ? PersistentPathURL + abName : StreamingPathURL + abName;
            //ab = AssetBundle.LoadFromFile(StreamingPathURL + abName);
            ab = AssetBundle.LoadFromFile(path);
            abDic.Add(abName, ab);
        }
    }

    //异步加载的方法
    //这里的异步加载 AB包并没有使用异步加载
    //只是从AB包中 加载资源时 使用异步
    public void LoadResAsync(string abName, string resName, UnityAction<Object> callback)
    {
        StartCoroutine(LoadResCoroutine(abName, resName, callback));
    }
    private IEnumerator LoadResCoroutine(string abName, string resName, UnityAction<Object> callback)
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName);
        yield return abr;

        if (abr.asset is GameObject)
        {
            callback(Instantiate(abr.asset));
        }
        else
            callback(abr.asset);
    }

    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callback)
    {
        StartCoroutine(LoadResCoroutine(abName, resName, type, callback));
    }
    private IEnumerator LoadResCoroutine(string abName, string resName, System.Type type, UnityAction<Object> callback)
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName, type);
        yield return abr;

        if (abr.asset is GameObject)
        {
            callback(Instantiate(abr.asset));
        }
        else
            callback(abr.asset);
    }

    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback) where T : Object
    {
        StartCoroutine(LoadResCoroutine<T>(abName, resName, callback));
    }
    private IEnumerator LoadResCoroutine<T>(string abName, string resName, UnityAction<T> callback) where T : Object
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(resName);
        yield return abr;

        if (abr.asset is GameObject)
        {
            callback(Instantiate(abr.asset) as T);
        }
        else
            callback(abr.asset as T);
    }

    /// <summary>
    /// 单个包卸载
    /// </summary>
    /// <param name="abName"></param>
    public void UnLoadSingle(string abName)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(false);
            abDic.Remove(abName);
        }
    }

    /// <summary>
    /// 所有包的卸载
    /// </summary>
    public void UnloadAll()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();
        abMain = null;
        abManifest = null;
    }
}