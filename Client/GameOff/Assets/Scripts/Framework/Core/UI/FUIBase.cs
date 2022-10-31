using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 框架层的UIBase
    /// </summary>
    public class FUIBase
    {
        public int UIID;
        public string prefabPath;
        public PanelType PanelType = PanelType.NormalUI;
        /// <summary> 是否加载完成 </summary>
        public bool isFinishLoad;

        public GameObject gameObject;
        private Transform _transform;
        public Transform transform
        {
            get
            {
                if (_transform == null && gameObject)
                {
                    _transform = gameObject.transform;
                }
                return _transform;
            }
        }
        private Canvas _canvas;
        /// <summary>
        /// 除HUD外，每个界面都有一个Canvas
        /// </summary>
        public Canvas Canvas
        {
            get
            {
                if (_canvas == null && gameObject)
                {
                    _canvas = gameObject.GetComponent<Canvas>();
                }
                return _canvas;
            }
        }

        public AbstractResourceLoader UIResourceLoader; // 加载器，用于手动释放资源



        public virtual void OnOpen(object args) { }

        public virtual void OnLoaded() { }

        public virtual void OnShow() { }

        public virtual void OnHide() { }

        public virtual void OnDestroy() { }

        public virtual void OnEventRegister(bool toRegister) { }
    }

}