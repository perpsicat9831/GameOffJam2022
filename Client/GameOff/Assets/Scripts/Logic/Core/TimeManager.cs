using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public class TimeManager : BaseManager<TimeManager>,IManagerUpdateModule
    {
        private List<Timer> timerList;
        /// <summary>
        /// timer缓存容器
        /// </summary>
        private Queue<Timer> cacheTimer;

        #region lifeCycle
        public override void OnInit()
        {
            timerList = new List<Timer>();
            cacheTimer = new Queue<Timer>(2);
        }
        public override void OnDestroy()
        {
            timerList?.Clear();
            cacheTimer?.Clear();
        }
        public void OnUpData()
        {
            for (int i = 0; i < timerList.Count;)
            {
                Timer timer = timerList[i];
                timer.Run();
                //未激活的timer放进池子里
                if (!timer.isActive)
                {
                    //未在缓存池里
                    if (!cacheTimer.Contains(timer))
                    {
                        cacheTimer.Enqueue(timer);
                    }
                    timerList.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }
        #endregion

        #region func
        public Timer RegisterTimer(float duration, Action callAction = null, Action updateAction = null)
        {
            Timer timer = GetTimer();
            timer.SetTimer(duration, callAction, updateAction);
            timerList.Add(timer);
            return timer;
        }

        private Timer GetTimer()
        {
            Timer timer;
            if (cacheTimer.Count > 0)
            {
                timer = cacheTimer.Dequeue();
            }
            else
            {
                timer = new Timer();
            }
            return timer;
        }
        #endregion

        #region event
        #endregion

    }
}
