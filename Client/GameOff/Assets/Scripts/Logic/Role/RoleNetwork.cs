﻿using Framework;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Logic
{
    public class RoleNetwork : NetworkBehaviour
    {
        public NetworkVariable<ulong> clientID;

        public ulong ID;

        private FishNetwork CatchedFish;

        private RoleController roleControlMono;
        public Transform FishCatcher;
        private AssetBundleLoader abRoleLoader;
        [SerializeField]
        Transform followTarget;

        //private string abRolePath = "Prefab/Role/Cat/Cat";
        //private string abRolePrefabName = "Cat";
        private string abRolePath = "Prefab/Role/Cat/Cat2";
        private string abRolePrefabName = "Cat2";

        private Action actRoleDead;
        private GameObject goBaseRole;
        private GameObject goRole;

        public GameObject modelPrefab;

        public bool isInit;
        public bool hasSetCam;

        private void Awake()
        {
            clientID = new NetworkVariable<ulong>();
            //clientID.OnValueChanged += SycClientID;
            isInit = false;
        }

        public void SetClientID(ulong clientID)
        {
            this.clientID.Value = clientID;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            roleControlMono = gameObject.GetComponent<RoleController>();
            roleControlMono.IsClient = IsClient;
            roleControlMono.RegisterRoleDeadEvent(OnRoleDead);
            roleControlMono.RegisterHoldFishing(OnHoldFishing);
            roleControlMono.RegisterCatchFishEvent(OnFishCatch);
            if (!isInit && IsServer)
            {
                Debug.Log("isSelf  " + clientID);
                
                isInit = true;
            }
        }

        private void Update()
        {
            if (isInit)
            {
                if (IsOwner)
                {
                    SycTransformPos_ServerRpc(transform.position,clientID.Value);
                    TrySetCamera();
                }
            }

            
        }


        public override void OnGainedOwnership()
        {
            Debug.Log("GainOwner");
            if (!isInit && IsOwner&&!IsServer)
            {
                isInit = true;
            }
            base.OnGainedOwnership();
        }

        public void TrySetCamera()
        {
            if (!isInit)
                return;
            if (hasSetCam)
                return;
            var camera = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
            if (camera == null)
                return;
            if (camera.ActiveVirtualCamera == null)
                return;
            camera.ActiveVirtualCamera.Follow = followTarget;
            hasSetCam = true;

        }


        //[ServerRpc]
        //public void SycCam_ServerRpc(ulong clientID)
        //{
        //    SycTransformPos_ClientRpc(pos, clientID);
        //}

        //[ClientRpc]
        //public void SycCam_ClientRpc(Vector3 pos, ulong clientID)
        //{
        //    if (!IsOwner && clientID == this.clientID.Value)
        //    {
        //        transform.position = pos;
        //    }
        //}


        [ServerRpc]
        public void SycTransformPos_ServerRpc(Vector3 pos,ulong clientID)
        {
            SycTransformPos_ClientRpc(pos, clientID);
        }

        [ClientRpc]
        public void SycTransformPos_ClientRpc(Vector3 pos,ulong clientID)
        {
            if (!IsOwner&& clientID==this.clientID.Value)
            {
                transform.position = pos;
            }
        }

        private void OnRoleDead()
        {
            if (IsServer)
            {
                if (goBaseRole != null)
                {
                    //手里的鱼回收
                    if (CatchedFish != null)
                    {
                        CatchedFish.Recycle2Pool();
                        CatchedFish = null;
                    }
                    //goBaseRole.transform.position = RoleSpawn.position;
                }
            }
        }

        private void OnHoldFishing(float time)
        {
            //var itemID = FishingManager.Instance.GetFishingRewardItemId(time);
            //var itemData = CSVManager.CSVData.TbItem.GetOrDefault(itemID);
            //if (itemData != null)
            //{
            //    Log.LogInfo("钓上来了 " + itemData.ItemNameCn);
            //    if (itemData.ItemType == 2)
            //    {
            //        if (FishSpawn)
            //        {
            //            var fish = ObjectPool<Fish>.Instance.Allocate();
            //            fish.OnCreate(FishSpawn);
            //        }
            //        else
            //        {
            //            Log.Error("找不到fishSpawn");
            //        }
            //    }
            //}
            //else
            //{
            //    Log.LogInfo("钓了个寂寞" + itemID);
            //}
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
                        if (colliderArr[i].gameObject.name == "fish2" || colliderArr[i].gameObject.name == "fish2(Clone)")
                        {
                            var fish = colliderArr[i].gameObject;
                            var curFish = fish.GetComponent<FishNetwork>();
                            if (!curFish.IsCatched.Value)
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