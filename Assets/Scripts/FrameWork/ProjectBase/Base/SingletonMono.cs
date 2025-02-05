using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.BaseFramework
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T GetInstance()
        {
            return instance;
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
        }
    }
}
