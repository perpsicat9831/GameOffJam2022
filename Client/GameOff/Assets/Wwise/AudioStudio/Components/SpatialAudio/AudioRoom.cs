using AK.Wwise;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{
	[AddComponentMenu("AudioStudio/Audio Room")]
	[RequireComponent(typeof(Collider))]
	[DisallowMultipleComponent]
	public class AudioRoom : AsTriggerHandler
	{
		[Tooltip("Higher number has a higher priority")]
		// In cases where a game object is in an area with two rooms, the higher priority room will be chosen for AK::SpatialAudio::SetGameObjectInRoom()
		// The higher the priority number, the higher the priority of a room.
		public int Priority;
		public AuxBusExt RoomReverb;

		[Range(0, 1)]
		// Occlusion level modeling transmission through walls.
		public float WallOcclusion = 1;

		/// Wwise Event to be posted on the room game object.
		public AudioEvent RoomToneEvent;

		[Range(0, 1)] [Tooltip("Send level for sounds that are posted on the room game object; adds reverb to ambience and room tones. Valid range: (0.f-1.f). A value of 0 disables the aux send.")]
		public float RoomToneAuxSend;
		
		public ulong GetId()
		{
			return AudioStudioWrapper.GetAkGameObjectID(gameObject);
		}

		protected override void HandleEnableEvent()
		{
			ulong geometryID = 0;

			var roomParams = new AkRoomParams
			{
				Up = new Vector3(transform.up.x, transform.up.y, transform.up.z),
				Front = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z),
				ReverbAuxBus = RoomReverb.Id,
				ReverbLevel = RoomReverb.SendAmount,
				RoomGameObj_AuxSendLevelToSelf = RoomToneAuxSend,
				RoomGameObj_KeepRegistered = RoomToneEvent.IsValid()
			};
			SpatialAudioManager.AddRoom(roomParams, geometryID, gameObject);
		}

		protected override void HandleDisableEvent()
		{
			SpatialAudioManager.RemoveRoom(gameObject);
		}

		private void OnTriggerEnter(Collider other)
		{
			var components = other.GetComponentsInChildren<AsSpatialHandler>();
			foreach (var obj in components)
			{
				if (obj.enabled)
					obj.EnterRoom(this);
			}
			if (SetOn == TriggerCondition.TriggerEnterExit)
				Activate(gameObject);
		}

		private void OnTriggerExit(Collider other)
		{
			foreach (var obj in other.GetComponentsInChildren<AsSpatialHandler>())
			{
				if (obj.enabled)
					obj.ExitRoom(this);
			}
			if (SetOn == TriggerCondition.TriggerEnterExit)
				Deactivate(gameObject);
		}

		public override void Activate(GameObject source = null)
		{
			RoomToneEvent.Post(source, AudioTriggerSource.AudioRoom);
		}

		public override void Deactivate(GameObject source = null)
		{
			RoomToneEvent.Stop(source, 0.5f, AudioTriggerSource.AudioRoom);
		}

	}
}