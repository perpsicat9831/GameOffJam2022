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

            BindSpawn();
            CreateRole();
        }

        void Update()
        {

        }


        private void BindSpawn()
        {
            FishingManager.Instance.RegisterFishSpawn(listFishSpawn[0]);
            PlayerManager.Instance.RegisterRoleSpawn(listRoleSpawn[0]);
        }

        private void CreateRole()
        {
            var role = new Role();
            bool isClient = true;
            role.isClient = isClient;
            role.CreateRole(RoleParent);
            if (isClient)
            {
                role.RegisterCameraFollowTarget(SetVCameraFollowTarget);
            }
            listRole.Add(role);
        }

        private void SetVCameraFollowTarget(Transform target)
        {
            vCamera.Follow = target;
        }
    }
}

