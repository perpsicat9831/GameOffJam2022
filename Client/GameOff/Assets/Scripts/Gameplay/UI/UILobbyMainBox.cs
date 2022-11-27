using System;
using Unity.BossRoom.Gameplay.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = Unity.BossRoom.Gameplay.Actions.Action;
using Unity.BossRoom.Gameplay.GameState;

namespace Unity.BossRoom.Gameplay.UI
{
    /// <summary>
    /// Controls the "information box" on the character-select screen.
    /// </summary>
    /// <remarks>
    /// This box also includes the "READY" button. The Ready button's state (enabled/disabled) is controlled
    /// here, but note that the actual behavior (when clicked) is set in the editor: the button directly calls
    /// ClientCharSelectState.OnPlayerClickedReady().
    /// </remarks>
    public class UILobbyMainBox : MonoBehaviour
    {
        [SerializeField]
        private Button m_ReadyBtn;
        [SerializeField]
        private Button m_StartBtn;
        [SerializeField]
        private Sprite m_ReadyImg;
        [SerializeField]
        private Sprite m_UnReadyImg;

        public void Refresh(bool isHost,bool isReady,bool canStart)
        {
            m_StartBtn.gameObject.SetActive(isHost);
            m_ReadyBtn.gameObject.SetActive(!isHost);

            if (!isHost)
            {
                m_ReadyBtn.image.sprite = isReady ? m_ReadyImg : m_UnReadyImg;
            }
            else
            {
                m_StartBtn.interactable = canStart;
            }
        }

        private void Awake()
        {
            m_StartBtn.interactable = false;
        }

        public void OnClickReady()
        {
            ClientRoomState.Instance.OnPlayerClickedReady();
        }

        public void OnClickStartGame()
        {
            ClientRoomState.Instance.OnPlayerClickedStart();
        }
    }
}
