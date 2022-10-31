#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
using AudioStudio;
using AudioStudio.Tools;
using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class SwitchEx : AudioSwitch
	{
		public void SetValue(GameObject gameObject = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
		{
			if (!IsValid()) return;
			AudioManager.SetSwitch(GroupName, ChildName, gameObject, trigger);
		}
	}

	[Serializable]
	public class AudioSwitch : Switch
    {
		public string GroupName
		{
			get { return GroupWwiseObjectReference.ObjectName; }
		}

		public string ChildName
		{
			get { return ObjectReference.ObjectName; }
		}

		public override bool Equals(object obj)
		{
			var other = obj as BaseType;
			if (other != null)
				return ObjectReference == other.ObjectReference;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.