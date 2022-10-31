using System;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace AudioStudio.Tools
{
    public class AudioProfiler : EditorWindow
    {
        #region Pivate Variants
        //-----------------------------------------------------------------------------------------
        //Store the total numbers of the profiler message
        private Queue<ProfilerMessage>
        _ProfilerMessages = new Queue<ProfilerMessage>();

        // Store the types of the components to desplay
        private static readonly Dictionary<AudioTriggerSource, bool>
        _componentInclusions = new Dictionary<AudioTriggerSource, bool>();

        // Toolbar Variables
        private bool _showToggleFilter = true;
        private bool _showTextFilter = false;

        // Toggles to check if the severity level should be included inthe the log
        private bool _includeNotification = true;
        private bool _includeWarning = true;
        private bool _includeError = true;

        // Toggles to check if the trigger type should be included inthe the log
        private bool _includeComponents = true;
        private bool _includeCode = true;
        private bool _includeAudition = true;

        // Toggles to check if the component type should be included inthe the log
        private bool _includeSound = true;
        private bool _includeMusic = true;
        private bool _includeVoice = true;
        private bool _includeBank = true;
        private bool _includeSwitch = true;
        private bool _includeParameter = true;

        // The Toggle to pause the profiler
        private bool _paused = false;
        // The filter to filter the message which only has the input text
        private string _objectNameFilter;

        private bool _autoScroll = true;
        private Vector2 _scrollPosition;
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Init
        //-----------------------------------------------------------------------------------------
        private void OnEnable()
        {
            AsUnityHelper.ProfilerCallback += AddLog;
            RegisterComponents();
        }

        private void AddLog(ProfilerMessage message)
        {
            if (_paused) return;
            
            if (FilterLog(message))
            {
                _ProfilerMessages.Enqueue(message);
                if (_ProfilerMessages.Count > 200)
                    _ProfilerMessages.Dequeue();
            }
            Repaint();
        }

        private void RegisterComponents()
        {
            _componentInclusions[AudioTriggerSource.AnimationSound] = true;
            _componentInclusions[AudioTriggerSource.AudioListener3D] = true;
            _componentInclusions[AudioTriggerSource.AudioState] = true;
            _componentInclusions[AudioTriggerSource.ButtonSound] = true;
            _componentInclusions[AudioTriggerSource.ColliderSound] = true;
            _componentInclusions[AudioTriggerSource.DropdownSound] = true;
            _componentInclusions[AudioTriggerSource.EffectSound] = true;
            _componentInclusions[AudioTriggerSource.EmitterSound] = true;
            _componentInclusions[AudioTriggerSource.EventSound] = true;
            _componentInclusions[AudioTriggerSource.GlobalAuxSend] = true;
            _componentInclusions[AudioTriggerSource.LoadBank] = true;
            _componentInclusions[AudioTriggerSource.MenuSound] = true;
            _componentInclusions[AudioTriggerSource.MusicSwitch] = true;
            _componentInclusions[AudioTriggerSource.ReverbZone] = true;
            _componentInclusions[AudioTriggerSource.ScrollSound] = true;
            _componentInclusions[AudioTriggerSource.SetState] = true;
            _componentInclusions[AudioTriggerSource.SetSwitch] = true;
            _componentInclusions[AudioTriggerSource.SliderSound] = true;
            _componentInclusions[AudioTriggerSource.TimelineSound] = true;
            _componentInclusions[AudioTriggerSource.ToggleSound] = true;
            _componentInclusions[AudioTriggerSource.WwiseTimelineClip] = true;
        }

        private void OnDestroy()
        {
            AsUnityHelper.ProfilerCallback = null;
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region GUI
        //-----------------------------------------------------------------------------------------
        private static readonly Color Yellow = new Color(1f, 0.9f, 0.6f);
        private static readonly Color Skin = new Color(1f, 0.67f, 0.67f);
        private static readonly Color Orange = new Color(1f, 0.5f, 0f);
        private static readonly Color Pink = new Color(1f, 0.67f, 1f);
        private static readonly Color Rose = new Color(1f, 0.33f, 0.67f);
        private static readonly Color Aqua = new Color(0.5f, 1f, 1f);
        private static readonly Color LightGreen = new Color(0.5f, 1f, 0f);
        private static readonly Color DarkGreen = new Color(0f, 0.67f, 0.33f);
        private static readonly Color Purple = new Color(0.67f, 0.5f, 1f);
        private static readonly Color Blue = new Color(0.33f, 0.67f, 1f);

        private readonly int _severityMaxWidth = 70;
        private readonly int _timeMaxWidth = 60;
        private readonly int _typeMaxWidth = 80;
        private readonly int _actionMaxWidth = 70;
        private readonly int _nameMinWidth = 100;
        private readonly int _nameMaxWidth = 250;
        private readonly int _triggerMaxWidth = 80;
        private readonly int _componentMinWidth = 100;
        private readonly int _componentMaxWidth = 250;
        private readonly int _dividerWidth = 2;

        private void OnGUI()
        {
            // Expanding Toolbar
            GUILayout.BeginHorizontal();
            _showTextFilter = GUILayout.Toggle(_showTextFilter, "Text Filter", GUILayout.MaxWidth(100));
            _showToggleFilter = GUILayout.Toggle(_showToggleFilter, "Toggle Filter", GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (_showTextFilter)
            {
                // Text Filter
                EditorGUILayout.LabelField("Filter Name", GUILayout.Width(80));
                _objectNameFilter = EditorGUILayout.TextField(
                    _objectNameFilter,
                    GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

            if (_showToggleFilter)
            {
                // Toggle Filter
                DrawSeverityOptions();
                DrawTriggerOptions();
                DrawTypeOptions();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All", GUILayout.Height(20))) SelectAll(true);
                if (GUILayout.Button("Deselect All", GUILayout.Height(20))) SelectAll(false);
                GUILayout.EndHorizontal();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

            // Playback Toggles
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause", GUILayout.Height(20))) _paused = true;
            if (GUILayout.Button("Resume", GUILayout.Height(20))) _paused = false;
            if (GUILayout.Button("Clear", GUILayout.Height(20))) _ProfilerMessages.Clear();
            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto Scroll", GUILayout.Width(100), GUILayout.Height(25));
            GUILayout.EndHorizontal();

            // Message Display Screen
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("Severity", EditorStyles.boldLabel, GUILayout.MaxWidth(_severityMaxWidth));
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("Time", EditorStyles.boldLabel, GUILayout.MaxWidth(_timeMaxWidth));
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("Type", EditorStyles.boldLabel, GUILayout.MaxWidth(_typeMaxWidth));
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("Action", EditorStyles.boldLabel, GUILayout.MaxWidth(_actionMaxWidth));
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("Name", EditorStyles.boldLabel, GUILayout.MinWidth(_nameMinWidth), GUILayout.MaxWidth(_nameMaxWidth));
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("Trigger", EditorStyles.boldLabel, GUILayout.MaxWidth(_triggerMaxWidth));
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("GameObject", EditorStyles.boldLabel, GUILayout.MinWidth(_componentMinWidth), GUILayout.MaxWidth(_componentMaxWidth));
                EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(_dividerWidth));
                EditorGUILayout.LabelField("Message", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                _scrollPosition = EditorGUILayout.
                    BeginScrollView(_scrollPosition, GUILayout.MaxHeight(position.height));
                if (_autoScroll && Application.isPlaying && !EditorApplication.isPaused)
                    _scrollPosition.y = Mathf.Infinity;

                // Load Message 
                foreach (var message in _ProfilerMessages)
                {
                    // Go through the filter first
                    if (!FilterLog(message)) continue;

                    // Choose the severity
                    switch (message.Severity)
                    {
                        case Severity.Error:
                            GUI.color = Color.red;
                            break;
                        case Severity.Warning:
                            GUI.color = Color.yellow;
                            break;
                    }

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth - 1));
                    EditorGUILayout.LabelField(message.Severity.ToString(), GUILayout.MaxWidth(_severityMaxWidth));

                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth));
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField(message.Time, GUILayout.MaxWidth(_timeMaxWidth));

                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth));
                    DrawObject(message.ObjectType);

                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth));
                    DrawAction(message.Action);

                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth));
                    EditorGUILayout.LabelField(message.ObjectName, GUILayout.MinWidth(_nameMinWidth), GUILayout.MaxWidth(_nameMaxWidth));

                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth));
                    DrawTrigger(message.TriggerFrom);

                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth));
                    if (GUILayout.Button(message.GameObjectName, GUI.skin.label,
                        GUILayout.MinWidth(_componentMinWidth), GUILayout.MaxWidth(_componentMaxWidth)))
                        Selection.activeGameObject = message.GameObject;

                    EditorGUILayout.LabelField("", GUILayout.Width(_dividerWidth));
                    EditorGUILayout.LabelField(message.Message);
                    GUILayout.EndHorizontal();

                }

                GUILayout.EndScrollView();

            }

        }

        private void DrawSeverityOptions()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Severity:", GUILayout.MaxWidth(60));
            _includeNotification = GUILayout.Toggle(_includeNotification, "Notification", GUILayout.MaxWidth(100));
            _includeWarning = GUILayout.Toggle(_includeWarning, "Warning", GUILayout.MaxWidth(100));
            _includeError = GUILayout.Toggle(_includeError, "Error", GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
        }

        private void DrawTriggerOptions()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Trigger:", GUILayout.MaxWidth(60));
            _includeCode = GUILayout.Toggle(_includeCode, "Code", GUILayout.MaxWidth(100));
            _includeComponents = GUILayout.Toggle(_includeComponents, GUIContent.none, GUILayout.Width(10));
            if (GUILayout.Button("Components", GUI.skin.label, GUILayout.MaxWidth(86)))
                ProfilerComponentToggle.Init();
            _includeAudition = GUILayout.Toggle(_includeAudition, "Inspector Audition");
            GUILayout.EndHorizontal();
        }

        private void DrawTypeOptions()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type:", GUILayout.MaxWidth(60));
            _includeSound = GUILayout.Toggle(_includeSound, "SFX", GUILayout.MaxWidth(100));
            _includeMusic = GUILayout.Toggle(_includeMusic, "Music", GUILayout.MaxWidth(100));
            _includeVoice = GUILayout.Toggle(_includeVoice, "Voice", GUILayout.MaxWidth(100));
            _includeBank = GUILayout.Toggle(_includeBank, GUIContent.none, GUILayout.MaxWidth(10));
            if (GUILayout.Button(string.Format("{0} ({1})", "Bank", BankManager.LoadedBankList.Count),
                                 GUI.skin.label, GUILayout.MaxWidth(66)))
                BankInstanceList.Init();
            _includeSwitch = GUILayout.Toggle(_includeSwitch, "State/Switch", GUILayout.MaxWidth(100));
            _includeParameter = GUILayout.Toggle(_includeParameter, "RTPC", GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
        }

        private bool FilterLog(ProfilerMessage message)
        {
            if (!string.IsNullOrEmpty(_objectNameFilter) &&
                !message.ObjectName.ToLower().Contains(_objectNameFilter.ToLower()))
                return false;

            switch (message.Severity)
            {
                case Severity.Notification:
                    if (!_includeNotification) return false;
                    break;
                case Severity.Warning:
                    if (!_includeWarning) return false;
                    break;
                case Severity.Error:
                    if (!_includeError) return false;
                    break;
            }

            switch (message.ObjectType)
            {
                case AudioObjectType.Music:
                    if (!_includeMusic) return false;
                    break;
                case AudioObjectType.SFX:
                    if (!_includeSound) return false;
                    break;
                case AudioObjectType.Voice:
                    if (!_includeVoice) return false;
                    break;
                case AudioObjectType.SoundBank:
                    if (!_includeBank) return false;
                    break;
                case AudioObjectType.Switch:
                case AudioObjectType.State:
                    if (!_includeSwitch) return false;
                    break;
                case AudioObjectType.RTPC:
                    if (!_includeParameter) return false;
                    break;
            }

            switch (message.TriggerFrom)
            {
                case AudioTriggerSource.Code:
                    if (!_includeCode) return false;
                    break;
                case AudioTriggerSource.InspectorAudition:
                    if (!_includeAudition) return false;
                    break;
                case AudioTriggerSource.Initialization:
                    return true;
                default:
                    if (!_includeComponents || !_componentInclusions[message.TriggerFrom]) return false;
                    break;
            }
            return true;
        }

        private void DrawObject(AudioObjectType type)
        {
            switch (type)
            {
                case AudioObjectType.SFX:
                    GUI.color = Aqua;
                    break;
                case AudioObjectType.Music:
                case AudioObjectType.Trigger:
                    GUI.color = LightGreen;
                    break;
                case AudioObjectType.Voice:
                case AudioObjectType.Language:
                    GUI.color = Yellow;
                    break;
                case AudioObjectType.State:
                case AudioObjectType.AudioState:
                case AudioObjectType.Switch:
                case AudioObjectType.RTPC:
                    GUI.color = Pink;
                    break;
                case AudioObjectType.SoundBank:
                case AudioObjectType.AudioPackage:
                    GUI.color = Blue;
                    break;
                case AudioObjectType.Emitter:
                case AudioObjectType.Listener:
                case AudioObjectType.AuxBus:
                    GUI.color = Skin;
                    break;
            }
            EditorGUILayout.LabelField(type.ToString(), GUILayout.MaxWidth(_typeMaxWidth));
            GUI.color = Color.white;
        }

        private void DrawAction(AudioAction action)
        {
            switch (action)
            {
                case AudioAction.PostEvent:
                    GUI.color = Skin;
                    break;
                case AudioAction.StopEvent:
                    GUI.color = Orange;
                    break;
                case AudioAction.Load:
                case AudioAction.Reload:
                    GUI.color = Aqua;
                    break;
                case AudioAction.Unload:
                    GUI.color = Blue;
                    break;
                case AudioAction.Activate:
                case AudioAction.Register:
                    GUI.color = Pink;
                    break;
                case AudioAction.Deactivate:
                case AudioAction.Unregister:
                    GUI.color = Purple;
                    break;
                case AudioAction.Mute:
                case AudioAction.Pause:
                    GUI.color = DarkGreen;
                    break;
                case AudioAction.Unmute:
                case AudioAction.Resume:
                    GUI.color = LightGreen;
                    break;
                case AudioAction.SetValue:
                case AudioAction.GetValue:
                    GUI.color = Rose;
                    break;
            }
            EditorGUILayout.LabelField(action.ToString(), GUILayout.MaxWidth(_actionMaxWidth));
            GUI.color = Color.white;
        }

        private void DrawTrigger(AudioTriggerSource trigger)
        {
            switch (trigger)
            {
                case AudioTriggerSource.Code:
                    GUI.color = Orange;
                    break;
                case AudioTriggerSource.Initialization:
                    GUI.color = Pink;
                    break;
            }
            EditorGUILayout.LabelField(trigger.ToString(), GUILayout.MaxWidth(_triggerMaxWidth));
            GUI.color = Color.white;
        }

        private void SelectAll(bool enabled)
        {
            _includeSound = enabled;
            _includeMusic = enabled;
            _includeVoice = enabled;
            _includeBank = enabled;
            _includeSwitch = enabled;
            _includeParameter = enabled;
            _includeError = enabled;
            _includeNotification = enabled;
            _includeWarning = enabled;
            _includeComponents = enabled;
            _includeCode = enabled;
            _includeAudition = enabled;
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        /// <summary>
        /// Bank list popup
        /// </summary>
        private class BankInstanceList : EditorWindow
        {
            public static void Init()
            {
                var position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                position.y += 10;
                var window = CreateInstance<BankInstanceList>();
                window.ShowAsDropDown(new Rect(position, Vector2.zero),
                                      new Vector2(150, BankManager.LoadedBankList.Count * 20));
            }

            private void OnGUI()
            {
                foreach (var bankLoadStatus in BankManager.LoadedBankList)
                {
                    EditorGUILayout.LabelField(bankLoadStatus.Key + " @ " + bankLoadStatus.Value);
                }
            }
        }

        /// <summary>
        /// Component toggle filter popup
        /// </summary>
        private class ProfilerComponentToggle : EditorWindow
        {
            public static void Init()
            {
                var position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                position.y += 10;
                var window = CreateInstance<ProfilerComponentToggle>();
                var optionsCount = Enum.GetNames(typeof(AudioTriggerSource)).Length;
                window.ShowAsDropDown(new Rect(position, Vector2.zero),
                                      new Vector2(130, optionsCount * 15));
            }

            private void OnGUI()
            {
                var selections = new Dictionary<AudioTriggerSource, bool>(_componentInclusions);
                foreach (var component in selections)
                {
                    _componentInclusions[component.Key] =
                        GUILayout.Toggle(component.Value, component.Key.ToString());
                }
            }
        }
    }
}