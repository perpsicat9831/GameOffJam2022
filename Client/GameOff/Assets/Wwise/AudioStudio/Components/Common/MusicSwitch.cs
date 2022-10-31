using AK.Wwise;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{
	/// <summary>
	/// Play a music AudioEvent or Trigger in Wwise.
	/// </summary>
	[AddComponentMenu("AudioStudio/MusicSwitch")]
	[DisallowMultipleComponent]
	public class MusicSwitch : AsTriggerHandler
	{
		public bool PlayLastMusic;
		public AudioEvent OnMusic = new AudioEvent();
		public AudioEvent OffMusic = new AudioEvent();
		public TriggerExt OnTrigger = new TriggerExt();
		public TriggerExt OffTrigger = new TriggerExt();
		
		public override void Activate(GameObject source = null)
		{
			OnMusic.Post(source, AudioTriggerSource.MusicSwitch);
			OnTrigger.Post(source, AudioTriggerSource.MusicSwitch);
		}

		public override void Deactivate(GameObject source = null)
		{
			if (PlayLastMusic)
				AudioManager.PlayLastMusic(source, AudioTriggerSource.MusicSwitch);
			else 
				OffMusic.Post(source, AudioTriggerSource.MusicSwitch);
			OffTrigger.Post(source, AudioTriggerSource.MusicSwitch);
		}

		public override bool IsValid()
		{
			return OnMusic.IsValid() || OffMusic.IsValid() || PlayLastMusic;
		}
	}
}