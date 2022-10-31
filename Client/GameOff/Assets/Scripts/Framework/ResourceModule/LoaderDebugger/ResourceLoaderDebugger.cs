using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// ֻ�ڱ༭���³��֣��ֱ��Ӧһ��Loader~����һ��GameObject����Ϊ�˷�����ԣ�
    /// </summary>
    public class ResourceLoaderDebugger : MonoBehaviour
    {

        public AbstractResourceLoader TheLoader;
        public int RefCount;
        public float FinishUsedTime; // �ο����������ʱ��
        public static bool IsApplicationQuit = false;

        public static ResourceLoaderDebugger Create(string type, string url, AbstractResourceLoader loader)
        {
            if (IsApplicationQuit) return null;

            const string bigType = "ResourceLoaderDebuger";

            Func<string> getName = () => string.Format("{0}-{1}-{2}", type, url, loader.Desc);

            var newHelpGameObject = new GameObject(getName());
            DebuggerObjectTool.SetParent(bigType, type, newHelpGameObject);
            var newHelp = newHelpGameObject.AddComponent<ResourceLoaderDebugger>();
            newHelp.TheLoader = loader;

            loader.SetDescEvent += (newDesc) =>
            {
                if (loader.RefCount > 0)
                    newHelpGameObject.SetName(getName());
            };


            loader.DisposeEvent += () =>
            {
                if (!IsApplicationQuit)
                    DebuggerObjectTool.RemoveFromParent(bigType, type, newHelpGameObject);
            };


            return newHelp;
        }

        private void Update()
        {
            if (TheLoader.RefCount != RefCount)
                RefCount = TheLoader.RefCount;
            if (!TheLoader.FinishUsedTime.Equals(FinishUsedTime))
                FinishUsedTime = TheLoader.FinishUsedTime;
        }

        private void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }
    }
}
