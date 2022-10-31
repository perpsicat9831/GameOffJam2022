using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{
	[RequireComponent(typeof(AudioListener3D))]
	[DisallowMultipleComponent]
	public class SpatialAudioListener : AsSpatialHandler
	{
		internal AudioListener3D AudioListener3D;

		public override bool IsValid()
		{
			AudioListener3D = GetComponent<AudioListener3D>();
			return AudioListener3D != null;
		}

		protected override void HandleEnableEvent()
		{
			if (SpatialAudioManager.SpatialAudioListeners.Add(this))
				AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.Listener, AudioTriggerSource.SpatialAudioListener, AudioAction.Activate, "", gameObject);
			else
				AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.Listener, AudioTriggerSource.SpatialAudioListener, AudioAction.Activate, "", gameObject, "Listener Already Registered");
		}

		protected override void HandleDisableEvent()
		{
			if (SpatialAudioManager.SpatialAudioListeners.Remove(this))
				AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.Listener, AudioTriggerSource.SpatialAudioListener, AudioAction.Deactivate, "", gameObject);
			else
				AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.Listener, AudioTriggerSource.SpatialAudioListener, AudioAction.Deactivate, "", gameObject, "Listener Already Unregistered");
		}
	}
}