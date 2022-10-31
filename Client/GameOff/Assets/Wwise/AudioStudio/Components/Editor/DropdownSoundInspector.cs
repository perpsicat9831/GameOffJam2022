using AK.Wwise;
using UnityEditor;
using UnityEngine.UI;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(DropdownSound)), CanEditMultipleObjects]
    public class DropdownSoundInspector : AsComponentInspector
    {
        private DropdownSound _component;

        private void OnEnable()
        {
            _component = target as DropdownSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            AsGuiDrawer.DrawList(serializedObject.FindProperty("ValueChangeEvents"), "On Select:", WwiseObjectType.Event, AddValueChangeEvent);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("PopupEvents"), "On Expand:", WwiseObjectType.Event, AddPopupEvent);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("CloseEvents"), "On Fold:", WwiseObjectType.Event, AddCloseEvent);
            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<Dropdown>(_component);
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddCloseEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.CloseEvents, newEvent);
        }

        private void AddValueChangeEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.ValueChangeEvents, newEvent);
        }

        private void AddPopupEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.PopupEvents, newEvent);
        }
    }
}