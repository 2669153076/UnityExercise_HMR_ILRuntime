using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Custom.BaseFramework
{
    public class PoolData
    {
        public List<GameObject> poolList;
        public GameObject fatherObj;

        public PoolData(GameObject obj, GameObject poolObj)
        {
            fatherObj = new GameObject(obj.name + "S");
            fatherObj.transform.SetParent(poolObj.transform);

            poolList = new List<GameObject>();
            Push(obj);
        }

        /// <summary>
        /// 池子中存
        /// </summary>
        /// <param name="obj"></param>
        public void Push(GameObject obj)
        {
            obj.SetActive(false);
            poolList.Add(obj);
            obj.transform.SetParent(fatherObj.transform);
        }
        /// <summary>
        /// 池子中取
        /// </summary>
        public GameObject Pop()
        {
            GameObject obj = null;

            if (poolList.Count > 0)
            {
                obj = poolList[0];
                poolList.RemoveAt(0);
                obj.SetActive(true);
                obj.transform.SetParent(null);
            }

            return obj;
        }

    }

    public class PoolMgr : BaseManager<PoolMgr>
    {
        public Dictionary<string, PoolData> poolDir = new Dictionary<string, PoolData>();

        private GameObject poolObj;

        /// <summary>
        /// 从对象池获取物品
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject Get(string path)
        {
            GameObject obj = null;
            string name = GetObjName(path);

            //池子中有对应名称的物体，并且池子中该物体数量大于0
            if (poolDir.ContainsKey(name) && poolDir[name].poolList.Count > 0)
            {
                obj = poolDir[name].Pop();
            }
            else
            {

                //实例化物体并返回
                obj = GameObject.Instantiate(Resources.Load<GameObject>(path));
                obj.name = GetObjName(path);
            }

            return obj;
        }
        public GameObject Get(string path, UnityAction<GameObject> callback)
        {
            GameObject obj = null;
            string name = GetObjName(path);

            //池子中有对应名称的物体，并且池子中该物体数量大于0
            if (poolDir.ContainsKey(name) && poolDir[name].poolList.Count > 0)
            {
                callback(poolDir[name].Pop());
            }
            else
            {
                //异步加载资源 创建对象
                ResMgr.GetInstance().LoadAsync<GameObject>(path, (o) =>
                {
                    o.name = GetObjName(path);
                    callback(o);
                });
            }

            return obj;
        }

        /// <summary>
        /// 存入对象池中
        /// </summary>
        public void Push(string name, GameObject obj)
        {
            if (poolObj == null)
            {
                poolObj = new GameObject("Pool");
            }

            if (poolDir.ContainsKey(name))
            {
                poolDir[name].Push(obj);
            }
            else
            {
                poolDir.Add(name, new PoolData(obj, poolObj));
            }
        }

        /// <summary>
        /// 清空缓存池
        /// 一般过场景时使用
        /// </summary>
        public void Clear()
        {
            poolDir.Clear();
            poolObj = null;
        }

        /// <summary>
        /// 获取物体名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetObjName(string path)
        {
            int lastIndex = path.LastIndexOf("/");
            return path.Substring(lastIndex + 1);
        }
    }
}