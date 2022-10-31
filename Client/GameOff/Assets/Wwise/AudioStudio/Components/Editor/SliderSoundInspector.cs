using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(SliderSound)), CanEditMultipleObjects]
    public class SliderSoundInspector : AsComponentInspector
    {
        private SliderSound _component;

        private void OnEnable()
        {
            _component = target as SliderSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Parameter Mapping", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ConnectedRTPC"));
                if (_component.ConnectedRTPC.IsValid())
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ValueScale"));
            }

            EditorGUILayout.LabelField("On Drag:", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "DragEvent");

            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<Slider>(_component);
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }
    }
}