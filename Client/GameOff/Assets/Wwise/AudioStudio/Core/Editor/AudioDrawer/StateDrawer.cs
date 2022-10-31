using AudioStudio;
using AudioStudio.Tools;
using System;
using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
    [CustomPropertyDrawer(typeof(StateExt))]
    public class StateExtDrawer : ASStateDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            DrawAuditionButtons();
            EditorGUILayout.EndHorizontal();
			
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("ResetOnDisable"));
        }
    }
	
    [CustomPropertyDrawer(typeof(ASState))]
    public class ASStateDrawer : AudioBaseTypeDrawer
    {
        protected override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.State; } }
		
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
            var stateName = GetObjectName(this.WwiseObjectReference);
            if (!Application.isPlaying || string.IsNullOrEmpty(stateName)) return;
            GUI.contentColor = Color.green;
            if (GUILayout.Button("▶", EditorStyles.miniButton, GUILayout.Width(20)))
                AudioManager.SetState(stateName, null, AudioTriggerSource.InspectorAudition);
            GUI.contentColor = Color.white;
        }
    }
}