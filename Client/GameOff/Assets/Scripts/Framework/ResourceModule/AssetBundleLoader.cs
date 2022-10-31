using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// ab������
    /// �÷���1.Load
    ///       2.����֮���ֶ�����Relase����refCountΪ0ʱ��Unload(true)���ͷ�ab������
    /// </summary>
    public class AssetBundleLoader : AbstractResourceLoader
    {

        public delegate void CAssetBundleLoaderDelegate(bool isOk, AssetBundle ab);

        public static Action<string> NewAssetBundleLoaderEvent;

        public AssetBundle Bundle
        {
            get { return ResultObject as AssetBundle; }
        }

        private string RelativeResourceUrl;
        private List<UnityEngine.Object> _loadedAssets;

        /// <summary>
        /// AssetBundle���ط�ʽ
        /// </summary>
        private LoaderMode _loaderMode;

        private float beginTime;
        private string dependFrom = string.Empty;
        /// <summary>
        /// ����ab
        /// </summary>
        /// <param name="url">��Դ·��</param>
        /// <param name="callback">������ɵĻص�</param>
        /// <param name="loaderMode">Async�첽��syncͬ��</param>
        /// <returns></returns>
        public static AssetBundleLoader Load(string url, CAssetBundleLoaderDelegate callback = null,
            LoaderMode loaderMode = LoaderMode.Async)
        {
            if (!url.EndsWith(AppConfig.AssetBundleExt))
                url = url + AppConfig.AssetBundleExt;
            url = url.ToLower();
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) => callback(isOk, obj as AssetBundle);
            }
            var newLoader = AutoNew<AssetBundleLoader>(url, newCallback, false, loaderMode);

            return newLoader;
        }

        private static bool _hasPreloadAssetBundleManifest = false;
        private static AssetBundle _mainAssetBundle;
        private static AssetBundleManifest _assetBundleManifest;
        /// <summary>
        /// Unity5�£�ʹ��manifest����AssetBundle�ļ���
        /// bool isForce,���ȸ��º󣬿�����Ҫǿ��ˢ��AssetBundleManifest��
        /// </summary>
        public static void PreLoadManifest(bool isForce = false)
        {
            if (_hasPreloadAssetBundleManifest && isForce == false)
                return;

            _hasPreloadAssetBundleManifest = true;
            //�˷������ܼ��ص�manifest�ļ�
            //var manifestPath = string.Format("{0}/{1}/{1}.manifest", KResourceModule.BundlesPathRelative,KResourceModule.BuildPlatformName);
            // _mainAssetBundle = AssetBundle.LoadFromFile(manifestPath);
            // _assetBundleManifest = _mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            var manifestPath = ResourceModule.BundlesPathRelative + ResourceModule.GetBuildPlatformName();
            BytesLoader bytesLoader = BytesLoader.Load(manifestPath, LoaderMode.Sync);
            Debuger.Assert(bytesLoader != null, $"load manifest byte error path:{manifestPath}");
            _mainAssetBundle = AssetBundle.LoadFromMemory(bytesLoader.Bytes);
            Debuger.Assert(_mainAssetBundle != null, "load manifest ab error");
            _assetBundleManifest = _mainAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        }

        public override void Init(string url, params object[] args)
        {
            PreLoadManifest();
            base.Init(url);
            _loaderMode = (LoaderMode)args[0];

            if (NewAssetBundleLoaderEvent != null)
                NewAssetBundleLoaderEvent(url);

            RelativeResourceUrl = url;
            if (AppConfig.IsLogAbLoadCost) Log.LogInfo("[Start] Load AssetBundle, {0}", RelativeResourceUrl);
            ResourceModule.Instance.StartCoroutine(LoadAssetBundle(url));
        }

        /// <summary>
        /// ������AssetBundleLoader
        /// </summary>
        private AssetBundleLoader[] _depLoaders;

        private IEnumerator LoadAssetBundle(string relativeUrl)
        {
            // Unity 5 Manifest�й���������
            var abPath = relativeUrl.ToLower();
            var deps = _assetBundleManifest.GetAllDependencies(abPath);
            _depLoaders = new AssetBundleLoader[deps.Length];
            for (var d = 0; d < deps.Length; d++)
            {
                var dep = deps[d];
                _depLoaders[d] = AssetBundleLoader.Load(dep, null, _loaderMode);
                if (_depLoaders[d].dependFrom == string.Empty)
                    _depLoaders[d].dependFrom = relativeUrl;
            }
            for (var l = 0; l < _depLoaders.Length; l++)
            {
                var loader = _depLoaders[l];
                while (!loader.IsCompleted)
                {
                    yield return null;
                }
            }

            // Unity 5 AssetBundle�Զ�תСд
            relativeUrl = relativeUrl.ToLower();
            if (AppConfig.IsLogAbLoadCost) beginTime = Time.realtimeSinceStartup;

            string _fullUrl = ResourceModule.GetAbFullPath(relativeUrl);

            if (string.IsNullOrEmpty(_fullUrl))
            {
                OnFinish(null);
                yield break;
            }

            AssetBundle assetBundle = null;
            if (_loaderMode == LoaderMode.Sync)
            {
                assetBundle = AssetBundle.LoadFromFile(_fullUrl);
            }
            else
            {
                var request = AssetBundle.LoadFromFileAsync(_fullUrl);
                while (!request.isDone)
                {
                    if (IsReadyDisposed) // ��;�ͷ�
                    {
                        OnFinish(null);
                        yield break;
                    }
                    Progress = request.progress;
                    yield return null;
                }
                assetBundle = request.assetBundle;
            }
            if (assetBundle == null)
                Log.Error("assetBundle is NULL: {0}", RelativeResourceUrl);
            if (AppConfig.IsLogAbLoadCost) Log.LogInfo("[Finish] Load AssetBundle {0}, CostTime {1}s {2}", relativeUrl, Time.realtimeSinceStartup - beginTime, dependFrom);
            //if (AppConfig.IsSaveCostToFile && !relativeUrl.StartsWith("ui/")) LogFileRecorder.WriteLoadAbLog(relativeUrl, Time.realtimeSinceStartup - beginTime);
            OnFinish(assetBundle);
        }

        protected override void OnFinish(object resultObj)
        {
            base.OnFinish(resultObj);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            if (Bundle != null && RefCount <= 0)
            {
                Bundle.Unload(true);
            }
            if (_depLoaders != null && _depLoaders.Length > 0)
            {
                foreach (var depLoader in _depLoaders)
                {
                    if (depLoader.Bundle != null && depLoader.RefCount <= 0)
                        depLoader.Bundle.Unload(true);
                    depLoader.Release();
                }
            }

            _depLoaders = null;

            if (_loadedAssets != null)
            {
                foreach (var loadedAsset in _loadedAssets)
                {
                    Object.DestroyImmediate(loadedAsset, true);
                }
                _loadedAssets.Clear();
            }
        }

        public override void Release()
        {
            if (Application.isEditor)
            {
                if (Url.Contains("Arial"))
                {
                    Log.Error("Ҫ�ͷ�Arial���壡����������builtinextra:{0}", Url);
                    //UnityEditor.EditorApplication.isPaused = true;
                }
            }

            base.Release();
        }

        /// �f��tips~����
        /// ԭ��Ϊ��ÿ�ζ�ͨ��getterȡһ��assetBundle�������л���ѹ���⣬����һ�㣬����AddWatch���Թ������������.assetBundle�ŵ�Dictionary�ﻺ�棬��ѯ�����
        /// ��Ϊ������.assetBundle��һ����Getter��û�������л����⡣  ������֤.mainAsset��
        public void PushLoadedAsset(Object getAsset)
        {
            if (_loadedAssets == null)
                _loadedAssets = new List<Object>();
            _loadedAssets.Add(getAsset);
        }
    }
}
