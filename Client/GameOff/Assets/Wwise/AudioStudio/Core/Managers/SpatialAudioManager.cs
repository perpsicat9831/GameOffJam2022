using System.Collections.Generic;
using AudioStudio.Components;
using UnityEngine;

namespace AudioStudio
{
    public class ObstructionOcclusionValue
    {
        internal float CurrentValue;
        internal float TargetValue;

        // update obstruction/occlusion amount each frame, ignore if reaches target
        internal bool Update(float fadeTime)
        {
            if (Mathf.Approximately(TargetValue, CurrentValue))
                return false;

            CurrentValue += Mathf.Sign(TargetValue - CurrentValue) * Time.deltaTime / fadeTime;
            CurrentValue = Mathf.Clamp(CurrentValue, 0.0f, 1.0f);
            return true;
        }
    }
    
    internal static class SpatialAudioManager
    {
        internal const int MAX_NUM_ENVIRONMENTS = 4;
        internal const ulong INVALID_ROOM_ID = unchecked((ulong) -1.0f);
        
        internal static readonly ReverbZonePriorityCompare CompareByPriority = new ReverbZonePriorityCompare();
        internal static readonly ReverbZoneCompareAlgorithm CompareBySelectionAlgorithm = new ReverbZoneCompareAlgorithm();
        private static int _roomCount;

        internal static SpatialAudioListener SpatialAudioListener;
        internal static readonly SpatialAudioListenerList SpatialAudioListeners = new SpatialAudioListenerList();
        internal static bool SpatialAudioEnabled;
        internal static bool IsSpatialAudioActive
        {
            get { return SpatialAudioEnabled && SpatialAudioListener != null && _roomCount > 0; }
        }

        internal static void AddRoom(AkRoomParams roomParams, ulong geometryID, GameObject roomGameObject)
        {
            _roomCount++;
            AudioStudioWrapper.SetRoom(AudioStudioWrapper.GetAkGameObjectID(roomGameObject), roomParams, geometryID, roomGameObject.name);
        }
        
        internal static void RemoveRoom(GameObject roomGameObject)
        {
            _roomCount--;
            AudioStudioWrapper.RemoveRoom(AudioStudioWrapper.GetAkGameObjectID(roomGameObject));
        }
    }

    internal class SpatialAudioListenerList
    {
        private readonly List<SpatialAudioListener> _listenerList = new List<SpatialAudioListener>();

        internal bool Add(SpatialAudioListener listener)
        {
            if (_listenerList.Contains(listener))
                return false;

            _listenerList.Add(listener);
            Refresh();
            return true;
        }

        internal bool Remove(SpatialAudioListener listener)
        {
            if (!_listenerList.Contains(listener))
                return false;

            _listenerList.Remove(listener);
            Refresh();
            return true;
        }

        private void Refresh()
        {
            if (_listenerList.Count == 1)
            {
                if (SpatialAudioManager.SpatialAudioListeners != null)
                    AudioStudioWrapper.UnregisterSpatialAudioListener(SpatialAudioManager.SpatialAudioListener.gameObject);

                SpatialAudioManager.SpatialAudioListener = _listenerList[0];

                if (AudioStudioWrapper.RegisterSpatialAudioListener(SpatialAudioManager.SpatialAudioListener.gameObject) == AKRESULT.AK_Success)
                    SpatialAudioManager.SpatialAudioListener.SetGameObjectInRoom();
            }
            else if (_listenerList.Count == 0 && SpatialAudioManager.SpatialAudioListener != null)
            {
                AudioStudioWrapper.UnregisterSpatialAudioListener(SpatialAudioManager.SpatialAudioListener.gameObject);
                SpatialAudioManager.SpatialAudioListener = null;
            }
        }
    }
    
    internal class AudioGameObjectEnvironmentData
    {
        /// Contains all active environments sorted by default, excludeOthers and priority, even those inside a portal.
        private readonly List<ReverbZone> _activeEnvironments = new List<ReverbZone>();

