using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// ����ģʽ��ͬ�����첽
    /// </summary>
    public enum LoaderMode
    {
        Async,
        Sync,
    }
    /// <summary>
    /// ������ԴLoader�̳����
    /// </summary>
    public abstract class AbstractResourceLoader : IAsyncObject
    {
        public delegate void LoaderDelgate(bool isOk, object resultObject);
        private readonly List<AbstractResourceLoader.LoaderDelgate> _afterFinishedCallbacks = new List<AbstractResourceLoader.LoaderDelgate>();

        /// <summary>
        /// ���ռ��ؽ������Դ
        /// </summary>
        public object ResultObject { get; private set; }

        /// <summary>
        /// �Ƿ��Ѿ���ɣ����Ĵ�����Loader��������Э��StartCoroutine
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// ����WWW, IsFinished���ж��Ƿ��д���
        /// </summary>
        public virtual bool IsError { get; private set; }

        /// <summary>
        /// �첽���̷��ص���Ϣ
        /// </summary>
        public virtual string AsyncMessage
        {
            get { return null; }
        }

        /// <summary>
        /// ͬResultObject
        /// </summary>
        public object AsyncResult
        {
            get { return ResultObject; }
        }
        /// <summary>
        /// �Ƿ����
        /// </summary>
        public bool IsSuccess
        {
            get { return !IsError && ResultObject != null && !IsReadyDisposed; }
        }

        /// <summary>
        /// �Ƿ���Application�˳�״̬
        /// </summary>
        private bool _isQuitApplication = false;

        /// <summary>
        /// ForceNew�ģ���AutoNew
        /// </summary>
        public bool IsForceNew;

        /// <summary>
        /// RefCount Ϊ 0������Ԥ��״̬
        /// </summary>
        protected bool IsReadyDisposed { get; private set; }

        /// <summary>
        ///  �����¼�
        /// </summary>
        public event Action DisposeEvent;

        [System.NonSerialized]
        public float InitTiming = -1;
        [System.NonSerialized]
        public float FinishTiming = -1;

        /// <summary>
        /// ��ʱ
        /// </summary>
        public float FinishUsedTime
        {
            get
            {
                if (!IsCompleted) return -1;
                return FinishTiming - InitTiming;
            }
        }

        /// <summary>
        /// ���ü���
        /// </summary>
        private int refCount = 0;

        public int RefCount
        {
            get { return refCount; }
            set
            {
                //if(Application.isEditor && !string.IsNullOrEmpty(Url)) Log.Info($"{Url} ,refCount:{refCount}->{value}");
                refCount = value;
            }
        }

        public string Url { get; private set; }

        /// <summary>
        /// ���Ȱٷֱ�~ 0-1����
        /// </summary>
        public virtual float Progress { get; protected set; }


        public event Action<string> SetDescEvent;
        private string _desc = "";

        /// <summary>
        /// ����, ��������, һ��������ԴDebugger��
        /// </summary>
        /// <returns></returns>
        public virtual string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value;
                if (SetDescEvent != null)
                    SetDescEvent(_desc);
            }
        }

        /// <summary>
        /// ͳһ�Ķ��󹤳�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <param name="forceCreateNew"></param>
        /// <returns></returns>
        protected static T AutoNew<T>(string url, LoaderDelgate callback = null, bool forceCreateNew = false,
            params object[] initArgs) where T : AbstractResourceLoader, new()
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.Error("[{0}:AutoNew]urlΪ��", typeof(T));
                return null;
            }

            Dictionary<string, AbstractResourceLoader> typesDict = ABManager.GetTypeDict(typeof(T));
            AbstractResourceLoader loader;
            typesDict.TryGetValue(url, out loader);
            if (forceCreateNew || loader == null)
            {
                loader = new T();
                if (!forceCreateNew)
                {
                    typesDict[url] = loader;
                }

                loader.IsForceNew = forceCreateNew;
                loader.Init(url, initArgs);

                if (Application.isEditor)
                {
                    ResourceLoaderDebugger.Create(typeof(T).Name, url, loader);
                }
            }
            else if (loader != null && loader.IsCompleted && loader.IsError)
            {
                loader.Init(url, initArgs);
            }
            else
            {
                if (loader.RefCount < 0)
                {
                    //loader.IsDisposed = false;  // ת�������Ŀ���
                    Log.Error("Error RefCount!");
                }
            }

            loader.RefCount++;

            // RefCount++�ˣ����¼���ڶ�����׼�������Loader
            if (ABManager.UnUsesLoaders.ContainsKey(loader))
            {
                ABManager.UnUsesLoaders.Remove(loader);
                loader.Revive();
            }

            loader.AddCallback(callback);

            return loader as T;
        }

        /// <summary>
        /// ����
        /// </summary>
        public virtual void Revive()
        {
            IsReadyDisposed = false; // ���
        }

        protected AbstractResourceLoader()
        {
            RefCount = 0;
        }

        public virtual void Init(string url, params object[] args)
        {
            InitTiming = Time.realtimeSinceStartup;
            ResultObject = null;
            IsReadyDisposed = false;
            IsError = false;
            IsCompleted = false;

            Url = url;
            Progress = 0;
        }

        protected virtual void OnFinish(object resultObj)
        {
            // ���ReadyDispose����Ч�����ô������ս����
            ResultObject = resultObj;

            // ���ReadyDisposed, ��Ȼ�ᱣ��ResultObject, ���ڻص�ʱ��ʧ��~�޻ص�����
            var callbackObject = !IsReadyDisposed ? ResultObject : null;

            FinishTiming = Time.realtimeSinceStartup;
            Progress = 1;
            IsError = callbackObject == null;

            IsCompleted = true;
            DoCallback(IsSuccess, callbackObject);

            if (IsReadyDisposed)
            {
                //Dispose();
                Log.LogInfo("[AbstractResourceLoader:OnFinish]ʱ��׼��Disposed {0}", Url);
            }
        }

        /// <summary>
        /// ��IsFinisehd���ִ�еĻص�
        /// </summary>
        /// <param name="callback"></param>
        public void AddCallback(LoaderDelgate callback)
        {
            if (callback != null)
            {
                if (IsCompleted)
                {
                    if (ResultObject == null)
                        Log.Error("Null ResultAsset {0}", Url);
                    callback(ResultObject != null, ResultObject);
                }
                else
                    _afterFinishedCallbacks.Add(callback);
            }
        }

        protected void DoCallback(bool isOk, object resultObj)
        {
            foreach (var callback in _afterFinishedCallbacks)
            {
                callback(isOk, resultObj);
            }
            _afterFinishedCallbacks.Clear();
        }

        /// <summary>
        /// ִ��Release�������̴�����������
        /// </summary>
        /// <param name="gcNow">�Ƿ����̴����������գ�Ĭ�����������Ǹ�������е�</param>
        public virtual void Release(bool gcNow)
        {
            //            if (gcNow)
            //                IsBeenReleaseNow = true;

            Release();

            if (gcNow)
                ABManager.DoGarbageCollect();
        }

        /// <summary>
        /// �ͷ���Դ���������ü���
        /// </summary>
        public virtual void Release()
        {
            if (IsReadyDisposed)
            {
                Log.Warning("[{0}]repeat  dispose! {1}, Count: {2}", GetType().Name, this.Url, RefCount);
            }

            RefCount--;
            if (RefCount <= 0)
            {
                if (AppConfig.IsLogAbInfo)
                {
                    if (RefCount < 0)
                    {
                        Log.Error("[{3}]RefCount< 0, {0} : {1}, NowRefCount: {2}, Will be fix to 0", GetType().Name, Url, RefCount, GetType());

                        RefCount = Mathf.Max(0, RefCount);
                    }

                    if (ABManager.UnUsesLoaders.ContainsKey(this))
                    {
                        Log.Error("[{1}]UnUsesLoader exists: {0}", this, GetType());
                    }
                }

                // ������У�׼��Dispose
                ABManager.UnUsesLoaders[this] = Time.time;

                IsReadyDisposed = true;
                OnReadyDisposed();
            }
        }

        /// <summary>
        /// ����Ϊ0ʱ������׼��Disposed״̬ʱ����
        /// </summary>
        protected virtual void OnReadyDisposed()
        {
        }

        /// <summary>
        /// Dispose�������ü��ģ� DoDisposeһ�����ڼ̳���д
        /// </summary>
        public void Dispose()
        {
            if (DisposeEvent != null)
                DisposeEvent();

            if (!IsForceNew)
            {
                var type = GetType();
                var typeDict = ABManager.GetTypeDict(type);
                //if (Url != null) // TODO: �Ժ�ȥ��
                {
                    var bRemove = typeDict.Remove(Url);
                    if (!bRemove)
                    {
                        Log.Warning("[{0}:Dispose]No Url: {1}, Cur RefCount: {2}", type.Name, Url, RefCount);
                    }
                }
            }

            if (IsCompleted)
                DoDispose();
            // δ��ɣ���OnFinishʱ��ִ��DoDispose
        }

        protected virtual void DoDispose()
        {
        }


        /// <summary>
        /// ǿ�ƽ���Dispose������Ref����������������RefCountΪ1��Loader��
        /// </summary>
        public virtual void ForceDispose()
        {
            if (_isQuitApplication)
                return;
            if (RefCount != 1)
            {
                Log.Warning("[ForceDisose]Use force dispose to dispose loader, recommend this loader RefCount == 1");
            }
            Dispose();
        }

        /// <summary>
        /// By Unity Reflection
        /// </summary>
        protected void OnApplicationQuit()
        {
            _isQuitApplication = true;
        }
    }

    // UnityǱ����: �ȴ�֡�����ִ�У�����һЩ(DestroyImmediatly)��Phycis������

}
