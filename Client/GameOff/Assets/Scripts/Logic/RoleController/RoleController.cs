using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public enum ERoleMoveState
    {
        Move,
        Rushing,
    }
    public class RoleController : MonoBehaviour
    {
        public bool IsClient;
        public Role role;

        private Transform selfTrans;
        private Rigidbody rig;
        private Vector2 speedDir = Vector2.one;//速度方向
        private Vector2 lastSpeedDir = Vector2.one;//上一个不为0的速度方向
        private ERoleMoveState moveState = ERoleMoveState.Move;

        private float baseSpeed = 5f;
        private float Speed = 5f;
        private float RushCD = 1f;
        private bool isRushCD = false;
        private float RushDic = 4.5f;//冲刺距离
        private Vector2 RushTarget;

        private float HoldTime = 0f;
        /// <summary>
        /// 按住最短时长，小于这个时长不算hold
        /// </summary>
        private float HoldMinTime = 0.3f;
        //private Vector2 moveDirection;

        private Action actRoleDead;
        private Action<float> actHoldFishing;
        private Action actCatchFish;
        private void Awake()
        {
            selfTrans = transform;
            rig = selfTrans.GetComponent<Rigidbody>();
        }
        private void Start()
        {
            Speed = baseSpeed * PlayerManager.Instance.GetPlayerSpeedRate();
            RushCD = PlayerManager.Instance.GetPlayerDashCD();

        }
        private void Update()
        {
            if (IsClient)
            {
                if (moveState == ERoleMoveState.Move)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        if (!isRushCD && !role.HasFish())
                        {
                            isRushCD = true;
                            TimeManager.Instance.RegisterTimer(RushCD, () => { isRushCD = false; });
                            //冲刺逻辑
                            RushTarget = new Vector2(selfTrans.localPosition.x + RushDic * lastSpeedDir.x, selfTrans.localPosition.z + RushDic * lastSpeedDir.y);
                            moveState = ERoleMoveState.Rushing;
                        }
                    }

                    float dirX = 0;
                    float dirZ = 0;
                    if (Input.GetKey(KeyCode.A))
                    {
                        dirX = -1;
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        dirX = 1;
                    }
                    if (Input.GetKey(KeyCode.W))
                    {
                        dirZ = 1;
                    }
                    else if (Input.GetKey(KeyCode.S))
                    {
                        dirZ = -1;
                    }
                    speedDir = dirX != 0 || dirZ != 0 ? new Vector2(dirX, dirZ).normalized : new Vector2(dirX, dirZ);
                    if (dirX != 0 || dirZ != 0)
                    {
                        lastSpeedDir = speedDir;
                    }

                    if (Input.GetKey(KeyCode.E))
                    {
                        HoldTime += Time.deltaTime;
                    }
                    if (Input.GetKeyUp(KeyCode.E))
                    {
                        Debug.Log("Hold E Time " + HoldTime);
                        if (HoldTime > HoldMinTime)
                        {
                            actHoldFishing?.Invoke(HoldTime);
                        }
                        else
                        {
                            actCatchFish?.Invoke();
                        }
                        HoldTime = 0;
                    }
                }
            }
        }
        private void FixedUpdate()
        {
            switch (moveState)
            {
                case ERoleMoveState.Move:
                    Move();
                    break;
                case ERoleMoveState.Rushing:
                    Rush();
                    break;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if(other.name == "DeadPlane")
            {
                //死亡逻辑
                actRoleDead?.Invoke();
            }
        }
        private void Rush()
        {
            if (IsClient)
            {
                if (Mathf.Abs(RushTarget.x - selfTrans.localPosition.x) + Mathf.Abs(RushTarget.y - selfTrans.localPosition.z) < 0.8f)
                {
                    moveState = ERoleMoveState.Move;
                }
                float lerp = 0.8f;
                float nextX = selfTrans.localPosition.x + (RushTarget.x - selfTrans.localPosition.x) * lerp * Speed * Time.deltaTime;
                float nextZ = selfTrans.localPosition.z + (RushTarget.y - selfTrans.localPosition.z) * lerp * Speed * Time.deltaTime;
                selfTrans.localPosition = new Vector3(nextX, selfTrans.localPosition.y, nextZ);
            }
        }
        private void Move()
        {
            if (IsClient)
            {
                float nextX = selfTrans.localPosition.x + speedDir.x * Speed * Time.deltaTime;
                float nextZ = selfTrans.localPosition.z + speedDir.y * Speed * Time.deltaTime;
                selfTrans.localPosition = new Vector3(nextX, selfTrans.localPosition.y, nextZ);
                //方向

                float angleY = Vector3.Angle(new Vector3(0, 0, 1), new Vector3(lastSpeedDir.x, 0, lastSpeedDir.y)) * (lastSpeedDir.x > 0 ? 1 : -1);
                selfTrans.localEulerAngles = new Vector3(0, angleY, 0);
            }
        }

        /// <summary>
        /// 注册角色死亡事件
        /// </summary>
        public void RegisterRoleDeadEvent(Action act)
        {
            actRoleDead = act;
        }
        /// <summary>
        /// 注册钓鱼事件
        /// </summary>
        public void RegisterHoldFishing(Action<float> act)
        {
            actHoldFishing = act;
        }
        /// <summary>
        /// 注册抓鱼事件
        /// </summary>
        public void RegisterCatchFishEvent(Action act)
        {
            actCatchFish = act;
        }
    }
}
