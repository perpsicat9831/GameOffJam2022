using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{

    public class BattleCenterMono : MonoBehaviour
    {
        public List<Transform> listFishSpawn;
        public List<Transform> listRoleSpawn;
        public Transform RoleParent;
        public Transform FishesParent;

        public List<Role> listRole = new List<Role>();
        public Transform TransVirtualCamera;
        private Cinemachine.CinemachineVirtualCamera vCamera;

        /// <summary>
        /// 回收后池对象的父节点
        /// </summary>
        public Transform PoolParent;
        private void Awake()
        {
            vCamera = TransVirtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        }
        void Start()
        {
            FishingManager.Instance.FishParent = FishesParent;
            FishingManager.Instance.UnUseFishParent = PoolParent;
            CreateRole();
        }

        void Update()
        {

        }

        private void CreateRole()
        {
            var role = new Role();
            bool isClient = true;
            role.isClient = isClient;
            role.FishSpawn = listFishSpawn[0];
            role.CreateRole(RoleParent, listRoleSpawn[0]);
            if (isClient)
            {
                role.RegisterCameraFollowTarget(SetVCameraFollowTarget);
            }
            listRole.Add(role);

            //假人
            var role2 = new Role();
            role2.FishSpawn = listFishSpawn[1];
            role2.CreateRole(RoleParent, listRoleSpawn[1]);
            listRole.Add(role2);
        }

        private void SetVCameraFollowTarget(Transform target)
        {
            vCamera.Follow = target;
        }
    }
}

