using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Custom.BaseFramework
{
    public interface IEventInfo { }

    public class EventInfo<T> : IEventInfo
    {
        public UnityAction<T> actions;

        public EventInfo(UnityAction<T> action)
        {
            this.actions += action;
        }
    }
    public class EventInfo : IEventInfo
    {
        public UnityAction actions;

        public EventInfo(UnityAction action)
        {
            this.actions += action;
        }
    }


    /// <summary>
    /// 事件中心 单例模式对象
    /// </summary>
    public class EventCenter : BaseManager<EventCenter>
    {
        //private Dictionary<string, UnityAction<object>> eventDic = new Dictionary<string, UnityAction<object>>();
        private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="name">事件名</param>
        /// <param name="action">处理事件的委托函数</param>
        public void AddEventListener<T>(string name, UnityAction<T> action)
        {
            //if (eventDic.ContainsKey(name))
            //{
            //    eventDic[name] += action;
            //}
            //else
            //{
            //    eventDic.Add(name, action);
            //}
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo<T>).actions += action;
            }
            else
            {
                eventDic.Add(name, new EventInfo<T>(action));
            }
        }
        public void AddEventListener(string name, UnityAction action)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo).actions += action;
            }
            else
            {
                eventDic.Add(name, new EventInfo(action));
            }
        }

        /// <summary>
        /// 事件触发
        /// </summary>
        /// <param name="name">事件名</param>
        public void EventTrigger<T>(string name, T info)
        {
            if (eventDic.ContainsKey(name))
            {
                //eventDic[name].Invoke(info);
                if ((eventDic[name] as EventInfo<T>).actions != null)
                    (eventDic[name] as EventInfo<T>).actions.Invoke(info);
            }
        }
        public void EventTrigger(string name)
        {
            if (eventDic.ContainsKey(name))
            {
                if ((eventDic[name] as EventInfo).actions != null)
                    (eventDic[name] as EventInfo).actions.Invoke();
            }
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RemoveEventListener<T>(string name, UnityAction<T> action)
        {
            //if( eventDic.ContainsKey(name))
            //{
            //    eventDic[name] -= action;
            //}
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo<T>).actions -= action;
            }
        }
        public void RemoveEventListener(string name, UnityAction action)
        {
            if (eventDic.ContainsKey(name))
            {
                (eventDic[name] as EventInfo).actions -= action;
            }
        }

        /// <summary>
        /// 清空事件中心
        /// </summary>
        public void Clear()
        {
            eventDic.Clear();
        }
    }

}