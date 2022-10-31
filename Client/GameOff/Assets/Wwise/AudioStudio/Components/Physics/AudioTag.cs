using UnityEngine;

namespace AudioStudio.Components
{
	/// <summary>
	/// Tag system just for any objects that affects audio by collision and trigger.
	/// </summary>
	[AddComponentMenu("AudioStudio/Audio Tag")]
	[DisallowMultipleComponent]
	public class AudioTag : AsComponent
	{
		[AkEnumFlag(typeof(AudioTags))]
		public AudioTags Tags;
	}
}