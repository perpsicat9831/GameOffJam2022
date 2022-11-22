using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{

    public class BattleCenterMono : MonoBehaviour
    {
        public List<Transform> listFishSpawn;
        public List<Transform> listRoleSpawn;
        // Start is called before the first frame update
        void Start()
        {
            BindSpawn();
        }

        // Update is called once per frame
        void Update()
        {

        }


        private void BindSpawn()
        {
            FishingManager.Instance.RegisterFishSpawn(listFishSpawn[0]);
            PlayerManager.Instance.RegisterRoleSpawn(listRoleSpawn[0]);
        }
    }
}

