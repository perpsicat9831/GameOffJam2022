using System;
using System.Linq;
using AK.Wwise;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{
    // Place to save audio event and frame mapping
    [Serializable]
    public class AnimationAudioEvent
    {
        public string ClipName;
        public int Frame;
        public AudioEventAction Action;
        public AudioEvent AudioEvent = new AudioEvent();
		
        public override bool Equals(object obj)
        {
            var other = obj as AnimationAudioEvent;
            if (other != null)
                return AudioEvent.Equals(other.AudioEvent) && ClipName == other.ClipName && Frame == other.Frame;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    
    /// <summary>
    /// Post or stop Wwise events with legacy animation component.
    /// </summary>
    [AddComponentMenu("AudioStudio/LegacyAnimationSound")]
    [DisallowMultipleComponent]
    public class LegacyAnimationSound : AudioEmitter3D
    {
        public int FrameRate = 30;
        public AnimationAudioEvent[] AudioEvents = new AnimationAudioEvent[0];

        protected override void Start()
        {
            base.Start();
            RegisterEvents();
        }

        // temporarily add events to animation clip when initialized
        private void RegisterEvents()
        {
            var anim = GetComponent<Animation>();
            if (!anim) return;
            
            foreach (var evt in AudioEvents)
            {
                var clip = anim.GetClip(evt.ClipName);
                if (!clip) continue;
                var existingEvent = clip.events.FirstOrDefault(e => e.stringParameter == evt.AudioEvent.Name);
                if (existingEvent != null) continue;
                var newEvent = new AnimationEvent {time = evt.Frame * 1f / FrameRate, stringParameter = evt.AudioEvent.Name};
                switch (evt.Action)
                {
                    case AudioEventAction.Play:
                        newEvent.functionName = "Play";
                        break;
                    case AudioEventAction.Stop:
                        newEvent.functionName = "Stop";
                        break;
                }
                clip.AddEvent(newEvent);
            }
        }

        public void Play(string eventName)
        {
            if (eventName.StartsWith("Music_"))
                AudioManager.PlayMusic(eventName, gameObject, AudioTriggerSource.AnimationSound);
            else if (eventName.StartsWith("Vo_"))
                AudioManager.PlayVoice(eventName, gameObject, AudioTriggerSource.AnimationSound);
            else
                AudioManager.PlaySound(UnderPlayerControl ? eventName + "_PC" : eventName, gameObject, AudioTriggerSource.AnimationSound);
        }
        
        public void Stop(string eventName)
        {
            if (eventName.StartsWith("Music_"))
                AudioManager.StopMusic(gameObject, AudioTriggerSource.AnimationSound);
            else if (eventName.StartsWith("Vo_"))
                AudioManager.StopVoice(eventName, 0.2f, gameObject, AudioTriggerSource.AnimationSound);
            else
                AudioManager.StopSound(UnderPlayerControl ? eventName + "_PC" : eventName, gameObject, 0.2f, AudioTriggerSource.AnimationSound);
        }
        

        public override bool IsValid()
        {
            return AudioEvents.Any(e => e.AudioEvent.IsValid());
        }
    }
}