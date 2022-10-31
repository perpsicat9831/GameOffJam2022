using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class CenterManager
    {
        List<IManagerModule> listAllManager = new List<IManagerModule>();
        List<IManagerUpdateModule> listUpdateManager = new List<IManagerUpdateModule>();



        public void RegisterAllManager()
        {
            RegisterManager(PoolManager.Instance);
            RegisterManager(SoundManager.Instance);
            RegisterManager(TimeManager.Instance);



            InitAllManager();
        }


        private void RegisterManager(IManagerModule curModel)
        {
            if(curModel == null)
            {
                return;
            }
            listAllManager.Add(curModel);
            IManagerUpdateModule modelUpdate = curModel as IManagerUpdateModule;
            if(modelUpdate != null)
            {
                listUpdateManager.Add(modelUpdate);
            }
        }
        public void UpdateAllManager()
        {
            for (int i = 0; i < listUpdateManager.Count; i++)
            {
                listUpdateManager[i].OnUpData();
            }
        }
        public void InitAllManager()
        {
            for (int i = 0; i < listAllManager.Count; i++)
            {
                listAllManager[i].OnInit();
            }
        }
        public void DestroyAllManager()
        {
            for (int i = 0; i < listAllManager.Count; i++)
            {
                listAllManager[i].OnDestroy();
            }
        }
    }
}
