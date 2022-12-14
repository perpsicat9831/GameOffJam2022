using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// 根據不同模式，從AssetBundle中獲取Asset或從Resources中獲取,两种加载方式同时实现的桥接类
    /// 读取一个文件的对象，不做拷贝和引用
    /// </summary>
    public class AssetFileLoader : AbstractResourceLoader
    {
        public delegate void AssetFileBridgeDelegate(bool isOk, Object resultObj);

        public Object Asset
        {
            get { return ResultObject as Object; }
        }

        public override float Progress
        {
            get
            {
                if (_bundleLoader != null)
                    return _bundleLoader.Progress;
                return 0;
            }
        }

        private AssetBundleLoader _bundleLoader;

        public static AssetFileLoader Load(string path, AssetFileBridgeDelegate assetFileLoadedCallback = null, LoaderMode loaderMode = LoaderMode.Async)
        {
            // 添加扩展名
            if (!ResourceModule.IsEditorLoadAsset)
                path = path + AppConfig.AssetBundleExt;

            LoaderDelgate realcallback = null;
            if (assetFileLoadedCallback != null)
            {
                realcallback = (isOk, obj) => assetFileLoadedCallback(isOk, obj as Object);
            }

            return AutoNew<AssetFileLoader>(path, realcallback, false, loaderMode);
        }

        /// <summary>
        /// Check Bundles/[Platform]/xxxx.kk exists?
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsBundleResourceExist(string url)
        {
            if (ResourceModule.IsEditorLoadAsset)
            {
                var editorPath = "Assets/" + AppDef.ResourcesBuildDir + "/" + url;
                var hasEditorUrl = File.Exists(editorPath);
                if (hasEditorUrl) return true;
            }

            return ResourceModule.IsResourceExist(ResourceModule.BundlesPathRelative + url.ToLower() + AppConfig.AssetBundleExt);
        }
        public override void Init(string url, params object[] args)
        {
            var loaderMode = (LoaderMode)args[0];

            base.Init(url, args);
            ResourceModule.Instance.StartCoroutine(_Init(Url, loaderMode));
        }

        private IEnumerator _Init(string path, LoaderMode loaderMode)
        {
            Debug.Log(path);

            Object getAsset = null;

            if (ResourceModule.IsEditorLoadAsset)
            {
#if UNITY_EDITOR
                if (path.EndsWith(".unity"))
                {
                    // scene
                    getAsset = ResourceModule.Instance;
                    Log.Warning("Load scene from Build Settings: {0}", path);
                }
                else
                {
                    getAsset = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/" + AppDef.ResourcesBuildDir + "/" + path, typeof(UnityEngine.Object));
                    if (getAsset == null)
                    {
                        Log.Error("Asset is NULL(from {0} Folder): {1}", AppDef.ResourcesBuildDir, path);
                    }
                }
#else
				Log.Error("`IsEditorLoadAsset` is Unity Editor only");

#endif
            }
            else if (!AppConfig.IsLoadAssetBundle)
            {
                string extension = Path.GetExtension(path);
                path = path.Substring(0, path.Length - extension.Length); // remove extensions

                getAsset = Resources.Load<Object>(path);
                if (getAsset == null)
                {
                    Log.Error("Asset is NULL(from Resources Folder): {0}", path);
                }
            }
            else
            {
                _bundleLoader = AssetBundleLoader.Load(path, null, loaderMode);

                while (!_bundleLoader.IsCompleted)
                {
                    if (IsReadyDisposed) // 中途释放
                    {
                        _bundleLoader.Release();
                        OnFinish(null);
                        yield break;
                    }
                    yield return null;
                }

                if (!_bundleLoader.IsSuccess)
                {
                    Log.Error("[AssetFileLoader]Load BundleLoader Failed(Error) when Finished: {0}", path);
                    _bundleLoader.Release();
                    OnFinish(null);
                    yield break;
                }

                var assetBundle = _bundleLoader.Bundle;

                DateTime beginTime = DateTime.Now;
                // Unity 5 下，不能用mainAsset, 要取对象名
                var abAssetName = Path.GetFileNameWithoutExtension(Url).ToLower();
                if (!assetBundle.isStreamedSceneAssetBundle)
                {
                    if (loaderMode == LoaderMode.Sync)
                    {
                        getAsset = assetBundle.LoadAsset(abAssetName);
                        Debuger.Assert(getAsset);
                        _bundleLoader.PushLoadedAsset(getAsset);
                    }
                    else
                    {
                        var request = assetBundle.LoadAssetAsync(abAssetName);
                        while (!request.isDone)
                        {
                            yield return null;
                        }
                        Debuger.Assert(getAsset = request.asset);
                        _bundleLoader.PushLoadedAsset(getAsset);
                    }
                }
                else
                {
                    // if it's a scene in asset bundle, did nothing
                    // but set a fault Object the result
                    getAsset = ResourceModule.Instance;
                }

                if (AppConfig.IsLogAbLoadCost) Log.LogInfo("[Finsh] Load {0}, {1}, {2}s", "AssetFileBridge", path, (System.DateTime.Now - beginTime).TotalSeconds);

                if (getAsset == null)
                {
                    Log.Error("Asset is NULL: {0}", path);
                }

            }

            if (AppConfig.UseAssetDebugger)
            {
                if (getAsset != null)
                    ResoourceLoadedAssetDebugger.Create(getAsset.GetType().Name, Url, getAsset as Object);

                // 编辑器环境下，如果遇到GameObject，对Shader进行Fix
                if (getAsset is GameObject)
                {
                    var go = getAsset as GameObject;
                    foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                    {
                        RefreshMaterialsShaders(r);
                    }
                }
            }

            if (getAsset != null)
            {
                // 更名~ 注明来源asset bundle 带有类型
                getAsset.name = String.Format("{0}~{1}", getAsset, Url);
            }
            OnFinish(getAsset);
        }

        /// <summary>
        /// 编辑器模式下，对指定GameObject刷新一下Material
        /// </summary>
        public static void RefreshMaterialsShaders(Renderer renderer)
        {
            if (renderer.sharedMaterials != null)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null && mat.shader != null)
                    {
                        mat.shader = Shader.Find(mat.shader.name);
                    }
                }
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            if (_bundleLoader != null)
                _bundleLoader.Release(); // 释放Bundle(WebStream)

            //if (IsFinished)
            {
                if (!AppConfig.IsLoadAssetBundle)
                {
                    Resources.UnloadAsset(ResultObject as Object);
                }
                else
                {
                    //Object.DestroyObject(ResultObject as UnityEngine.Object);

                    // Destroying GameObjects immediately is not permitted during physics trigger/contact, animation event callbacks or OnValidate. You must use Destroy instead.
                    //                    Object.DestroyImmediate(ResultObject as Object, true);
                }

                //var bRemove = Caches.Remove(Url);
                //if (!bRemove)
                //{
                //    Log.Warning("[DisposeTheCache]Remove Fail(可能有两个未完成的，同时来到这) : {0}", Url);
                //}
            }
            //else
            //{
            //    // 交给加载后，进行检查并卸载资源
            //    // 可能情况TIPS：两个未完成的！会触发上面两次！
            //}
        }

    }
}
