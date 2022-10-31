using AudioStudio.Components;
using UnityEngine;
using UnityEditor;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(AudioTransformFollower)), CanEditMultipleObjects]
    public class AudioTransformFollowerInspector : AsComponentInspector
    {
        private AudioTransformFollower _component;

        private void OnEnable()
        {
            _component = target as AudioTransformFollower;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Update Self Position"))
                _component.UpdatePosition();
            AsGuiDrawer.CheckLinkedComponent<AudioEmitter3D>(_component);
        }
    }
}
