using AudioStudio.Components;
using UnityEditor;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(SurfaceReflector)), CanEditMultipleObjects]
    public class SurfaceReflectorInspector : AsComponentInspector
    {
        private SurfaceReflector _component;

        private void OnEnable()
        {
            _component = target as SurfaceReflector;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Acoustic Texture:", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "AcousticTexture");
            EditorGUILayout.BeginHorizontal();
            AsGuiDrawer.DrawProperty(serializedObject.FindProperty("EnableDiffraction"), "Diffraction", 80, 20);
            if (_component.EnableDiffraction)
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("EnableDiffractionOnBoundaryEdges"), "On Boundary Edges", 120);
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            ShowButtons(_component);
        }
    }
}