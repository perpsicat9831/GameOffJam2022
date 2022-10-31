using System.Collections.Generic;
using AudioStudio.Components;
using UnityEngine;

namespace AudioStudio
{
    internal static class ListenerManager
    {
        private struct ListenerMapping
        {
            // the actual listener game object registered to Wwise
            internal GameObject GameObject;
            // saving the AkID of listener game object so we don't need to get it repeatedly
            internal ulong ID;
            // list of listeners registered to this index
            internal List<AudioListener3D> Listeners;
            // list of emitters sending to this listener
            internal List<GameObject> Emitters;
        }
        
        private static ListenerMapping[] _listenerMappings;

        /// <summary>
        /// get the position of the a listener by index
        /// </summary>
        /// <param name="listenerIndices"></param>
        /// <returns></returns>
        internal static Vector3 GetListenerPosition(string listenerIndices = "0")
        {
            var listenerIndex = byte.Parse(listenerIndices.Substring(0, 1));
            if (_listenerMappings == null || _listenerMappings.Length <= listenerIndex)
                return Vector3.zero;
            var listeners = _listenerMappings[listenerIndex].Listeners;
            return listeners[listeners.Count - 1].transform.position;
        }

        /// <summary>
        /// initialize the listener mappings
        /// </summary>
        /// <param name="alternativeListenersCount"></param>
        internal static void Init(int alternativeListenersCount)
        {
            _listenerMappings = new ListenerMapping[alternativeListenersCount + 1];
            for (byte i = 0; i <= alternativeListenersCount; i++)
            {
                // create listener game object and attach to GlobalAudioEmitter
                var listenerGameObject = new GameObject(i == 0 ? "Default 3D Listener" : "Alternative Listener #" + i);
                listenerGameObject.transform.parent = GlobalAudioEmitter.GameObject.transform;
                AudioStudioWrapper.RegisterGameObj(listenerGameObject, listenerGameObject.name);
                _listenerMappings[i] = new ListenerMapping
                {
                    GameObject = listenerGameObject, 
                    ID = AudioStudioWrapper.GetAkGameObjectID(listenerGameObject),
                    Emitters = new List<GameObject>(),
                    Listeners = new List<AudioListener3D>()
                };
            }
        }

        /// <summary>
        /// get a list of listeners based on the string parameter
        /// </summary>
        /// <param name="listenerIndices"></param>
        /// <returns></returns>
        internal static List<AudioListener3D> GetListenersFromString(string listenerIndices)
        {
            var listenerIndexArray = listenerIndices.Split(',');
            var listenerList = new List<AudioListener3D>();
            foreach (var index in listenerIndexArray)
            {
                var numIndex = byte.Parse(index);
                var listeners = _listenerMappings[numIndex].Listeners;
                if (listeners.Count > 0)
                    listenerList.Add(listeners[listeners.Count - 1]);
            }
            return listenerList;
        }

        /// <summary>
        /// activate a listener when it is enabled
        /// </summary>
        /// <param name="listener"></param>
        internal static void AssignAudioListener(AudioListener3D listener)
        {
            var index = listener.Index;
            // if it is the first listener of an index, map all the emitters to it
            if (_listenerMappings[index].Listeners.Count == 0)
            {
                foreach (var emitter in _listenerMappings[index].Emitters)
                {
                    if (emitter)
                        AudioStudioWrapper.AddListener(emitter, _listenerMappings[index].GameObject);
                    else
                        _listenerMappings[index].Emitters.Remove(emitter);
                }
            }
            _listenerMappings[index].Listeners.Add(listener);
        }

        /// <summary>
        /// deactivate a listener when it is disabled
        /// </summary>
        /// <param name="listener"></param>
        internal static void RemoveAudioListener(AudioListener3D listener)
        {
            var index = listener.Index;
            if (!_listenerMappings[index].Listeners.Contains(listener)) return;
            _listenerMappings[index].Listeners.Remove(listener);
            // if it is the last alternative listener, remove all emitter mappings
            if (index > 0 && _listenerMappings[index].Listeners.Count == 0)
            {
                foreach (var emitter in _listenerMappings[index].Emitters)
                {
                    AudioStudioWrapper.RemoveListener(emitter, _listenerMappings[index].GameObject);
                }
            }
        }

        /// <summary>
        /// update positions of listeners each frame
        /// </summary>
        internal static void UpdateListenerPositions()
        {
            foreach (var listenerMapping in _listenerMappings)
            {
                if (listenerMapping.Listeners.Count > 0)
                {
                    // get the active listener, which is the last added to the list
                    var activeListener = listenerMapping.Listeners[listenerMapping.Listeners.Count - 1];
                    AudioStudioWrapper.SetObjectPosition(listenerMapping.GameObject, activeListener.Position, activeListener.GetForward(), activeListener.GetUp());
                }
            }
        }

        /// <summary>
        /// register new emitter to its assigned listeners
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="is3D"></param>
        /// <param name="listenerIndices"></param>
        internal static void AssignEmitterToListeners(GameObject emitter, bool is3D, string listenerIndices = "")
        {
            // 2D sounds just use GlobalAudioEmitter as listener
            if (!is3D)
                AudioStudioWrapper.AddListener(emitter, GlobalAudioEmitter.GameObject);
            else
            {
                // split listener indices by comma
                var listenerIndexArray = listenerIndices.Split(',');
                var listenerArray = new List<ulong>();
                foreach (var index in listenerIndexArray)
                {
                    var numIndex = byte.Parse(index);
                    _listenerMappings[numIndex].Emitters.Add(emitter);
                    // if there are active listeners of that index, map emitter to that listener
                    if (_listenerMappings[numIndex].Listeners != null)
                        listenerArray.Add(_listenerMappings[numIndex].ID);
                }
                AudioStudioWrapper.SetListeners(emitter, listenerArray.ToArray(), (uint)listenerArray.Count);
            }
        }

        /// <summary>
        /// unregister emitter, remove from any listener mapping、
        /// </summary>
        /// <param name="emitter"></param>
        internal static void RemoveEmitterFromListeners(GameObject emitter)
        {
            foreach (var mapping in _listenerMappings)
            {
                mapping.Emitters.Remove(emitter);
            }
        }

        /// <summary>
        /// for changing the emitter-listener send amount by code
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="listenerIndex"></param>
        /// <param name="volume"></param>
        public static void SetListenerVolume(GameObject emitter, byte listenerIndex, float volume)
        {
            AudioStudioWrapper.SetGameObjectOutputBusVolume(emitter, _listenerMappings[listenerIndex].GameObject, volume);
        }

        public static void SetObstructionOcclusion(GameObject emitter, string listeners, float obs, float occ)
        {
            var listenerIndexArray = listeners.Split(',');
            foreach (var index in listenerIndexArray)
            {
                var numIndex = byte.Parse(index);
                AudioStudioWrapper.SetObjectObstructionAndOcclusion(emitter, _listenerMappings[numIndex].GameObject, obs, occ);
            }
        }
    }
}