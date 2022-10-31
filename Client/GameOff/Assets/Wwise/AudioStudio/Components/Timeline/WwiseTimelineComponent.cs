using AudioStudio.Tools;
using UnityEngine;
using UnityEngine.Playables;

namespace AudioStudio.Timeline
{
    public class WwiseTimelineComponent : PlayableBehaviour
    {
        private GameObject _emitter;
        private WwiseTimelineClip _component;
        private bool _started;
        private PlayableDirector _director;
        private double _endTime;

        public void Init(WwiseTimelineClip component, GameObject emitter, GameObject director)
        {
            _component = component;
            _director = director.GetComponent<PlayableDirector>();
            _endTime = component.EndTime;
            _emitter = emitter ? emitter : null;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!_director || _director.state != PlayState.Playing)
                    return;
            }
#endif
            if (_started) return;
            _started = true;
            ProcessStartActions();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!IsClipFinished()) return;
            ProcessEndActions();
        }
#if UNITY_EDITOR
        public override void OnGraphStop(Playable playable)
        {
            if (!Application.isPlaying)
            {
                ProcessEndActions();
                _started = false;
            }
            base.OnGraphStop(playable);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying && !_started)
            {
                if (info.deltaTime == 0 && _component.StartTime == 0)
                    OnBehaviourPlay(playable, info);
            }
            base.ProcessFrame(playable, info, playerData);
        }
#endif

        private bool IsClipFinished()
        {
            if (!_director) return true;
            if (_director.time >= _endTime ||_director.time == 0 && _started)
            {
                _started = false;
                return true;
            }
            return false;
        }

        private void ProcessStartActions()
        {
            foreach (var state in _component.StartStates)
            {
                state.SetValue(_emitter, AudioTriggerSource.WwiseTimelineClip);
            }

            foreach (var evt in _component.StartEvents)
            {
                evt.Post(_emitter, AudioTriggerSource.WwiseTimelineClip);
            }
        }
        
        private void ProcessEndActions()
        {
            foreach (var state in _component.StartStates)
            {
                if (state.ResetOnDisable)
                    state.Reset(_emitter, AudioTriggerSource.WwiseTimelineClip);
            }

            foreach (var state in _component.EndStates)
            {
                state.SetValue(_emitter, AudioTriggerSource.WwiseTimelineClip);
            }

            foreach (var evt in _component.StartEvents)
            {
                if (evt.StopOnDisable)
                    evt.Stop(_emitter, evt.FadeOutTime, AudioTriggerSource.WwiseTimelineClip);
            }

            foreach (var evt in _component.EndEvents)
            {
                evt.Post(_emitter, AudioTriggerSource.WwiseTimelineClip);
            }
        }
    }
}