using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Custom.BaseFramework
{
    public class SceneMgr : BaseManager<SceneMgr>
    {

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="name"></param>
        public void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }
        public void LoadScene(string name, UnityAction callback)
        {
            SceneManager.LoadScene(name);
            callback?.Invoke();
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="name"></param>
        public void LoadSceneAsync(string name)
        {
            MonoMgr.GetInstance().StartCoroutine(LoadSceneCoroutine(name));
        }
        public void LoadSceneAsync(string name, UnityAction callback)
        {
            MonoMgr.GetInstance().StartCoroutine(LoadSceneCoroutine(name, callback));
        }
        private IEnumerator LoadSceneCoroutine(string name)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(name);

            while (!operation.isDone)
            {
                //更新进度条
                EventCenter.GetInstance().EventTrigger("UpdateProgress", operation.progress);
                Debug.Log(operation.progress);  //加载进度
                yield return operation;
            }

        }
        private IEnumerator LoadSceneCoroutine(string name, UnityAction callback)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(name);

            while (!operation.isDone)
            {
                //更新进度条
                EventCenter.GetInstance().EventTrigger("UpdateProgress", operation.progress);
                Debug.Log(operation.progress);  //加载进度
                yield return operation;
            }
            callback?.Invoke();
        }
    }
}