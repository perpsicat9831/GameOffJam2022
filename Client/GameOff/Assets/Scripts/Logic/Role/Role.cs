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


        private AssetBundleLoader abBaseLoader;
        private GameObject goBaseRole;
        private string abBasePath = "Prefab/Role/Role";
        private string abBasePrefabName = "Role";
        private RoleController roleControlMono;

        private AssetBundleLoader abRoleLoader;
        private GameObject goRole;
        private string abRolePath = "Prefab/Role/Apple/apple";
        private string abRolePrefabName = "apple";

        private Action<Transform> actBindCameraFollowTarget;
        private Action actRoleDead;
        public void CreateRole(Transform parent)
        {
            if (goBaseRole == null)
            {
                abBaseLoader = AssetBundleLoader.Load(abBasePath, (isOk, ab) =>
                {
                    if (isOk)
                    {
                        var request = ab.LoadAsset<GameObject>(abBasePrefabName);
                        goBaseRole = GameObject.Instantiate(request);
                        roleControlMono = goBaseRole.GetComponent<RoleController>();
                        roleControlMono.RegisterRoleDeadEvent(OnRoleDead);
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
                goBaseRole.transform.position = PlayerManager.Instance.RoleSpawn.position;

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
                goBaseRole.transform.position = PlayerManager.Instance.RoleSpawn.position;
            }
        }
    }
}