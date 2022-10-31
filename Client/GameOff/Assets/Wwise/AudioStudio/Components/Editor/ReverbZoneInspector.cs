using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;
using UnityEditor;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(ReverbZone)), CanEditMultipleObjects]
    public class ReverbZoneInspector : AsComponentInspector
    {
        private ReverbZone _component;

        private void OnEnable()
        {
            _component = target as ReverbZone;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Overlap Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Priority"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsDefault"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeOthers"));
            }
            
            EditorGUILayout.LabelField("Aux Bus:", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "AuxBus");
            
            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<Collider>(_component);
            ShowButtons(_component);
        }
    }
}