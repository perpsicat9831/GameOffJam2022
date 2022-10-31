using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Load www, A wrapper of WWW.
    /// </summary>
    public class WWWLoader : AbstractResourceLoader
    {

        // 前几项用于监控器
        private static IEnumerator CachedWWWLoaderMonitorCoroutine; // 专门监控WWW的协程
        private const int MAX_WWW_COUNT = 15; // 同时进行的最大Www加载个数，超过的排队等待
        private static int WWWLoadingCount = 0; // 有多少个WWW正在运作, 有上限的

        private static readonly Stack<WWWLoader> WWWLoadersStack = new Stack<WWWLoader>();
        // WWWLoader的加载是后进先出! 有一个协程全局自我管理. 后来涌入的优先加载！

        public static event Action<string> WWWFinishCallback;

        public float BeginLoadTime;
        public float FinishLoadTime;
#pragma warning disable 0618
        public WWW Www;
#pragma warning restore 0618
        public int Size
        {
            get
            {
                return Www.bytesDownloaded;
            }
        }

        public float LoadSpeed
        {
            get
            {
                if (!IsCompleted)
                    return 0;
                return Size / (FinishLoadTime - BeginLoadTime);
            }
        }

        //public int DownloadedSize { get { return Www != null ? Www.bytesDownloaded : 0; } }
        public override bool IsError
        {
            get { return Www != null && !string.IsNullOrEmpty(Www.error); }
        }

        /// <summary>
        /// Use this to directly load WWW by Callback or Coroutine, pass a full URL.
        /// A wrapper of Unity's WWW class.
        /// </summary>
        public static WWWLoader Load(string url, LoaderDelgate callback = null)
        {
            var wwwLoader = AutoNew<WWWLoader>(url, callback);
            return wwwLoader;
        }

        public override void Init(string url, params object[] args)
        {
            base.Init(url, args);
            WWWLoadersStack.Push(this); // 不执行开始加载，由www监控器协程控制

            if (CachedWWWLoaderMonitorCoroutine == null)
            {
                CachedWWWLoaderMonitorCoroutine = WWWLoaderMonitorCoroutine();
                ResourceModule.Instance.StartCoroutine(CachedWWWLoaderMonitorCoroutine);
            }
        }

        protected void StartLoad()
        {
            ResourceModule.Instance.StartCoroutine(CoLoad(Url)); //开启协程加载Assetbundle，执行Callback
        }

        /// <summary>
        /// 协程加载Assetbundle，加载完后执行callback
        /// </summary>
        /// <param name="url">资源的url</param>
        /// <param name="callback"></param>
        /// <param name="callbackArgs"></param>
        /// <returns></returns>
        private IEnumerator CoLoad(string url)
        {
            if (AppConfig.IsLogAbInfo) Log.LogInfo("[Request] WWW, {1}", url);

            // 潜规则：不用LoadFromCache~它只能用在.assetBundle
#pragma warning disable 0618
            Www = new WWW(url);
#pragma warning restore 0618
            BeginLoadTime = Time.time;
            WWWLoadingCount++;

            //设置AssetBundle解压缩线程的优先级
            Www.threadPriority = Application.backgroundLoadingPriority; // 取用全局的加载优先速度
            while (!Www.isDone)
            {
                Progress = Www.progress;
                yield return null;
            }

            yield return Www;
            WWWLoadingCount--;
            Progress = 1;
            if (IsReadyDisposed)
            {
                Log.Error("[KWWWLoader]Too early release: {0}", url);
                OnFinish(null);
                yield break;
            }
            if (!string.IsNullOrEmpty(Www.error))
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    // TODO: Android下的错误可能是因为文件不存在!
                }

                if (url.StartsWith(ResourceModule.GetFileProtocol))
                {
                    string fileRealPath = url.Replace(ResourceModule.GetFileProtocol, "");
                    Log.Error("File {0} Exist State: {1}", fileRealPath, System.IO.File.Exists(fileRealPath));
                }
                Log.Error("[KWWWLoader:Error]{0} {1}", Www.error, url);

                OnFinish(null);
                yield break;
            }
            else
            {

                if (WWWFinishCallback != null)
                    WWWFinishCallback(url);

                Desc = string.Format("{0}K", Www.bytes.Length / 1024f);
                OnFinish(Www);
            }

            // 预防WWW加载器永不反初始化, 造成内存泄露~
            if (Application.isEditor)
            {
                while (ABManager.GetCount<WWWLoader>() > 0)
                    yield return null;

                yield return new WaitForSeconds(5f);

                while (!IsReadyDisposed)
                {
                    Log.Error("[KWWWLoader]Not Disposed Yet! : {0}", this.Url);
                    yield return null;
                }
            }
        }

        protected override void OnFinish(object resultObj)
        {
            FinishLoadTime = Time.time;
            base.OnFinish(resultObj);
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            Www.Dispose();
            Www = null;
        }


        /// <summary>
        /// 监视器协程
        /// 超过最大WWWLoader时，挂起~
        /// 后来的新loader会被优先加载
        /// </summary>
        /// <returns></returns>
        protected static IEnumerator WWWLoaderMonitorCoroutine()
        {
            //yield return new WaitForEndOfFrame(); // 第一次等待本帧结束
            yield return null;

            while (WWWLoadersStack.Count > 0)
            {
                if (ResourceModule.LoadByQueue)
                {
                    while (ABManager.GetCount<WWWLoader>() != 0)
                        yield return null;
                }
                while (WWWLoadingCount >= MAX_WWW_COUNT)
                {
                    yield return null;
                }

                var wwwLoader = WWWLoadersStack.Pop();
                wwwLoader.StartLoad();
            }

            ResourceModule.Instance.StopCoroutine(CachedWWWLoaderMonitorCoroutine);
            CachedWWWLoaderMonitorCoroutine = null;
        }
    }
}
