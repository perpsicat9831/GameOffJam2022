using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
    /// <summary>
    /// Trigger sound when an visual effect is instantiated or destroyed.
    /// </summary>
    [AddComponentMenu("AudioStudio/EffectSound")]
    [DisallowMultipleComponent]
    public class EffectSound : AudioEmitter3D
    {
        public AudioEventExt[] EnableEvents = new AudioEventExt[0];
        public AudioEvent[] DisableEvents = new AudioEvent[0];
        [Range(0f, 2f)]
        public float DelayTime;
        
        protected override void Init()
        {
            StopOnDestroy = false;          
        }

        protected override void HandleEnableEvent()
        {
            if (DelayTime > 0f)
                StartCoroutine(PlaySoundDelayed());
            else
                PostEvents(EnableEvents, AudioTriggerSource.EffectSound, gameObject, UnderPlayerControl);
        } 

        protected override void HandleDisableEvent()
        {
            StopEnableEvents(); 
            PostEvents(DisableEvents, AudioTriggerSource.EffectSound, gameObject, UnderPlayerControl);
            base.HandleDisableEvent();
        }

        private IEnumerator PlaySoundDelayed()
        {
            yield return new WaitForSeconds(DelayTime);
            PostEvents(EnableEvents, AudioTriggerSource.EffectSound, gameObject, UnderPlayerControl);
        }

        private void StopEnableEvents()
        {            
            foreach (var evt in EnableEvents)
            {
                if (evt.StopOnDisable) 
                    evt.Stop(gameObject, evt.FadeOutTime, AudioTriggerSource.EffectSound, UnderPlayerControl);                
            }            
        }
        
        public override bool IsValid()
        {
            return GetEvents().Any(s => s.IsValid());
        }
        
        public override IEnumerable<AudioEvent> GetEvents()
        {
            return EnableEvents.Concat(DisableEvents);
        }
        
#if UNITY_EDITOR
        private void Reset()
        {
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
                return;

            var reference = AkWwiseTypes.DragAndDropObjectReference;
            if (reference)
            {
                GUIUtility.hotControl = 0;  
                EnableEvents = new []{new AudioEventExt()};                    
                EnableEvents[0].SetupReference(reference.ObjectName, reference.Guid);                
            }
        }
#endif  
    }
}
