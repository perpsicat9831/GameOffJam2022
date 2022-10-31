using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/MenuSound")]
    [DisallowMultipleComponent]
    public class MenuSound : AsUIHandler
    {
        public AudioEvent[] OpenEvents = new AudioEvent[0];
        public AudioEvent[] CloseEvents = new AudioEvent[0];
        
        protected override void HandleEnableEvent()
        {
            PostEvents(OpenEvents, AudioTriggerSource.MenuSound, gameObject);                 
        }

        protected override void HandleDisableEvent()
        {
            PostEvents(CloseEvents, AudioTriggerSource.MenuSound, gameObject);   
        }
        
        public override bool IsValid()
        {            
            return OpenEvents.Any(s => s.IsValid()) || CloseEvents.Any(s => s.IsValid());
        }
    }
}
