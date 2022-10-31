using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Logic
{

    public class NewUIBase : UIBase
    {
        private Button btnClose;

        #region lifeCycle
        public override void OnOpen(object args)
        {
            //if (args.GetType() == typeof(uint))
            //{
            //    openNum = (uint)args;
            //}
        }
        public override void OnLoaded()
        {
            Parse();
        }
        public override void OnShow()
        {
            UpdateView();
        }
        public override void OnHide()
        {

        }
        public override void OnDestroy()
        {

        }
        public override void OnEventRegister(bool toRegister)
        {

        }
        #endregion 

        #region func
        private void Parse()
        {

        }
        private void UpdateView()
        {

        }
        #endregion

        #region event
        private void OnBtnCloseClick()
        {
            this.CloseSelf();
        }
        #endregion
    }

}