using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public class UIBase : FUIBase
    {

        protected void CloseSelf()
        {
            UIManager.CloseUI(UIID);
        }
    }
}