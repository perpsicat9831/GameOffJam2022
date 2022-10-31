using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine.UI;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(ScrollSound)), CanEditMultipleObjects]
    public class ScrollSoundInspector : AsComponentInspector
    {
        private ScrollSound _component;

        private void OnEnable()
        {
            _component = target as ScrollSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            // Costum plugin layout
            EditorGUILayout.LabelField("Scroll Down Effect: ", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "downScrollEvent");
            EditorGUILayout.LabelField("Scroll Up Effect:", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "upScrollEvent");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollDistance"));

            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<ScrollRect>(_component);
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }
    }
}