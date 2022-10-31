using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using AK.Wwise;
using AudioStudio.Tools;
using UnityEngine.UI;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/ButtonSound")]
    [DisallowMultipleComponent]    
    public class ButtonSound : AsUIHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public AudioEvent[] ClickEvents = new AudioEvent[0];
        public AudioEvent PointerEnterEvent = new AudioEvent();
        public AudioEvent PointerExitEvent = new AudioEvent();
        private Button _button;

        protected override void Awake()
        {
            base.Awake();
            _button = GetComponent<Button>();
        }

        public override void AddListener()
        {
            if (_button)
                _button.onClick.AddListener(PlaySound);
        }
        
        public override void RemoveListener()
        {
            if (_button)
                _button.onClick.RemoveListener(PlaySound);
        }

        public void PlaySound()
        {
            PostEvents(ClickEvents, AudioTriggerSource.ButtonSound, gameObject); 
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (_button && _button.enabled)
                PointerEnterEvent.Post(gameObject, AudioTriggerSource.ButtonSound);
        }
        
        public void OnPointerExit(PointerEventData data)
        {
            if (_button && _button.enabled)
                PointerExitEvent.Post(gameObject, AudioTriggerSource.ButtonSound);
        }
        
        public override bool IsValid()
        {            
            return ClickEvents.Any(s => s.IsValid()) || PointerEnterEvent.IsValid() || PointerExitEvent.IsValid();
        }
    }
}

