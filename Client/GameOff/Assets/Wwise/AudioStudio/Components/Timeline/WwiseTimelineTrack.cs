using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AudioStudio.Timeline
{
    [TrackColor(1f, 0.5f, 0f)]
    [TrackClipType(typeof(WwiseTimelineClip))]
    public class WwiseTimelineTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var clip in GetClips())
            {
                var wwiseClip = clip.asset as WwiseTimelineClip;
                if (!wwiseClip) continue;
                wwiseClip.EndTime = clip.end;
                wwiseClip.StartTime = clip.start;
#if UNITY_EDITOR
                clip.displayName = wwiseClip.AutoRename();
#endif
            }
            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}