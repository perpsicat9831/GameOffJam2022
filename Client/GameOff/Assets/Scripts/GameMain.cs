using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using Logic;

public class GameMain : MonoBehaviour
{
    public enum EGameState
    {
        None,
        FlashStart,//ÉÁÆÁ¿ªÊ¼
        Loading,
        Running,
    }
    private EGameState eGameState = EGameState.None;
    CenterManager centerManager = new CenterManager();
    private void Start()
    {
        eGameState = EGameState.FlashStart;
    }
    private void Update()
    {
        switch (eGameState)
        {
            case EGameState.Running:
                {
                    centerManager.UpdateAllManager();
                }
                break;
            case EGameState.FlashStart:
                {
                    //var go = Resources.Load("Flash");
                    //Instantiate(go);
                    eGameState = EGameState.Loading;
                }
                break;
            case EGameState.Loading:
                {
                    UIManager.Init();
                    CSVManager.InitTable();
                    centerManager.RegisterAllManager();
                    UIManager.OpenUI(EUIID.GameMain);
                    eGameState = EGameState.Running;
                }
                break;
        }
        Log.TotalFrame++;
    }
    private void OnDestroy()
    {
        centerManager.DestroyAllManager();
    }
}
