using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public class FishingManager : BaseManager<FishingManager>
    {
        /// <summary>
        /// 钓鱼阶段ID列表
        /// </summary>
        private List<int> listStageId = new List<int>();
        /// <summary>
        /// 钓鱼按住阶段时长列表
        /// </summary>
        private List<int> listStageTime = new List<int>();
        /// <summary>
        /// 鱼权重 key 稀有度 value [鱼id列表][权重值列表]
        /// </summary>
        private Dictionary<int, List<List<int>>> dicFishWeight = new Dictionary<int, List<List<int>>>();

        public enum EEvents
        {
            onEventTrigger,
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
            listStageId.Clear();
            listStageTime.Clear();
            var listData = CSVManager.CSVData.TbFishingStage.DataList;
            for (int i = 0; i < listData.Count; i++)
            {
                var curData = listData[i];
                listStageId.Add(curData.Id);
                listStageTime.Add(curData.FishingTime);
            }
            var listFishData = CSVManager.CSVData.TbFish.DataList;
            for (int i = 0; i < listFishData.Count; i++)
            {
                var curFish = listFishData[i];
                if (dicFishWeight.TryGetValue(curFish.FishRarity, out List<List<int>> idWeightList))
                {
                    idWeightList[0].Add(curFish.Id);
                    idWeightList[1].Add(curFish.FishWeight);
                }
                else
                {
                    var newIdWeightList = new List<List<int>>();
                    newIdWeightList[0].Add(curFish.Id);
                    newIdWeightList[1].Add(curFish.FishWeight);
                    dicFishWeight.Add(curFish.FishRarity, newIdWeightList);
                }
            }
        }

        /// <summary>
        /// 根据钓鱼时长，获取itemId
        /// </summary>
        /// <returns></returns>
        public int GetFishingRewardItemId(float time)
        {
            float deltaTime = 0;
            int stageId = 0;
            for (int i = 0; i < listStageTime.Count; i++)
            {
                deltaTime += listStageTime[i];
                if (time > deltaTime)
                {
                    stageId = listStageId[i];
                }
                else
                {
                    break;
                }
            }
            var stageData = CSVManager.CSVData.TbFishingStage.GetOrDefault(stageId);
            if (stageData != null)
            {
                var type = MathTool.GetWeight(stageData.ItemType, stageData.ItemWeight);
                if (type == 1)
                {
                    var rarity = MathTool.GetWeight(stageData.FishRarity, stageData.RarityWeight);
                    return GetFishIdByRarity(rarity);
                }
                else if (type == 2)
                {
                    //道具
                    return ItemManager.Instance.GetRandomItemIdByWeight();
                }
            }
            return 0;
        }
        /// <summary>
        /// 根据鱼稀有度随鱼id
        /// </summary>
        private int GetFishIdByRarity(int fishRarity)
        {
            if (dicFishWeight.TryGetValue(fishRarity, out List<List<int>> idWeightList))
            {
                return MathTool.GetWeight(idWeightList[0], idWeightList[1]);
            }
            return 0;
        }
        #endregion

        #region event
        #endregion

    }
}
