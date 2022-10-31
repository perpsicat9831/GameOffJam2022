using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
    /// <summary>
    /// The mode of allocating and positioning emitters in Wwise.
    /// </summary>
    public enum MultiPositionType
    {
        SimpleMode,
        LargeMode,
        MultiPositionMode,
        MovingPointMode
    }

    /// <summary>
    /// Play a looping sound once or fire individual events periodically
    /// </summary>
    public enum EventPlayMode
    {
        SingleLoop,
        PeriodTrigger
    }

    /// <summary>
    /// Play sound for scene objects emitting environment sounds.
    /// </summary>
    [AddComponentMenu("AudioStudio/Emitter Sound")]
    [DisallowMultipleComponent]
    public class EmitterSound : AudioEmitter3D
    {        
        public AudioEvent[] AudioEvents = new AudioEvent[0];
        public float FadeOutTime = 0.5f;
        public float InitialDelay;
        public float MinInterval = 5;
        public float MaxInterval = 10;
        public List<Vector3> MultiPositionArray = new List<Vector3>();
        public MultiPositionType MultiPositionType = MultiPositionType.SimpleMode;
        public EventPlayMode PlayMode = EventPlayMode.SingleLoop;
        // pause the sound if away from camera
        public bool PauseIfInvisible;
        private bool _isPlaying;
        private Collider _emitterZone;
        private Vector3 _closestPointWithinZone;
        private Vector3 _lastPosition;
        private bool bFirst = false;

        protected override void Init()
        {
            IsUpdatePosition = true;
            StopOnDestroy = false;
            switch (MultiPositionType)
            {
                // do not update position if emitting from multiple points
                case MultiPositionType.LargeMode:
                    UpdateFrequency = UpdateFrequency.Never;
                    break;
                case MultiPositionType.MultiPositionMode:
                    UpdateFrequency = UpdateFrequency.Low;
                    break;
                case MultiPositionType.MovingPointMode:
                    _emitterZone = AsUnityHelper.GetOrAddComponent<Collider>(gameObject);
                    break;
            }
        }

        public override void Activate(GameObject source = null)
        {
            if (MultiPositionType == MultiPositionType.MultiPositionMode)
            {
                bFirst = EmitterManager.RegisterMultiPositionEmitter(this);
                _lastPosition = transform.position;
            }

            if (SetOn == TriggerCondition.ManuallyControl)
                SetObjectPosition();

            if (PlayMode == EventPlayMode.SingleLoop)
                PostEvents();
            else
                StartCoroutine(PlaySoundPeriod());
        }

        public override void Deactivate(GameObject source = null)
        {
            if (PlayMode == EventPlayMode.PeriodTrigger)
            {
                if (enabled)
                    _isPlaying = false;
                StopCoroutine(PlaySoundPeriod());
            }

            if (MultiPositionType == MultiPositionType.MultiPositionMode)
                EmitterManager.UnregisterMultiPositionEmitter(this);
            else
            {
                foreach (var audioEvent in AudioEvents)
                {
                    audioEvent.Stop(gameObject, (int) (FadeOutTime * 1000), AudioTriggerSource.EmitterSound, UnderPlayerControl);
                }
            }
        }

        private IEnumerator PlaySoundPeriod()
        {
            if (_isPlaying) yield break;
            yield return new WaitForSeconds(InitialDelay);
            _isPlaying = true;
            while (isActiveAndEnabled)
            {
                PostEvents();
                var waitSecond = Random.Range(MinInterval, MaxInterval);
                yield return new WaitForSeconds(waitSecond);
            }
            _isPlaying = false;
        }

        private void PostEvents()
        {
            if (MultiPositionType != MultiPositionType.MultiPositionMode)
                PostEvents(AudioEvents, AudioTriggerSource.EmitterSound, gameObject);
            else
            {
                foreach (var audioEvent in AudioEvents)
                {
                    if (bFirst)
                        audioEvent.Post(EmitterManager.GetActualMultiPosEmitter(audioEvent), AudioTriggerSource.EmitterSound);
                }
            }
        }

        private void OnBecameVisible()
        {
            if (!PauseIfInvisible) return;
            foreach (var evt in AudioEvents)
            {
                AkSoundEngine.ExecuteActionOnEvent(evt.Name, AkActionOnEventType.AkActionOnEventType_Pause, gameObject, (int) (FadeOutTime * 1000));
            }
        }
        
        private void OnBecameInvisible()
        {
            if (!PauseIfInvisible) return;
            foreach (var evt in AudioEvents)
            {
                AkSoundEngine.ExecuteActionOnEvent(evt.Name, AkActionOnEventType.AkActionOnEventType_Resume, gameObject, (int) (FadeOutTime * 1000));
            }
        }

        internal override void SetObjectPosition()
        {
            switch (MultiPositionType)
            {
                case MultiPositionType.SimpleMode:
                    base.SetObjectPosition();
                    break;
                case MultiPositionType.MultiPositionMode:    
                    if (!transform.position.Equals(_lastPosition))
                    {
                        _lastPosition = transform.position;
                        EmitterManager.UpdateMultiPositionEmitter(this);
                    }
                    break;
                // build multi-position array
                case MultiPositionType.LargeMode:
                    var positionArray = new AkPositionArray((uint) MultiPositionArray.Count);
                    foreach (var position in MultiPositionArray)
                    {
                        positionArray.Add(transform.position + transform.rotation * position, transform.forward, transform.up);
                    }
                    AkSoundEngine.SetMultiplePositions(gameObject, positionArray, (ushort)MultiPositionArray.Count, AkMultiPositionType.MultiPositionType_MultiDirections);
                    break;
                // find closest point to listener
                case MultiPositionType.MovingPointMode:
                    _closestPointWithinZone = _emitterZone.ClosestPoint(ListenerManager.GetListenerPosition(Listeners));
                    AkSoundEngine.SetObjectPosition(gameObject, _closestPointWithinZone, transform.forward, transform.up);
                    break;
            }
        }

        public override bool IsValid()
        {
            return AudioEvents.Any(s => s.IsValid());
        }
        
        public override IEnumerable<AudioEvent> GetEvents()
        {
            return AudioEvents;
        }
        
        protected override void OnDrawGizmos()
        {
            switch (MultiPositionType)
            {
                case MultiPositionType.SimpleMode:
                case MultiPositionType.MultiPositionMode:
                    base.OnDrawGizmos();
                    break;
                // draw emitter icons at every location
                case MultiPositionType.LargeMode:
                    foreach (var position in MultiPositionArray)
                    {
                        Gizmos.DrawIcon(position, "AudioStudio/WwiseAudioSpeaker.png", WwisePathSettings.Instance.GizmosIconScaling);
                    }
                    break;
                case MultiPositionType.MovingPointMode:
                    if (Application.isPlaying)
                        Gizmos.DrawIcon(_closestPointWithinZone, "AudioStudio/WwiseAudioSpeaker.png", WwisePathSettings.Instance.GizmosIconScaling);
                    break;
            }
        }
    }
}
