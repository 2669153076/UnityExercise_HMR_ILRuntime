using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Custom.BaseFramework
{
    public enum E_UI_Layer
    {
        Top,
        Mid,
        Bot,
        System
    }

    public class UIManager : BaseManager<UIManager>
    {
        public Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

        private Transform top;
        private Transform mid;
        private Transform bot;
        private Transform system;

        //记录UI的Canvas父对象，方便外部调用
        public RectTransform canvas;

        public UIManager()
        {
            GameObject obj = ResMgr.GetInstance().Load<GameObject>("UI/Canvas");
            canvas = obj.transform as RectTransform;
            GameObject.DontDestroyOnLoad(obj);

            top = canvas.Find("Top");
            mid = canvas.Find("Mid");
            bot = canvas.Find("Bot");
            system = canvas.Find("System");

            obj = ResMgr.GetInstance().Load<GameObject>("UI/EventSystem");
            GameObject.DontDestroyOnLoad(obj);

        }

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="panelName"></param>
        /// <param name="layer"></param>
        /// <param name="callback"></param>
        public void ShowPanel<T>(string panelName, E_UI_Layer layer = E_UI_Layer.Bot, UnityAction<T> callback = null) where T : BasePanel
        {
            if (panelDic.ContainsKey(panelName))
            {
                panelDic[panelName].ShowSelf();
                callback?.Invoke(panelDic[panelName] as T);

                return;//避免面板重复加载
            }

            ResMgr.GetInstance().LoadAsync<GameObject>("UI/" + panelName, (obj) =>
            {
                Transform father = null;
                switch (layer)
                {
                    case E_UI_Layer.Top:
                        father = top;
                        break;
                    case E_UI_Layer.Mid:
                        father = mid;
                        break;
                    case E_UI_Layer.Bot:
                        father = bot;
                        break;
                    case E_UI_Layer.System:
                        father = system;
                        break;
                }
                obj.name = panelName;

                obj.transform.SetParent(father);

                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;

                (obj.transform as RectTransform).offsetMax = Vector2.zero;
                (obj.transform as RectTransform).offsetMin = Vector2.zero;

                T panel = obj.GetComponent<T>();
                callback?.Invoke(panel);

                panel.ShowSelf();

                panelDic.Add(panelName, panel);
            });
        }

        /// <summary>
        /// 获取层级父对象
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Transform GetLayerFather(E_UI_Layer layer)
        {
            switch (layer)
            {
                case E_UI_Layer.Top:
                    return this.top;
                case E_UI_Layer.Mid:
                    return this.mid;
                case E_UI_Layer.Bot:
                    return this.bot;
                case E_UI_Layer.System:
                    return this.system;
            }
            return null;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="panelName"></param>
        public void HidePanel(string panelName)
        {
            if (panelDic.ContainsKey(panelName))
            {
                GameObject.Destroy(panelDic[panelName].gameObject);
                panelDic.Remove(panelName);
            }
        }

        /// <summary>
        /// 获取指定面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="panelName"></param>
        /// <returns></returns>
        public T GetPanel<T>(string panelName) where T : BasePanel
        {
            if (panelDic.ContainsKey(panelName))
            {
                return panelDic[panelName] as T;
            }
            return null;
        }

        public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            EventTrigger trigger = control.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                trigger = control.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(callback);

            trigger.triggers.Add(entry);
        }
    }
}