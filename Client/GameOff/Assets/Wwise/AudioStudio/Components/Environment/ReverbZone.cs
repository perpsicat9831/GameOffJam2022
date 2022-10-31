using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
	/// <summary>
	/// Send emitter AudioEvents to AuxBus individually.
	/// </summary>
	[AddComponentMenu("AudioStudio/Reverb Zone")]
	[DisallowMultipleComponent]
	public class ReverbZone : AsComponent
	{
		//if excludeOthers, then only the environment with the excludeOthers flag set to true and with the highest priority will be active
		public bool ExcludeOthers;

		//if isDefault, then this environment will be bumped out if any other is present 
		public bool IsDefault;

		public AuxBusExt AuxBus = new AuxBusExt();
		public Collider Collider { get; private set; }

		//smaller number has a higher priority
		public byte Priority = 0;

		public float GetAuxSendValueForPosition(Vector3 in_position)
		{
			return AuxBus.SendAmount;
		}

		protected override void Awake()
		{
			base.Awake();
			Collider = GetComponent<Collider>();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!EmitterManager.IsEnvironmentAware(other.gameObject)) return;
			EmitterManager.EnterReverbZone(Collider, other);
			AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.AuxBus, AudioTriggerSource.ReverbZone, AudioAction.Activate, AuxBus.Name, gameObject, other.name + " Enters");
		}

		private void OnTriggerExit(Collider other)
		{
			if ( !EmitterManager.IsEnvironmentAware(other.gameObject)) return;
			EmitterManager.ExitReverbZone(Collider, other);
			AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.AuxBus, AudioTriggerSource.ReverbZone, AudioAction.Deactivate, AuxBus.Name, gameObject, other.name + " Exits");
		}

		public override bool IsValid()
		{
			return AuxBus.IsValid();
		}
	}
}