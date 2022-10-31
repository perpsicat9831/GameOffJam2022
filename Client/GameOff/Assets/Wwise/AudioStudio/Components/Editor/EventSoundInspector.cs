using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(EventSound)), CanEditMultipleObjects]
    public class EventSoundInspector : AsComponentInspector
    {
        private EventSound _component;

        private void OnEnable()
        {
            _component = target as EventSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            AsGuiDrawer.DrawList(serializedObject.FindProperty("UIAudioEvents"), "Audio Events");
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }
    }
    
    [CustomPropertyDrawer(typeof(UIAudioEvent))]
    public class UIAudioEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("AudioEvent"), GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Action"), GUIContent.none, GUILayout.Width(45));
            EditorGUILayout.LabelField("on", GUILayout.Width(20));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("TriggerType"), GUIContent.none, GUILayout.MinWidth(100));
        }
    }
}