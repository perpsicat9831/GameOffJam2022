using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

namespace Framework
{
    public class UIModule
    {
        private int _loadingUICount = 0;
        /// <summary>
        /// 正在加载的UI统计
        /// </summary>
        public int LoadingUICount
        {
            get { return _loadingUICount; }
            set
            {
                _loadingUICount = value;
                if (_loadingUICount < 0) Log.Error("Error ---- LoadingUICount < 0");
            }
        }

        private Dictionary<int, FUIBase> dictUIBase = new Dictionary<int, FUIBase>();

        public List<string> CommonAtlases = new List<string>() { "atlas_common" };

        public GameObject UIRoot { get; private set; }
        public Camera UICamera { get; private set; }
        public GameObject MainUIRoot { get; private set; }
        public GameObject NormalUIRoot { get; private set; }
        public GameObject PopupUIRoot { get; private set; }
        public GameObject HeadInfoUIRoot { get; private set; }

        public EventSystem EventSystem;
        private GameObject goEventSystem;

        /// <summary>
        /// 每个界面打开都会+1，新打开的界面始终在最顶层
        /// </summary>
        public static int sortOrder = 0;
        public void CreateRoot()
        {
            UIRoot = new GameObject("UIRoot");
            MainUIRoot = new GameObject("MainUIRoot");
            NormalUIRoot = new GameObject("NormalUIRoot");
            PopupUIRoot = new GameObject("PopupUIRoot");
            HeadInfoUIRoot = new GameObject("HeadInfoUIRoot");
            MainUIRoot.transform.SetParent(UIRoot.transform, true);
            NormalUIRoot.transform.SetParent(UIRoot.transform, true);
            PopupUIRoot.transform.SetParent(UIRoot.transform, true);
            HeadInfoUIRoot.transform.SetParent(UIRoot.transform, true);

            goEventSystem = new GameObject("EventSystem");
            EventSystem = goEventSystem.AddComponent<EventSystem>();
            goEventSystem.AddComponent<StandaloneInputModule>();

            //create camera
            UICamera = new GameObject("UICamera").AddComponent<Camera>();
            UICamera.transform.SetParent(UIRoot.transform, true);
            UICamera.cullingMask = (1 << (int)UnityLayerDef.UI);
            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.nearClipPlane = UIDefs.Camera_Near;
            UICamera.farClipPlane = UIDefs.Camera_Far;
            UICamera.orthographic = true;
            UICamera.orthographicSize = UIDefs.Camera_Size;
            UICamera.depth = UIDefs.Camera_Depth;
            GameObject.DontDestroyOnLoad(UIRoot);
        }

