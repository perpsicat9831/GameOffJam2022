using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public class NewManager : BaseManager<NewManager>
    {

        public enum EEvents
        {
            onEventTrigger,
        }
        public EventEmitter<EEvents> eventEmitter = new EventEmitter<EEvents>();


        #region lifeCycle
        public override void OnInit()
        {

        }
        public override void OnDestroy()
        {

        }
        public void OnUpData()
        {
            //需要继承接口 IManagerUpdateModule 
        }
        #endregion

        #region func
        public void func()
        {
            eventEmitter.Trigger(EEvents.onEventTrigger);
        }
        #endregion

        #region event
        #endregion

    }
}
