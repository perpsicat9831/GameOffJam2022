using AK.Wwise;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(SetSwitch)), CanEditMultipleObjects]
    public class SetSwitchInspector : AsComponentInspector
    {
        private SetSwitch _component;

        private void OnEnable()
        {
            _component = target as SetSwitch;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            ShowPhysicalSettings(_component, true);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("OnSwitches"), 
                OnLabel(_component), WwiseObjectType.Switch, AddOnSwitch);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("OffSwitches"), 
                OffLabel(_component), WwiseObjectType.Switch, AddOffSwitch);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddOnSwitch(WwiseObjectReference reference)
        {
            var newState = new SwitchEx();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.OnSwitches, newState);
        }

        private void AddOffSwitch(WwiseObjectReference reference)
        {
            var newState = new SwitchEx();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.OffSwitches, newState);
        }
    }
}