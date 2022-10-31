using AudioStudio.Components;
using UnityEditor;

namespace AudioStudio.Editor
{
	[CustomEditor(typeof(AudioTag)), CanEditMultipleObjects]
	public class AudioTagInspector : AsComponentInspector
	{

		private AudioTag _component;

		private void OnEnable()
		{
			_component = target as AudioTag;
			CheckDataBackedUp(_component);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Tags"));
			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
			ShowButtons(_component);
		}
	}
}