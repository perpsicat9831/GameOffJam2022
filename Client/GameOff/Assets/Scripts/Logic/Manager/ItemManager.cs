using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public class ItemManager : BaseManager<ItemManager>
    {
        /// <summary>
        /// 道具id列表
        /// </summary>
        private List<int> listItemID = new List<int>();
        /// <summary>
        /// 道具权重列表
        /// </summary>
        private List<int> listItemWeight = new List<int>();

        public enum EEvents
        {
        }
        public EventEmitter<EEvents> eventEmitter = new EventEmitter<EEvents>();


        #region lifeCycle
        public override void OnInit()
        {
            InitData();
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
        private void InitData()
        {
            var listData = CSVManager.CSVData.TbItem.DataList;
            for (int i = 0; i < listData.Count; i++)
            {
                var curData = listData[i];
                if (curData.ItemType == 1)
                {
                    listItemID.Add(curData.Id);
                    listItemWeight.Add(curData.ItemWeight);
                }
            }
        }
        /// <summary>
        /// 根据权重随道具id
        /// </summary>
        public int GetRandomItemIdByWeight()
        {
            return MathTool.GetWeight(listItemID, listItemWeight);
        }
        #endregion

        #region event
        #endregion

    }
}
