using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Custom.BaseFramework
{
    public class ILRuntimeMgr : SingletonAutoMono<ILRuntimeMgr>
    {
        public ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        //dll文件和pdb文件的流对象
        private System.IO.MemoryStream dllStream;
        private System.IO.MemoryStream pdbStream;

        private bool isStart = false;   //是否已经加载了对应文件
        private bool isDebug = false;   //是否是调试模式，开发模式时为true

        /// <summary>
        /// 启动ILRuntime
        /// 加载对应的dll和pdb文件
        /// </summary>
        /// <param name="callback">ILRuntime初始化结束后要做的事情</param>
        /// <param name="infoCallBack">更新进度条上方文本显示信息</param>
        public void StartILRuntime(UnityAction callback = null,UnityAction<string> infoCallBack=null)
        {
            if (!isStart)
            {
                isStart = true;
                //加载对应的dll和pdb等文件
                appdomain = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntime.Runtime.ILRuntimeJITFlags.JITOnDemand);
                infoCallBack?.Invoke("开始更新dll文件");
                //通过AB包管理器异步加载DLL文件信息
                ABMgr.GetInstance().LoadResAsync<TextAsset>("dll_res", "HotFix_Project.dll", (dllFile) =>
                {
                    infoCallBack?.Invoke("开始更新pdb文件");
                    ABMgr.GetInstance().LoadResAsync<TextAsset>("dll_res", "HotFix_Project.pdb", (pdbFile) =>
                    {
                        dllStream = new System.IO.MemoryStream(dllFile.bytes);
                        pdbStream = new System.IO.MemoryStream(pdbFile.bytes);

                        appdomain.LoadAssembly(dllStream, pdbStream, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());

                        infoCallBack?.Invoke("开始初始化ILRuntime");
                        //初始化相关操作
                        InitILRuntime();


                        if (isDebug)
                        {
                            infoCallBack?.Invoke("等待IDE接入，开启调试");
                            StartCoroutine(WaitDebugger(callback));
                        }
                        else
                        {
                            infoCallBack?.Invoke("初始化结束");
                            callback?.Invoke();
                        }
                    });
                });

            }
        }

        /// <summary>
        /// 初始化ILRuntime
        /// </summary>
        private void InitILRuntime()
        {

            //如果想使用Unity自带的性能调试窗口 调试ILRuntime相关内容 就需要加入该行代码
            //生成AB包时，该行代码需要注释掉，因为会引发冲突
            //appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }
        /// <summary>
        /// 等待ILRuntime调试程序启动协程
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator WaitDebugger(UnityAction callback)
        {
            while (!appdomain.DebugService.IsDebuggerAttached)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            callback?.Invoke();
        }

        public void StopILRuntime()
        {
            if (dllStream != null)
            {
                dllStream.Close();
            }
            if (pdbStream != null)
            {
                pdbStream.Close();
            }
            dllStream = null;
            pdbStream = null;
            appdomain = null;

            isStart = false;

        }
    }
}