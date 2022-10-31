using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{	
	/// <summary>
	/// Send all 3D sounds to AuxBus based on listener position.
	/// </summary>
	[AddComponentMenu("AudioStudio/Global Aux Send")]
	[DisallowMultipleComponent]
	public class GlobalAuxSend : AsTriggerHandler
	{
		public AuxBusExt[] AuxBuses = new AuxBusExt[0];
		public override void Activate(GameObject source = null)
		{
			EmitterManager.AddAuxBuses(AuxBuses);
			foreach (var auxBus in AuxBuses)
			{
				AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.AuxBus, AudioTriggerSource.GlobalAuxSend, AudioAction.Activate, auxBus.Name, source);	
			}
		}
		
		public override void Deactivate(GameObject source = null)
		{
			EmitterManager.RemoveAuxBuses(AuxBuses);
			foreach (var auxBus in AuxBuses)
			{
				AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.AuxBus, AudioTriggerSource.GlobalAuxSend, AudioAction.Deactivate, auxBus.Name, source);	
			}
		}
		
		public override bool IsValid()
		{
			return AuxBuses.Any(s => s.IsValid());
		}
		
#if UNITY_EDITOR
		private void Reset()
		{
			if (UnityEditor.BuildPipeline.isBuildingPlayer)
				return;

			var reference = AkWwiseTypes.DragAndDropObjectReference;
			if (reference)
			{
				GUIUtility.hotControl = 0;
				AuxBuses = new []{new AuxBusExt()};
				AuxBuses[0].SetupReference(reference.ObjectName, reference.Guid);                
			}
		}
#endif
	}
}