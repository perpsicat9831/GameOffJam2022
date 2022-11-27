using System;
using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.ConnectionManagement;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Infrastructure;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Unity.BossRoom.Gameplay.GameState
{
    /// <summary>
    /// Server specialization of Character Select game state.
    /// </summary>
    [RequireComponent(typeof(NetcodeHooks), typeof(NetworkRoomPlayerHandle))]
    public class ServerRoomState : GameStateBehaviour
    {
        [SerializeField]
        NetcodeHooks m_NetcodeHooks;

        [SerializeField]
        public int seatCount = 3;

        public override GameState ActiveState => GameState.CharSelect;
        public NetworkRoomPlayerHandle networkRoomPlayerHandle { get; private set; }

        Coroutine m_WaitToEndLobbyCoroutine;

        [Inject]
        ConnectionManager m_ConnectionManager;

        protected override void Awake()
        {
            base.Awake();
            networkRoomPlayerHandle = GetComponent<NetworkRoomPlayerHandle>();

            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_NetcodeHooks)
            {
                m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }
        }

        /// <summary>
        /// 获取当前空位置
        /// </summary>
        /// <returns></returns>
        private int GetEmptySeatID()
        {

            List<int> otherSeatIdx = new List<int>();
            foreach (NetworkRoomPlayerHandle.LobbyPlayerState playerInfo in networkRoomPlayerHandle.LobbyPlayers)
            {
                otherSeatIdx.Add(playerInfo.SeatIdx);
            }
            for (int i = 0; i < seatCount; i++)
            {
                if (!otherSeatIdx.Contains(i))
                {
                    return i;
                }
            }
            return -1;
        }

        private void SetReady(ulong clientID, bool isReady)
        {
            NetworkRoomPlayerHandle.LobbyPlayerState temp = new NetworkRoomPlayerHandle.LobbyPlayerState() ;
            int index = 0;

            for (int i = 0; i < networkRoomPlayerHandle.LobbyPlayers.Count; i++)
            {
                if (networkRoomPlayerHandle.LobbyPlayers[i].ClientId == clientID)
                {
                    temp = networkRoomPlayerHandle.LobbyPlayers[i];
                    index = i;
                    break;
                }
            }
            temp.SetIsReady(isReady);
            networkRoomPlayerHandle.LobbyPlayers.RemoveAt(index);
            networkRoomPlayerHandle.LobbyPlayers.Add(temp);
            //networkRoomPlayerHandle.ReadyRefresh.Value = !networkRoomPlayerHandle.ReadyRefresh.Value;
        }

        private void StartGame()
        {
            if (networkRoomPlayerHandle.LobbyPlayers.Count < 3)
                return;

            // everybody's ready at the same time! Lock it down!
            networkRoomPlayerHandle.IsLobbyClosed.Value = true;

            // remember our choices so the next scene can use the info
            SaveLobbyResults();

            // Delay a few seconds to give the UI time to react, then switch scenes
            m_WaitToEndLobbyCoroutine = StartCoroutine(WaitToEndLobby());
        }

        /// <summary>
        /// Cancels the process of closing the lobby, so that if a new player joins, they are able to chose a character.
        /// </summary>
        void CancelCloseLobby()
        {
            if (m_WaitToEndLobbyCoroutine != null)
            {
                StopCoroutine(m_WaitToEndLobbyCoroutine);
            }
            networkRoomPlayerHandle.IsLobbyClosed.Value = false;
        }

        void SaveLobbyResults()
        {
            foreach (NetworkRoomPlayerHandle.LobbyPlayerState playerInfo in networkRoomPlayerHandle.LobbyPlayers)
            {
                var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerInfo.ClientId);

                if (playerNetworkObject && playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer))
                {
                    // pass avatar GUID to PersistentPlayer
                    // it'd be great to simplify this with something like a NetworkScriptableObjects :(
                    persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value =
                        networkRoomPlayerHandle.AvatarConfiguration[playerInfo.SeatIdx].Guid.ToNetworkGuid();
                }
            }
        }

        IEnumerator WaitToEndLobby()
        {
            yield return new WaitForSeconds(3);
            //SceneLoaderWrapper.Instance.LoadScene("BossRoom", useNetworkSceneManager: true);
            SceneLoaderWrapper.Instance.LoadScene("BattleScene", useNetworkSceneManager: true);
        }
            
        public void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
            if (networkRoomPlayerHandle)
            {
                networkRoomPlayerHandle.OnSetReady -= SetReady;
                networkRoomPlayerHandle.OnStart -= StartGame;
            }
        }

        public void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
                networkRoomPlayerHandle.OnSetReady += SetReady;
                networkRoomPlayerHandle.OnStart += StartGame;
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
            }
        }

        void OnSceneEvent(SceneEvent sceneEvent)
        {
            // We need to filter out the event that are not a client has finished loading the scene
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
            // When the client finishes loading the Lobby Map, we will need to Seat it
            SeatNewPlayer(sceneEvent.ClientId);
        }

        int GetAvailablePlayerNumber()
        {
            for (int possiblePlayerNumber = 0; possiblePlayerNumber < m_ConnectionManager.MaxConnectedPlayers; ++possiblePlayerNumber)
            {
                if (IsPlayerNumberAvailable(possiblePlayerNumber))
                {
                    return possiblePlayerNumber;
                }
            }
            // we couldn't get a Player# for this person... which means the lobby is full!
            return -1;
        }

        bool IsPlayerNumberAvailable(int playerNumber)
        {
            bool found = false;
            foreach (NetworkRoomPlayerHandle.LobbyPlayerState playerState in networkRoomPlayerHandle.LobbyPlayers)
            {
                if (playerState.PlayerNumber == playerNumber)
                {
                    found = true;
                    break;
                }
            }

            return !found;
        }

        void SeatNewPlayer(ulong clientId)
        {
            // If lobby is closing and waiting to start the game, cancel to allow that new player to select a character
            if (networkRoomPlayerHandle.IsLobbyClosed.Value)
            {
                CancelCloseLobby();
            }
            SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (sessionPlayerData.HasValue)
            {
                var playerData = sessionPlayerData.Value;
                if (playerData.PlayerNumber == -1 || !IsPlayerNumberAvailable(playerData.PlayerNumber))
                {
                    // If no player num already assigned or if player num is no longer available, get an available one.
                    playerData.PlayerNumber = GetAvailablePlayerNumber();
                }
                if (playerData.PlayerNumber == -1)
                {
                    // Sanity check. We ran out of seats... there was no room!
                    throw new Exception($"we shouldn't be here, connection approval should have refused this connection already for client ID {clientId} and player num {playerData.PlayerNumber}");
                }

                int emptyID = GetEmptySeatID();
                if (emptyID != -1)
                {
                    bool isHost = networkRoomPlayerHandle.LobbyPlayers.Count == 0;
                    bool isReady = isHost;
                    networkRoomPlayerHandle.LobbyPlayers.Add(new NetworkRoomPlayerHandle.LobbyPlayerState(clientId, playerData.PlayerName, playerData.PlayerNumber, isReady, emptyID,0, isHost));
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(clientId, playerData);
                }
            }
        }

        void OnClientDisconnectCallback(ulong clientId)
        {
            // clear this client's PlayerNumber and any associated visuals (so other players know they're gone).
            for (int i = 0; i < networkRoomPlayerHandle.LobbyPlayers.Count; ++i)
            {
                if (networkRoomPlayerHandle.LobbyPlayers[i].ClientId == clientId)
                {
                    networkRoomPlayerHandle.LobbyPlayers.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
