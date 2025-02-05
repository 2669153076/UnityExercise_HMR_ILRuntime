using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Custom.BaseFramework
{
    public class BasePanel : MonoBehaviour
    {
        private Dictionary<string, List<UIBehaviour>> controlDic = new Dictionary<string, List<UIBehaviour>>();
        // Start is called before the first frame update
        protected virtual void Awake()
        {
            FindChildrenControl<Button>();
            FindChildrenControl<Toggle>();
            FindChildrenControl<Slider>();
            FindChildrenControl<Image>();
            FindChildrenControl<Text>();
            FindChildrenControl<ScrollRect>();
            FindChildrenControl<InputField>();
        }

        public virtual void ShowSelf() { }

        public virtual void HideSelf() { }

        public virtual void OnClick(string btnName) { }
        public virtual void OnValueChanged(string togName, bool value) { }

        /// <summary>
        /// 得到对应名字的控件脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected T GetControl<T>(string name) where T : UIBehaviour
        {
            if (controlDic.ContainsKey(name))
            {
                for (int i = 0; i < controlDic[name].Count; i++)
                {
                    if (controlDic[name][i] is T)
                    {
                        return controlDic[name][i] as T;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取子控件，并存储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void FindChildrenControl<T>() where T : UIBehaviour
        {
            T[] controls = GetComponentsInChildren<T>();
            string objName;
            for (int i = 0; i < controls.Length; i++)
            {
                objName = controls[i].gameObject.name;
                if (controlDic.ContainsKey(objName))
                {
                    controlDic[objName].Add(controls[i]);
                }
                else
                {
                    controlDic.Add(objName, new List<UIBehaviour>() { controls[i] });
                }

                if (controls[i] is Button)
                {
                    (controls[i] as Button).onClick.AddListener(() =>
                    {
                        OnClick(objName);
                    });
                }
                else if (controls[i] is Toggle)
                {
                    (controls[i] as Toggle).onValueChanged.AddListener((value) =>
                    {
                        OnValueChanged(objName, value);
                    });
                }
            }
        }
    }
}