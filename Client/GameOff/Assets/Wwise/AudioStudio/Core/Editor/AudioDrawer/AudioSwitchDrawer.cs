using AudioStudio;
using AudioStudio.Tools;
using System;
using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
    [CustomPropertyDrawer(typeof(SwitchEx))]
    public class SwitchExDrawer : AudioSwitchDrawer
    {
        
    }

    [CustomPropertyDrawer(typeof(AudioSwitch))]
    public class AudioSwitchDrawer : AudioBaseTypeDrawer
    {
        protected override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Switch; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);
            if (Application.isPlaying)
                DrawAuditionButtons();
            EditorGUI.EndProperty();
        }

        protected void DrawAuditionButtons()
        {
            var switchName = GetObjectName(this.WwiseObjectReference);
            if (!Application.isPlaying || string.IsNullOrEmpty(switchName)) return;
            GUI.contentColor = Color.green;
            if (GUILayout.Button("?", EditorStyles.miniButton, GUILayout.Width(20)))
                AudioManager.SetSwitch(switchName, null);
            GUI.contentColor = Color.white;
        }
    }
}