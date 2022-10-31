using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// ��ȡ�ֽڣ�����WWW, ���Զ�ʶ��Product/Bundles/PlatformĿ¼��StreamingAssets·��
    /// </summary>
    public class BytesLoader : AbstractResourceLoader
    {

        public byte[] Bytes { get; private set; }

        /// <summary>
        /// �첽ģʽ��ʹ����WWWLoader
        /// </summary>
        private WWWLoader _wwwLoader;

        private LoaderMode _loaderMode;

        public static BytesLoader Load(string path, LoaderMode loaderMode)
        {
            var newLoader = AutoNew<BytesLoader>(path, null, false, loaderMode);
            return newLoader;
        }

        public override void Init(string url, params object[] args)
        {
            base.Init(url, args);

            _loaderMode = (LoaderMode)args[0];
            ResourceModule.Instance.StartCoroutine(CoLoad(url));
        }

        private IEnumerator CoLoad(string url)
        {
            if (_loaderMode == LoaderMode.Sync)
            {
                Bytes = ResourceModule.LoadAssetsSync(url);
            }
            else
            {
                string _fullUrl;
                var getResPathType = ResourceModule.GetResourceFullPath(url, _loaderMode == LoaderMode.Async, out _fullUrl);
                if (getResPathType == ResourceModule.GetResourceFullPathType.Invalid)
                {
                    Log.Error("[HotBytesLoader]Error Path: {0}", url);
                    OnFinish(null);
                    yield break;
                }
                _wwwLoader = WWWLoader.Load(_fullUrl);
                while (!_wwwLoader.IsCompleted)
                {
                    Progress = _wwwLoader.Progress;
                    yield return null;
                }

                if (!_wwwLoader.IsSuccess)
                {
                    Log.Error("[HotBytesLoader]Error Load WWW: {0}", url);
                    OnFinish(null);
                    yield break;
                }
                //TODO ����WebRequst
                //Bytes = _wwwLoader.Www.downloadHandler.data;
                Bytes = _wwwLoader.Www.bytes;
            }

            OnFinish(Bytes);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            if (_wwwLoader != null)
            {
                _wwwLoader.Release();
            }
        }
    }
}
