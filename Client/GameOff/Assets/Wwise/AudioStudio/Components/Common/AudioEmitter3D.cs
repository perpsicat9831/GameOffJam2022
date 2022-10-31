using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;

namespace AudioStudio.Components
{
    /// <summary>
    /// Base class for any AudioStudio components that can play 3D sounds.
    /// </summary>
    public class AudioEmitter3D : AsTriggerHandler
    {
        private const byte LOW_FREQUENCY_FRAME_COUNT = 9;
        private const byte MID_FREQUENCY_FRAME_COUNT = 4;
        
        // if sound should be stopped when game object is deactivated
        public bool StopOnDestroy = true;
        // if sound should be 3D
        public bool IsUpdatePosition = true;
        // how often the position of this emitter should be send to Wwise
        public UpdateFrequency UpdateFrequency = UpdateFrequency.High;   
        // if the pivot point should not be where sound emits
        public Vector3 PositionOffset;
        // which listener should the emitter to send to
        public string Listeners = "0";
        // if this emitter should implement any game defined auxiliary busses
        public bool IsEnvironmentAware;
        // the mode of aux send for this emitter
        public EnvironmentSource EnvironmentSource = EnvironmentSource.AutoDetect;
        // if this emitter is the player owned character. Some sounds might be treated differently when played by player character
        public bool UnderPlayerControl { protected get; set; }

        // if none of the three are checked, sound should be treated as 2D, thus shouldn't have an individual emitter
        private bool RegisterToWwise
        {
            get { return IsUpdatePosition || IsEnvironmentAware || StopOnDestroy;}
        }

        private bool _registeredToWwise;
        
        private byte _frameCounter;
        
        #region Position
        // send the position of emitter to Wwise
        internal void UpdatePosition()
        {            
            switch (UpdateFrequency)
            {
                case UpdateFrequency.High:
                    break;
                case UpdateFrequency.Mid:
                    _frameCounter++;
                    if (_frameCounter == MID_FREQUENCY_FRAME_COUNT) 
                        _frameCounter = 0;
                    else return;                    
                    break;
                case UpdateFrequency.Low:
                    _frameCounter++;
                    if (_frameCounter == LOW_FREQUENCY_FRAME_COUNT) 
                        _frameCounter = 0;
                    else return;                    
                    break; 
                case UpdateFrequency.Never:
                    return;
            }
            SetObjectPosition();
        }

        internal virtual void SetObjectPosition()
        {
            AudioStudioWrapper.SetObjectPosition(gameObject, Position, transform.forward, transform.up);
        }
        
        // get the actual emitter position after applying offset
        public Vector3 Position
        {
            get
            {
                if (PositionOffset == Vector3.zero)
                    return transform.position;
                return transform.position + transform.rotation * PositionOffset;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            Init();
            if (RegisterToWwise)
                _registeredToWwise = EmitterManager.RegisterAudioGameObject(this);
        }

        protected virtual void Init()
        {            
        }
        
        protected override void HandleDisableEvent()
        {
            base.HandleDisableEvent();
            if (StopOnDestroy)
                AudioStudioWrapper.StopAll(gameObject);                        
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_registeredToWwise) 
                EmitterManager.UnregisterAudioGameObject(this);    
        }
        
        // get all events under this component
        public virtual IEnumerable<AudioEvent> GetEvents()
        {
            return new List<AudioEvent>();
        }
        
        // draw a speaker icon in editor scene view for testing
        protected virtual void OnDrawGizmos()
        {
            Gizmos.DrawIcon(Position, "AudioStudio/WwiseAudioSpeaker.png", WwisePathSettings.Instance.GizmosIconScaling);
        }
    }
}
