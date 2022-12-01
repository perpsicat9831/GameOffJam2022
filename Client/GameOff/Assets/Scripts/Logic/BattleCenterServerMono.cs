using Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using Unity.BossRoom.Infrastructure;

namespace Logic
{
    public class BattleCenterServerMono : NetworkBehaviour
    {
        public List<Transform> listRoleSpawn;
        public List<Transform> listFishSpawn;
        public Transform RoleParent;
        public Transform FishesParent;

        public GameObject rolePrefab;
        public GameObject fishPrefab;

        public List<RoleNetwork> listRole = new List<RoleNetwork>();
        private AssetBundleLoader abBaseLoader;
        private GameObject goBaseRole;
        private string abBasePath = "Prefab/Role/Role";
        private string abBasePrefabName = "Role";
        private RoleController roleControlMono;


        public void SpawnRoles()
        {
            if (IsServer)
            {
                var clientIDs = NetworkManager.Singleton.ConnectedClientsIds;

                for (int i = 0; i < clientIDs.Count; i++)
                {
                    SpawnRole(clientIDs[i], i);
                }
            }
        }

        private void SpawnRole(ulong clientID,int index)
        {
            goBaseRole = GameObject.Instantiate(rolePrefab);
            var networkObject = goBaseRole.GetComponent<NetworkObject>();
            networkObject.transform.position = listRoleSpawn[index].position;
            networkObject.transform.rotation = listRoleSpawn[index].rotation;
            var roleNet = goBaseRole.GetComponent<RoleNetwork>();
            listRole.Add(roleNet);
            roleNet.SetClientID(clientID);
            roleNet.fishSpawn = listFishSpawn[index];
            roleNet.goFishPrefab = fishPrefab;
            networkObject.Spawn();
            networkObject.ChangeOwnership(clientID);


        }
        public Transform PoolParent;
        void Start()
        {
            
            
        }

        public override void OnNetworkSpawn()
        {
            FishingManager.Instance.FishParent = FishesParent;
            FishingManager.Instance.UnUseFishParent = PoolParent;
            SpawnRoles();
            base.OnNetworkSpawn();
        }

        [ServerRpc]
        public void SpawnFishServerRpc()
        {
            int fishId = 0;
            var fish = NetworkObjectPool.Singleton.GetNetworkObject(fishPrefab);
            var fishNetwork = fish.GetComponent<FishNetwork>();
            fishNetwork.OnCreate(fishId);
        }
    }
}

