using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Framework;
using Unity.BossRoom.Infrastructure;

namespace Logic
{

    public class FishNetwork : NetworkBehaviour
    {
        public NetworkVariable<int> FishId;

        /// <summary>
        /// ��ץס��
        /// </summary>
        public NetworkVariable<bool> IsCatched;

        public List<Material> listMat;
        private Rigidbody rig;
        private MeshCollider meshCollider;

        private void Awake()
        {
            rig = transform.GetComponent<Rigidbody>();
            meshCollider = transform.GetComponent<MeshCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name == "DeadPlane")
            {
                //�����߼� ���ն���
                Recycle2Pool();
            }
        }

        public void OnCreate(int id)
        {
            FishId.Value = id;
            //transform.SetParent(FishingManager.Instance.FishParent);
            //transform.position = pos.position;
            meshCollider.isTrigger = false;
            rig.isKinematic = false;
            rig.useGravity = true;
            UpdateMatrial(FishId.Value);
        }

        public void Recycle2Pool()
        {
            var netObject = transform.GetComponent<NetworkObject>();
            NetworkObjectPool.Singleton.ReturnNetworkObject(netObject, this.gameObject);
        }

        public void BeCatched(Transform catchTarget)
        {
            IsCatched.Value = true;
            rig.useGravity = false;
            rig.isKinematic = true;
            meshCollider.isTrigger = true;
            //������������
            transform.SetParent(catchTarget);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = new Vector3(0, 90, 0);
        }

        /// <summary>
        /// ������
        /// </summary>
        public void BeLayDown(Transform layDownTarget)
        {
            transform.SetParent(FishingManager.Instance.FishParent);
            transform.position = layDownTarget.position;
            meshCollider.isTrigger = false;
            rig.isKinematic = false;
            rig.useGravity = true;
            IsCatched.Value = false;
        }

        /// <summary>
        /// ˢ�������
        /// </summary>
        /// <param name="id"></param>
        public void UpdateMatrial(int fishId)
        {
            var fishData = CSVManager.CSVData.TbFish.GetOrDefault(fishId);
            if (fishData != null && fishData.FishRarity <= listMat.Count)
            {
                gameObject.GetComponent<Renderer>().material = listMat[fishData.FishRarity - 1];
            }
        }
    }
}
