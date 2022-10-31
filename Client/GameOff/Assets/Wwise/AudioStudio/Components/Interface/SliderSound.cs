using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/SliderSound")]
    [DisallowMultipleComponent]
    public class SliderSound : AsUIHandler
    {
        public AudioEvent DragEvent = new AudioEvent();        
        public RTPCExt ConnectedRTPC = new RTPCExt();
        public float ValueScale = 1f;

        public override void AddListener()
        {
            var slider = GetComponent<Slider>();
            if (slider)
                slider.onValueChanged.AddListener(OnSliderChanged);
        }
        
        public override void RemoveListener()
        {
            var slider = GetComponent<Slider>();
            if (slider)
                slider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void OnSliderChanged(float value)
        {
            ConnectedRTPC.SetValue(value * ValueScale, gameObject, AudioTriggerSource.SliderSound);
            DragEvent.Post(gameObject, AudioTriggerSource.SliderSound);
        }
        
        public override bool IsValid()
        {
            return ConnectedRTPC.IsValid() || DragEvent.IsValid();
        }
    }   
}

