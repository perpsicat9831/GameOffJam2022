using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/DropdownSound")]
    [DisallowMultipleComponent]
    public class DropdownSound : AsUIHandler
    {
        public AudioEvent[] ValueChangeEvents = new AudioEvent[0];
        public AudioEvent[] PopupEvents = new AudioEvent[0];
        public AudioEvent[] CloseEvents = new AudioEvent[0];
        private bool _isPoppedUp;

        protected override void Start()
        {
            var dropDown = gameObject.GetComponent<Dropdown>();
            if (dropDown == null) return;
            dropDown.onValueChanged.AddListener(x => PostEvents(ValueChangeEvents, AudioTriggerSource.DropdownSound, gameObject));

            var trigger = AsUnityHelper.GetOrAddComponent<EventTrigger>(gameObject);
            var entry = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};

            entry.callback.AddListener((x) => { PlayPopupSound(); });
            trigger.triggers.Add(entry);

            var submit = new EventTrigger.Entry {eventID = EventTriggerType.Submit};

            submit.callback.AddListener((x) => { PlayPopupSound(); });
            trigger.triggers.Add(submit);

            var close = new EventTrigger.Entry {eventID = EventTriggerType.Select};

            close.callback.AddListener((x) => { PlayCloseSound(); });
            trigger.triggers.Add(close);

            var cancel = new EventTrigger.Entry {eventID = EventTriggerType.Cancel};

            cancel.callback.AddListener((x) => { PlayCloseSound(); });
            trigger.triggers.Add(cancel);            
        }

        private void PlayPopupSound()
        {
            _isPoppedUp = true;
            PostEvents(PopupEvents, AudioTriggerSource.DropdownSound, gameObject);
        }

        private void PlayCloseSound()
        {
            if (_isPoppedUp)
                PostEvents(CloseEvents, AudioTriggerSource.DropdownSound, gameObject);
            _isPoppedUp = false;
        }
        
        public override bool IsValid()
        {
            return PopupEvents.Any(s => s.IsValid()) || ValueChangeEvents.Any(s => s.IsValid()) || CloseEvents.Any(s => s.IsValid());
        }
    }
}

