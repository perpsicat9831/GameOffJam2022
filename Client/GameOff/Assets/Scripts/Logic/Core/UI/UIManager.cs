using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{

    public static class UIManager
    {
        private static UIModule uiModule = null;

        public static bool IsInit { get; private set; } = false;

        public static void Init()
        {
            uiModule = new UIModule();
            uiModule.CreateRoot();
            IsInit = true;
        }

        public static void OpenUI(EUIID id, object args = null)
        {
            OpenUI((int)id, args);
        }
        public static void OpenUI(int id, object args = null)
        {
            UIConfigData uiConfigData = UIConfig.GetConfigData(id);
            if (uiConfigData != null)
            {
                uiModule.OpenWindow(id, uiConfigData, args);
            }
        }

        public static void CloseUI(EUIID id, bool destroyImmediate = false)
        {
            CloseUI((int)id, destroyImmediate);
        }
        public static void CloseUI(int id, bool destroyImmediate = false)
        {
            uiModule.DestroyWindow(id, destroyImmediate);
        }
    }

}