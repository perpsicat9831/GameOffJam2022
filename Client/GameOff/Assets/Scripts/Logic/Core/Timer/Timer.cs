using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;


namespace Logic
{
    public class Timer 
    {
        //总时间
        public float duration;
        //剩余时间
        public float leftTime;
        //激活 false会被回收
        public bool isActive;
        //是否暂停
        public bool isPause;


        //持续事件
        public Action onUpdateAct;
        //退出事件
        public Action onEndAct;


        public void Run()
        {
            if (isActive && !isPause)
            {
                leftTime -= Time.deltaTime;
                if (leftTime <= 0)
                {
                    if (onEndAct != null)
                    {
                        onEndAct.Invoke();
                    }
                    isActive = false;
                }
                else
                {
                    if (onUpdateAct != null)
                        onUpdateAct.Invoke();
                }
            }
        }

        public void SetTimer(float _duration, Action callAction = null, Action updateAction = null)
        {
            Reset();
            duration = leftTime = _duration;
            onEndAct = callAction;
            onUpdateAct = updateAction;
        }
        //重置计时器
        public void Reset()
        {
            isActive = true;
            isPause = false;
            leftTime = duration;
        }

        /// <summary>
        /// 设置计时器暂停
        /// </summary>
        public void SetPause(bool pause)
        {
            isPause = pause;
        }
        public void Clear()
        {
            //回收
            isActive = false;
            onUpdateAct = null;
            onEndAct = null;
        }
    }
}