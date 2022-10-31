using UnityEngine;
using System.Collections.Generic;

namespace AudioStudio.Components
{
	[AddComponentMenu("AudioStudio/Emitter Obstruction Occlusion")]
	[RequireComponent(typeof(AudioEmitter3D))]
	[DisallowMultipleComponent]
	public class EmitterObstructionOcclusion : AudioObstructionOcclusion
	{
		private AudioEmitter3D _emitter;

		private void Awake()
		{
			_emitter = GetComponent<AudioEmitter3D>();
			InitIntervalsAndFadeRates();
		}

		protected override void UpdateObstructionOcclusionValuesForListeners()
		{
			if (SpatialAudioManager.IsSpatialAudioActive)
				UpdateObstructionOcclusionValues(SpatialAudioManager.SpatialAudioListener.AudioListener3D);
			else
				UpdateObstructionOcclusionValues(ListenerManager.GetListenersFromString(_emitter.Listeners));
		}

		protected override void SetObstructionOcclusion(KeyValuePair<AudioListener3D, ObstructionOcclusionValue> obsOccPair)
		{
			if (SpatialAudioManager.IsSpatialAudioActive)
				ListenerManager.SetObstructionOcclusion(gameObject, _emitter.Listeners, obsOccPair.Value.CurrentValue, 0.0f);
			else
				ListenerManager.SetObstructionOcclusion(gameObject, _emitter.Listeners, 0.0f, obsOccPair.Value.CurrentValue);
		}
	}
}