        /// Contains all active environments sorted by priority, even those inside a portal.
        private readonly List<ReverbZone> _activeEnvironmentsFromPortals = new List<ReverbZone>();

        /// Contains all active portals.
        private readonly List<ReverbZoneConnector> _activePortals = new List<ReverbZoneConnector>();

        private readonly AkAuxSendArray _auxSendValues = new AkAuxSendArray();
        private Vector3 _lastPosition = Vector3.zero;
        private bool _hasEnvironmentListChanged = true;
        private bool _hasActivePortalListChanged = true;
        private bool _hasSentZero = false;

        private void AddHighestPriorityEnvironmentsFromPortals(Vector3 position)
        {
            for (var i = 0; i < _activePortals.Count; i++)
            for (var j = 0; j < ReverbZoneConnector.MAX_ENVIRONMENTS_PER_PORTAL; j++)
            {
                var env = _activePortals[i].Environments[j];
                if (env != null)
                {
                    var index = _activeEnvironmentsFromPortals.BinarySearch(env, SpatialAudioManager.CompareByPriority);
                    if (index >= 0 && index < SpatialAudioManager.MAX_NUM_ENVIRONMENTS)
                    {
                        _auxSendValues.Add(env.AuxBus.Id, _activePortals[i].GetAuxSendValueForPosition(position, j));
                        if (_auxSendValues.isFull)
                            return;
                    }
                }
            }
        }

        private void AddHighestPriorityEnvironments(Vector3 position)
        {
            if (!_auxSendValues.isFull && _auxSendValues.Count() < _activeEnvironments.Count)
            {
                for (var i = 0; i < _activeEnvironments.Count; i++)
                {
                    var env = _activeEnvironments[i];
                    var auxBusId = env.AuxBus.Id;

                    if ((!env.IsDefault || i == 0) && !_auxSendValues.Contains(auxBusId))
                    {
                        _auxSendValues.Add(auxBusId, env.GetAuxSendValueForPosition(position));

                        //No other environment can be added after an environment with the excludeOthers flag set to true
                        if (env.ExcludeOthers || _auxSendValues.isFull)
                            break;
                    }
                }
            }
        }

        internal void UpdateAuxSend(GameObject gameObject, Vector3 position)
        {
            if (!_hasEnvironmentListChanged && !_hasActivePortalListChanged && _lastPosition == position)
                return;

            _auxSendValues.Reset();
            AddHighestPriorityEnvironmentsFromPortals(position);
            AddHighestPriorityEnvironments(position);

            var isSendingZero = _auxSendValues.Count() == 0;
            if (!_hasSentZero || !isSendingZero)
                AudioStudioWrapper.SetGameObjectAuxSendValues(gameObject, _auxSendValues, (uint) _auxSendValues.Count());

            _hasSentZero = isSendingZero;
            _lastPosition = position;
            _hasActivePortalListChanged = false;
            _hasEnvironmentListChanged = false;
        }

        private void TryAddEnvironment(ReverbZone env)
        {
            if (env != null)
            {
                var index = _activeEnvironmentsFromPortals.BinarySearch(env, SpatialAudioManager.CompareByPriority);
                if (index < 0)
                {
                    _activeEnvironmentsFromPortals.Insert(~index, env);

                    index = _activeEnvironments.BinarySearch(env, SpatialAudioManager.CompareBySelectionAlgorithm);
                    if (index < 0)
                        _activeEnvironments.Insert(~index, env);

                    _hasEnvironmentListChanged = true;
                }
            }
        }

        private void RemoveEnvironment(ReverbZone env)
        {
            _activeEnvironmentsFromPortals.Remove(env);
            _activeEnvironments.Remove(env);
            _hasEnvironmentListChanged = true;
        }

