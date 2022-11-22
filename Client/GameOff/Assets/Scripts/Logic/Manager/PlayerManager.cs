using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public class PlayerManager : BaseManager<PlayerManager>
    {
        /// <summary>
        /// 鱼生成点
        /// </summary>
        public Transform RoleSpawn;
        public enum EEvents
        {
            onEventTrigger,
        }
        public EventEmitter<EEvents> eventEmitter = new EventEmitter<EEvents>();


        #region lifeCycle
        public override void OnInit()
        {

        }
        public override void OnDestroy()
        {

        }
        public void OnUpData()
        {
            //需要继承接口 IManagerUpdateModule 
        }
        #endregion

        #region func
        /// <summary>
        /// 获取玩家速度倍率
        /// </summary>
        public float GetPlayerSpeedRate()
        {
            var data = CSVManager.CSVData.TbPlayerProp.GetOrDefault(1);
            if (data != null)
            {
                return data.PlayerSpeedMul;
            }
            return 1;
        }
        /// <summary>
        /// 获取玩家冲撞CD
        /// </summary>
        public float GetPlayerDashCD()
        {
            var data = CSVManager.CSVData.TbPlayerProp.GetOrDefault(1);
            if (data != null)
            {
                return data.DashCd;
            }
            return 1;
        }
        /// <summary>
        /// 获取玩家重生时间
        /// </summary>
        public float GetPlayerRebornTime()
        {
            var data = CSVManager.CSVData.TbPlayerProp.GetOrDefault(1);
            if (data != null)
            {
                return data.RebornTime;
            }
            return 1;
        }
        /// <summary>
        /// 获取玩家重生后保护时间
        /// </summary>
        public float GetPlayerRebornProtectTime()
        {
            var data = CSVManager.CSVData.TbPlayerProp.GetOrDefault(1);
            if (data != null)
            {
                return data.RebornProtect;
            }
            return 1;
        }
        /// <summary>
        /// 获取玩家体积倍数
        /// </summary>
        public float GetPlayerVolumeRate()
        {
            var data = CSVManager.CSVData.TbPlayerProp.GetOrDefault(1);
            if (data != null)
            {
                return data.PlayerVolMul;
            }
            return 1;
        }
        /// <summary>
        /// 获取玩家击退距离
        /// </summary>
        public float GetPlayerDashDic()
        {
            var data = CSVManager.CSVData.TbPlayerProp.GetOrDefault(1);
            if (data != null)
            {
                return data.PlayerHitbackMul;
            }
            return 1;
        }

        /// <summary>
        /// 绑定鱼生成位置
        /// </summary>
        public void RegisterRoleSpawn(Transform trans)
        {
            RoleSpawn = trans;
        }
        #endregion

        #region event
        #endregion

    }
}
