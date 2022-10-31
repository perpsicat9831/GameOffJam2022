#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
using System;
using AudioStudio;
using AudioStudio.Tools;
using UnityEngine;

namespace AK.Wwise
{		
	public enum AudioEventAction
	{
		Play,
		Stop
	}
	
	/// <summary>
	/// Extension of AudioEvent with stop on disable and fade out feature
	/// </summary>
	[Serializable]
	public class AudioEventExt : AudioEvent
	{		
		public bool StopOnDisable;
		public float FadeOutTime = 0.2f;

		public override bool Equals(object obj)
		{
			var other = obj as AudioEventExt;
			if (other != null)
				return base.Equals(obj) && StopOnDisable == other.StopOnDisable && FadeOutTime == other.FadeOutTime;
			return false;
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	[Serializable]
	public class AudioEvent : Event
	{
		// call different method based on type of event
		public void Post(GameObject soundSource = null, AudioTriggerSource trigger = AudioTriggerSource.Code, bool playerControl = false)
		{
			if (!IsValid()) return;    
			if (Name.StartsWith("Music_"))
				AudioManager.PlayMusic(Name, soundSource, trigger);
			else if (Name.StartsWith("Vo_"))
				AudioManager.PlayVoice(Name, soundSource, trigger);
			else
				AudioManager.PlaySound(playerControl ? Name + "_PC" : Name, soundSource, trigger);
		}

		public void Stop(GameObject soundSource = null, float fadeOutTime = 0.2f, AudioTriggerSource trigger = AudioTriggerSource.Code, bool playerControl = false)
		{
			if (!IsValid()) return;
			if (Name.StartsWith("Music_"))
				return;
			if (Name.StartsWith("Vo_"))
				AudioManager.StopVoice(Name, fadeOutTime, soundSource, trigger);
			else
				AudioManager.StopSound(playerControl ? Name + "_PC" : Name, soundSource, fadeOutTime, trigger); 
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