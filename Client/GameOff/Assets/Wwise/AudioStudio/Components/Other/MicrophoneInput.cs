using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioStudio.Components
{
    /// <summary>
    /// BETA: Record voice from player input and send to Wwise for DSP.
    /// </summary>
    [DisallowMultipleComponent]
    public class MicrophoneInput : MonoBehaviour
    {
        public static MicrophoneInput Instance;

        private AudioSource _source;
        private Mutex _mutex = new Mutex();
        private List<float> _frameBuffer = new List<float>();

        private uint SampleRate
        {
            get { return AkWwiseInitializationSettings.Instance.UserSettings.m_SampleRate; }
        }

        private void Awake()
        {
            Instance = this;
            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.spread = 1;
            _source.ignoreListenerVolume = true;
            _source.outputAudioMixerGroup = Resources.Load<AudioMixerGroup>("Audio/AudioMixer");
        }

        public void StartRecording()
        {
            if (!AudioInitSettings.Initialized) return;
            _source.clip = Microphone.Start(null, true, 1, (int)SampleRate);
            while (Microphone.GetPosition(null) <= 0)
            {
            }
            _source.Play();
            AkAudioInputManager.PostAudioInputEvent("Voice_Record_Start", gameObject, OnInputBuffer, FormatDelegate);
        }

        public void EndRecording()
        {
            if (!AudioInitSettings.Initialized) return;
            _source.Stop();
            Microphone.End(null);
            AudioStudioWrapper.PlaySound("Voice_Record_End", gameObject);
        }
        
        private void FormatDelegate(uint playingID, AkAudioFormat format)
        {
            format.uSampleRate = SampleRate;
            format.channelConfig.uNumChannels = 1;
        }
        
        private bool OnInputBuffer(uint playingID, uint channelIndex, float[] samples)
        {
            _mutex.WaitOne();
            
            var blockSize = Mathf.Min(_frameBuffer.Count, samples.Length);
            List<float> block = _frameBuffer.GetRange(0, blockSize);
            _frameBuffer.RemoveRange(0, blockSize);
            block.CopyTo(samples);
            _mutex.ReleaseMutex();
            return true;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            _mutex.WaitOne();
            for (var i = 0; i < data.Length; i += channels)
            {
                _frameBuffer.Add(data[i]);
            }
            _mutex.ReleaseMutex();
        }
    }
}