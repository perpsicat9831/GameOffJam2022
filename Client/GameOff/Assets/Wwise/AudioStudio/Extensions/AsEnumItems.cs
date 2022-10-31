using System;

namespace AudioStudio
{
    /// <summary>
    /// All options for AudioState component. 
    /// </summary>
    public enum AnimationAudioState
    {
        None,
        Show,
        Battle
    }

    /// <summary>
    /// All options for AudioTag component. 
    /// </summary>
    [Flags]
    public enum AudioTags
    {
        None = 0,
        AllTags = ~0,
        Camera = 0x1,
        Player = 0x2,
        Enemy = 0x4,
        Ground = 0x8,
        Water = 0x10
    }

    /// <summary>
    /// All options for voice languages.
    /// </summary>
    public enum Languages
    {
        Chinese,
        English
    }
}