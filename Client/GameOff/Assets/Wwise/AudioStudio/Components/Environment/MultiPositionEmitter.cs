using System.Collections.Generic;
using UnityEngine;

namespace AudioStudio
{
    public class MultiPositionEmitter : MonoBehaviour
    {
        public List<GameObject> Emitters = new List<GameObject>();
        
        private void Awake()
        {
            AkSoundEngine.RegisterGameObj(gameObject, gameObject.name);
            ListenerManager.AssignEmitterToListeners(gameObject, true, "0");
        }

        // when this event is no longer played, destroy this game object
        internal void OnAllEmittersDisabled()
        {
            AkSoundEngine.UnregisterGameObj(gameObject);
            ListenerManager.RemoveEmitterFromListeners(gameObject);
            DestroyImmediate(gameObject);
        }
        void OnDestroy()
        {
            EmitterManager.UnregisterMultiPositionEmitter(this);
        }
    }
}