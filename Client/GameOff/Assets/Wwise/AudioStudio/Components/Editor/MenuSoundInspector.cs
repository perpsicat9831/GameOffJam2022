using AK.Wwise;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(MenuSound)), CanEditMultipleObjects]
    public class MenuSoundInspector : AsComponentInspector
    {
        private MenuSound _component;

        private void OnEnable()
        {
            _component = target as MenuSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            AsGuiDrawer.DrawList(serializedObject.FindProperty("OpenEvents"), "On Menu Open:", WwiseObjectType.Event, AddOpenEvent);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("CloseEvents"), "On Menu Close:", WwiseObjectType.Event, AddCloseEvent);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddOpenEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.OpenEvents, newEvent);
        }

        private void AddCloseEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.CloseEvents, newEvent);
        }
    }
}