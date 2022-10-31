using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Framework
{


    public class ABManager
    {
        /// <summary>
        /// type -> <url,loader>
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> _loadersPool = new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();
        /// <summary>
        /// todo �ȴ������е�ab
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> waiting = new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();
        public static int MAX_LOAD_NUM = 10;


        #region �������� Garbage Collect

        /// <summary>
        /// Loader�ӳ�Dispose
        /// </summary>
        private const float LoaderDisposeTime = 0;

        /// <summary>
        /// �����������һ��GC(��AutoNewʱ)
        /// </summary>
        public static float GcIntervalTime
        {
            get
            {
                /*if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXEditor)
                    return 1f;

                return AppConfig.IsDebugBuild ? 5f : 10f;*/
                return 0.1f;
            }
        }

        /// <summary>
        /// �ϴ���GC��ʱ��
        /// </summary>
        private static float _lastGcTime = -1;

        /// <summary>
        /// ��������Ҫɾ���ģ���DoGarbageCollect������, �����ظ���new List
        /// </summary>
        private static readonly List<AbstractResourceLoader> CacheLoaderToRemoveFromUnUsed = new List<AbstractResourceLoader>();

        /// <summary>
        /// ������������
        /// </summary>
        internal static readonly Dictionary<AbstractResourceLoader, float> UnUsesLoaders = new Dictionary<AbstractResourceLoader, float>();

        #endregion

        public static int GetCount<T>()
        {
            return GetTypeDict(typeof(T)).Count;
        }

        public static Dictionary<string, AbstractResourceLoader> GetTypeDict(Type type)
        {
            Dictionary<string, AbstractResourceLoader> typesDict;
            if (!_loadersPool.TryGetValue(type, out typesDict))
            {
                typesDict = _loadersPool[type] = new Dictionary<string, AbstractResourceLoader>();
            }

            return typesDict;
        }

        public static int GetRefCount<T>(string url)
        {
            var dict = GetTypeDict(typeof(T));
            AbstractResourceLoader loader;
            if (dict.TryGetValue(url, out loader))
            {
                return loader.RefCount;
            }

            return 0;
        }

        /// <summary>
        /// �Ƿ���������ռ�
        /// </summary>
        public static void CheckGcCollect()
        {
            if (_lastGcTime.Equals(-1) || (Time.time - _lastGcTime) >= GcIntervalTime)
            {
                DoGarbageCollect();
                _lastGcTime = Time.time;
            }
        }

        /// <summary>
        /// ������������
        /// </summary>
        internal static void DoGarbageCollect()
        {
            foreach (var kv in UnUsesLoaders)
            {
                var loader = kv.Key;
                var time = kv.Value;
                if ((Time.time - time) >= LoaderDisposeTime)
                {
                    CacheLoaderToRemoveFromUnUsed.Add(loader);
                }
            }

            for (var i = CacheLoaderToRemoveFromUnUsed.Count - 1; i >= 0; i--)
            {
                try
                {
                    var loader = CacheLoaderToRemoveFromUnUsed[i];
                    UnUsesLoaders.Remove(loader);
                    CacheLoaderToRemoveFromUnUsed.RemoveAt(i);
                    loader.Dispose();
                }
                catch (Exception e)
                {
                    Log.LogException(e);
                }
            }

            if (CacheLoaderToRemoveFromUnUsed.Count > 0)
            {
                Log.Error("[DoGarbageCollect]CacheLoaderToRemoveFromUnUsed not empty!!");
            }
        }

        #region ����ӿ�
        public static bool UseAssetBundle()
        {
            //Ĭ��ʹ��
#if UNITY_EDITOR
            if(!EditorPrefs.GetBool(AppConfig.UseABPrefsKey, true))
            {
                return false;
            }
#endif
            return true;
        }
        #endregion

        public static Dictionary<string, SpriteAtlas> SpriteAtlases = new Dictionary<string, SpriteAtlas>();

        public static void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
        {
            SpriteAtlas atlas = null;
            if (SpriteAtlases.TryGetValue(tag, out atlas))
            {
                if (atlas != null)
                {
                    if (Application.isEditor) Log.LogInfo($"Request spriteAtlas {tag}");
                    callback(atlas);
                }
                else
                {
                    SpriteAtlases.Remove(tag);
                    Log.Error($"not load spriteAtlas {tag}");
                }
            }
            else
            {
                Log.Error($"not load spriteAtlas {tag}");
            }
        }
    }
}