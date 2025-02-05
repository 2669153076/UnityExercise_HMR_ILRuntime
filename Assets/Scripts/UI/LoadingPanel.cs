using Custom.BaseFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    public Image imgPro;
    public Text textInfo;
    // Start is called before the first frame update
    void Start()
    {
        imgPro.rectTransform.sizeDelta = new Vector2(0, 50);
        textInfo.text = "资源加载中...";
    }

    //一：需要更新资源服务器上的AB包
    public void BeginUpdateABPackage()
    {
        //第一个委托 是用于 AB包下载更新结束后 处理逻辑的
        //第二个委托 是用于 更新当前加载信息的
        ABUpdateMgr.GetInstance().CheckUpdate(ABUpdateOverDoSomthing, (info) =>
        {
            textInfo.text = info;
        }, (nowNum, maxNum) =>
        {
            imgPro.rectTransform.sizeDelta = new Vector2(nowNum / maxNum * 1600, 50);
        });
    }

    //二：AB包更新完毕后，处理ILRuntime初始化相关的逻辑
    public void ABUpdateOverDoSomthing(bool isOver)
    {
        if (!isOver)
        {
            textInfo.text = "AB包下载更新出错，请检查网络连接";
            return;
        }

        textInfo.text = "资源加载结束";
        //ILRuntime的初始化相关
        ILRuntimeMgr.GetInstance().StartILRuntime(() =>
        {
            //ILRuntime相关内容加载结束
            //执行游戏逻辑
            textInfo.text = "游戏初始化完毕";

            //热更相关逻辑执行
            ILRuntimeMgr.GetInstance().appdomain.Invoke("HotFix_Project.ILRuntimeMain", "Start", null, null);

        }, (str) =>
        {
            textInfo.text = str;
        });
    }
}
