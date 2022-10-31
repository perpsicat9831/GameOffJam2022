using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// KEngine MonoBehaivour
    /// Without Update, With some cache
    /// </summary>
    public class CacheBehaviour : MonoBehaviour
    {
        private Transform _cachedTransform;

        public Transform CachedTransform
        {
            get { return _cachedTransform ?? (_cachedTransform = transform); }
        }

        private GameObject _cacheGameObject;

        public GameObject CachedGameObject
        {
            get { return _cacheGameObject ?? (_cacheGameObject = gameObject); }
        }

        protected bool IsDestroyed = false;
        public static event System.Action<CacheBehaviour> StaticDestroyEvent;
        public event System.Action<CacheBehaviour> DestroyEvent;

        private static bool _isApplicationQuited = false; // ȫ�ֱ��, �����Ƿ��˳�״̬

        public static bool IsApplicationQuited
        {
            get { return _isApplicationQuited; }
        }

        public static System.Action ApplicationQuitEvent;

        private float _TimeScale = 1f; // TODO: In Actor, Bullet,....

        public virtual float TimeScale
        {
            get { return _TimeScale; }
            set { _TimeScale = value; }
        }

        public virtual void Delete()
        {
            Delete(0);
        }

        /// <summary>
        /// GameObject.Destory����
        /// </summary>
        public virtual void Delete(float time)
        {
            if (!IsApplicationQuited)
                UnityEngine.Object.Destroy(gameObject, time);
        }

        // ֻɾ���Լ����Component
        public virtual void DeleteSelf()
        {
            UnityEngine.Object.Destroy(this);
        }

        // �̳�CBehaivour����ͨ��Deleteɾ��
        // �����˳�ʱ��ǿ��Destroy���У��������˸����
        protected virtual void OnDestroy()
        {
            IsDestroyed = true;
            if (DestroyEvent != null)
                DestroyEvent(this);
            if (StaticDestroyEvent != null)
                StaticDestroyEvent(this);
        }

        private void OnApplicationQuit()
        {
            if (!_isApplicationQuited)
                Log.LogInfo("OnApplicationQuit!");

            _isApplicationQuited = true;

            if (ApplicationQuitEvent != null)
                ApplicationQuitEvent();
        }
    }
}
