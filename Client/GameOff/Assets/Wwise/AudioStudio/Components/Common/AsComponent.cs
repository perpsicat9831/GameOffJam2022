using System;
using System.Collections.Generic;
using AK.Wwise;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{
    /// <summary>
    /// Base class for all AudioStudio Components.
    /// </summary>
    public abstract class AsComponent : MonoBehaviour
    {
        //if component is empty, destroy it to optimize performance
        protected virtual void Awake()
        {
            if (AudioManager.DisableWwise) 
                Destroy(this);
            if (!IsValid())
                AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.Component, AudioTriggerSource.Initialization, AudioAction.Activate, GetType().Name, gameObject, "Component is empty");
        }

        private bool _started;
        private bool _enabled;

        private void OnEnable()
        {
            if (_started && !_enabled)
            {
                HandleEnableEvent();
                _enabled = true;
            }                                                                       
        }

        //make sure the first time it is played at Start instead of OnEnable
        protected virtual void Start()
        {
            _started = true;
            if (!_enabled)
            {
                HandleEnableEvent();
                _enabled = true;
            }
        }

        private void OnDisable()
        {
            if (_started && _enabled)
            {
                HandleDisableEvent();
                _enabled = false;
            }            
        }
        
        protected virtual void HandleEnableEvent(){}
        protected virtual void HandleDisableEvent(){}
        
        //check if the component is empty
        public virtual bool IsValid()
        {
            return true;
        }
        
        //shortcut for posting multiple events
        protected static void PostEvents(IEnumerable<AudioEvent> events, AudioTriggerSource trigger, GameObject soundSource = null, bool underPlayerControl = false)
        {
            foreach (var evt in events)
            {				
                evt.Post(soundSource, trigger, underPlayerControl);
            }  
        }
    }

    public abstract class AsUIHandler : AsComponent
    {
        protected override void HandleEnableEvent()
        {
            AddListener();
        }

        protected override void HandleDisableEvent()
        {
            RemoveListener();
        }

        public virtual void AddListener() {}
        public virtual void RemoveListener() {}
    }
    
    /// <summary>
    /// Sound emitter when dealing with collision.
    /// </summary>
    public enum PostFrom
    {
        Self,
        Other
    }
    
    /// <summary>
    /// Define when the event is triggered. 
    /// </summary>
    public enum TriggerCondition
    {
        EnableDisable,
        AwakeDestroy,
        TriggerEnterExit,
        CollisionEnterExit,   
        ManuallyControl
    }

    // for any components that can be triggered by physics
    public abstract class AsTriggerHandler : AsComponent
    {
        public TriggerCondition SetOn = TriggerCondition.EnableDisable;
        public PostFrom PostFrom = PostFrom.Self;
        [AkEnumFlag(typeof(AudioTags))]
        public AudioTags MatchTags = AudioTags.None;        
        
        //use enum bit comparison to check if tags match
        protected bool CompareAudioTag(Collider other)
        {
            if (MatchTags == AudioTags.None) return true;
            var audioTag = other.GetComponent<AudioTag>();
            if (!audioTag) return false;
            var result = MatchTags & audioTag.Tags;
            return result != AudioTags.None;
        }

        // determine if sound should be played by collider itself or other game object
        protected GameObject GetEmitter(GameObject other)
        {
            return PostFrom == PostFrom.Self ? gameObject : other.gameObject;
        }

        // common interface for any trigger conditions
        public virtual void Activate(GameObject source = null)
        {
        }

        public virtual void Deactivate(GameObject source = null)
        {
        }

        protected override void Awake()
        {
            base.Awake();
            if (SetOn == TriggerCondition.AwakeDestroy)
                Activate(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (SetOn == TriggerCondition.AwakeDestroy)
                Deactivate(gameObject);
        }

        protected override void HandleEnableEvent()
        {            
            if (SetOn == TriggerCondition.EnableDisable)
                Activate(gameObject);
        }
        
        protected override void HandleDisableEvent()
        {            
            if (SetOn == TriggerCondition.EnableDisable)
                Deactivate(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (SetOn == TriggerCondition.TriggerEnterExit && CompareAudioTag(other))
                Activate(GetEmitter(other.gameObject));                         
        }

        private void OnTriggerExit(Collider other)
        {
            if (SetOn == TriggerCondition.TriggerEnterExit && CompareAudioTag(other))
                Deactivate(GetEmitter(other.gameObject));                                          
        }      
        
        private void OnCollisionEnter(Collision other)
        {
            if (SetOn == TriggerCondition.CollisionEnterExit && CompareAudioTag(other.collider))
                Activate(GetEmitter(other.gameObject));                         
        }

        private void OnCollisionExit(Collision other)
        {
            if (SetOn == TriggerCondition.CollisionEnterExit && CompareAudioTag(other.collider))
                Deactivate(GetEmitter(other.gameObject));                        
        }
    }
    
    // for any components that uses spatial audio
    public abstract class AsSpatialHandler : AsComponent
    {
        private readonly RoomPriorityList _roomRoomPriorityList = new RoomPriorityList();

        private void SetGameObjectInHighestPriorityRoom()
        {
            var highestPriorityRoomId = _roomRoomPriorityList.GetHighestPriorityRoomId();
            AudioStudioWrapper.SetGameObjectInRoom(gameObject, highestPriorityRoomId);
        }

        public void EnterRoom(AudioRoom room)
        {
            _roomRoomPriorityList.Add(room);
            SetGameObjectInHighestPriorityRoom();
        }

        public void ExitRoom(AudioRoom room)
        {
            _roomRoomPriorityList.Remove(room);
            SetGameObjectInHighestPriorityRoom();
        }

        public void SetGameObjectInRoom()
        {
            var colliders = Physics.OverlapSphere(transform.position, 0.0f);
            foreach (var col in colliders)
            {
                var room = col.GetComponent<AudioRoom>();
                if (room != null)
                    _roomRoomPriorityList.Add(room);
            }
            SetGameObjectInHighestPriorityRoom();
        }
    }
}