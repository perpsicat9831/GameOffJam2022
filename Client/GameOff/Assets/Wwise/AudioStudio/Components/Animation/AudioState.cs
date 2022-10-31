using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{    
    /// <summary>
    /// Set audio state machine for AnimationSound. It can also play sounds or set states/switches when a animator state enters/exits.
    /// </summary>
    public class AudioState : StateMachineBehaviour
    {        
        public AudioEventExt[] EnterEvents = new AudioEventExt[0];
        public AudioEvent[] ExitEvents = new AudioEvent[0];        
        public SwitchEx[] EnterSwitches = new SwitchEx[0];
        public SwitchEx[] ExitSwitches = new SwitchEx[0];
        public ASState[] EnterStates = new ASState[0];
        public ASState[] ExitStates = new ASState[0];
        
        public AnimationAudioState AnimationAudioState = AnimationAudioState.None;
        public bool ResetStateOnExit;
        
        private GameObject _emitter;
        private AnimationSound _animationSound;


        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,int layerIndex)
        {
            if (!_animationSound)
                _animationSound = animator.gameObject.GetComponent<AnimationSound>();
            
            _emitter = _animationSound ? _animationSound.gameObject : animator.gameObject;
            foreach (var state in EnterStates)
            {
                state.SetValue(_emitter, AudioTriggerSource.AudioState);             
            }
            foreach (var swc in EnterSwitches)
            {
                swc.SetValue(_emitter, AudioTriggerSource.AudioState);             
            }
            foreach (var evt in EnterEvents)
            {
                evt.Post(_emitter, AudioTriggerSource.AudioState);
            }            
            
            if (_animationSound) 
                _animationSound.SetAnimationState(AnimationAudioState);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo,int layerIndex)
        {
            foreach (var state in ExitStates)
            {
                state.SetValue(_emitter, AudioTriggerSource.AudioState);             
            }
            foreach (var swc in ExitSwitches)
            {
                swc.SetValue(_emitter, AudioTriggerSource.AudioState);             
            }
            foreach (var evt in EnterEvents)
            {
                if (evt.StopOnDisable)
                    evt.Stop(_emitter, evt.FadeOutTime, AudioTriggerSource.AudioState);                        
            }                
            foreach (var evt in ExitEvents)
            {
                evt.Post(_emitter, AudioTriggerSource.AudioState);         
            }

            if (ResetStateOnExit && _animationSound)
                _animationSound.SetAnimationState(AnimationAudioState.None);
        }
        
        public bool IsValid()
        {
            return EnterEvents.Any(s => s.IsValid()) || ExitEvents.Any(s => s.IsValid()) || 
                   EnterSwitches.Any(s => s.IsValid()) || ExitSwitches.Any(s => s.IsValid()) ||
                   EnterStates.Any(s => s.IsValid()) || ExitStates.Any(s => s.IsValid()) || 
                   AnimationAudioState != AnimationAudioState.None;
        }

        private void OnDisable()
        {
            foreach (var evt in EnterEvents)
            {
                if (evt.StopOnDisable)
                    evt.Stop(_emitter, evt.FadeOutTime, AudioTriggerSource.AudioState);
            }
        }
    }
}
