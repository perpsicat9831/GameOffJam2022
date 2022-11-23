using System;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using Unity.BossRoom.Gameplay.GameState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.BossRoom.Gameplay.UI
{
    /// <summary>
    /// Controls one of the eight "seats" on the character-select screen (the boxes along the bottom).
    /// </summary>
    public class UIPlayerSeat : MonoBehaviour
    {
        [SerializeField]
        private Image m_PlayerReady;
        [SerializeField]
        private Image m_IsHost;
        [SerializeField]
        private Sprite m_HostTexture;
        [SerializeField]
        private Sprite m_JoinTexture;
        [SerializeField]
        private TextMeshProUGUI m_PlayerNameHolder;
        [SerializeField]
        private Image m_Icon;

        public void Initialize()
        {
            //PlayerEnterUpdate(false);
        }

        private void PlayerEnterUpdate(bool isEnter)
        {
            m_PlayerNameHolder.gameObject.SetActive(isEnter);
            m_PlayerReady.gameObject.SetActive(isEnter);
            m_Icon.gameObject.SetActive(isEnter);
            m_IsHost.sprite = m_JoinTexture;
        }

        public void Refresh(bool hasPlayer,bool isReady,bool isHost,bool isSelf,string playerName)
        {
            PlayerEnterUpdate(hasPlayer);
            if (!hasPlayer)
                return;
            m_IsHost.sprite = isHost?m_HostTexture: m_JoinTexture;
            if (isHost)
            {
                m_PlayerReady.gameObject.SetActive(true);
            }
            else
            {
                m_PlayerReady.gameObject.SetActive(isReady);
            }
            m_PlayerNameHolder.text = playerName;
        }

    }
}
