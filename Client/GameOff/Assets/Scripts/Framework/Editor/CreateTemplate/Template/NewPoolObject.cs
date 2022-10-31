using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Framework;

namespace Logic
{
    public class PoolObject : IPoolable, IPoolType
    {
        protected AssetBundleLoader abLoader;
        /// <summary>
        /// 表现层对象
        /// </summary>
        protected GameObject goCell;
        protected string abPath = "Prefab/path/ObjectName";
        protected string abPrefabName = "ObjectName";

        public bool IsRecycled { get; set; }

        public void OnRecycled()
        {

        }

        public void OnDestroy()
        {
            if (abLoader != null)
            {
                abLoader.Release();
                abLoader = null;
            }
            if (goCell != null)
            {
                GameObject.Destroy(goCell);
                goCell = null;
            }
        }

        public virtual void Recycle2Cache()
        {
            if (goCell != null)
            {
                goCell.SetActive(false);
            }
        }



        /// <summary>
        /// 创建时调用
        /// </summary>
        public void OnCreate(Transform parent, float posX, float posY)
        {
            if (goCell == null)
            {
                abLoader = AssetBundleLoader.Load(abPath, (isOk, ab) =>
                {
                    if (isOk)
                    {
                        var request = ab.LoadAsset<GameObject>(abPrefabName);
                        goCell = GameObject.Instantiate(request);

                        SetPlatformPos(parent, posX, posY);
                    }
                });
            }
            else
            {
                SetPlatformPos(parent, posX, posY);
            }
        }

        private void SetPlatformPos(Transform parent, float posX, float posY)
        {
            if (goCell != null)
            {
                goCell.SetActive(true);
                goCell.transform.SetParent(parent);
                goCell.transform.localPosition = new Vector3(posX, posY, 0);
            }
        }
    }
}
