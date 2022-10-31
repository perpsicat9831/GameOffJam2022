using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Logic
{

    public class FlashMono : MonoBehaviour
    {
        float targetAlpha = 1;

        private float showTime;
        private Image imgLogo;
        private Color logoColor;

        private void Awake()
        {
            imgLogo = transform.Find("Panel/Logo").GetComponent<Image>();
        }
        void Start()
        {
            logoColor = imgLogo.color;
            imgLogo.color = new Color(logoColor.r, logoColor.g, logoColor.b, 0);
        }

        void Update()
        {
            UpdateLogo();
            showTime += Time.deltaTime;
            if (showTime >= 5)
            {
                CloseSelf();
            }
        }

        private void UpdateLogo()
        {
            float curAlpha = imgLogo.color.a;
            float nextAlpha = curAlpha + (targetAlpha - curAlpha) * 0.7f * Time.deltaTime;
            imgLogo.color = new Color(logoColor.r, logoColor.g, logoColor.b, nextAlpha);
            if(targetAlpha - nextAlpha < 0.1f)
            {
                targetAlpha = 0;
            }
        }

        private void CloseSelf()
        {
            if (UIManager.IsInit)
            {
                UIManager.OpenUI(EUIID.GameMain, 2u);
                Destroy(gameObject);
            }
        }
    }
}
