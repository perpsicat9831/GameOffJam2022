using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using TMPro;

namespace Unity.BossRoom.Gameplay.UI
{
    public class LobbyCreationUI : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_LobbyNameInputField;
        [SerializeField] GameObject m_LoadingIndicatorObject;
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] LobbyUIMediator m_LobbyUIMediator;

        void Awake()
        {
            EnableUnityRelayUI();
        }

        void EnableUnityRelayUI()
        {
            m_LoadingIndicatorObject.SetActive(false);
        }

        public void OnCreateClick()
        {
            m_LobbyUIMediator.CreateLobbyRequest(m_LobbyNameInputField.text,false);
        }

        public void Show()
        {
            m_CanvasGroup.alpha = 1f;
            m_CanvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            m_CanvasGroup.alpha = 0f;
            m_CanvasGroup.blocksRaycasts = false;
        }
    }
}
