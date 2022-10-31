using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{
	/// <summary>
	/// Add a 3D listener to Wwise with ability to lock any rotation.
	/// </summary>
	[AddComponentMenu("AudioStudio/Audio Listener 3D")]
	[DisallowMultipleComponent]
	public class AudioListener3D : AsComponent
	{
		public byte Index;
		public bool FollowCamera = true;
		public bool LockYaw;
		public bool LockRoll;
		public bool LockPitch;
		public Vector3 PositionOffset = Vector3.zero;
		public bool UseSpatialAudio;

		private Transform _transform;
		private Vector3 _originalRotation;

		protected override void Awake()
		{
			base.Awake();
			ResetCameraTransform();
		}

		/// <summary>
		///  get the original forward and up direction
		/// </summary>
		public void ResetCameraTransform()
		{
			if (FollowCamera)
			{
				var camera = GetComponent<Camera>();
				if (!camera) camera = Camera.main;
				if (camera != null)
					_transform = camera.transform;
				else
					_transform = transform;
			}
			else
				_transform = transform;
			UpdateDefaultRotation();
		}

		/// <summary>
		/// Set the new default rotation if locking any axis.
		/// </summary>
		public void UpdateDefaultRotation()
		{
			_originalRotation = _transform.eulerAngles;
		}

		protected override void HandleEnableEvent()
		{
			ListenerManager.AssignAudioListener(this);
			if (UseSpatialAudio)
				AsUnityHelper.GetOrAddComponent<SpatialAudioListener>(gameObject);
			AsUnityHelper.DebugToProfiler(Severity.Notification, 
										AudioObjectType.Listener, 
										AudioTriggerSource.AudioListener3D, 
										AudioAction.Activate, 
										"Add 3D Listener", 
										gameObject, 
										"Index is " + Index);
		}

		protected override void HandleDisableEvent()
		{
			ListenerManager.RemoveAudioListener(this);
			AsUnityHelper.DebugToProfiler(Severity.Notification, 
										AudioObjectType.Listener, 
										AudioTriggerSource.AudioListener3D, 
										AudioAction.Deactivate, 
										"Remove 3D Listener", 
										gameObject, 
										"Index is " + Index);
		}

		public Vector3 GetForward()
		{
			if (!_transform) return new Vector3(0, 0, 0);
			var forward = _transform.forward;
			var deltaRotation = _transform.eulerAngles - _originalRotation;
			// rotate the forward vector back by same amount among locked axis
			if (LockYaw)
				forward = Quaternion.AngleAxis(-deltaRotation.y, Vector3.up) * forward;
			if (LockPitch)
				forward = Quaternion.AngleAxis(-deltaRotation.x, Vector3.right) * forward;
			if (LockRoll)
				forward = Quaternion.AngleAxis(-deltaRotation.z, Vector3.forward) * forward;
			return forward;
		}

		public Vector3 GetUp()
		{
			if (!_transform) return new Vector3(0, 0, 0);
			var up = _transform.up;
			var deltaRotation = _transform.eulerAngles - _originalRotation;
			if (LockYaw)
				up = Quaternion.AngleAxis(-deltaRotation.y, Vector3.up) * up;
			if (LockPitch)
				up = Quaternion.AngleAxis(-deltaRotation.x, Vector3.right) * up;
			if (LockRoll)
				up = Quaternion.AngleAxis(-deltaRotation.z, Vector3.forward) * up;
			return up;
		}
		
		public Vector3 Position
		{
			get
			{
				if (PositionOffset == Vector3.zero)
					return transform.position;
				return transform.position + transform.rotation * PositionOffset;
			}
		}
		
		// draw an icon at listener position
		private void OnDrawGizmos()
		{
			Gizmos.DrawIcon(Position, "AudioStudio/WwiseAudioListener.png", WwisePathSettings.Instance.GizmosIconScaling);
		}
	}
}