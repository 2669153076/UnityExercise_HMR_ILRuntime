using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Custom.BaseFramework
{
    public class MonoController : MonoBehaviour
    {
        private event UnityAction updateEvent;


        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            updateEvent?.Invoke();
        }

        /// <summary>
        /// 添加帧更新事件的函数
        /// </summary>
        /// <param name="fun"></param>
        public void AddUpdateListener(UnityAction fun)
        {
            updateEvent += fun;
        }

        public void RemoveUpdateListener(UnityAction fun)
        {
            updateEvent -= fun;
        }

    }
}