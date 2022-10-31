using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{   
    /// <summary>
    /// Play a sound and set force RTPC when two game objects collide with each other.
    /// </summary>
    [AddComponentMenu("AudioStudio/Collider Sound")]
    [DisallowMultipleComponent]
    public class ColliderSound : AudioEmitter3D
    {        
        public AudioEvent[] EnterEvents = new AudioEvent[0];
        public AudioEvent[] ExitEvents = new AudioEvent[0];                
        public RTPCExt CollisionForceRTPC = new RTPCExt();
        public float ValueScale = 1f;

        private void OnTriggerEnter(Collider other)
        {
            if (!CompareAudioTag(other) || EnterEvents.Length == 0) return;
            PostEvents(EnterEvents, AudioTriggerSource.ColliderSound, GetEmitter(other.gameObject));         
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!CompareAudioTag(other) || ExitEvents.Length == 0) return;
            PostEvents(ExitEvents, AudioTriggerSource.ColliderSound, GetEmitter(other.gameObject));       
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!CompareAudioTag(other.collider) || EnterEvents.Length == 0) return;
            CollisionForceRTPC.SetValue(other.relativeVelocity.magnitude * ValueScale, GetEmitter(other.gameObject));             
            PostEvents(EnterEvents, AudioTriggerSource.ColliderSound, GetEmitter(other.gameObject));           
        }
        
        private void OnCollisionExit(Collision other)
        {
            if (!CompareAudioTag(other.collider) || ExitEvents.Length == 0) return;
            PostEvents(ExitEvents, AudioTriggerSource.ColliderSound, GetEmitter(other.gameObject));           
        }
        
        public override bool IsValid()
        {            
            return GetEvents().Any(s => s.IsValid());
        }
        
        public override IEnumerable<AudioEvent> GetEvents()
        {
            return EnterEvents.Concat(ExitEvents);
        }
    }
}
