using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/ToggleSound")]
    [DisallowMultipleComponent]
    public class ToggleSound : AsUIHandler
    {
        public AudioEvent[] ToggleOnEvents = new AudioEvent[0];
        public AudioEvent[] ToggleOffEvents = new AudioEvent[0];

        public override void AddListener()
        {
            var toggle = GetComponent<Toggle>();
            if (toggle)
                toggle.onValueChanged.AddListener(PlaySound);
        }
        
        public override void RemoveListener()
        {
            var toggle = GetComponent<Toggle>();
            if (toggle)
                toggle.onValueChanged.RemoveListener(PlaySound);
        }

        private void PlaySound(bool isOn)
        {
            PostEvents(isOn ? ToggleOnEvents : ToggleOffEvents, AudioTriggerSource.ToggleSound, gameObject);
        }

        public override bool IsValid()
        {            
            return ToggleOnEvents.Any(s => s.IsValid()) || ToggleOffEvents.Any(s => s.IsValid());
        }
    }
}