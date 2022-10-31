using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio
{
    /// <summary>
    /// Setting config for AudioStudio initialization settings.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioInitSettings", menuName = "AudioStudio/Audio Init Settings")]
    public class AudioInitSettings : ScriptableObject
    {
        private static AudioInitSettings _instance;
        public static AudioInitSettings Instance
        {
            get
            {
                if (!_instance)
                    _instance = AsUnityHelper.GetOrCreateAsset<AudioInitSettings>("Assets/" + 
                        WwisePathSettings.AUDIO_STUDIO_LIBRARY_PATH + "/Configs/AudioInitSettings.asset");
                return _instance;
            }
            set { _instance = value; }
        }

        #region Field
        //-----------------------------------------------------------------------------------------
        public static bool Initialized;
        public bool DisableWwise;
        public Severity DebugLogLevel = Severity.Error;
        public bool PackageMode;
        public bool AddListenerToMainCamera = true;
        [Range(0, 4)]
        public int AlternativeListenersCount;
        public bool AutoInitialize;
        public bool EnableSpatialAudio;
        public bool UseMicrophone;
        public AkWwiseInitializationSettings WwiseInitializationSettings;

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Initialization
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// Initialize Wwise audio engine with option to load default banks and packages.
        /// </summary>
        /// <param name="loadAudioData"></param>
        public void Initialize(bool loadAudioData = false)
        {
            if (Initialized) return;
            AudioManager.DisableWwise = DisableWwise;
            if (DisableWwise) return;
            
            Initialized = true;
            AsUnityHelper.DebugLogLevel = DebugLogLevel;
            //AkWwiseInitializationSettings.Instance = WwiseInitializationSettings;
            CreateGlobalAudioEmitter();
            InitSpatialAudio();
            string hotfixpath = System.IO.Path.Combine(Application.persistentDataPath, WwisePathSettings.UPDATE_BANK_SUB_FOLDER, AkBasePathGetter.GetPlatformName());
            AkSoundEngine.AddBasePath(hotfixpath);
            LoadLanguageSettings();
            LoadAudioDataBeforeHotUpdate();
            if (loadAudioData)
                AsAssetLoader.LoadAudioInitData();
            //LoadVolumeSettings();
        }

        /// <summary>
        /// Generate a global audio emitter playing all around the game to play 2D sound 
        /// </summary>
        private void CreateGlobalAudioEmitter()
        {
            var globalAudioEmitter = new GameObject("Global Audio Emitter");
            globalAudioEmitter.AddComponent<AkInitializer>();
            globalAudioEmitter.AddComponent<GlobalAudioEmitter>();
            ListenerManager.Init(AlternativeListenersCount);
            if (UseMicrophone) globalAudioEmitter.AddComponent<MicrophoneInput>();
        }
        
        /// <summary>
        /// Initialize the spatial audio
        /// </summary>
        private void InitSpatialAudio()
        {
            SpatialAudioManager.SpatialAudioEnabled = EnableSpatialAudio;
            if (AddListenerToMainCamera && Camera.main != null)
                AsUnityHelper.GetOrAddComponent<AudioListener3D>(Camera.main.gameObject);
        }

        /// <summary>
        /// Load the language preference saved by player
        /// </summary>
        private static void LoadLanguageSettings()
        {
            var language = AudioManager.VoiceLanguage.ToString();
            var result = AudioStudioWrapper.SetCurrentLanguage(language);
            if (result == AKRESULT.AK_Success)
                AsUnityHelper.DebugToProfiler(Severity.Notification, 
                                              AudioObjectType.Language, 
                                              AudioTriggerSource.Initialization, 
                                              AudioAction.SetValue, 
                                              language);
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, 
                                              AudioObjectType.Language, 
                                              AudioTriggerSource.Initialization, 
                                              AudioAction.SetValue, 
                                              language, 
                                              null, 
                                              result.ToString());
        }

        /// <summary>
        /// Load the banks that are used before hot update occurs
        /// </summary>
        private void LoadAudioDataBeforeHotUpdate()
        {
#if !UNITY_EDITOR
            if (PackageMode) 
                PackageManager.LoadPackage("Init");
#endif
            AudioManager.LoadBank("Init", null, null, AudioTriggerSource.Initialization);
        }

        /// <summary>
        /// Load Volume Settings
        /// </summary>
        private static void LoadVolumeSettings()
        {
            AudioStudioWrapper.PlaySound(AudioManager.SoundEnabled ? "Sound_On" : "Sound_Off", GlobalAudioEmitter.GameObject);
            AudioStudioWrapper.PlaySound(AudioManager.VoiceEnabled ? "Voice_On" : "Voice_Off", GlobalAudioEmitter.GameObject);
            AudioStudioWrapper.PlaySound(AudioManager.MusicEnabled ? "Music_On" : "Music_Off", GlobalAudioEmitter.GameObject);
            AudioStudioWrapper.SetRTPCValue("Sound_Volume", AudioManager.SoundVolume);
            AudioStudioWrapper.SetRTPCValue("Voice_Volume", AudioManager.VoiceVolume);
            AudioStudioWrapper.SetRTPCValue("Music_Volume", AudioManager.MusicVolume);			
        }

        //-----------------------------------------------------------------------------------------
        #endregion 
    }
}