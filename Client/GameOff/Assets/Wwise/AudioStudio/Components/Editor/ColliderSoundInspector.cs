using AK.Wwise;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Editor
{
	[CustomEditor(typeof(ColliderSound)), CanEditMultipleObjects]
	public class ColliderSoundInspector : AsComponentInspector
	{
		private ColliderSound _component;

		private void OnEnable()
		{
			_component = target as ColliderSound;
			CheckDataBackedUp(_component);
			
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			ShowSpatialSettings(_component);

			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("PostFrom"), new GUIContent("Emitter"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("MatchTags"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionForceRTPC"));
				if (_component.CollisionForceRTPC.IsValid())
					EditorGUILayout.PropertyField(serializedObject.FindProperty("ValueScale"));
			}

			AsGuiDrawer.DrawList(serializedObject.FindProperty("EnterEvents"), "On Collision/Trigger Enter:", WwiseObjectType.Event, AddEnterEvent);
			AsGuiDrawer.DrawList(serializedObject.FindProperty("ExitEvents"), "On Collision/Trigger Exit:", WwiseObjectType.Event, AddExitEvent);
			serializedObject.ApplyModifiedProperties();
			AsGuiDrawer.CheckLinkedComponent<Collider>(_component);
			if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
			ShowButtons(_component);
		}

		private void AddEnterEvent(WwiseObjectReference reference)
		{
			var newEvent = new AudioEvent();
			newEvent.SetupReference(reference.ObjectName, reference.Guid);
			AsScriptingHelper.AddToArray(ref _component.EnterEvents, newEvent);
		}

		private void AddExitEvent(WwiseObjectReference reference)
		{
			var newEvent = new AudioEvent();
			newEvent.SetupReference(reference.ObjectName, reference.Guid);
			AsScriptingHelper.AddToArray(ref _component.ExitEvents, newEvent);
		}
	}
}