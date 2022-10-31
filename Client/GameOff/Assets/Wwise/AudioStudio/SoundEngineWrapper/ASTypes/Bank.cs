
using System.Linq;
#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
using System;
using AudioStudio;
using AudioStudio.Tools;
using UnityEngine;

namespace AK.Wwise
{
	/// <summary>
	/// Extension of SoundBank with load finish post event callback, unload on disable and counter features.
	/// </summary>
	[Serializable]
	public class BankExt : AudioBank
	{
		public AudioEvent[] LoadFinishEvents = new AudioEvent[0];
		public bool UnloadOnDisable = true;
		public bool UseCounter = true;

		public override bool Equals(object obj)
		{
			var other = obj as BankExt;
			if (other != null) 
				return base.Equals(obj) && other.UnloadOnDisable == UnloadOnDisable && other.UseCounter == UseCounter && LoadFinishEvents.SequenceEqual(other.LoadFinishEvents);
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override void Load(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
		{
			if (!IsValid()) return;
			if (LoadFinishEvents.Length == 0)
				AudioManager.LoadBank(Name, null, source, trigger);
			else
			{
				AudioManager.LoadBank(Name, () =>
				{
					if (LoadFinishEvents == null) return;
					foreach (var evt in LoadFinishEvents)
					{
						evt.Post(source, trigger);	
					}
				}, source, trigger);
			}
		}
		
		public override void Unload(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
		{
			if (!IsValid()) return;
			AudioManager.UnloadBank(Name, UseCounter, null, source, trigger);
		}
	}

	[Serializable]
	public class AudioBank : Bank
	{				
		public virtual void Load(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
		{
			if (!IsValid()) return;
			AudioManager.LoadBank(Name, null, source, trigger);
		}

		public virtual void Unload(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
		{
			if (!IsValid()) return;
			AudioManager.UnloadBank(Name, true, null, source, trigger);
		}				
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.