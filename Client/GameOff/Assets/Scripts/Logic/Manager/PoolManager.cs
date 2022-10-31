using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    
    public class PoolManager : BaseManager<PoolManager>
    {
        public enum EEvents
        {
            onGet,
        }
        EventEmitter<EEvents> eventEmitter = new EventEmitter<EEvents>();

        public void func()
        {
            eventEmitter.Trigger(EEvents.onGet);
            eventEmitter.Trigger(PoolManager.EEvents.onGet);
        }

    }
}
