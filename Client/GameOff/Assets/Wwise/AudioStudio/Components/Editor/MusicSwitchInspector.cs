using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(MusicSwitch)), CanEditMultipleObjects]
    public class MusicSwitchInspector : AsComponentInspector
    {
        private MusicSwitch _component;

        private void OnEnable()
        {
            _component = target as MusicSwitch;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            ShowPhysicalSettings(_component, false);
            EditorGUILayout.LabelField(OnLabel(_component), EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "OnMusic");
            AsGuiDrawer.DrawWwiseObject(serializedObject, "OnTrigger");
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(OffLabel(_component), EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayLastMusic"), GUIContent.none, GUILayout.Width(15));
            EditorGUILayout.LabelField("Play Last Music");
            EditorGUILayout.EndHorizontal();

            if (!_component.PlayLastMusic)
                AsGuiDrawer.DrawWwiseObject(serializedObject, "OffMusic");
            AsGuiDrawer.DrawWwiseObject(serializedObject, "OffTrigger");

            
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }
    }
}