using AudioStudio.Components;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Editor
{
	[CustomEditor(typeof(AudioListener3D)), CanEditMultipleObjects]
	public class AudioListener3DInspector : AsComponentInspector
	{
		private AudioListener3D _component;

		private void OnEnable()
		{
			_component = target as AudioListener3D;
			CheckDataBackedUp(_component);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.LabelField("Rotation Settings", EditorStyles.boldLabel);
			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("FollowCamera"));
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Lock Rotation", GUILayout.Width(90));
				AsGuiDrawer.DrawProperty(serializedObject.FindProperty("LockYaw"), "Yaw", 30, 10);
				AsGuiDrawer.DrawProperty(serializedObject.FindProperty("LockPitch"), "Pitch", 30, 10);
				AsGuiDrawer.DrawProperty(serializedObject.FindProperty("LockRoll"), "Roll", 30, 10);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("PositionOffset"));
			AsGuiDrawer.DrawProperty(serializedObject.FindProperty("Index"), "Index (0 is Default)", 150);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("UseSpatialAudio"));
			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
			ShowButtons(_component);
		}
	}
}
