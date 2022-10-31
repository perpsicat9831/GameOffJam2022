using UnityEngine;
using UnityEditor;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(AudioInitSettings))]
    public class AudioInitSettingsInspector : UnityEditor.Editor
    {
        private AudioInitSettings _component;

        private void OnEnable()
        {
            _component = target as AudioInitSettings;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DisableWwise"));
            if (!_component.DisableWwise)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WwiseInitializationSettings"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoInitialize"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DebugLogLevel"));
                DrawListenerSettings();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseMicrophone"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PackageMode"));
            }
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = Color.yellow;
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                AkWwiseProjectInfo.Populate();
            AsGuiDrawer.DrawSaveButton(_component);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawListenerSettings()
        {
            EditorGUILayout.LabelField("Listener Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableSpatialAudio"));
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("AddListenerToMainCamera"), "", 220);
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("AlternativeListenersCount"), "", 220);
            }
        }
    }
}