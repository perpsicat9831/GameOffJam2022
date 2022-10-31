using AK.Wwise;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(SetState)), CanEditMultipleObjects]
    public class SetStateInspector : AsComponentInspector
    {
        private SetState _component;

        private void OnEnable()
        {
            _component = target as SetState;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            ShowPhysicalSettings(_component, false);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("OnStates"), OnLabel(_component), WwiseObjectType.State, AddOnState);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("OffStates"), OffLabel(_component), WwiseObjectType.State, AddOffState);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddOnState(WwiseObjectReference reference)
        {
            var newState = new StateExt();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.OnStates, newState);
        }

        private void AddOffState(WwiseObjectReference reference)
        {
            var newState = new ASState();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.OffStates, newState);
        }
    }
}