        internal void AddReverbZone(Collider environmentCollider, Collider gameObjectCollider)
        {
            var portal = environmentCollider.GetComponent<ReverbZoneConnector>();
            if (portal != null)
            {
                _activePortals.Add(portal);
                _hasActivePortalListChanged = true;

                for (var i = 0; i < ReverbZoneConnector.MAX_ENVIRONMENTS_PER_PORTAL; i++)
                    TryAddEnvironment(portal.Environments[i]);
            }
            else
            {
                var env = environmentCollider.GetComponent<ReverbZone>();
                TryAddEnvironment(env);
            }
        }

        private bool AkEnvironmentBelongsToActivePortals(ReverbZone env)
        {
            for (var i = 0; i < _activePortals.Count; i++)
            for (var j = 0; j < ReverbZoneConnector.MAX_ENVIRONMENTS_PER_PORTAL; j++)
            {
                if (env == _activePortals[i].Environments[j])
                    return true;
            }

            return false;
        }

        internal void RemoveReverbZone(Collider environmentCollider, Collider gameObjectCollider)
        {
            var portal = environmentCollider.GetComponent<ReverbZoneConnector>();
            if (portal != null)
            {
                for (var i = 0; i < ReverbZoneConnector.MAX_ENVIRONMENTS_PER_PORTAL; i++)
                {
                    var env = portal.Environments[i];
                    if (env != null && !gameObjectCollider.bounds.Intersects(env.Collider.bounds))
                        RemoveEnvironment(env);
                }

                _activePortals.Remove(portal);
                _hasActivePortalListChanged = true;
            }
            else
            {
                var env = environmentCollider.GetComponent<ReverbZone>();
                if (env != null && !AkEnvironmentBelongsToActivePortals(env))
                    RemoveEnvironment(env);
            }
        }
    }
    
    internal class ReverbZonePriorityCompare : IComparer<ReverbZone>
    {
        public virtual int Compare(ReverbZone a, ReverbZone b)
        {
            var result = a.Priority.CompareTo(b.Priority);
            return result == 0 && a != b ? 1 : result;
        }
    }
    
    internal class ReverbZoneCompareAlgorithm : ReverbZonePriorityCompare
    {
        public override int Compare(ReverbZone a, ReverbZone b)
        {
            if (a.IsDefault)
                return b.IsDefault ? base.Compare(a, b) : 1;

            if (b.IsDefault)
                return -1;

            if (a.ExcludeOthers)
                return b.ExcludeOthers ? base.Compare(a, b) : -1;

            return b.ExcludeOthers ? 1 : base.Compare(a, b);
        }
    }

    public class RoomPriorityList
    {
        private static readonly CompareByPriority _sCompareByPriority = new CompareByPriority();

        /// Contains all active rooms sorted by priority.
        public readonly List<AudioRoom> Rooms = new List<AudioRoom>();

        internal ulong GetHighestPriorityRoomId()
        {
            var room = GetHighestPriorityRoom();
            return room == null ? SpatialAudioManager.INVALID_ROOM_ID : AudioStudioWrapper.GetAkGameObjectID(room.gameObject);
        }

        internal AudioRoom GetHighestPriorityRoom()
        {
            return Rooms.Count == 0 ? null : Rooms[0];
        }

        internal void Add(AudioRoom room)
        {
            var index = BinarySearch(room);
            if (index < 0)
                Rooms.Insert(~index, room);
        }

        internal void Remove(AudioRoom room)
        {
            Rooms.Remove(room);
        }

        internal bool Contains(AudioRoom room)
        {
            return BinarySearch(room) >= 0;
        }

        public int BinarySearch(AudioRoom room)
        {
            if (room)
                return Rooms.BinarySearch(room, _sCompareByPriority);
            return -1;
        }

        private class CompareByPriority : IComparer<AudioRoom>
        {
            public int Compare(AudioRoom a, AudioRoom b)
            {
                var result = a.Priority.CompareTo(b.Priority);

                if (result == 0 && a != b)
                    return 1;

                return -result; // inverted to have highest priority first
            }
        }
    }
}