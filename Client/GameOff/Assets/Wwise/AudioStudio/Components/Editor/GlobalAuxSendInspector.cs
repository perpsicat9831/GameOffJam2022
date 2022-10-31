using AK.Wwise;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
	[CustomEditor(typeof(GlobalAuxSend)), CanEditMultipleObjects]
	public class GlobalAuxSendInspector : AsComponentInspector
	{
		private GlobalAuxSend _component;

		private void OnEnable()
		{
			_component = target as GlobalAuxSend;
			CheckDataBackedUp(_component);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			ShowPhysicalSettings(_component, false);
			AsGuiDrawer.DrawList(serializedObject.FindProperty("AuxBuses"), "Aux Buses:", WwiseObjectType.Bus, AddAuxBus);
			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
			ShowButtons(_component);
		}

		private void AddAuxBus(WwiseObjectReference reference)
		{
			var newEvent = new AuxBusExt();
			newEvent.SetupReference(reference.ObjectName, reference.Guid);
			AsScriptingHelper.AddToArray(ref _component.AuxBuses, newEvent);
		}
	}
}