using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{
    /// <summary>
    /// Play AudioEvents from Animator component, fired by AnimationEvents in AnimationClip.
    /// Events can be filtered by AudioState with dot as separator.
    /// </summary>
    [AddComponentMenu("AudioStudio/AnimationSound")]
    [DisallowMultipleComponent]
    public class AnimationSound : AudioEmitter3D
    {
        private AnimationAudioState _animationAudioState;

        protected override void Awake()
        {
            base.Awake();
        }

        // receive signal from AudioState component
        internal void SetAnimationState(AnimationAudioState newState)
        {
            if (_animationAudioState == newState) return;
            _animationAudioState = newState;
            AsUnityHelper.DebugToProfiler(Severity.Notification,
                                          AudioObjectType.AudioState,
                                          AudioTriggerSource.AudioState,
                                          AudioAction.SetValue,
                                          newState.ToString(),
                                          gameObject);
        }

        // play sound from AnimationEvent
        public void PlaySound(AnimationEvent evt)
        {
            var eventName = evt.stringParameter;
            // parse event name and state names
            var eventSplit = eventName.Split('.');
            if (eventSplit.Length < 2)
            {
                // audio state can also be matched by int
                if (evt.intParameter == 0 || 
                    evt.intParameter - 1 == (int) _animationAudioState)
                    DoPlaySound(eventName);
            }
            else
            {
                // check if state name matches
                for (var i = 1; i < eventSplit.Length; i++)
                {
                    if (_animationAudioState.ToString() == eventSplit[i] || 
                        evt.animatorStateInfo.IsName(eventSplit[i]))
                        DoPlaySound(eventSplit[0]);								
                } 
            }
        }
        
        private void DoPlaySound(string eventName)
        {
            AudioManager.PlaySound(UnderPlayerControl ? eventName + "_PC" : eventName,
                                       gameObject,
                                       AudioTriggerSource.AnimationSound);
            
        }
        
        // play voice from AnimationEvent
        public void PlayVoice(AnimationEvent evt)
        {
            var eventName = evt.stringParameter;
            var eventSplit = eventName.Split('.');
            if (eventSplit.Length < 2)
            {
                if (evt.intParameter == 0 || evt.intParameter - 1 == (int) AudioManager.VoiceLanguage)
                    DoPlayVoice(eventName);
            }
            else
            {
                for (var i = 1; i < eventSplit.Length; i++)
                {
                    if (_animationAudioState.ToString() == eventSplit[i] || evt.animatorStateInfo.IsName(eventSplit[i]))
                        DoPlayVoice(eventSplit[0]);								
                } 
            }
        }

        private void DoPlayVoice(string eventName)
        {
            AudioManager.PlayVoice(eventName, gameObject, AudioTriggerSource.AnimationSound);
        }
        
        public void PlayMusic(string eventName)
        {
            AudioManager.PlayMusic(eventName, gameObject, AudioTriggerSource.AnimationSound);
        }

        public void StopSound(string eventName)
        {                                    
            AudioManager.StopSound(UnderPlayerControl? eventName + "_PC" : eventName, gameObject, 0.2f, AudioTriggerSource.AnimationSound);
        }
        
        public void StopVoice(string eventName)
        {                                    
            AudioManager.StopVoice(eventName, 0.2f, gameObject, AudioTriggerSource.AnimationSound);
        }
        
        public void StopMusic(string eventName)
        {                                    
            AudioManager.StopMusic(gameObject, AudioTriggerSource.AnimationSound);
        }
        
        public override bool IsValid()
        {
            return GetComponent<Animator>() != null || GetComponent<Animation>() != null;
        }
    }
}