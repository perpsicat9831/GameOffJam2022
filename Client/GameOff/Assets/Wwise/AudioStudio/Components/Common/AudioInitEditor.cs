#if UNITY_EDITOR
using AudioStudio.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Components
{
    [ExecuteInEditMode]
    public class AudioInitEditor : MonoBehaviour
    {
        public enum Language
        {
            English = 1,
            Chinese = 2
        }

        //public bool InitOnAwake = true;
        public bool LoadAudioData = true;
        public GameObject listenergo;
        public GameObject[] emitters;
        public Language Lan = Language.Chinese;
        public AudioInitLoadData AudioInitData;
        public AkWwiseInitializationSettings WwiseInitializationSettings;

        private static bool Initialized;
        private GameObject GAE;
        private AudioListener3D Listener3d;
        private AudioInitSettings AudioInitSettings;
        private AkInitializer aki;
        // Start is called before the first frame update
        void Awake()
        {
            // without a referenced config, create a new one instead
            AudioInitSettings = AudioInitSettings.Instance;

            if (Initialized)
                return;
            //if (InitOnAwake)
            //{
            //    Initialize();
            //}
        }

        private void CreateGlobalAudioEmitter()
        {
            GAE = new GameObject("Global Audio Emitter");
            aki = GAE.AddComponent<AkInitializer>();
            if (aki) aki.enabled = true;
            GAE.AddComponent<GlobalAudioEmitter>();
            AudioStudioWrapper.RegisterGameObj(GAE, GAE.name);
            GlobalAudioEmitter.GameObject = GAE;
            ListenerManager.Init(0);
            if (listenergo)
            {
                Listener3d = AsUnityHelper.GetOrAddComponent<AudioListener3D>(listenergo);
                AKRESULT res = AudioStudioWrapper.RegisterGameObj(listenergo);
                res = AudioStudioWrapper.AddDefaultListener(listenergo);
            }
        }

        private void LoadBanks()
        {
            uint bankID;
            AudioStudioWrapper.LoadBank("Init", out bankID);
            //path = Path.Combine(path, "Android");
            foreach(var bank in AudioInitData.Banks)
            {
                AudioStudioWrapper.LoadBank(bank.Name, out bankID);
            }
        }

        private void Initialize()
        {
            if (Initialized) return;

            Initialized = true;
            AudioInitSettings.Initialized = true;
            AsUnityHelper.DebugLogLevel = Severity.Error;
            //AkWwiseInitializationSettings.Instance = WwiseInitializationSettings;
            CreateGlobalAudioEmitter();
            string language = "Chinese";
            if (Lan == Language.English)
                language = "English";
            AudioStudioWrapper.SetCurrentLanguage(language);
            //LoadVolumeSettings();
            LoadBanks();
        }

        IEnumerator DestroyAE()
        {
            yield return new WaitForSeconds(.01f);
            AudioEmitter3D ae = Listener3d.gameObject.GetComponent<AudioEmitter3D>();
            if (ae)
                DestroyImmediate(ae, true);
            DestroyImmediate(Listener3d, true);
        }
        private void Terminate()
        {
            if (Initialized)
            {
                Initialized = false;
                AudioManager.UnloadBank("init", false);
                if (AudioInitData) AudioInitData.UnloadAllBanks();
                if (Listener3d)
                {
                    AudioStudioWrapper.RemoveDefaultListener(Listener3d.gameObject);
                    AudioStudioWrapper.UnregisterGameObj(Listener3d.gameObject);
                    StartCoroutine("DestroyAE");
                }
                if (GAE)
                {
                    AudioStudioWrapper.UnregisterGameObj(GAE);
                    DestroyImmediate(GAE, true);
                }
                AudioStudioWrapper.TerminateSoundEngine();
            }
        }

        private void OnEnable()
        {
            if (Initialized)
                return;

            Initialize();
        }

        private void OnDisable()
        {
            Terminate();
        }

        private void OnDestroy()
        {
            Terminate();
        }
        // Update is called once per frame
        void Update()
        {
            if (Initialized)
            {
                if (Listener3d && Listener3d.IsValid())
                    AudioStudioWrapper.SetObjectPosition(Listener3d.gameObject, Listener3d.gameObject.transform);

                for (int i = 0; i < emitters.Length; i++)
                {
                    if (emitters[i])
                        AudioStudioWrapper.SetObjectPosition(emitters[i], emitters[i].transform);
                }
            }
        }
    }
}

#endif