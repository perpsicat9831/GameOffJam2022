using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public interface IManagerModule
    {
        void OnInit();
        void OnDestroy();

    }
    public interface IManagerUpdateModule
    {
        void OnUpData();
    }
    public class BaseManager<T> : IManagerModule where T : class, new()
    {
        #region Singleton
        private static T _instance;
        private static readonly Object _lock = new Object();
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        public virtual void OnInit() { }
        public virtual void OnDestroy() { }

    }
}
