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
                //死亡逻辑
                actDead?.Invoke();
            }
        }

        public void OnCatched()
        {
            UseGravity(false);
            IsKinematic(true);
            meshCollider.isTrigger = true;
            //重新设置坐标
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
        /// 注册鱼死亡(掉出世界)的事件
        /// </summary>
        public void RegisterFishDeadEvent(Action act)
        {
            actDead = act;
        }

        /// <summary>
        /// 刷新鱼材质
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
