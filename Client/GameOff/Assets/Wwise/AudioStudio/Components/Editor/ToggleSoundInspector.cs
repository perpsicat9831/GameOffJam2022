using AK.Wwise;
using UnityEditor;
using UnityEngine.UI;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(ToggleSound)), CanEditMultipleObjects]
    public class ToggleSoundInspector : AsComponentInspector
    {
        private ToggleSound _component;

        private void OnEnable()
        {
            _component = target as ToggleSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            AsGuiDrawer.DrawList(serializedObject.FindProperty("ToggleOnEvents"), "Toggle On:", WwiseObjectType.Event, AddOnEvent);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("ToggleOffEvents"), "Toggle Off:", WwiseObjectType.Event, AddOffEvent);
            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<Toggle>(_component);
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddOnEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.ToggleOnEvents, newEvent);
        }

        private void AddOffEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.ToggleOffEvents, newEvent);
        }
    }
}