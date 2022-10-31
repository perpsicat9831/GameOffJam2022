using AK.Wwise;
using AudioStudio.Timeline;
using UnityEditor;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Editor
{
	[CustomEditor(typeof(WwiseTimelineClip)), CanEditMultipleObjects]
	public class WwiseTimelineClipInspector : AsComponentInspector
	{
		private WwiseTimelineClip _component;

		private void OnEnable()
		{
			_component = target as WwiseTimelineClip;
			CheckDataBackedUp();
		}
		
		private void CheckDataBackedUp()
		{
			var path = AssetDatabase.GetAssetPath(_component);
			var clip = AsTimelineAudioBackup.GetClipFromComponent(path, _component);
			BackedUp = AsTimelineAudioBackup.Instance.ComponentBackedUp(path, clip);
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.indentLevel = 0;
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			DrawEmitter();
			AsGuiDrawer.DrawList(serializedObject.FindProperty("StartEvents"), "Start Events:", WwiseObjectType.Event, AddStartEvent);
			AsGuiDrawer.DrawList(serializedObject.FindProperty("EndEvents"), "End Events:", WwiseObjectType.Event, AddEndEvent);
			EditorGUILayout.Separator();
			AsGuiDrawer.DrawList(serializedObject.FindProperty("StartStates"), "Start States:", WwiseObjectType.State, AddStartState);
			AsGuiDrawer.DrawList(serializedObject.FindProperty("EndStates"), "End States:", WwiseObjectType.State, AddEndState);

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck()) CheckDataBackedUp();
			ShowButtons(_component);
		}

		private void DrawEmitter()
		{
			var names = _component.GetEmitterNames();
			_component.EmitterIndex = EditorGUILayout.Popup("Emitter", _component.EmitterIndex, names);
		}

		private void AddStartEvent(WwiseObjectReference reference)
		{
			var newEvent = new AudioEventExt();
			newEvent.SetupReference(reference.ObjectName, reference.Guid);
			AsScriptingHelper.AddToArray(ref _component.StartEvents, newEvent);
		}

		private void AddEndEvent(WwiseObjectReference reference)
		{
			var newEvent = new AudioEvent();
			newEvent.SetupReference(reference.ObjectName, reference.Guid);
			AsScriptingHelper.AddToArray(ref _component.EndEvents, newEvent);
		}

		private void AddStartState(WwiseObjectReference reference)
		{
			var newState = new StateExt();
			newState.SetupReference(reference.ObjectName, reference.Guid);
			AsScriptingHelper.AddToArray(ref _component.StartStates, newState);
		}

		private void AddEndState(WwiseObjectReference reference)
		{
			var newState = new ASState();
			newState.SetupReference(reference.ObjectName, reference.Guid);
			AsScriptingHelper.AddToArray(ref _component.EndStates, newState);
		}

		protected override void ShowButtons(Object component)
		{
			EditorGUILayout.Separator();
			if (!BackedUp)
			{
				EditorGUILayout.HelpBox("Component's settings modified, " +
					"Please save your data or revert to the previous version",
					MessageType.Warning);
			}
			if (BlanEvents)
			{
				EditorGUILayout.HelpBox("Components with blank events are forbidden to saved, " +
					"please assign events to your component",
					MessageType.Error);
			}

			EditorGUILayout.BeginHorizontal();
			GUI.contentColor = Color.yellow;
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
			{
				var dataLoadPath = "Assets/" + System.IO.Path.
				Combine(WwisePathSettings.EditorConfigPath, "AkWwiseProjectData.asset");
				AssetDatabase.DeleteAsset(dataLoadPath);
				AkWwiseProjectInfo.Populate();
			}

			EditorGUI.BeginDisabledGroup(BackedUp);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
				UpdateXml(component, XmlAction.Save);

			GUI.contentColor = Color.magenta;
			if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
				UpdateXml(component, XmlAction.Revert);
			EditorGUI.EndDisabledGroup();

			GUI.contentColor = Color.red;
			if (GUILayout.Button("Remove", EditorStyles.toolbarButton))
				UpdateXml(component, XmlAction.Remove);

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
		}

		protected override void UpdateXml(Object obj, XmlAction action)
		{
			var edited = false;
			var component = (WwiseTimelineClip) obj;
			var assetPath = AssetDatabase.GetAssetPath(obj);
			var clip = AsTimelineAudioBackup.GetClipFromComponent(assetPath, component);
			switch (action)
			{
				case XmlAction.Remove:
					AsTimelineAudioBackup.Instance.RemoveComponentXml(assetPath, clip);
					DestroyImmediate(component, true);
					break;
				case XmlAction.Save:
					if (!component.IsValid())
					{
						BlanEvents = true;
						break;
					}
					BlanEvents = false;
					clip.displayName = _component.AutoRename();
					edited = AsTimelineAudioBackup.Instance.SaveLocalDataToServer(assetPath, clip, component);
					break;
				case XmlAction.Revert:
					edited = AsTimelineAudioBackup.Instance.RevertXmlDataToComponent(assetPath, clip, component);
					break;
			}
			BackedUp = true;
			if (edited) 
				AssetDatabase.SaveAssets();
		}
	}
}