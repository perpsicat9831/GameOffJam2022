using AudioStudio;
using AudioStudio.Tools;
using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
	[CustomPropertyDrawer(typeof(AudioEventExt))]
	public class AudioEventExtDrawer : AudioEventDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(position, property, label);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Stop On Disable", GUILayout.MinWidth(95));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("StopOnDisable"), GUIContent.none, GUILayout.MinWidth(15));
			EditorGUILayout.LabelField("Fade out", GUILayout.MinWidth(55));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("FadeOutTime"), GUIContent.none, GUILayout.MinWidth(25));
		}
	}
	
	[CustomPropertyDrawer(typeof(AudioEvent))]
	public class AudioEventDrawer : AudioBaseTypeDrawer
	{
		protected override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Event; } }	
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(position, property, label);
			DrawAuditionButtons();
		}

		protected void DrawAuditionButtons()
		{
			var eventName = GetComponentName(WwiseObjectReference);
			if (string.IsNullOrEmpty(eventName)) return;
			GUI.contentColor = Color.green;
			if (GUILayout.Button("▶", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
			{
				if (Application.isPlaying)
					AudioManager.PlaySound(eventName, null, AudioTriggerSource.InspectorAudition);
				else
					AsWaapiTools.StartTransportPlayback(GetObjectGuid(WwiseObjectReference)); 
			}
			GUI.contentColor = Color.red;
			if (GUILayout.Button("■", EditorStyles.miniButtonRight, GUILayout.Width(20)))
			{
				if (Application.isPlaying)
					AudioManager.StopSound(eventName, null, 0.2f, AudioTriggerSource.InspectorAudition);
				else
					AsWaapiTools.StopTransportPlayback();
			}
			GUI.contentColor = Color.white;
		}
	}
}