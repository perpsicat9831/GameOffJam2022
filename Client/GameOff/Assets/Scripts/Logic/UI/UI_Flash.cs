using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Logic
{
    public class UI_Flash : UIBase
    {
        private uint openNum = 0;
        private Button btn;
        private Button btnClose;

        public override void OnOpen(object args)
        {
            if(args.GetType() == typeof(uint))
            {
                openNum = (uint)args;
            }
        }

        public override void OnLoaded()
        {
            Debug.Log(transform != null);
            btn = transform.Find("Panel/Button").GetComponent<Button>();
            btn.onClick.AddListener(OnBtnClick);
            btnClose = transform.Find("Panel/BtnClose").GetComponent<Button>();
            btnClose.onClick.AddListener(OnBtnCloseClick);
        }
        public override void OnShow()
        {
            Debug.Log("OnShow" + openNum);
        }

        private void OnBtnClick()
        {
            Debug.Log("OnBtnClick");
        }
        private void OnBtnCloseClick()
        {
            this.CloseSelf();
        }
    }
}
