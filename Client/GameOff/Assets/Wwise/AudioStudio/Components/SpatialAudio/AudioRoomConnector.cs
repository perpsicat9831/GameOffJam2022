using UnityEngine;

namespace AudioStudio.Components
{
	[AddComponentMenu("AudioStudio/Audio Room Connector")]
	[RequireComponent(typeof(BoxCollider))]
	[DisallowMultipleComponent]
	public class AudioRoomConnector : AsTriggerHandler
	{
		private AkExtent extent;
		private AkTransform _portalTransform;

		private ulong _frontRoomId = SpatialAudioManager.INVALID_ROOM_ID;
		private ulong _backRoomId = SpatialAudioManager.INVALID_ROOM_ID;

		public AudioRoom[] Rooms = new AudioRoom[2];

		private BoxCollider _collider;

		public override bool IsValid()
		{
			_collider = GetComponent<BoxCollider>();
			if (!_collider || !_collider.isTrigger) return false;
			return _frontRoomId != _backRoomId;
		}

		protected override void Awake()
		{
			base.Awake();
			_portalTransform = new AkTransform();
			var center = _collider.bounds.center;
			_portalTransform.Set(center.x, center.y, center.z, transform.forward.x, transform.forward.y, transform.forward.z, transform.up.x, transform.up.y, transform.up.z);
			Vector3 _extent = new Vector3 (_collider.size.x * transform.localScale.x / 2, _collider.size.y * transform.localScale.y / 2, _collider.size.z * transform.localScale.z / 2);
			 extent = new AkExtent(
				Mathf.Abs(_extent.x),
				Mathf.Abs(_extent.y),
				Mathf.Abs(_extent.z));
			_frontRoomId = Rooms[1] == null ? SpatialAudioManager.INVALID_ROOM_ID : Rooms[1].GetId();
			_backRoomId = Rooms[0] == null ? SpatialAudioManager.INVALID_ROOM_ID : Rooms[0].GetId();
		}

		protected override void HandleEnableEvent()
		{
			if (SetOn != TriggerCondition.EnableDisable) return;
			ActivatePortal(true);
		}

		protected override void HandleDisableEvent()
		{
			if (SetOn != TriggerCondition.EnableDisable) return;
			ActivatePortal(false);
		}

		private void ActivatePortal(bool active)
		{
			if (!enabled)
				return;

			if (_frontRoomId != _backRoomId)
				AudioStudioWrapper.SetRoomPortal((ulong) GetInstanceID(), _frontRoomId, _backRoomId, _portalTransform, extent, active, name);
			else
				Debug.LogError(name + " is not placed/oriented correctly");
		}

		public void FindOverlappingRooms(RoomPriorityList[] roomList)
		{
			var portalCollider = gameObject.GetComponent<BoxCollider>();
			if (portalCollider == null)
				return;

			// compute halfExtents and divide the local z extent by 2
			var halfExtents = new Vector3(portalCollider.size.x * transform.localScale.x / 2,
				portalCollider.size.y * transform.localScale.y / 2, portalCollider.size.z * transform.localScale.z / 4);

			// move the center backward
			FillRoomList(Vector3.forward * -0.25f, halfExtents, roomList[0]);

			// move the center forward
			FillRoomList(Vector3.forward * 0.25f, halfExtents, roomList[1]);
		}

		private void FillRoomList(Vector3 center, Vector3 halfExtents, RoomPriorityList list)
		{
			list.Rooms.Clear();

			center = transform.TransformPoint(center);

			var colliders = Physics.OverlapBox(center, halfExtents, transform.rotation, -1, QueryTriggerInteraction.Collide);

			foreach (var col in colliders)
			{
				var room = col.gameObject.GetComponent<AudioRoom>();
				if (room != null && !list.Contains(room))
					list.Add(room);
			}
		}

		public void SetFrontRoom(AudioRoom room)
		{
			Rooms[1] = room;
			_frontRoomId = Rooms[1] == null ? SpatialAudioManager.INVALID_ROOM_ID : Rooms[1].GetId();
		}

		public void SetBackRoom(AudioRoom room)
		{
			Rooms[0] = room;
			_backRoomId = Rooms[0] == null ? SpatialAudioManager.INVALID_ROOM_ID : Rooms[0].GetId();
		}

		public void UpdateOverlappingRooms()
		{
			var roomList = new[] {new RoomPriorityList(), new RoomPriorityList()};

			FindOverlappingRooms(roomList);
			for (var i = 0; i < 2; i++)
			{
				if (!roomList[i].Contains(Rooms[i]))
					Rooms[i] = roomList[i].GetHighestPriorityRoom();
			}

			_frontRoomId = Rooms[1] == null ? SpatialAudioManager.INVALID_ROOM_ID : Rooms[1].GetId();
			_backRoomId = Rooms[0] == null ? SpatialAudioManager.INVALID_ROOM_ID : Rooms[0].GetId();
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (!enabled)
				return;
			if (!_collider)
			{
				_collider = GetComponent<BoxCollider>();
				if (!_collider) return;
			}

			Gizmos.matrix = transform.localToWorldMatrix;

			var centreOffset = _collider.center;
			var sizeMultiplier = _collider.size;

			// color faces
			var faceCenterPos = new Vector3[4];
			faceCenterPos[0] = Vector3.Scale(new Vector3(0.5f, 0.0f, 0.0f), sizeMultiplier);
			faceCenterPos[1] = Vector3.Scale(new Vector3(0.0f, 0.5f, 0.0f), sizeMultiplier);
			faceCenterPos[2] = Vector3.Scale(new Vector3(-0.5f, 0.0f, 0.0f), sizeMultiplier);
			faceCenterPos[3] = Vector3.Scale(new Vector3(0.0f, -0.5f, 0.0f), sizeMultiplier);

			var faceSize = new Vector3[4];
			faceSize[0] = new Vector3(0, 1, 1);
			faceSize[1] = new Vector3(1, 0, 1);
			faceSize[2] = faceSize[0];
			faceSize[3] = faceSize[1];

			Gizmos.color = new Color32(255, 204, 0, 100);
			for (var i = 0; i < 4; i++)
				Gizmos.DrawCube(faceCenterPos[i] + centreOffset, Vector3.Scale(faceSize[i], sizeMultiplier));

			// draw line in the center of the portal
			var cornerCenterPos = faceCenterPos;
			cornerCenterPos[0].y += 0.5f * sizeMultiplier.y;
			cornerCenterPos[1].x -= 0.5f * sizeMultiplier.x;
			cornerCenterPos[2].y -= 0.5f * sizeMultiplier.y;
			cornerCenterPos[3].x += 0.5f * sizeMultiplier.x;

			Gizmos.color = Color.red;
			for (var i = 0; i < 4; i++)
				Gizmos.DrawLine(cornerCenterPos[i] + centreOffset, cornerCenterPos[(i + 1) % 4] + centreOffset);
		}
#endif
	}
}