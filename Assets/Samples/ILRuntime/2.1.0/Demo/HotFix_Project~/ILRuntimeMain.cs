using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix_Project
{
    internal class ILRuntimeMain
    {
        /// <summary>
        /// 热更逻辑主入口
        /// </summary>
        public static void Start()
        {
            UnityEngine.Debug.Log("热更逻辑执行");

            //打包后编写的测试脚本
            GameObject.Destroy(GameObject.Find("Canvas"));
            GameObject.Destroy(GameObject.Find("EventSystem"));
            GameObject.CreatePrimitive(PrimitiveType.Cube).transform.localScale = Vector3.one * 5;
        }
    }
}
