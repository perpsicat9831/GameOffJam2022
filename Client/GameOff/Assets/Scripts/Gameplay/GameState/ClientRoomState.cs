using System;
using System.Collections.Generic;
using Unity.BossRoom.Gameplay.UI;
using TMPro;
using Unity.BossRoom.ConnectionManagement;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Avatar = Unity.BossRoom.Gameplay.Configuration.Avatar;

namespace Unity.BossRoom.Gameplay.GameState
{
    /// <summary>
    /// Client specialization of the Character Select game state. Mainly controls the UI during character-select.
    /// </summary>
    [RequireComponent(typeof(NetcodeHooks))]
    public class ClientRoomState : GameStateBehaviour
    {
        /// <summary>
        /// Reference to the scene's state object so that UI can access state
        /// </summary>
        public static ClientRoomState Instance { get; private set; }

        [SerializeField]
        NetcodeHooks m_NetcodeHooks;

        public override GameState ActiveState { get { return GameState.CharSelect; } }

        [SerializeField]
        NetworkRoomPlayerHandle m_NetworkCharSelection;

        [SerializeField]
        [Tooltip("This is triggered when the player chooses a character")]
        string m_AnimationTriggerOnCharSelect = "BeginRevive";

        [SerializeField]
        [Tooltip("This is triggered when the player presses the \"Ready\" button")]
        string m_AnimationTriggerOnCharChosen = "BeginRevive";

        [Header("Lobby Seats")]
        [SerializeField]
        List<UIPlayerSeat> m_PlayerSeats;

        [Header("Lobby Main")]
        [SerializeField]
        UILobbyMainBox m_MainBox;

        bool m_IsReady = false;

        [Inject]
        ConnectionManager m_ConnectionManager;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;

            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            base.OnDestroy();
        }

        protected override void Start()
        {
            base.Start();
            for (int i = 0; i < m_PlayerSeats.Count; ++i)
            {
                m_PlayerSeats[i].Initialize();
            }
        }

        void OnNetworkDespawn()
        {
            if (m_NetworkCharSelection)
            {
                m_NetworkCharSelection.IsLobbyClosed.OnValueChanged -= OnLobbyClosedChanged;
                m_NetworkCharSelection.LobbyPlayers.OnListChanged -= OnLobbyPlayerStateChanged;
            }
        }

        void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
            }
            else
            {
                m_NetworkCharSelection.IsLobbyClosed.OnValueChanged += OnLobbyClosedChanged;
                m_NetworkCharSelection.LobbyPlayers.OnListChanged += OnLobbyPlayerStateChanged;
            }
        }

        /// <summary>
        /// Internal utility that sets the graphics for the eight lobby-seats (based on their current networked state)
        /// </summary>
        void UpdateSeats()
        {
            for (int i = 0; i < m_PlayerSeats.Count; i++)
            {
                bool noPlayer = true;
                foreach (var playerState in m_NetworkCharSelection.LobbyPlayers)
                {
                    if (playerState.SeatIdx == i)
                    {
                        noPlayer = false;

                        m_PlayerSeats[i].Refresh(true, playerState.IsReady, playerState.IsHost, playerState.ClientId == NetworkManager.Singleton.LocalClientId, playerState.PlayerName);
                        break;
                    }
                }
                if (noPlayer)
                {
                    m_PlayerSeats[i].Refresh(false, false, false, false, "");
                }
            }
        }

        void UpdateMain()
        {
            bool isSelfHost = false;
            bool isReady = false;
            foreach (var playerState in m_NetworkCharSelection.LobbyPlayers)
            {
                if (playerState.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    isSelfHost = playerState.IsHost;
                    isReady = playerState.IsReady;
                    break;
                }
            }
            m_MainBox.Refresh(isSelfHost, isReady);
        }

        /// <summary>
        /// Called by the server when the lobby closes (because all players are seated and locked in)
        /// </summary>
        void OnLobbyClosedChanged(bool wasLobbyClosed, bool isLobbyClosed)
        {
            if (isLobbyClosed)
            {
            }
            else
            {
            }
        }


        /// <summary>
        /// Called by the server when any of the seats in the lobby have changed. (Including ours!)
        /// </summary>
        void OnLobbyPlayerStateChanged(NetworkListEvent<NetworkRoomPlayerHandle.LobbyPlayerState> changeEvent)
        {
            UpdateSeats();
            UpdateMain();
        }

        /// <summary>
        /// Called directly by UI elements!
        /// </summary>
        public void OnPlayerClickedReady()
        {
            if (m_NetworkCharSelection.IsSpawned)
            {
                // request to lock in or unlock if already locked in
                m_NetworkCharSelection.SetReadyServerRpc(NetworkManager.Singleton.LocalClientId, !m_IsReady);
            }
        }

        public void OnPlayerClickedStart()
        {
            if (m_NetworkCharSelection.IsSpawned)
            {
                // request to lock in or unlock if already locked in
                m_NetworkCharSelection.StartGameServerRpc();
            }
        }

    }
}
