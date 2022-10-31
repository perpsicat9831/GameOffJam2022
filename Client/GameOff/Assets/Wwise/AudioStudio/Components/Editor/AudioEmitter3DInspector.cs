using UnityEditor;
using AudioStudio.Components;
using UnityEngine;

namespace AudioStudio.Editor
{
	[CustomEditor(typeof(AudioEmitter3D)), CanEditMultipleObjects]
	public class AudioEmitter3DInspector : AsComponentInspector
	{
		private AudioEmitter3D _component;

		private void OnEnable()
		{
			_component = target as AudioEmitter3D;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			ShowSpatialSettings(_component);
			serializedObject.ApplyModifiedProperties();
		}

		private void OnSceneGUI()
		{
			if (_component.PositionOffset == Vector3.zero) return;
			var pos = _component.transform.TransformPoint(_component.PositionOffset);
			pos = Handles.PositionHandle(pos, Quaternion.identity);
		}
	}
}