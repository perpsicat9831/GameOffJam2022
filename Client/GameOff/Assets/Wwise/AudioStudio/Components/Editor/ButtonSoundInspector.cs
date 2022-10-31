using AK.Wwise;
using UnityEditor;
using UnityEngine.UI;
using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(ButtonSound)), CanEditMultipleObjects]
    public class ButtonSoundInspector : AsComponentInspector
    {
        private ButtonSound _component;

        private void OnEnable()
        {
            _component = target as ButtonSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            AsGuiDrawer.DrawList(serializedObject.FindProperty("ClickEvents"), "On Click:", WwiseObjectType.Event, AddAudioEvent);
            EditorGUILayout.LabelField("Mouse Enter:", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "PointerEnterEvent");
            EditorGUILayout.LabelField("Mouse Exit:", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "PointerExitEvent");
            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<Button>(_component);
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddAudioEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.ClickEvents, newEvent);
        }
    }
}