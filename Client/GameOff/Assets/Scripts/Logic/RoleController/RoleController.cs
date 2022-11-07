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
        private Transform selfTrans;
        private Rigidbody rig;
        private Vector2 speedDir = Vector2.one;//速度方向
        private Vector2 lastSpeedDir = Vector2.one;//上一个不为0的速度方向
        private ERoleMoveState moveState = ERoleMoveState.Move;

        private float Speed = 5f;
        private float RushCD = 1f;
        private bool isRushCD = false;
        private float RushDic = 4.5f;//冲刺距离
        private Vector2 RushTarget;

        private float HoldTime = 0f;
        //private Vector2 moveDirection;
        private void Awake()
        {
            selfTrans = transform;
            rig = selfTrans.GetComponent<Rigidbody>();
        }
        private void Update()
        {
            if (moveState == ERoleMoveState.Move)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    if (!isRushCD)
                    {
                        isRushCD = true;
                        TimeManager.Instance.RegisterTimer(RushCD, () => { isRushCD = false; });
                        //冲刺逻辑
                        RushTarget = new Vector2(selfTrans.localPosition.x + RushDic * lastSpeedDir.x, selfTrans.localPosition.z + RushDic * lastSpeedDir.y);
                        Debug.Log(RushTarget.x + "|" + RushTarget.y);
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
                    HoldTime = 0;
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
        private void Rush()
        {
            if (Mathf.Abs(RushTarget.x - selfTrans.localPosition.x) + Mathf.Abs(RushTarget.y - selfTrans.localPosition.z) < 0.5f)
            {
                moveState = ERoleMoveState.Move;
            }
            float lerp = 0.8f;
            float nextX = selfTrans.localPosition.x + (RushTarget.x - selfTrans.localPosition.x) * lerp * Speed * Time.deltaTime;
            float nextZ = selfTrans.localPosition.z + (RushTarget.y - selfTrans.localPosition.z) * lerp * Speed * Time.deltaTime;
            selfTrans.localPosition = new Vector3(nextX, selfTrans.localPosition.y, nextZ);
        }
        private void Move()
        {
            float nextX = selfTrans.localPosition.x + speedDir.x * Speed * Time.deltaTime;
            float nextZ = selfTrans.localPosition.z + speedDir.y * Speed * Time.deltaTime;
            selfTrans.localPosition = new Vector3(nextX, selfTrans.localPosition.y, nextZ);
        }
    }
}
