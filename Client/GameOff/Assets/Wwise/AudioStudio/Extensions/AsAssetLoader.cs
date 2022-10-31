using UnityEngine;

namespace AudioStudio
{
    /// <summary>
    /// Class for loading AudioStudio config files, different for each game project.
    /// </summary>
    public static class AsAssetLoader
    {
        public static void LoadAudioInitData()
        {
            var config = Resources.Load<AudioInitLoadData>("AudioStudio/AudioInitLoadData");
            if (config)
                config.LoadAudioData();
        }
    }
}