        private void InitUIAsset(FUIBase uiBase)
        {
            if (!uiBase.gameObject)
            {
                Log.Error("uiObj is null !");
                return;
            }
            //var windowAsset = uiObj.GetComponent<UIWindowAsset>();
            var canvas = uiBase.Canvas;
            switch (uiBase.PanelType)
            {
                case PanelType.MainUI:
                    uiBase.gameObject.transform.SetParent(MainUIRoot.transform);
                    break;
                case PanelType.NormalUI:
                    uiBase.gameObject.transform.SetParent(NormalUIRoot.transform);
                    break;
                case PanelType.HeadInfoUI:
                    uiBase.gameObject.transform.SetParent(HeadInfoUIRoot.transform);
                    break;
                default:
                    Log.Error("not define PanelType", uiBase.PanelType);
                    uiBase.gameObject.transform.SetParent(UIRoot.transform);
                    break;
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UICamera;
        }

        public void OpenWindow(int id, UIConfigData uiConfig, object args)
        {
            FUIBase uiBase;
            if (!dictUIBase.TryGetValue(id, out uiBase))
            {
                LoadWindow(id, uiConfig, args);
                return;
            }
            //加载未完成说明还在第一次加载，不处理
            if (uiBase.isFinishLoad)
            {
                uiBase.OnOpen(args);
                ShowWindow(uiBase);
            }

        }

        public void PopupUI()
        {

        }

        public FUIBase LoadWindow(int id, UIConfigData uiConfig, object args)
        {
            if (dictUIBase.ContainsKey(id))
            {
                Log.Error("[LoadWindow]多次重复LoadWindow: {0}", id.ToString());
            }
            Debuger.Assert(!dictUIBase.ContainsKey(id));

            FUIBase uiBase = Activator.CreateInstance(uiConfig.script) as FUIBase;
            uiBase.UIID = id;
            uiBase.prefabPath = uiConfig.prefabPath;
            uiBase.OnOpen(args);

            dictUIBase.Add(id, uiBase);
            ResourceModule.Instance.StartCoroutine(LoadUIAssetBundle(uiBase));

            return uiBase;
        }

        private IEnumerator LoadUIAssetBundle(FUIBase uiBase)
        {
            if (uiBase.UIResourceLoader != null)
            {
                uiBase.UIResourceLoader.Release(true);// now!
                Log.LogInfo("Release UI ResourceLoader: {0}", uiBase.UIResourceLoader.Url);
                uiBase.UIResourceLoader = null;
            }

            LoadingUICount++;

            yield return ResourceModule.Instance.StartCoroutine(LoadUIAsset(uiBase));

            if (uiBase.gameObject != null)
            {
                InitUIAsset(uiBase);
                // 具体加载逻辑结束

                uiBase.Canvas.enabled = false;//设置Canvas的enable，减少SetActive的消耗
                //uiObj.name = openState.TemplateName;
                ShowWindow(uiBase);
                uiBase.isFinishLoad = true;
            }
            else
            {
                Log.Error("load ui {0} result.Asset not a gameobject", uiBase.UIID);
            }
            LoadingUICount--;
        }

        public IEnumerator LoadUIAsset(FUIBase uiBase)
        {
            float beginTime = Time.realtimeSinceStartup;
            //string path = string.Format("ui/{0}", uiBase.prefabPath);
            string path = uiBase.prefabPath;
            if (ABManager.UseAssetBundle())
            {
                var loader = AssetBundleLoader.Load(path);
                while (!loader.IsCompleted)
                    yield return null;
                if (AppConfig.IsLogAbLoadCost) Log.LogInfo("{0} Load AB, cost:{1:0.000}s", uiBase.prefabPath, Time.realtimeSinceStartup - beginTime);
                if (loader.Bundle == null)
                {
                    yield break;
                }

                beginTime = Time.realtimeSinceStartup;
                var pathSplit = uiBase.prefabPath.Split('/');
                string bundleName = pathSplit[pathSplit.Length - 1].ToLower();
                var req = loader.Bundle.LoadAssetAsync<GameObject>(bundleName);
                while (!req.isDone)
                    yield return null;
                uiBase.gameObject = GameObject.Instantiate(req.asset) as GameObject;

                uiBase.OnLoaded();
                //管理图集
                GameObject go = req.asset as GameObject;
                if (go)
                {
                    var windowAsset = go.GetComponent<UIWindowAsset>();
                    if (windowAsset && !string.IsNullOrEmpty(windowAsset.atals_arr))
                    {
                        string[] arr = windowAsset.atals_arr.Split(',');
                        int sprite_count = 0;
                        for (int i = 0; i < arr.Length; i++)
                        {
                            string atlas_name = arr[i].ToLower();
                            if (!CommonAtlases.Contains(atlas_name)) sprite_count++;
                            var atlas = loader.Bundle.LoadAsset<SpriteAtlas>(atlas_name);
                            if (atlas != null) ABManager.SpriteAtlases[atlas_name] = atlas;
                        }

                        if (sprite_count >= 2) Log.Error($"UI:{uiBase.prefabPath}包括多个图集({windowAsset.atals_arr})，请处理");
                    }
                }
                uiBase.UIResourceLoader = loader;
                if (AppConfig.IsLogAbLoadCost)
                {
                    var cost = Time.realtimeSinceStartup - beginTime;
                    Log.LogInfo($"Load Asset from {0}, cost:{1:0.###} s", uiBase.prefabPath, cost);

                }
            }
            else
            {
#if UNITY_EDITOR
                var localPath = "Assets/" + AppDef.ResourcesBuildDir + "/" + path + ".prefab";
                var localPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath(localPath, typeof(UnityEngine.Object));
                uiBase.gameObject = GameObject.Instantiate(localPrefab) as GameObject;
                uiBase.OnLoaded();
#endif
            }
        }

        private void ShowWindow(FUIBase uiBase)
        {
            if (uiBase != null && uiBase.Canvas != null)
            {
                uiBase.Canvas.enabled = true;
                if (sortOrder >= int.MaxValue)
                {
                    sortOrder = 0;
                }
                uiBase.Canvas.sortingOrder = sortOrder++;
                uiBase.OnShow();
                uiBase.OnEventRegister(true);
            }
        }

        private void HideWindow(FUIBase uiBase)
        {
            if (uiBase != null && uiBase.Canvas != null)
            {
                uiBase.Canvas.enabled = false;
                uiBase.OnEventRegister(false);
                uiBase.OnHide();
            }
        }

        public void DestroyWindow(int id, bool destroyImmediate = false)
        {
            FUIBase uiBase;
            dictUIBase.TryGetValue(id, out uiBase);
            if (uiBase == null || uiBase.gameObject == null)
            {
                Log.LogInfo("UI {0} has been destroyed", id);
                return;
            }
            uiBase.OnEventRegister(false);
            uiBase.OnDestroy();
            if (destroyImmediate)
            {
                UnityEngine.Object.DestroyImmediate(uiBase.gameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(uiBase.gameObject);
            }
            if (uiBase.UIResourceLoader != null)
            {
                uiBase.UIResourceLoader.Release();
            }
            dictUIBase.Remove(id);
        }
    }
}
