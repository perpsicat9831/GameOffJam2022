using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 对XXXLoader的结果Asset进行Debug显示
    /// </summary>
    public class ResoourceLoadedAssetDebugger : MonoBehaviour
    {
        public string MemorySize;
        public UnityEngine.Object TheObject;
        private const string bigType = "LoadedAssetDebugger";
        public string Type;
        private bool IsRemoveFromParent = false;

        public static ResoourceLoadedAssetDebugger Create(string type, string url, UnityEngine.Object theObject)
        {
            var newHelpGameObject = new GameObject(string.Format("LoadedObject-{0}-{1}", type, url));
            DebuggerObjectTool.SetParent(bigType, type, newHelpGameObject);

            var newHelp = newHelpGameObject.AddComponent<ResoourceLoadedAssetDebugger>();
            newHelp.Type = type;
            newHelp.TheObject = theObject;
            newHelp.MemorySize = string.Format("{0:F5}KB",
                UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(theObject) / 1024f
            );
            return newHelp;
        }

        private void Update()
        {
            if (TheObject == null && !IsRemoveFromParent)
            {
                DebuggerObjectTool.RemoveFromParent(bigType, Type, gameObject);
                IsRemoveFromParent = true;
            }
        }

        // 可供调试删资源
        private void OnDestroy()
        {
            if (!IsRemoveFromParent)
            {
                DebuggerObjectTool.RemoveFromParent(bigType, Type, gameObject);
                IsRemoveFromParent = true;
            }
        }

    }
}
