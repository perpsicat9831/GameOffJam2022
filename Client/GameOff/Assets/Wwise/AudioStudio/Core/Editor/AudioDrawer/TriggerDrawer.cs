using AudioStudio;
using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
	[CustomPropertyDrawer(typeof(Trigger))]
	public class AudioTriggerDrawer : AudioBaseTypeDrawer
	{
		protected override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Trigger; } }
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(position, property, label);
			EditorGUI.BeginProperty(position, label, property);
			if (Application.isPlaying)
				DrawAuditionButtons();
			EditorGUI.EndProperty();
		}

		private void DrawAuditionButtons()
		{
			var triggerName = GetObjectName(WwiseObjectReference);
			if (!Application.isPlaying || string.IsNullOrEmpty(triggerName)) return;
			GUI.contentColor = Color.green;
			if (GUILayout.Button("▶", EditorStyles.miniButton, GUILayout.Width(20)))
				AudioManager.PostTrigger(triggerName);
			GUI.contentColor = Color.white;	
		}
	}
}
