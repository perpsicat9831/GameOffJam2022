using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio
{
    public static class AudioManager
    {
        #region Sound
        /// <summary>
        /// Post a simple sound effect event.
        /// </summary>
        public static void PlaySound(string eventName, GameObject soundSource = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {            
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;
            var emitter = ValidateSoundSource(soundSource);
            var playingID = AudioStudioWrapper.PlaySound(eventName, emitter);
            ValidateSound(playingID, eventName, soundSource, trigger);
        }

        /// <summary>
        /// Play a sound effect with callback.
        /// </summary>
        public static void PlaySound(string eventName, AkCallbackType callbackType, Action callbackAction, GameObject soundSource = null)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;            
            var emitter = ValidateSoundSource(soundSource);         
            var playingID = AudioStudioWrapper.PlaySound(eventName, emitter, (uint)callbackType, CallbackSimple, callbackAction);
            ValidateSound(playingID, eventName, soundSource);
        }
        
        /// <summary>
        /// Play a sound effect with multiple callbacks.
        /// </summary>
        public static void PlaySound(string eventName, IEnumerable<AkCallbackType> callbackTypes, Action<AkCallbackType, AkCallbackInfo> callbackAction, GameObject soundSource = null)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;            
            var emitter = ValidateSoundSource(soundSource);
            var total = callbackTypes.Aggregate(0U, (current, type) => current + (uint) type);
            var playingID = AudioStudioWrapper.PlaySound(eventName, emitter, total, CallbackWithInfo, callbackAction);
            ValidateSound(playingID, eventName, soundSource);
        }

        // get the actual emitter from sound trigger source
        private static GameObject ValidateSoundSource(GameObject soundSource)
        {
            if (!soundSource || !EmitterManager.IsGameObjectRegistered(soundSource))
                return GlobalAudioEmitter.GameObject;
            return soundSource;
        }

        // creating profiler log
        private static void ValidateSound(uint playingID, string eventName, GameObject soundSource, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (playingID != 0)                
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.SFX, trigger, AudioAction.PostEvent, eventName, soundSource);                
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.SFX, trigger, AudioAction.PostEvent, eventName, soundSource, "Sound Event not found");
        }
        
        /// <summary>
        /// Stop a sound currently playing with optional fade out.
        /// </summary>
        public static void StopSound(string eventName, GameObject soundSource = null, float fadeOutTime = 0.2f, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;
            var emitter = ValidateSoundSource(soundSource);
            fadeOutTime = Mathf.Clamp(fadeOutTime * 1000, 0, 10000);            
            var result = AudioStudioWrapper.ExecuteActionOnEvent(eventName, AkActionOnEventType.AkActionOnEventType_Stop,
                emitter, (int) fadeOutTime, AkCurveInterpolation.AkCurveInterpolation_Linear);
            if (result == AKRESULT.AK_Success)                
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.SFX, trigger, AudioAction.StopEvent, eventName, soundSource);                
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.SFX, trigger, AudioAction.StopEvent, eventName, soundSource, result.ToString());            
        }
        #endregion
        
        #region Music
        private static string _lastPlayedMusic;
        private static string _currentPlayingMusic;
        
        /// <summary>
        /// Switch back to the last music played.
        /// </summary>
        public static void PlayLastMusic(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {                
            PlayMusic(_lastPlayedMusic, source, trigger);
        }

        /// <summary>
        /// Post a background music event, ignore if same event is already playing.
        /// </summary>
        public static void PlayMusic(string eventName, GameObject triggerFrom = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;
            if (eventName != _currentPlayingMusic)
            {
                var playingID = AudioStudioWrapper.PlaySound(eventName, GlobalAudioEmitter.GameObject);
                ValidateMusic(playingID, eventName, triggerFrom, trigger);
            }
            else
                AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.Music, trigger, AudioAction.PostEvent, eventName, triggerFrom, "Music already playing");
        }

        /// <summary>
        /// Stop the background music currently playing.
        /// </summary>
        public static void StopMusic(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized) return;
            AudioStudioWrapper.PlaySound("Music_Stop", GlobalAudioEmitter.GameObject);
            _lastPlayedMusic = _currentPlayingMusic;
            _currentPlayingMusic = string.Empty;
            AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.Music, trigger, AudioAction.StopEvent, "Global Music Stop", source);
        }
        
        /// <summary>
        /// Play background music with callback.
        /// </summary>
        public static void PlayMusic(string eventName, AkCallbackType callbackType, Action callbackAction)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName) || eventName == _currentPlayingMusic) return;
            var playingID = AudioStudioWrapper.PlaySound(eventName, GlobalAudioEmitter.GameObject, (uint) callbackType, CallbackSimple, callbackAction);
            ValidateMusic(playingID, eventName);
        }

        /// <summary>
        /// Play background music with multiple callbacks.
        /// </summary>
        public static void PlayMusic(string eventName, IEnumerable<AkCallbackType> callbackTypes, Action<AkCallbackType, AkCallbackInfo> callbackAction)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName) || eventName == _currentPlayingMusic) return;
            var total = callbackTypes.Aggregate(0U, (current, type) => current + (uint) type);
            var playingID = AudioStudioWrapper.PlaySound(eventName, GlobalAudioEmitter.GameObject, total, CallbackWithInfo, callbackAction);
            ValidateMusic(playingID, eventName);
        }

        // update music play history and create profiler log
        private static void ValidateMusic(uint playingID, string eventName, GameObject triggerFrom = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (playingID != 0)
            {
                _lastPlayedMusic = _currentPlayingMusic;
                _currentPlayingMusic = eventName;
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.Music, trigger, AudioAction.PostEvent, eventName, triggerFrom);
            }
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Music, trigger, AudioAction.PostEvent, eventName, triggerFrom, "Music Event not found");
        }
        #endregion

        #region Voice
        //-----------------------------------------------------------------------------------------

        private static string _currentPlayingVoice = "";
        
        /// <summary>
        /// Post a voice dialog event.
        /// </summary>
        public static void PlayVoice(string eventName, 
                                    GameObject triggerFrom = null, 
                                    AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;
            var result = AudioStudioWrapper.PlaySound(eventName, 
                triggerFrom ? triggerFrom : GlobalAudioEmitter.GameObject);
            ValidateVoice(result, eventName, triggerFrom, trigger);
        }

        /// <summary>
        /// Play voice with callback
        /// </summary>
        public static void PlayVoice(string eventName, 
                                    AkCallbackType callbackType, 
                                    Action callbackAction)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;
            var result = AudioStudioWrapper.PlaySound(eventName, 
                                                GlobalAudioEmitter.GameObject, 
                                                (uint)callbackType, 
                                                CallbackSimple, 
                                                callbackAction);
            ValidateVoice(result, eventName);
        }

        /// <summary>
        /// Play voice with multiple callbacks
        /// </summary>
        public static void PlayVoice(string eventName, 
                                    IEnumerable<AkCallbackType> callbackTypes, 
                                    Action<AkCallbackType, AkCallbackInfo> callbackAction)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;
            var total = callbackTypes.Aggregate(0U, (current, type) => current + (uint) type);
            var result = AudioStudioWrapper.PlaySound(eventName, 
                                                GlobalAudioEmitter.GameObject, 
                                                total, 
                                                CallbackWithInfo, 
                                                callbackAction);
            ValidateVoice(result, eventName);
        }

        public static void PlayExternalVoice(string eventName, string externalName, AkCallbackType callbackType = 0, Action callbackAction = null, GameObject gameObj = null)
        {
            AkExternalSourceInfo source = new AkExternalSourceInfo();

            source.iExternalSrcCookie = AkSoundEngine.GetIDFromString("External_Source"); // cookie 是 External Source 对象名称的哈希索引。
            source.szFile = (AudioStudio.AudioManager.VoiceLanguage + "/" + eventName + ".wem");      //我们将播放的文件。
            source.idCodec = AkSoundEngine.AKCODECID_VORBIS; //编码和Wwise中设定的编码保持一致。

            var array = new AkExternalSourceInfoArray(1);
            array[0] = source;

            AkSoundEngine.PostEvent(externalName, gameObj ? gameObj : GlobalAudioEmitter.GameObject, (uint)callbackType, CallbackWithInfo, callbackAction, 1, array);
        }

        // creating profiler log
        private static void ValidateVoice(uint playingID, 
                                          string eventName, 
                                          GameObject triggerFrom = null, 
                                          AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (playingID != 0)
            {
                AsUnityHelper.DebugToProfiler(Severity.Notification, 
                                              AudioObjectType.Voice, 
                                              trigger, 
                                              AudioAction.PostEvent, 
                                              eventName, 
                                              triggerFrom);
                _currentPlayingVoice = eventName;
            }
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, 
                                              AudioObjectType.Voice, 
                                              trigger, 
                                              AudioAction.PostEvent, 
                                              eventName, 
                                              triggerFrom, 
                                              "Voice Event not found");
        }  
        
        /// <summary>
        /// Pause a playing voice event. Leave event name empty if pausing the current playing event.
        /// </summary>
        public static void PauseVoice(string eventName = "", float fadeOutTime = 0.2f)
        {
            ExecuteActionOnVoice(eventName, 
                                AkActionOnEventType.AkActionOnEventType_Pause, 
                                AudioAction.Pause, 
                                fadeOutTime);
        }

        /// <summary>
        /// Resume a playing voice event. Leave event name empty if resuming the current playing event.
        /// </summary>
        public static void ResumeVoice(string eventName = "", float fadeInTime = 0.2f)
        {
            ExecuteActionOnVoice(eventName, 
                                AkActionOnEventType.AkActionOnEventType_Resume, 
                                AudioAction.Resume, 
                                fadeInTime);
        }
        
        /// <summary>
        /// Stop a voice dialog event from playing with fade out. Leave event name empty if stopping the current playing event.
        /// </summary>
        public static void StopVoice(string eventName = "", 
                                    float fadeInTime = 0.2f, 
                                    GameObject triggerFrom = null, 
                                    AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            ExecuteActionOnVoice(eventName, 
                                AkActionOnEventType.AkActionOnEventType_Stop, 
                                AudioAction.StopEvent, 
                                fadeInTime, 
                                triggerFrom, 
                                trigger);
        }

        // pass pause/resume/stop command to Wwise
        private static void ExecuteActionOnVoice(string eventName, 
                                                AkActionOnEventType akAction, 
                                                AudioAction action, 
                                                float fadeOutTime = 0.2f, 
                                                GameObject triggerFrom = null, 
                                                AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized) return;
            if (string.IsNullOrEmpty(eventName))
            {
                if (_currentPlayingVoice != string.Empty)
                    eventName = _currentPlayingVoice;
                else
                {
                    AsUnityHelper.DebugToProfiler(Severity.Warning, 
                                                 AudioObjectType.Voice, 
                                                 trigger, 
                                                 action, 
                                                 eventName, 
                                                 triggerFrom, 
                                                 "No Voice is playing");    
                    return;
                }
            }
            fadeOutTime = Mathf.Clamp(fadeOutTime * 1000, 0, 10000);
            var result = AudioStudioWrapper.ExecuteActionOnEvent(eventName, akAction,
                GlobalAudioEmitter.GameObject, (int) fadeOutTime, AkCurveInterpolation.AkCurveInterpolation_Linear);
            if (result == AKRESULT.AK_Success)                
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.Voice, trigger, action, eventName, triggerFrom);                
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Voice, trigger, action, eventName, triggerFrom, result.ToString());
        }

        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Controls
        //-----------------------------------------------------------------------------------------
        private static readonly Dictionary <string, string> _currentStates = new Dictionary<string, string>();
        private static readonly Dictionary <string, string> _lastStates = new Dictionary<string, string>();
        
        /// <summary>
        /// Set the state group back to its last status. Only effective if it was only set by AudioManager and SetState component.
        /// </summary>
        public static void ResetLastState(string stateGroup, GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            var state = _lastStates.ContainsKey(stateGroup) ? _lastStates[stateGroup] : "None";
            SetState(stateGroup, state, source, trigger);
        }

        /// <summary>
        /// Set a global Wwise state in the format of [stateGroupName / stateName] 
        /// </summary>
        public static void SetState(string stateInfo, GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(stateInfo)) return;
            var nameSplit = stateInfo.Split('/');
            if (nameSplit.Length != 2)
            {
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.State, trigger, AudioAction.SetValue, stateInfo, source, "Invalid State format");
                return;
            }

            var groupName = nameSplit[0].Trim();
            var stateName = nameSplit[1].Trim();
            SetState(groupName, stateName, source, trigger);
        }
        
        /// <summary>
        /// Set a global Wwise state.
        /// </summary>
        public static void SetState(string stateGroup, string state, GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {      
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(stateGroup) || string.IsNullOrEmpty(state)) return;
            var result = AudioStudioWrapper.SetState(stateGroup, state);
            if (result == AKRESULT.AK_Success)
            {
                if (!_currentStates.ContainsKey(stateGroup))
                    _lastStates[stateGroup] = "None";
                else if (state != _lastStates[stateGroup])
                    _lastStates[stateGroup] = _currentStates[stateGroup];
                _currentStates[stateGroup] = state;
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.State, trigger, AudioAction.SetValue, stateGroup + " / " + state, source);                
            }
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.State, trigger, AudioAction.SetValue, stateGroup + " / " + state, source); 
        }

        /// <summary>
        /// Get the current string value of a state group. Only effective if it was only set by AudioManager or SetState component.
        /// </summary>
        public static string GetState(string stateGroup)
        {            
            if (_currentStates.ContainsKey(stateGroup))
            {
                var currentState = _currentStates[stateGroup];
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.State, AudioTriggerSource.Code, AudioAction.GetValue, stateGroup + " / " + currentState);
                return currentState;
            }
            AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.State, AudioTriggerSource.Code, AudioAction.SetValue, stateGroup, null, "State Group not initialized"); 
            return "";
        }

        /// <summary>
        /// Check if a state group is set at a certain state.
        /// </summary>
        public static bool IsStateAt(string stateGroup, string state)
        {
            var checkId = AudioStudioWrapper.GetIDFromString(state);
            uint currentId;
            AudioStudioWrapper.GetState(stateGroup, out currentId);
            return checkId == currentId;
        }
        
        /// <summary>
        /// Set a Wwise switch on a game object in the format of [switchGroupName / switchName] 
        /// </summary>
        public static void SetSwitch(string switchInfo, GameObject emitter = null)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(switchInfo)) return;
            var nameSplit = switchInfo.Split('/');
            if (nameSplit.Length != 2)
            {
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Switch, AudioTriggerSource.Code, AudioAction.SetValue, switchInfo, emitter, "Invalid Switch format");
                return;
            }

            var groupName = nameSplit[0].Trim();
            var switchName = nameSplit[1].Trim();
            SetSwitch(groupName, switchName, emitter);
        }
        
        /// <summary>
        /// Set a Wwise switch on a game object.
        /// </summary>
        public static void SetSwitch(string switchGroup, string switchName, GameObject emitter = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(switchGroup) || string.IsNullOrEmpty(switchName)) return;
            var target = ValidateSoundSource(emitter);
            var result = AudioStudioWrapper.SetSwitch(switchGroup, switchName, target);
            if (result == AKRESULT.AK_Success)                            
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.Switch, trigger, AudioAction.SetValue, switchGroup + " / " + switchName, emitter);                            
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Switch, trigger, AudioAction.SetValue, switchGroup + " / " + switchName, emitter, result.ToString()); 
        }
        
        /// <summary>
        /// Set a Wwise RTPC value on a game object.
        /// </summary>
        public static void SetRTPCValue(string parameterName, float value, GameObject emitter = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(parameterName)) return;
            var result = AKRESULT.AK_Success;
            if (!emitter || !EmitterManager.IsGameObjectRegistered(emitter))
                AudioStudioWrapper.SetRTPCValue(parameterName, value);
            else
                AudioStudioWrapper.SetRTPCValue(parameterName, value, emitter);                             
            if (result == AKRESULT.AK_Success)                            
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.RTPC, trigger, AudioAction.SetValue, parameterName, emitter, "Set to " + value.ToString("0.000"));                            
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.RTPC, trigger, AudioAction.SetValue, parameterName, emitter, result.ToString()); 
        }

        /// <summary>
        /// Post a Wwise trigger on a game object.
        /// </summary>
        public static void PostTrigger(string triggerName, 
                                       GameObject emitter = null, 
                                       AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(triggerName)) return;
            if (emitter == null) emitter = GlobalAudioEmitter.GameObject;
            var result = AudioStudioWrapper.PostTrigger(triggerName, emitter);
            if (result == AKRESULT.AK_Success)                            
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.Trigger, trigger, AudioAction.SetValue, triggerName, emitter);                            
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.Trigger, trigger, AudioAction.SetValue, triggerName, emitter, result.ToString());         
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region SoundBank
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// Load a sound bank with optional finish callback.
        /// </summary>
        public static void LoadBank(string bankName, 
                                    Action loadFinishedCallback = null, 
                                    GameObject source = null, 
                                    AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(bankName)) return;
            BankManager.LoadBank(bankName, loadFinishedCallback, source, trigger);
        }

        /// <summary>
        /// Unload a sound bank with optional finish callback.
        /// </summary>
        public static void UnloadBank(string bankName, 
                                      bool useCounter = true, 
                                      Action unloadFinishedCallback = null, 
                                      GameObject source = null, 
                                      AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(bankName)) return;            
            BankManager.UnloadBank(bankName, useCounter, unloadFinishedCallback, source, trigger);
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region MusicPlayer
        //-----------------------------------------------------------------------------------------
        private static GameObject GlobalMusicPlayerGameObj = null;
        private static string CurrentMusic = string.Empty;
        private static Action CallBack = null;
        private static bool isPlaying = false;
        private static void MusicEndCallBack()
        {
            isPlaying = false;
            if (CallBack != null) CallBack.Invoke();
        }
        public static void RegistMusicPlayer(GameObject gameobj)
        {
            GlobalMusicPlayerGameObj = gameobj;
        }

        public static void UnRegistMusicPlayer()
        {
            GlobalMusicPlayerGameObj = null;
        }

        public static void SeekAndPlay(string eventName, int in_pos = 0, Action callback = null)
        {
            if (!AudioInitSettings.Initialized || string.IsNullOrEmpty(eventName)) return;
            if (!string.IsNullOrEmpty(CurrentMusic) && isPlaying)
                StopMusicPlayer();
            CallBack = callback;
            PlaySound(eventName, AkCallbackType.AK_EndOfEvent, MusicEndCallBack, GlobalMusicPlayerGameObj);
            CurrentMusic = eventName;
            isPlaying = true;
            AKRESULT result = AudioStudioWrapper.SeekOnEvent(eventName, GlobalMusicPlayerGameObj, in_pos);
            if (result == AKRESULT.AK_Success)
                AsUnityHelper.DebugToProfiler(Severity.Notification, 
                                            AudioObjectType.Music, 
                                            AudioTriggerSource.Code, 
                                            AudioAction.PostEvent, 
                                            eventName, 
                                            GlobalMusicPlayerGameObj);
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, 
                                            AudioObjectType.Music, 
                                            AudioTriggerSource.Code, 
                                            AudioAction.PostEvent, 
                                            eventName, 
                                            GlobalMusicPlayerGameObj, 
                                            result.ToString());
        }

        public static void StopMusicPlayer()
        {
            if (!string.IsNullOrEmpty(CurrentMusic))
            {
                StopSound(CurrentMusic, GlobalMusicPlayerGameObj, 1.5f);
                CurrentMusic = string.Empty;
                isPlaying = false;
            }
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        #region ExternalAudioInput
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// Turn off audio temporarily for purposes like playing video.
        /// </summary>
        public static void MuteAudio()
        {
            if (!AudioInitSettings.Initialized) return;
            if (SoundEnabled)
                AudioStudioWrapper.PlaySound("Sound_Off", GlobalAudioEmitter.GameObject);
            if (VoiceEnabled)
                AudioStudioWrapper.PlaySound("Voice_Off", GlobalAudioEmitter.GameObject);
            if (MusicEnabled)
                AudioStudioWrapper.PlaySound("Music_Off", GlobalAudioEmitter.GameObject);
        }
        
        /// <summary>
        /// Put audio back for conditions like video finishes.
        /// </summary>
        public static void UnmuteAudio()
        {
            if (!AudioInitSettings.Initialized) return;
            if (SoundEnabled)
                AudioStudioWrapper.PlaySound("Sound_On", GlobalAudioEmitter.GameObject);
            if (VoiceEnabled)
                AudioStudioWrapper.PlaySound("Voice_On", GlobalAudioEmitter.GameObject);
            if (MusicEnabled)
                AudioStudioWrapper.PlaySound("Music_On", GlobalAudioEmitter.GameObject);
        }        
                
        /// <summary>
        /// Pause audio system temporarily for purposes like playing video.
        /// </summary>
        public static void PauseAudio()
        {               
            if (!AudioInitSettings.Initialized) return;
            AudioStudioWrapper.Suspend();
        }

        /// <summary>
        /// Resume audio system temporarily for conditions like video finishes.
        /// </summary>
        public static void ResumeAudio()
        {          
            if (!AudioInitSettings.Initialized) return;
            AudioStudioWrapper.WakeupFromSuspend();
        } 
        
        /// <summary>
        /// Duck main audio when player voice chat starts.
        /// </summary>
        public static void StartVoicePlay()
        {          
            if (!AudioInitSettings.Initialized) return;
            AudioStudioWrapper.PlaySound("Voice_Play_Start", GlobalAudioEmitter.GameObject);
        }
        
        /// <summary>
        /// Resume main audio when player voice chat ends.
        /// </summary>
        public static void EndVoicePlay()
        {            
            if (!AudioInitSettings.Initialized) return;
            AudioStudioWrapper.PlaySound("Voice_Play_End", GlobalAudioEmitter.GameObject);
        }
        
        /// <summary>
        /// Mute game music when player plays custom music.
        /// </summary>
        public static void StartCustomMusic()
        {            
            if (!AudioInitSettings.Initialized) return;
            AudioStudioWrapper.MuteBackgroundMusic(true);
        }
        
        /// <summary>
        /// Unmute game music when player stops custom music.
        /// </summary>
        public static void EndCustomMusic()
        {         
            if (!AudioInitSettings.Initialized) return;
            AudioStudioWrapper.MuteBackgroundMusic(false);
        }       

        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Preference
        /// <summary>
        /// Completely disable Wwise and all audio components to test performance without audio.
        /// </summary>
        public static bool DisableWwise;
        
        /// <summary>
        /// Turn on or off sound effects.
        /// </summary>
        public static bool SoundEnabled
        {
            get
            {
                return PlayerPrefs.GetInt("AUDIO_SOUND", 1) == 1;
            }
            set
            {
                if (SoundEnabled == value) return;
                var evt = value ? "Sound_On" : "Sound_Off";
                AudioStudioWrapper.PlaySound(evt, GlobalAudioEmitter.GameObject);   
                PlayerPrefs.SetInt("AUDIO_SOUND", value ? 1: 0);                
                AsUnityHelper.DebugToProfiler(Severity.Notification, 
                                              AudioObjectType.SFX, 
                                              AudioTriggerSource.Code, 
                                              value ? AudioAction.Activate : AudioAction.Deactivate, 
                                              evt);
            }
        }
        
        /// <summary>
        /// Turn on or off voice dialogs.
        /// </summary>
        public static bool VoiceEnabled
        {
            get
            {
                return PlayerPrefs.GetInt("AUDIO_VOICE", 1) == 1;
            }
            set
            {
                if (!AudioInitSettings.Initialized || VoiceEnabled == value) return;
                var evt = value ? "Voice_On" : "Voice_Off";
                AudioStudioWrapper.PlaySound(evt, GlobalAudioEmitter.GameObject);   
                PlayerPrefs.SetInt("AUDIO_VOICE", value ? 1: 0);        
                AsUnityHelper.DebugToProfiler(Severity.Notification, 
                                              AudioObjectType.Voice, 
                                              AudioTriggerSource.Code, 
                                              value ? AudioAction.Activate : AudioAction.Deactivate, 
                                              evt);
            }
        }
        
        /// <summary>
        /// Turn on or off background music.
        /// </summary>
        public static bool MusicEnabled
        {
            get
            {
                return PlayerPrefs.GetInt("AUDIO_MUSIC", 1) == 1;
            }
            set
            {
                if (!AudioInitSettings.Initialized || MusicEnabled == value) return;
                var evt = value ? "Music_On" : "Music_Off";
                AudioStudioWrapper.PlaySound(evt, GlobalAudioEmitter.GameObject);   
                PlayerPrefs.SetInt("AUDIO_MUSIC", value ? 1: 0);              
                AsUnityHelper.DebugToProfiler(Severity.Notification, 
                                              AudioObjectType.Music, 
                                              AudioTriggerSource.Code, 
                                              value ? AudioAction.Activate : AudioAction.Deactivate, 
                                              evt);
            }
        }

        /// <summary>
        /// Change sound effect bus volume.
        /// </summary>
        public static float SoundVolume
        {
            get
            {
                return PlayerPrefs.GetFloat("SOUND_VOLUME", 100f);
            }
            set
            {
                if (!AudioInitSettings.Initialized) return;
                AudioStudioWrapper.SetRTPCValue("Sound_Volume", value);
                PlayerPrefs.SetFloat("SOUND_VOLUME", value);                        
            }
        }

        /// <summary>
        /// Change voice dialog bus volume.
        /// </summary>
        public static float VoiceVolume
        {
            get
            {
                return PlayerPrefs.GetFloat("VOICE_VOLUME", 100f);
            }
            set
            {
                if (!AudioInitSettings.Initialized) return;
                AudioStudioWrapper.SetRTPCValue("Voice_Volume", value);
                PlayerPrefs.SetFloat("VOICE_VOLUME", value);                
            }
        }
        
        /// <summary>
        /// Change music bus volume.
        /// </summary>
        public static float MusicVolume
        {
            get
            {
                return PlayerPrefs.GetFloat("MUSIC_VOLUME", 100f);
            }
            set
            {         
                if (!AudioInitSettings.Initialized) return;
                AudioStudioWrapper.SetRTPCValue("Music_Volume", value);   
                PlayerPrefs.SetFloat("MUSIC_VOLUME", value);                
            }
        }

        /// <summary>
        /// Change language of voice dialog.
        /// </summary>
        public static Languages VoiceLanguage
        {
            get
            {
                var i = PlayerPrefs.GetInt("VOICE_LANGUAGE", 0);                
                return (Languages)i;
            }
            set
            {
                if (!AudioInitSettings.Initialized || VoiceLanguage == value) return;
                PlayerPrefs.SetInt("VOICE_LANGUAGE", (int)value);
                AudioStudioWrapper.SetCurrentLanguage(value.ToString());
                AsUnityHelper.DebugToProfiler(Severity.Notification, 
                                              AudioObjectType.Language, 
                                              AudioTriggerSource.Code, 
                                              AudioAction.SetValue, 
                                              value.ToString());
                BankManager.RefreshVoiceBanks();
            }
        }
        #endregion
        
        #region Callback
        //-----------------------------------------------------------------------------------------
        // post a simple Wwise callback without any information
        private static void CallbackSimple(object method, AkCallbackType in_type, AkCallbackInfo in_info)
        {
            var callback = (Action)method;
            if (callback != null) callback();              
        }
        
        // post a Wwise callback with information included
        private static void CallbackWithInfo(object method, AkCallbackType in_type, AkCallbackInfo in_info)
        {
            var callback = (Action<AkCallbackType, AkCallbackInfo>)method;
            if (callback != null) callback(in_type, in_info);              
        }
        //-----------------------------------------------------------------------------------------
        #endregion
    }
}