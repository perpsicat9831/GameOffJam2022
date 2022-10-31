using UnityEngine;
using UnityEngine.Playables;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/TimelineSound")]
    [DisallowMultipleComponent]
    public class TimelineSound : AudioEmitter3D
    {                        
        public GameObject[] Emitters = new GameObject[0];

        private new void Awake()
        {
            foreach (var emitter in Emitters)
            {
                EmitterManager.RegisterAudioGameObject(this);
            }
        }
        public override bool IsValid()
        {
            return GetComponent<PlayableDirector>() != null;
        }

        private new void OnDestroy()
        {
            foreach (var emitter in Emitters)
            {
                EmitterManager.UnregisterAudioGameObject(this);
            }
        }
    }
}