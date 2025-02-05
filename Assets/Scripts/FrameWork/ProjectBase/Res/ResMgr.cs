using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Custom.BaseFramework
{
    public class ResMgr : BaseManager<ResMgr>
    {
        public T Load<T>(string name) where T : Object
        {
            T res = Resources.Load<T>(name);
            if (res is GameObject)
                return GameObject.Instantiate(res);
            else
                return res;
        }

        public void LoadAsync<T>(string name, UnityAction<T> callback) where T : Object
        {
            MonoMgr.GetInstance().StartCoroutine(LoadAsyncCortinue<T>(name, callback));
        }

        private IEnumerator LoadAsyncCortinue<T>(string name, UnityAction<T> callback) where T : Object
        {
            ResourceRequest rq = Resources.LoadAsync<T>(name);

            yield return rq;

            if (rq.asset is GameObject)
            {
                callback(GameObject.Instantiate(rq.asset) as T);
            }
            else
            {
                callback(rq.asset as T);
            }
        }
    }
}