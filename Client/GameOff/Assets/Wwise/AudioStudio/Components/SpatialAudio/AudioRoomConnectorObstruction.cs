using UnityEngine;
using System.Collections.Generic;

namespace AudioStudio.Components
{
	[AddComponentMenu("AudioStudio/Audio Room Connector Obstruction")]
	[RequireComponent(typeof(AudioRoomConnector))]
	[DisallowMultipleComponent]
	public class AudioRoomConnectorObstruction : AudioObstructionOcclusion
	{
		private AudioRoomConnector _connector;

		private void Awake()
		{
			InitIntervalsAndFadeRates();
			_connector = GetComponent<AudioRoomConnector>();
		}

		protected override void UpdateObstructionOcclusionValuesForListeners()
		{
			UpdateObstructionOcclusionValues(SpatialAudioManager.SpatialAudioListener.AudioListener3D);
		}

		protected override void SetObstructionOcclusion(KeyValuePair<AudioListener3D, ObstructionOcclusionValue> obsOccPair)
		{
			if (_connector.IsValid())
				AudioStudioWrapper.SetPortalObstructionAndOcclusion((ulong) _connector.GetInstanceID(), obsOccPair.Value.CurrentValue, 0.0f);
		}
	}
}