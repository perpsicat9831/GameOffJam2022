using AK.Wwise;
using AudioStudio.Tools;
using System;
using UnityEngine;

namespace AudioStudio
{
    /// <summary>
    /// Data config for all the banks and packages that should be loaded when game starts.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioInitLoadData", menuName = "AudioStudio/Audio Init Load Data")]
    public class AudioInitLoadData : ScriptableObject
    {
        public bool LoadPackages;
        public string[] AudioPackages;
        public bool LoadBanks = true;
        public AudioBank[] Banks = new AudioBank[0];
        public bool PostEvents;
        public AudioEvent[] AudioEvents = new AudioEvent[0];
        public bool SetStates;
        public ASState[] States = new ASState[0];

        internal void LoadAudioData()
        {
            // only load packages in build because voice format might not match
#if !UNITY_EDITOR
            if (LoadPackages) 
            {
                foreach (var package in AudioPackages)
                {
                    PackageManager.LoadPackage(package);
                }
            }
#endif
            if (LoadBanks)
            {
                foreach (var bank in Banks)
                {
                    bank.Load(null, AudioTriggerSource.Initialization);
                }
            }

            if (SetStates)
            {
                foreach (var state in States)
                {
                    state.SetValue(null, AudioTriggerSource.Initialization);
                }
            }
            
            if (PostEvents)
            {
                foreach (var evt in AudioEvents)
                {
                    evt.Post(null, AudioTriggerSource.Initialization);
                }
            }
        }

        internal void UnloadAllBanks()
        {
            foreach (var bank in Banks)
            {
                AudioStudioWrapper.UnloadBank(bank.Name, IntPtr.Zero);
                //bank.Unload(null, AudioTriggerSource.Code);
            }
        }
    }
}