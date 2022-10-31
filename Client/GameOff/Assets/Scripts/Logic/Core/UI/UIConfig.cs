using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public enum EUIID
    {
        None,
        GameMain = 1,   //������
    }

    public static class UIConfig
    {
        public static UIConfigData GetConfigData(int id)
        {
            if (dicUIConfigs.TryGetValue(id, out UIConfigData data))
            {
                return data;
            }
            Log.Error("δ�ҵ�idΪ " + id.ToString() + " ��UIConfig");
            return null;
        }
        public static UIConfigData GetConfigData(EUIID id)
        {
            return GetConfigData((int)id);
        }

        private static readonly Dictionary<int, UIConfigData> dicUIConfigs = new Dictionary<int, UIConfigData>()
        {
            {(int)EUIID.GameMain,
                new UIConfigData(
                    typeof(UI_GameMain),
                    "UIPrefab/GameMain/UI_GameMain")
            },
        };
    }
}