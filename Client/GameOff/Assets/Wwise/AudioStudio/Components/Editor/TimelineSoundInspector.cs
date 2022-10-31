using UnityEditor;
using AudioStudio.Components;
using UnityEngine.Playables;
using UnityEngine;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(TimelineSound)), CanEditMultipleObjects]
    public class TimelineSoundInspector : AsComponentInspector
    {
        private TimelineSound _component;

        private void OnEnable()
        {
            _component = target as TimelineSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stop On Destroy", EditorStyles.boldLabel, GUILayout.Width(110));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StopOnDestroy"), GUIContent.none, GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();

            ShowSpatialSettings(_component);
            //AsGuiDrawer.DrawList(serializedObject.FindProperty("Emitters"), "Emitters", WwiseObjectType.GameObject);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Emitters"));
            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<PlayableDirector>(_component);
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }
    }

}