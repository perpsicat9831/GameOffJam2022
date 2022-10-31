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

        // ǰ�������ڼ����
        private static IEnumerator CachedWWWLoaderMonitorCoroutine; // ר�ż��WWW��Э��
        private const int MAX_WWW_COUNT = 15; // ͬʱ���е����Www���ظ������������Ŷӵȴ�
        private static int WWWLoadingCount = 0; // �ж��ٸ�WWW��������, �����޵�

        private static readonly Stack<WWWLoader> WWWLoadersStack = new Stack<WWWLoader>();
        // WWWLoader�ļ����Ǻ���ȳ�! ��һ��Э��ȫ�����ҹ���. ����ӿ������ȼ��أ�

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
            WWWLoadersStack.Push(this); // ��ִ�п�ʼ���أ���www�����Э�̿���

            if (CachedWWWLoaderMonitorCoroutine == null)
            {
                CachedWWWLoaderMonitorCoroutine = WWWLoaderMonitorCoroutine();
                ResourceModule.Instance.StartCoroutine(CachedWWWLoaderMonitorCoroutine);
            }
        }

        protected void StartLoad()
        {
            ResourceModule.Instance.StartCoroutine(CoLoad(Url)); //����Э�̼���Assetbundle��ִ��Callback
        }

        /// <summary>
        /// Э�̼���Assetbundle���������ִ��callback
        /// </summary>
        /// <param name="url">��Դ��url</param>
        /// <param name="callback"></param>
        /// <param name="callbackArgs"></param>
        /// <returns></returns>
        private IEnumerator CoLoad(string url)
        {
            if (AppConfig.IsLogAbInfo) Log.LogInfo("[Request] WWW, {1}", url);

            // Ǳ���򣺲���LoadFromCache~��ֻ������.assetBundle
#pragma warning disable 0618
            Www = new WWW(url);
#pragma warning restore 0618
            BeginLoadTime = Time.time;
            WWWLoadingCount++;

            //����AssetBundle��ѹ���̵߳����ȼ�
            Www.threadPriority = Application.backgroundLoadingPriority; // ȡ��ȫ�ֵļ��������ٶ�
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
                    // TODO: Android�µĴ����������Ϊ�ļ�������!
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

            // Ԥ��WWW��������������ʼ��, ����ڴ�й¶~
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
        /// ������Э��
        /// �������WWWLoaderʱ������~
        /// ��������loader�ᱻ���ȼ���
        /// </summary>
        /// <returns></returns>
        protected static IEnumerator WWWLoaderMonitorCoroutine()
        {
            //yield return new WaitForEndOfFrame(); // ��һ�εȴ���֡����
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
