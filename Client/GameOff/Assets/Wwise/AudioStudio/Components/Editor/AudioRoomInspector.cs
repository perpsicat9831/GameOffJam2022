using AudioStudio.Components;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(AudioRoom))]
    public class AudioRoomInspector : AsComponentInspector
    {
        private AudioRoom _component;
        private SerializedProperty priority;

        private SerializedProperty reverbAuxBus;
        private SerializedProperty reverbLevel;
        private SerializedProperty wallOcclusion;
        private SerializedProperty roomToneEvent;
        private SerializedProperty roomToneAuxSend;

        private void OnEnable()
        {
            _component = target as AudioRoom;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Room Reverb:", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "RoomReverb");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WallOcclusion"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Priority"));
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField("Room Tone Event (On Trigger Enter):", EditorStyles.boldLabel);
            AsGuiDrawer.DrawWwiseObject(serializedObject, "RoomToneEvent");
            
            serializedObject.ApplyModifiedProperties();
            AsGuiDrawer.CheckLinkedComponent<Rigidbody>(_component);
            ShowButtons(_component);
        }
    }
}