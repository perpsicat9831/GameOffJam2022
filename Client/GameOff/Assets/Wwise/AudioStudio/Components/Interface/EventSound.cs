using System;
using System.Linq;
using AK.Wwise;
using AudioStudio.Tools;
using UnityEngine;
using UnityEngine.EventSystems;


namespace AudioStudio.Components
{
    [Serializable]
    public class UIAudioEvent
    {
        public AudioEvent AudioEvent = new AudioEvent();
        public EventTriggerType TriggerType;
        public AudioEventAction Action;
    }
    
    [AddComponentMenu("AudioStudio/EventSound")]
    [DisallowMultipleComponent]
    public class EventSound : AsUIHandler
    {
        public UIAudioEvent[] UIAudioEvents = new UIAudioEvent[0];

        protected override void Start()
        {
            var et = AsUnityHelper.GetOrAddComponent<EventTrigger>(gameObject);
            foreach (var evt in UIAudioEvents)
            {
                var trigger = new EventTrigger.Entry
                {
                    eventID = evt.TriggerType
                };
                et.triggers.Add(trigger);
                if (evt.Action == AudioEventAction.Play)
                    trigger.callback.AddListener(data => 
                    evt.AudioEvent.Post(gameObject, AudioTriggerSource.EventSound));
                else if (evt.Action == AudioEventAction.Stop)
                    trigger.callback.AddListener(data => 
                    evt.AudioEvent.Stop(gameObject, 0.2f, AudioTriggerSource.EventSound));
            }
        }

        public override bool IsValid()
        {
            return UIAudioEvents.Any(e => e.AudioEvent.IsValid());
        }
    }
}