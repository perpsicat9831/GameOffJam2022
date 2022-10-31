using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// ��ܲ��UIBase
    /// </summary>
    public class FUIBase
    {
        public int UIID;
        public string prefabPath;
        public PanelType PanelType = PanelType.NormalUI;
        /// <summary> �Ƿ������� </summary>
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
        /// ��HUD�⣬ÿ�����涼��һ��Canvas
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

        public AbstractResourceLoader UIResourceLoader; // �������������ֶ��ͷ���Դ



        public virtual void OnOpen(object args) { }

        public virtual void OnLoaded() { }

        public virtual void OnShow() { }

        public virtual void OnHide() { }

        public virtual void OnDestroy() { }

        public virtual void OnEventRegister(bool toRegister) { }
    }

}