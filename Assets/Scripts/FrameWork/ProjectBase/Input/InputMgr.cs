using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.BaseFramework
{
    public class InputMgr : BaseManager<Input>
    {
        private bool isStart = false;

        public InputMgr()
        {
            MonoMgr.GetInstance().AddUpdateListener(Update);
        }

        public void Update()
        {
            if (!isStart)
                return;
            CheckKeyCode(KeyCode.Escape);
        }

        /// <summary>
        /// 切换输入检测的开启状态
        /// </summary>
        /// <param name="value"></param>
        public void SwitchInputCheckState(bool value)
        {
            isStart = value;
        }

        private void CheckKeyCode(KeyCode code)
        {
            if (Input.GetKeyDown(code))
            {
                EventCenter.GetInstance().EventTrigger("按下", code);
            }
            if (Input.GetKeyUp(code))
            {
                EventCenter.GetInstance().EventTrigger("抬起", code);
            }
        }
    }
}