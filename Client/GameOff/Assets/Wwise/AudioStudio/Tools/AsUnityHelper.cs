using AK.Wwise;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioStudio.Tools
{
    #region ProfilerEnums
    //-----------------------------------------------------------------------------------------
    /// <summary>
    /// What kind of audio object the message is related to.
    /// </summary>
    public enum AudioObjectType
    {
        SFX,
        Music,
        Voice,
        Switch,
        State,
        RTPC,
        Trigger,
        SoundBank,
        AudioPackage,
        AuxBus,
        Emitter,
        Listener,
        AudioState,
        Language,
        Component
    }
    
    /// <summary>
    /// How severe the audio message is.
    /// </summary>
    public enum Severity
    {
        Notification,
        Warning,
        Error,
        None
    }

    /// <summary>
    /// In which way an audio event is triggered from.
    /// </summary>
    public enum AudioTriggerSource
    {
        Code,
        InspectorAudition,
        Initialization,
        AnimationSound,
        AudioListener3D,
        AudioRoom,
        AudioState,
        ButtonSound,
        ColliderSound,
        DropdownSound,
        EffectSound,
        EmitterSound,
        EventSound,
        LoadBank,
        MenuSound,
        MusicSwitch,
        ScrollSound,
        SpatialAudioListener,
        GlobalAuxSend,
        ReverbZone,
        SetState,
        SetSwitch,
        SliderSound,
        TimelineSound,
        ToggleSound,
        WwiseTimelineClip
    }

    public enum AudioAction
    {
        PostEvent,
        StopEvent,
        Pause,
        Resume,
        Mute,
        Unmute,
        Load,
        Unload,
        Reload,
        SetValue,
        GetValue,
        Activate,
        Deactivate,
        Register,
        Unregister
    }
    
#if UNITY_EDITOR
    public struct ProfilerMessage
    {
        public Severity Severity;
        public string Time;
        public AudioObjectType ObjectType;
        public AudioAction Action;
        public AudioTriggerSource TriggerFrom;
        public string ObjectName;
        public GameObject GameObject;
        public string GameObjectName;
        public string Message;
    }
#endif    

    //-----------------------------------------------------------------------------------------
    #endregion

    public static class AsUnityHelper
    {
        /// <summary>
        /// Get or create a ScriptableObject asset. Save to path in editor mode.
        /// </summary>
        public static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
#if UNITY_EDITOR
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (!asset)
            {
                asset = ScriptableObject.CreateInstance<T>();
                UnityEditor.AssetDatabase.CreateAsset(asset, path);
            }
#else
            var asset = ScriptableObject.CreateInstance<T>();
            Debug.LogWarning(typeof(T).Name + " config not found, creating an empty one instead");
#endif
            return asset;
        }

        /// <summary>
        /// Get a component from game object. Add component if not found.
        /// </summary>
        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();
            return component;
        }
        #region Profiler
        //-----------------------------------------------------------------------------------------
        public static Severity DebugLogLevel = Severity.Error;
#if UNITY_EDITOR
        public static System.Action<ProfilerMessage> ProfilerCallback;
#endif
        
        /// <summary>
        /// Output a message to AudioProfiler in editor mode. Output to console in build.
        /// </summary>
        internal static void DebugToProfiler(Severity severity, 
                                            AudioObjectType objectType, 
                                            AudioTriggerSource triggerFrom, 
                                            AudioAction action, 
                                            string eventName, 
                                            GameObject gameObject = null, 
                                            string message = "")
        {
#if UNITY_EDITOR
            // if AudioProfiler is opened, just send message to it
            if (ProfilerCallback != null)
            {
                var newMessage = new ProfilerMessage
                {
                    Severity = severity,
                    Time = Time.time.ToString("0.000"),
                    ObjectType = objectType,
                    Action = action,
                    TriggerFrom = triggerFrom,
                    ObjectName = eventName,
                    GameObject = gameObject,
                    GameObjectName = gameObject ? gameObject.name : "Global Audio Emitter",
                    Message = message,
                };
                ProfilerCallback.Invoke(newMessage);
                return;
            }
#endif
            // only output messages that are more severe than the level set
            if (severity >= DebugLogLevel && Debug.unityLogger.logEnabled)
            {
                var log = string.Format("Audio{0}: {1}_{2}\tName: {3}\tTrigger: {4}\t" +
                    "GameObject: {5}\tMessage: {6}", severity, objectType, action, eventName, 
                    triggerFrom, gameObject ? gameObject.name : "Global Audio Emitter", message);
                switch (severity)
                {
                    case Severity.Error:
                        Debug.LogError(log);
                        break;
                    case Severity.Warning:
                        Debug.LogWarning(log);
                        break;
                    case Severity.Notification:
                        Debug.Log(log);
                        break;
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        #endregion
    }
}