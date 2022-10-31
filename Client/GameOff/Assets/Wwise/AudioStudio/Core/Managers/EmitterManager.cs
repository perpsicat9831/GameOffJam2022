using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio
{
    /// <summary>
    /// Global uses GlobalAuxSend, GameObject uses ReverbZone, AutoDetect will set based on if game object has a collider 
    /// </summary>
    public enum EnvironmentSource
    {
        AutoDetect,
        Global,
        GameObject        
    }

    /// <summary>
    /// how many frames will the position of emitter be passed to Wwise 
    /// </summary>
    public enum UpdateFrequency
    {
        High,
        Mid,
        Low,
        Never
    }

    public static class EmitterManager
    {				
        #region PlayerControl
        private static GameObject _player;
        /// <summary>
        /// Get or assign player game object to AudioStudio, so some events can be treated differently.
        /// </summary>
        public static GameObject Player
        {
            get { return _player; }
            set 
            {
                if (_player == value) return;				
                _player = value;
                // reset components on other objects 
                foreach (var audioGameObject in _positionObjects)
                {
                    audioGameObject.UnderPlayerControl = false;
                }			
                // set components on new player object 
                var audioGameObjects = _player.GetComponentsInChildren<AudioEmitter3D>();
                foreach (var audioGameObject in audioGameObjects)
                {
                    audioGameObject.UnderPlayerControl = true;
                }
            }
        }
        #endregion	

        #region Position
        // any emitter that has position information
        private static readonly List<AudioEmitter3D> _positionObjects = new List<AudioEmitter3D>();

        private static void AddPositionObject(AudioEmitter3D emitter)
        {
            if (_positionObjects.Contains(emitter)) return;
            if (emitter.UpdateFrequency != UpdateFrequency.Never)
                _positionObjects.Add(emitter);
            else
                emitter.SetObjectPosition();
        }
		
        internal static void RefreshPositions()
        {
            foreach (var po in _positionObjects)
            {
                if (!po || !po.isActiveAndEnabled) continue;
                po.UpdatePosition();			
            }
        }

        /// <summary>
        /// Stop any sound from playing if sound is set as 3D
        /// </summary>
        public static void StopAll3DSounds()
        {
            foreach (var ago in _positionObjects)
            {
                AudioStudioWrapper.StopAll(ago.gameObject);
            }
        }
        #endregion
        
        #region MultiPositionSounds
        // each audio event should have an individual MultiPositionEmitter game object
        private static readonly Dictionary<uint, MultiPositionEmitter> _multiPositionEvents = new Dictionary<uint, MultiPositionEmitter>();

        // check if a game object is the main emitter of the multi position emitter array
        internal static bool IsFirstMultiPosEmitter(AudioEvent audioEvent, GameObject emitter)
        {
            MultiPositionEmitter mpe;
            if (_multiPositionEvents.TryGetValue(audioEvent.Id, out mpe))
            {
                return mpe.Emitters[0] != emitter;
            }
            return true;
        }

        // get the created emitter game object of an audio event
        internal static GameObject GetActualMultiPosEmitter(AudioEvent audioEvent)
        {
            return _multiPositionEvents[audioEvent.Id].gameObject;
        }

        internal static void UpdateMultiPositionEmitter(EmitterSound component)
        {
            foreach (var audioEvent in component.AudioEvents)
            {
                MultiPositionEmitter mpe;
                if (_multiPositionEvents.TryGetValue(audioEvent.Id, out mpe))
                {
                    if (mpe.Emitters.Contains(component.gameObject))
                    {
                        var positionArray = BuildMultiDirectionArray(mpe.Emitters);
                        AkSoundEngine.SetMultiplePositions(mpe.gameObject, positionArray, (ushort)positionArray.Count, (AkMultiPositionType)component.MultiPositionType);
                    }
                }
            }
        }

        internal static bool RegisterMultiPositionEmitter(EmitterSound component)
        {
            // unregister the original EmitterSound game object
            AudioStudioWrapper.UnregisterGameObj(component.gameObject);
            bool bFirst = false;
            foreach (var audioEvent in component.AudioEvents)
            {
                bFirst = (_multiPositionEvents.Count == 0);
                MultiPositionEmitter mpe;
                // if this AudioEvent already has a MultiPositionEmitter playing
                if (_multiPositionEvents.TryGetValue(audioEvent.Id, out mpe))
                {
                    if (!mpe.Emitters.Contains(component.gameObject))
                        mpe.Emitters.Add(component.gameObject);
                }
                // create a new MultiPositionEmitter for this AudioEvent
                else
                {
                    var emitter = new GameObject(audioEvent.Name + "(MultiPos Emitter)");
                    mpe = emitter.AddComponent<MultiPositionEmitter>();
                    mpe.Emitters.Add(component.gameObject);
                    _multiPositionEvents[audioEvent.Id] = mpe;
                    _registeredGameObjects.Add(emitter);
                    bFirst = true;
                }
                // send multi position data to Wwise
                if (mpe != null)
                {
                    var positionArray = BuildMultiDirectionArray(mpe.Emitters);
                    AudioStudioWrapper.SetMultiplePositions(mpe.gameObject, positionArray, (ushort)positionArray.Count, (AkMultiPositionType)component.MultiPositionType);
                }
            }
            return bFirst;
        }

        internal static void UnregisterMultiPositionEmitter(EmitterSound component)
        {
            foreach (var audioEvent in component.AudioEvents)
            {
                MultiPositionEmitter mpe;
                if (_multiPositionEvents.TryGetValue(audioEvent.Id, out mpe))
                {
                    if (mpe)
                    {
                        mpe.Emitters.Remove(component.gameObject);
                        if (mpe.Emitters.Count == 0)
                        {
                            //AudioStudioWrapper.StopAll(mpe.gameObject);
                            AudioManager.StopSound(audioEvent.Name, mpe.gameObject, 0.5f);
                            _registeredGameObjects.Remove(mpe.gameObject);
                            mpe.OnAllEmittersDisabled();
                            _multiPositionEvents.Remove(audioEvent.Id);
                        }
                        else
                        {
                            // refresh the multi position data by the remaining emitters
                            var positionArray = BuildMultiDirectionArray(mpe.Emitters);
                            AudioStudioWrapper.SetMultiplePositions(mpe.gameObject, positionArray, (ushort)positionArray.Count, (AkMultiPositionType)component.MultiPositionType);
                        }
                    }
                    else
                        _multiPositionEvents.Remove(audioEvent.Id);
                }
            }
        }

        // create a position array with all emitters of an AudioEvent
        private static AkPositionArray BuildMultiDirectionArray(ICollection<GameObject> emitterList)
        {
            var positionArray = new AkPositionArray((uint) emitterList.Count);
            foreach (var emitter in emitterList)
            {
                if (emitter != null)
                    positionArray.Add(emitter.transform.position, emitter.transform.forward, emitter.transform.up);
            }
            return positionArray;
        }
        #endregion
		
        #region Registration
        private static readonly List<GameObject> _registeredGameObjects = new List<GameObject>();
		
        internal static bool IsGameObjectRegistered(GameObject emitter)
        {
            return _registeredGameObjects.Contains(emitter);
        }
		
        internal static bool RegisterAudioGameObject(AudioEmitter3D emitter)
        {
            // auto initialize if this game object gets initialized before AudioInit component
            if (!AudioInitSettings.Initialized && AudioInitSettings.Instance.AutoInitialize)
                AudioInitSettings.Instance.Initialize(true);
            
            var go = emitter.gameObject;
            // if there are more than one AudioEmitter3D component on the same game object, don't register again
            if (IsGameObjectRegistered(go))
                return true;
            
            var result = AudioStudioWrapper.RegisterGameObj(go, go.name);
            if (result == AKRESULT.AK_Success)
            {
                _registeredGameObjects.Add(go);
                if (emitter.IsUpdatePosition)
                    AddPositionObject(emitter);
                if (emitter.IsEnvironmentAware)
                    AddEnvironmentObject(emitter);
                ListenerManager.AssignEmitterToListeners(go, emitter.IsUpdatePosition, emitter.Listeners);
                return true;
            }
            AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Emitter, AudioTriggerSource.Code, AudioAction.Register, "", go, result.ToString());
            return false;
        }

        internal static void UnregisterAudioGameObject(AudioEmitter3D emitter)
        {
            if (!AudioInitSettings.Initialized) return;
            if (!AkSoundEngine.IsInitialized())
                return;
            var go = emitter.gameObject;
            var result = AudioStudioWrapper.UnregisterGameObj(go);
            if (result == AKRESULT.AK_Success)
            {
                _registeredGameObjects.Remove(go);
                _positionObjects.Remove(emitter);
                _environmentObjectsByListener.Remove(go);
                if (_environmentObjectsByEmitter.ContainsKey(go))
                    _environmentObjectsByEmitter.Remove(go);
                ListenerManager.RemoveEmitterFromListeners(emitter.gameObject);
            }
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Emitter, AudioTriggerSource.Code, AudioAction.Unregister, "", go, result.ToString());
        }

        internal static void UnregisterMultiPositionEmitter(MultiPositionEmitter emitter)
        {
            if (!AudioInitSettings.Initialized) return;
            if (!AkSoundEngine.IsInitialized())
                return;
            var result = AudioStudioWrapper.UnregisterGameObj(emitter.gameObject);
            if (result == AKRESULT.AK_Success)
            {
                foreach (var me in _multiPositionEvents)
                {
                    if (me.Value == emitter)
                    {
                        _multiPositionEvents.Remove(me.Key);
                        break;
                    }
                }
                _registeredGameObjects.Remove(emitter.gameObject);
                ListenerManager.RemoveEmitterFromListeners(emitter.gameObject);
            }
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Emitter, AudioTriggerSource.Code, AudioAction.Unregister, "", emitter.gameObject, result.ToString());
        }
        #endregion

        #region Environment
        private static readonly Dictionary<GameObject, AudioGameObjectEnvironmentData> _environmentObjectsByEmitter = new Dictionary<GameObject, AudioGameObjectEnvironmentData>();
        private static readonly List<GameObject> _environmentObjectsByListener = new List<GameObject>();
        private static readonly AkAuxSendArray _auxSendArray = new AkAuxSendArray();
        private static readonly Dictionary<uint, float> _activeSends = new Dictionary<uint, float>();
        
        private static void AddEnvironmentObject(AudioEmitter3D emitter)
        {
            switch (emitter.EnvironmentSource)
            {
                case EnvironmentSource.GameObject:
                    UseEmitterEnvironment(emitter.gameObject);
                    break;
                case EnvironmentSource.Global:
                    UseListenerEnvironment(emitter.gameObject);
                    break;
                case EnvironmentSource.AutoDetect:
                    if (emitter.GetComponent<Collider>())
                        UseEmitterEnvironment(emitter.gameObject);
                    else
                        UseListenerEnvironment(emitter.gameObject);
                    break;
            }
        }

        internal static bool IsEnvironmentAware(GameObject go)
        {
            return _environmentObjectsByEmitter.ContainsKey(go);
        }
        
        // individually send each emitter to aux bus
        private static void UseEmitterEnvironment(GameObject go)
        {
            if (!_environmentObjectsByEmitter.ContainsKey(go)) 
                _environmentObjectsByEmitter.Add(go, new AudioGameObjectEnvironmentData());
        }

        // send all emitters to aux bus defined by listener position
        private static void UseListenerEnvironment(GameObject go)
        {
            if (!_environmentObjectsByListener.Contains(go)) 
                _environmentObjectsByListener.Add(go);
            if (_auxSendArray.Count() > 0)
                AudioStudioWrapper.SetGameObjectAuxSendValues(go, _auxSendArray, 1);
        }
		
        // add and remove new global aux bus sends
        internal static void AddAuxBuses(IEnumerable<AuxBusExt> buses)
        {
            foreach (var bus in buses)
            {
                AddAuxBus(bus);
            }															
            RefreshGlobalAuxSends();
        }
        
        internal static void AddAuxBus(AuxBusExt bus)
        {
            var id = bus.Id;
            if (!_auxSendArray.Contains(id) && !_activeSends.ContainsKey(id))
            {
                _auxSendArray.Add(id, bus.SendAmount);
                _activeSends.Add(id, bus.SendAmount);
            }
        }

        internal static void RemoveAuxBuses(IEnumerable<AuxBusExt> buses)
        {
            _auxSendArray.Reset();
            foreach (var bus in buses)
            {					
                _activeSends.Remove(bus.Id);
            }
            foreach (var activeBus in _activeSends)
            {
                _auxSendArray.Add(activeBus.Key, activeBus.Value);
            }
            RefreshGlobalAuxSends();
        }
		
        internal static void RemoveAuxBus(AuxBusExt bus)
        {
            _auxSendArray.Reset();				
            _activeSends.Remove(bus.Id);
            foreach (var activeBus in _activeSends)
            {
                _auxSendArray.Add(activeBus.Key, activeBus.Value);
            }
            RefreshGlobalAuxSends();
        }

        internal static void RefreshEnvironments()
        {
            foreach (var eo in _environmentObjectsByEmitter)
            {
                if (eo.Key == null) return;
                eo.Value.UpdateAuxSend(eo.Key, eo.Key.transform.position);									
            }
        }

        // all aux send info should be refreshed when an AuxBus is set or reset
        internal static void RefreshGlobalAuxSends()
        {
            foreach (var eo in _environmentObjectsByListener)
            {
                AudioStudioWrapper.SetGameObjectAuxSendValues(eo, _auxSendArray, (uint) _auxSendArray.Count());				
            }	
        }
		
        // when emitter enters or exits a trigger volume defined by ReverbZone component
        internal static void EnterReverbZone(Collider colEnv, Collider colGameObj)
        {
            var envData = _environmentObjectsByEmitter[colGameObj.gameObject];				
            envData.AddReverbZone(colEnv, colGameObj);			
        }
		
        internal static void ExitReverbZone(Collider colEnv, Collider colGameObj)
        {			
            var envData = _environmentObjectsByEmitter[colGameObj.gameObject];			
            envData.RemoveReverbZone(colEnv, colGameObj);			
        }		
        #endregion
    }
}