using Framework;
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

        private Fish CatchedFish;

        private RoleController roleControlMono;
        public Transform FishCatcher;
        private AssetBundleLoader abRoleLoader;
        [SerializeField]
        Transform followTarget;

        private string abRolePath = "Prefab/Role/Cat/Cat";
        private string abRolePrefabName = "Cat";

        private Action actRoleDead;
        private GameObject goBaseRole;
        private GameObject goRole;

        public GameObject modelPrefab;

        public bool isInit;

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
            //if (IsClient && IsOwner)
            //{
            //    Debug.Log("isSelf");
            //    OnRoleCreatedClient();
            //    OnGainedOwnership();
            //    CreateRolePrefab();
            //}
            //if(IsServer)
            //    CreateRolePrefab();
        }

        private void Update()
        {
            if (!isInit)
            {
                if (IsOwner)
                {
                    Debug.Log("isSelf");
                    OnRoleCreatedClient();
                    OnGainedOwnership();
                    CreateRolePrefab();
                    isInit = true;
                }
            }
        }
        //[ServerRpc]
        //public void SycClientID_ServerRpc(ulong clientID)
        //{
        //    if (this.clientID.Value == clientID)
        //    {
        //        SycClientID_ClientRpc(clientID);
        //    }
        //}

        //[ClientRpc]
        //public void SycClientID_ClientRpc(ulong clientID)
        //{
        //    if (NetworkManager.Singleton.LocalClientId == clientID)
        //    {
        //        ID = clientID;
        //    }
        //}
        //public void SycClientID(ulong old, ulong newValue)
        //{
        //    if (IsClient)
        //    {
        //        if (clientID.Value == NetworkManager.Singleton.LocalClientId)
        //        {
        //            Debug.Log("isSelf");
        //            OnRoleCreatedClient();
        //            OnGainedOwnership();
        //            CreateRolePrefab();
        //        }
        //    }
        //}

        private void OnRoleCreatedClient()
        {
            BattleCenterServerMono.Instance.vCamera.Follow = followTarget;
        }

        private void CreateRolePrefab()
        {
            //goRole = GameObject.Instantiate(modelPrefab);
            //var networkObject = goRole.GetComponent<NetworkObject>();
            //networkObject.Spawn();
            //networkObject.TrySetParent(transform);
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
                        CatchedFish.Recycle2Cache();
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
                        if (colliderArr[i].gameObject.name == "Fish" || colliderArr[i].gameObject.name == "Fish(Clone)")
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