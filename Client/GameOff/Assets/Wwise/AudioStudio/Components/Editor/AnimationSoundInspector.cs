using UnityEngine;
using UnityEditor;
using AudioStudio.Components;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(AnimationSound)), CanEditMultipleObjects]
    public class AnimationSoundInspector : AsComponentInspector
    {
        private AnimationSound _component;

        private void OnEnable()
        {
            _component = target as AnimationSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            ShowSpatialSettings(_component);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StopOnDestroy"));
            serializedObject.ApplyModifiedProperties();
            CheckLinkedComponent();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void CheckLinkedComponent()
        {
            var animator = _component.GetComponent<Animator>();
            if (animator.ToString() == "null")
            {
                var animation = _component.GetComponent<Animation>();
                if (animation == null)
                    EditorGUILayout.HelpBox("Can't Find Animator or Animation Component!", MessageType.Error);
            }
        }
    }

}