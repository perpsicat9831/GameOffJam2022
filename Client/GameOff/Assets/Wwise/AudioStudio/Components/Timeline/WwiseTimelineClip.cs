using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using AK.Wwise;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Timeline
{
    [Serializable]
    public class WwiseTimelineClip : PlayableAsset
    {
        public AudioEventExt[] StartEvents = new AudioEventExt[0];
        public AudioEvent[] EndEvents = new AudioEvent[0];
        public StateExt[] StartStates = new StateExt[0];
        public ASState[] EndStates = new ASState[0];
        public int EmitterIndex;
        [NonSerialized]
        public double StartTime;
        public double EndTime;

        private TimelineSound _timelineSound;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<WwiseTimelineComponent>.Create(graph);
            _timelineSound = AsUnityHelper.GetOrAddComponent<TimelineSound>(go);
            if (EmitterIndex > 0 && _timelineSound.Emitters.Length >= EmitterIndex)
                playable.GetBehaviour().Init(this, _timelineSound.Emitters[EmitterIndex - 1], go);
            else
                playable.GetBehaviour().Init(this, null, go);
            return playable;
        }
        
        public bool IsValid()
        {
            return StartEvents.Any(s => s.IsValid()) || EndEvents.Any(s => s.IsValid()) || StartStates.Any(s => s.IsValid()) || EndStates.Any(s => s.IsValid());
        }

#if UNITY_EDITOR
        public string[] GetEmitterNames()
        {
            var names = new List<string>{"Timeline"};
            if (_timelineSound)
                names.AddRange(_timelineSound.Emitters.Select(emitter => emitter ? emitter.name : "Emitter is Missing!"));
            return names.ToArray();
        }

        public string AutoRename()
        {
            if (StartEvents.Length > 0)
                return StartEvents[0].Name;
            if (StartStates.Length > 0)
                return StartStates[0].Name;
            if (EndEvents.Length > 0)
                return EndEvents[0].Name;
            if (EndStates.Length > 0)
                return EndStates[0].Name;
            return "Empty Wwise Clip";
        }
#endif
    }
}