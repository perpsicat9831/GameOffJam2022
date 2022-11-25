using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Framework;

namespace Logic
{
    public class Fish : IPoolable, IPoolType
    {
        protected AssetBundleLoader abLoader;
        /// <summary>
        /// 表现层对象
        /// </summary>
        protected GameObject goCell;
        protected string abPath = "Prefab/Fish/Fish";
        protected string abPrefabName = "Fish";
        private FishMono fishMono;
        /// <summary>
        /// 被抓住了
        /// </summary>
        public bool IsCatched;

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
            IsCatched = false;
            if (goCell != null)
            {
                goCell.transform.SetParent(FishingManager.Instance.UnUseFishParent);
                goCell.transform.localPosition = new Vector3(999, 999, 999);
            }
            fishMono.UseGravity(false);
            ObjectPool<Fish>.Instance.Recycle(this);
        }



        /// <summary>
        /// 创建时调用
        /// </summary>
        public void OnCreate(Transform pos)
        {
            if (goCell == null)
            {
                abLoader = AssetBundleLoader.Load(abPath, (isOk, ab) =>
                {
                    if (isOk)
                    {
                        var request = ab.LoadAsset<GameObject>(abPrefabName);
                        goCell = GameObject.Instantiate(request);
                        fishMono = goCell.GetComponent<FishMono>();
                        fishMono.owner = this;
                        fishMono.RegisterFishDeadEvent(OnFishDead);
                        SetPlatformPos(pos);
                    }
                });
            }
            else
            {
                SetPlatformPos(pos);
            }
        }

        private void SetPlatformPos(Transform pos)
        {
            if (goCell != null)
            {
                //goCell.SetActive(true);
                goCell.transform.SetParent(FishingManager.Instance.FishParent);
                goCell.transform.position = pos.position;
            }
            fishMono.ResetRigCollider();
        }
        /// <summary>
        /// 被抓住
        /// </summary>
        public void BeCatched(Transform catchTarget)
        {
            IsCatched = true;
            fishMono.OnCatched();
            if (goCell != null)
            {
                goCell.transform.SetParent(catchTarget);
                goCell.transform.localPosition = Vector3.zero;
            }

        }

        /// <summary>
        /// 被放下
        /// </summary>
        public void BeLayDown(Transform layDownTarget)
        {
            if (goCell != null)
            {
                goCell.transform.SetParent(FishingManager.Instance.FishParent);
                goCell.transform.position = layDownTarget.position;
            }
            fishMono.OnLayDown();
            IsCatched = false;
        }

        /// <summary>
        /// 鱼死了(掉出世界)
        /// </summary>
        private void OnFishDead()
        {
            this.Recycle2Cache();
        }
    }
}
