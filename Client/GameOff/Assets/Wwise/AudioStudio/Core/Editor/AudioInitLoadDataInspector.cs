using AK.Wwise;
using UnityEngine;
using UnityEditor;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(AudioInitLoadData))]
    public class AudioInitLoadDataInspector : UnityEditor.Editor
    {
        private AudioInitLoadData _component;

        private void OnEnable()
        {
            _component = target as AudioInitLoadData;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (AudioInitSettings.Instance.PackageMode)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadPackages"));
                if (_component.LoadPackages)
                    //AsGuiDrawer.DrawList(serializedObject.FindProperty("AudioPackages"), "", WwiseObjectType.Package);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AudioPackages"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadBanks"));
            if (_component.LoadBanks)
                AsGuiDrawer.DrawList(serializedObject.FindProperty("Banks"), "Global Banks", WwiseObjectType.Soundbank, AddSoundBank);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PostEvents"));
            if (_component.PostEvents)
                AsGuiDrawer.DrawList(serializedObject.FindProperty("AudioEvents"), "", WwiseObjectType.Event, AddAudioEvent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SetStates"));
            if (_component.SetStates)
                AsGuiDrawer.DrawList(serializedObject.FindProperty("States"), "", WwiseObjectType.State, AddSetState);

            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = Color.yellow;
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                AkWwiseProjectInfo.Populate();
            AsGuiDrawer.DrawSaveButton(_component);
            EditorGUILayout.EndHorizontal();
        }

        private void AddAudioEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.AudioEvents, newEvent);
        }

        private void AddSoundBank(WwiseObjectReference reference)
        {
            var newBank = new AudioBank();
            newBank.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.Banks, newBank);
        }

        private void AddSetState(WwiseObjectReference reference)
        {
            var newState = new ASState();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.States, newState);
        }
    }
}