using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    /// <summary>
    /// 角色数据层
    /// </summary>
    public class Role
    {
        public bool isClient;
        public int clientID;
        public Transform RoleSpawn;
        public Transform FishSpawn;

        private AssetBundleLoader abBaseLoader;
        private GameObject goBaseRole;
        private string abBasePath = "Prefab/Role/Role";
        private string abBasePrefabName = "Role";
        private RoleController roleControlMono;
        private Transform FishCatcher;

        private AssetBundleLoader abRoleLoader;
        private GameObject goRole;
        private string abRolePath = "Prefab/Role/Cat/Cat"; 
        private string abRolePrefabName = "Cat";

        private Action<Transform> actBindCameraFollowTarget;
        private Action actRoleDead;

        /// <summary>
        /// 被抓住的鱼
        /// </summary>
        private Fish CatchedFish;
        public void CreateRole(Transform parent,Transform spawn)
        {
            RoleSpawn = spawn;
            if (goBaseRole == null)
            {
                abBaseLoader = AssetBundleLoader.Load(abBasePath, (isOk, ab) =>
                {
                    if (isOk)
                    {
                        var request = ab.LoadAsset<GameObject>(abBasePrefabName);
                        goBaseRole = GameObject.Instantiate(request);
                        roleControlMono = goBaseRole.GetComponent<RoleController>();
                        roleControlMono.IsClient = isClient;
                        roleControlMono.role = this;
                        roleControlMono.RegisterRoleDeadEvent(OnRoleDead);
                        roleControlMono.RegisterHoldFishing(OnHoldFishing);
                        roleControlMono.RegisterCatchFishEvent(OnFishCatch);
                        FishCatcher = goBaseRole.transform.Find("FishCatcher");

                        SetPlatformPos(parent);
                    }
                });
            }
            else
            {
                SetPlatformPos(parent);
            }
        }

        private void SetPlatformPos(Transform parent)
        {
            if (goBaseRole != null)
            {
                //goCell.SetActive(true);
                goBaseRole.transform.SetParent(parent);
                goBaseRole.transform.position = RoleSpawn.position;

                OnRoleCreated();
                CreateRolePrefab();
            }
        }

        private void CreateRolePrefab()
        {
            if (goRole == null)
            {
                abRoleLoader = AssetBundleLoader.Load(abRolePath, (isOk, ab) =>
                {
                    if (isOk)
                    {
                        var request = ab.LoadAsset<GameObject>(abRolePrefabName);
                        goRole = GameObject.Instantiate(request);
                        goRole.transform.SetParent(goBaseRole.transform);
                        goRole.transform.localPosition = Vector3.zero;
                        goRole.transform.localEulerAngles = Vector3.zero;
                    }
                });
            }
        }

        /// <summary>
        /// 注册相机跟随事件
        /// </summary>
        public void RegisterCameraFollowTarget(Action<Transform> act)
        {
            actBindCameraFollowTarget = act;
        }

        private void OnRoleCreated()
        {
            Transform followTarget = goBaseRole.transform.Find("FollowPoint");
            actBindCameraFollowTarget?.Invoke(followTarget);
        }

        private void OnRoleDead()
        {
            if (goBaseRole != null)
            {
                //手里的鱼回收
                if (CatchedFish != null)
                {
                    CatchedFish.Recycle2Cache();
                    CatchedFish = null;
                }
                goBaseRole.transform.position = RoleSpawn.position;
            }
        }

        private void OnHoldFishing(float time)
        {
            var itemID = FishingManager.Instance.GetFishingRewardItemId(time);
            var itemData = CSVManager.CSVData.TbItem.GetOrDefault(itemID);
            if (itemData != null)
            {
                Log.LogInfo("钓上来了 " + itemData.ItemNameCn);
                if (itemData.ItemType == 2)
                {
                    if (FishSpawn)
                    {
                        var fish = ObjectPool<Fish>.Instance.Allocate();
                        fish.OnCreate(FishSpawn);
                    }
                    else
                    {
                        Log.Error("找不到fishSpawn");
                    }
                }
            }
            else
            {
                Log.LogInfo("钓了个寂寞" + itemID);
            }
            //FishingManager.Instance.Fishing(time);
        }

        private void OnFishCatch()
        {
            if (CatchedFish == null)
            {
                //抓鱼
                Collider[] colliderArr = Physics.OverlapSphere(FishCatcher.position, 0.7f);
                if (colliderArr.Length > 0)
                {
                    for (int i = 0; i < colliderArr.Length; i++)
                    {
                        if(colliderArr[i].gameObject.name == "Fish" || colliderArr[i].gameObject.name == "Fish(Clone)")
                        {
                            var fish = colliderArr[i].gameObject;
                            var curFish = fish.GetComponent<FishMono>().owner;
                            if (!curFish.IsCatched)
                            {
                                CatchedFish = curFish;
                                CatchedFish.BeCatched(FishCatcher);
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                //放下
                CatchedFish.BeLayDown(FishCatcher);
                CatchedFish = null;
            }
        }
        /// <summary>
        /// 是否抓着鱼
        /// </summary>
        public bool HasFish()
        {
            return CatchedFish != null;
        }
    }
}