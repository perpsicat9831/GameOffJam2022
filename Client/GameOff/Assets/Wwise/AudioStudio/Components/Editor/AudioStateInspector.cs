using AK.Wwise;
using UnityEngine;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(AudioState)), CanEditMultipleObjects]
    public class AudioStateInspector : AsComponentInspector
    {
        private AudioState _component;

        private void OnEnable()
        {
            _component = target as AudioState;
            CheckXmlExistence();
        }

        private void CheckXmlExistence()
        {
            var path = AssetDatabase.GetAssetPath(_component);
            var state = "OnLayer";
            var layer = AsAudioStateBackup.GetLayerStateName(_component, ref state);
            BackedUp = AsAudioStateBackup.Instance.ComponentBackedUp(path, layer, state);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Animation Audio State", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Set Audio State to", GUILayout.Width(120));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimationAudioState"), GUIContent.none);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetStateOnExit"));
            }

            AsGuiDrawer.DrawList(serializedObject.FindProperty("EnterEvents"), "Enter Events:", WwiseObjectType.Event, AddEnterEvent);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("ExitEvents"), "Exit Events:", WwiseObjectType.Event, AddExitEvent);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("EnterSwitches"), "Enter Switches:", WwiseObjectType.Switch, AddEnterSwitch);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("ExitSwitches"), "Exit Switches:", WwiseObjectType.Switch, AddExitSwitch);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("EnterStates"), "Enter States:", WwiseObjectType.State, AddEnterState);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("ExitStates"), "Exit States:", WwiseObjectType.State, AddExitState);
            serializedObject.ApplyModifiedProperties();
            ShowButtons(_component);
        }

        private void AddEnterEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEventExt();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.EnterEvents, newEvent);
        }

        private void AddExitEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.ExitEvents, newEvent);
        }

        private void AddEnterSwitch(WwiseObjectReference reference)
        {
            var newState = new SwitchEx();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.EnterSwitches, newState);
        }
        
        private void AddExitSwitch(WwiseObjectReference reference)
        {
            var newState = new SwitchEx();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.ExitSwitches, newState);
        }
        
        private void AddEnterState(WwiseObjectReference reference)
        {
            var newState = new ASState();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.EnterStates, newState);
        }
        
        private void AddExitState(WwiseObjectReference reference)
        {
            var newState = new ASState();
            newState.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.ExitStates, newState);
        }

        protected override void UpdateXml(Object obj, XmlAction action)
        {
            var edited = false;
            var component = (AudioState) obj;
            var path = AssetDatabase.GetAssetPath(component);
            
            switch (action)
            {
                case XmlAction.Remove:
                    AsAudioStateBackup.Instance.RemoveComponentXml(path, component);
                    break;
                case XmlAction.Save:
                    edited = AsAudioStateBackup.Instance.UpdateXmlFromComponent(path, component);
                    break;
                case XmlAction.Revert:
                    edited = AsAudioStateBackup.Instance.RevertComponentToXml(path, component);
                    break;
            }
            BackedUp = true;
            if (edited) 
                AssetDatabase.SaveAssets();
        }
    }
}