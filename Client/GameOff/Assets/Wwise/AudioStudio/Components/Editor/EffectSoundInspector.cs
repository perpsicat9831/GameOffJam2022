using AK.Wwise;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(EffectSound)), CanEditMultipleObjects]
    public class EffectSoundInspector : AsComponentInspector
    {
        private EffectSound _component;

        private void OnEnable()
        {
            _component = target as EffectSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            ShowSpatialSettings(_component);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("EnableEvents"), "On Enable:", WwiseObjectType.Event, AddEnableEvent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DelayTime"));
            AsGuiDrawer.DrawList(serializedObject.FindProperty("DisableEvents"), "On Disable:", WwiseObjectType.Event, AddDisableEvent);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddEnableEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEventExt();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.EnableEvents, newEvent);
        }

        private void AddDisableEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.DisableEvents, newEvent);
        }
    }
}