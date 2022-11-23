using System;
using Unity.BossRoom.Gameplay.Configuration;
using Unity.BossRoom.Utils;
using Unity.Netcode;

namespace Unity.BossRoom.Gameplay.GameState
{
    /// <summary>
    /// Common data and RPCs for the CharSelect stage.
    /// </summary>
    public class NetworkRoomPlayerHandle : NetworkBehaviour
    {
        /// <summary>
        /// Describes one of the players in the lobby, and their current character-select status.
        /// </summary>
        public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
        {
            public ulong ClientId;

            private FixedPlayerName m_PlayerName; // I'm sad there's no 256Bytes fixed list :(
            public int PlayerNumber; // this player's assigned "P#". (0=P1, 1=P2, etc.)
            public int SeatIdx; // the latest seat they were in. -1 means none
            public float LastChangeTime;
            public bool IsHost;
            public bool IsReady;


            public LobbyPlayerState(ulong clientId, string name, int playerNumber, bool isReady=false , int seatIdx = -1, float lastChangeTime = 0,bool isHost=false)
            {
                ClientId = clientId;
                PlayerNumber = playerNumber;
                IsReady = isReady;
                SeatIdx = seatIdx;
                LastChangeTime = lastChangeTime;
                m_PlayerName = new FixedPlayerName();
                this.IsHost = isHost;
                PlayerName = name;
            }

            public string PlayerName
            {
                get => m_PlayerName;
                private set => m_PlayerName = value;
            }

            public void SetIsReady(bool isReady)
            {
                IsReady = isReady;
            }


            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref m_PlayerName);
                serializer.SerializeValue(ref PlayerNumber);
                serializer.SerializeValue(ref SeatIdx);
                serializer.SerializeValue(ref LastChangeTime);
                serializer.SerializeValue(ref IsHost);
                serializer.SerializeValue(ref IsReady);
            }

            public bool Equals(LobbyPlayerState other)
            {
                return ClientId == other.ClientId &&
                       m_PlayerName.Equals(other.m_PlayerName) &&
                       PlayerNumber == other.PlayerNumber &&
                       SeatIdx == other.SeatIdx &&
                       LastChangeTime.Equals(other.LastChangeTime);
            }
        }

        private NetworkList<LobbyPlayerState> m_LobbyPlayers;

        public Avatar[] AvatarConfiguration;

        private void Awake()
        {
            m_LobbyPlayers = new NetworkList<LobbyPlayerState>();
        }

        /// <summary>
        /// Current state of all players in the lobby.
        /// </summary>
        public NetworkList<LobbyPlayerState> LobbyPlayers => m_LobbyPlayers;

        /// <summary>
        /// When this becomes true, the lobby is closed and in process of terminating (switching to gameplay).
        /// </summary>
        public NetworkVariable<bool> IsLobbyClosed { get; } = new NetworkVariable<bool>(false);


        public event Action<ulong,bool> OnSetReady;
        /// <summary>
        /// RPC to notify the server that a client Seat ready
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(ulong clientID,bool isReady)
        {
            OnSetReady?.Invoke(clientID,isReady);
        }

        public event Action OnStart;
        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRpc()
        {
            OnStart?.Invoke();
        }
    }
}
