using AudioStudio.Components;
using AudioStudio.Tools;
using UnityEditor;
using UnityEngine;
using System.Threading;

namespace AudioStudio.Editor
{
	public abstract class AsComponentInspector : UnityEditor.Editor
	{
		protected bool BackedUp;
		protected bool BlanEvents = false;

		[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawGizmoSphere(AudioEmitter3D component, GizmoType gizmoType)
        {
	        switch (WwisePathSettings.Instance.GizmosSphereColor)
	        {
				case GizmosColor.Disabled:
					return;
				case GizmosColor.Red:
					Gizmos.color = new Color(1, 0, 0, 0.2f);
					break;
				case GizmosColor.Yellow:
					Gizmos.color = new Color(1, 1, 0, 0.2f);
					break;
				case GizmosColor.Green:
					Gizmos.color = new Color(0, 1, 0, 0.2f);
					break;
				case GizmosColor.Blue:
					Gizmos.color = new Color(0, 0, 1, 0.2f);
					break;
	        }
	        
	        foreach (var audioEvent in component.GetEvents())
            {
	            if (!audioEvent.IsValid()) continue;
	            var radius = AsWaapiTools.GetMaxAttenuationRadius(audioEvent);
	            if (radius < 0) continue;
	            // Multi Position special case
	            var es = component as EmitterSound;
	            if (es && es.MultiPositionType != MultiPositionType.SimpleMode)
	            {
		            foreach (var position in es.MultiPositionArray)
		            {
			            Gizmos.DrawSphere(es.Position + position, radius);
		            }
	            }
	            else
		            Gizmos.DrawSphere(component.Position, radius);
            }
        }

		protected static string OnLabel(AsTriggerHandler component)
		{
			switch (component.SetOn)
			{
				case TriggerCondition.EnableDisable:
					return "On Enable:";
				case TriggerCondition.TriggerEnterExit:
					return "On Trigger Enter:";
				case TriggerCondition.CollisionEnterExit:
					return "On Collision Enter:";
				case TriggerCondition.ManuallyControl:
					return "On Activate:";
			}
			return string.Empty;
		}

		protected static string OffLabel(AsTriggerHandler component)
		{
			switch (component.SetOn)
			{
				case TriggerCondition.EnableDisable:
					return "On Disable:";
				case TriggerCondition.TriggerEnterExit:
					return "On Trigger Exit:";
				case TriggerCondition.CollisionEnterExit:
					return "On Collision Exit:";
				case TriggerCondition.ManuallyControl:
					return "On Deactivate:";
			}
			return string.Empty;
		}

		protected void CheckDataBackedUp(AsComponent component)
		{
			var assetPath = AsComponentBackup.FindComponentAssetPath(component, true);
			BackedUp =  AsComponentBackup.Instance.IsComponentBackedUp(assetPath, component);
		}

		protected void ShowSpatialSettings(AudioEmitter3D emitter)
		{
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Spatial Settings", EditorStyles.boldLabel);
			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Update Position", GUILayout.Width(95));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("IsUpdatePosition"), GUIContent.none, GUILayout.Width(15));

				if (emitter.IsUpdatePosition)
				{
					EditorGUILayout.LabelField("at frequency", GUILayout.Width(75));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateFrequency"), GUIContent.none);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.PropertyField(serializedObject.FindProperty("PositionOffset"));
					AsGuiDrawer.DrawProperty(serializedObject.FindProperty("Listeners"), "Listeners (0 is default)", 150);
				}
				else
					EditorGUILayout.EndHorizontal();

				ShowEnvironmentSettings(emitter);
			}
		}

		protected void ShowEnvironmentSettings(AudioEmitter3D emitter)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Send to AuxBus", GUILayout.Width(95));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("IsEnvironmentAware"), GUIContent.none, GUILayout.Width(15));

			if (emitter.IsEnvironmentAware)
			{
				EditorGUILayout.LabelField("in mode", GUILayout.Width(55));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("EnvironmentSource"), GUIContent.none);
				EditorGUILayout.EndHorizontal();
				if (emitter.EnvironmentSource == EnvironmentSource.GameObject)
					AsGuiDrawer.CheckLinkedComponent<Collider>(emitter);
			}
			else
				EditorGUILayout.EndHorizontal();
		}
		
		protected void ShowPhysicalSettings(AsTriggerHandler component, bool is3D)
		{
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Trigger Settings", EditorStyles.boldLabel);
			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				AsGuiDrawer.DrawProperty(serializedObject.FindProperty("SetOn"), "Activate Upon");
				switch (component.SetOn)
				{
					case TriggerCondition.TriggerEnterExit:
					case TriggerCondition.CollisionEnterExit:	
						if (is3D)
							AsGuiDrawer.DrawProperty(serializedObject.FindProperty("PostFrom"), "Emitter");
						AsGuiDrawer.DrawProperty(serializedObject.FindProperty("MatchTags"), "Audio Tags");
						break;	
				}
			}
		}

		protected virtual void ShowButtons(Object component)
		{
			EditorGUILayout.Separator();
			if (!BackedUp)
			{
				EditorGUILayout.HelpBox("Component's settings are not backedup, " +
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

		protected virtual void UpdateXml(Object obj, XmlAction action)
		{
			var component = (AsComponent) obj;
			var go = component.gameObject;
			var assetPath = AsComponentBackup.FindComponentAssetPath(component);
			var edited = false;

			switch (action)
			{
				case XmlAction.Save:
					if (!component.IsValid())
					{
						BlanEvents = true;
						break;
					}
					BlanEvents = false;
					edited = AsComponentBackup.Instance.SaveComponentDataToXML(assetPath, component);
					AsComponentBackup.Instance.SaveComponentDataToSeparatedXML(assetPath, component);
					break;
				case XmlAction.Revert:
					BlanEvents = false;
					edited = AsComponentBackup.Instance.
						SaveXMLDataToComponent(assetPath, component);
					break;
				case XmlAction.Remove:
					AsComponentBackup.Instance.
						RemoveServerXml(assetPath, component);
					AsComponentBackup.Instance.
						RemoveSeparatedXml(assetPath, component);
					System.Type comType = component.GetType();
					bool needAddRemoved = !PrefabUtility.IsAddedComponentOverride(component);
					DestroyImmediate(component, true);
					if (PrefabUtility.IsPartOfPrefabInstance(go) && needAddRemoved)
					{
						var lists = PrefabUtility.GetRemovedComponents(go);
						foreach (UnityEditor.SceneManagement.RemovedComponent com in lists)
						{
							if (com.assetComponent is AsComponent)
								if (com.assetComponent.GetType() == comType)
									AsComponentBackup.Instance.SaveRemovedComponentDataToXML(com.containingInstanceGameObject, (AsComponent)com.assetComponent);
						}
					}
					edited = true;
					break;
			}

			if (edited)
			{
				BackedUp = true;
				AsComponentBackup.SaveComponentAsset(go, assetPath);
				if (!AsComponentBackup.Instance.bCombining)
				{
					Thread ct = new Thread(AsComponentBackup.Instance.CombineThread);
					ct.IsBackground = true;
					ct.Start();
				}
			}
		}
	}
}