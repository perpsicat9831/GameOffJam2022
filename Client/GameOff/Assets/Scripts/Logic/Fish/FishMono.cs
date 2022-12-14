using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{

    public class FishMono : MonoBehaviour
    {
        public Fish owner;

        public List<Material> listMat;

        private Rigidbody rig;
        private MeshCollider meshCollider;

        private Action actDead;

        private void Awake()
        {
            rig = transform.GetComponent<Rigidbody>();
            meshCollider = transform.GetComponent<MeshCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name == "DeadPlane")
            {
                //?????߼?
                actDead?.Invoke();
            }
        }

        public void OnCatched()
        {
            UseGravity(false);
            IsKinematic(true);
            meshCollider.isTrigger = true;
            //????????????
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = new Vector3(0, 90, 0);
        }

        public void OnLayDown()
        {
            meshCollider.isTrigger = false;
            IsKinematic(false);
            UseGravity(true);
        }
        public void UseGravity(bool isUse)
        {
            rig.useGravity = isUse;
        }
        public void IsKinematic(bool isUse)
        {
            rig.isKinematic = isUse;
        }
        public void ResetRigCollider()
        {
            meshCollider.isTrigger = false;
            rig.isKinematic = false;
            rig.useGravity = true;
        }
        /// <summary>
        /// ע????????(????????)???¼?
        /// </summary>
        public void RegisterFishDeadEvent(Action act)
        {
            actDead = act;
        }

        /// <summary>
        /// ˢ????????
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
