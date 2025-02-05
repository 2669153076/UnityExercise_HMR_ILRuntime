using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //显示加载界面 进行资源和代码热更新
        ABMgr.GetInstance().LoadResAsync<GameObject>("ui", "LoadingPanel", (panelObj) =>
        {
            GameObject canvas = GameObject.Find("Canvas");
            panelObj.transform.SetParent(canvas.transform, false);

            //在进行AB包热更新之前，将AB包管理器中记录的旧的相关AB包清理，之后再次使用才不会出问题
            ABMgr.GetInstance().UnloadAll();

            //获取面板脚本，执行AB包相关更新
            LoadingPanel panel = panelObj.GetComponent<LoadingPanel>();
            panel.BeginUpdateABPackage();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
