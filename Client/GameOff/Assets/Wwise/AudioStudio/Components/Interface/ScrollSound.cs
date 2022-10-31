using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/ScrollSound")]    
    [DisallowMultipleComponent]
    public class ScrollSound : AsUIHandler
    {        
        public AudioEvent downScrollEvent = new AudioEvent();
        public AudioEvent upScrollEvent = new AudioEvent();
        public int scrollDistance;
        private Vector2 _startPos;
        private Vector2 _initSnapPos;
        private Vector2 _initSlidePos;
        private float _currentDifferenceToStart;
        private int _currentSnapDifference;
        private int _currentSlideDifference;
        private ScrollRect _scrollRect;

        protected override void Awake()
        {
            base.Awake();
            _scrollRect = GetComponent<ScrollRect>();
            _startPos = _scrollRect.content.localPosition;
            _initSnapPos = _scrollRect.content.localPosition;
            _initSlidePos = _scrollRect.content.localPosition;
            scrollDistance = 60;
            _currentDifferenceToStart = 0;
            _currentSnapDifference = 0;
            _currentSlideDifference = 0;
        }

        public override void AddListener()
        {
            if (_scrollRect) _scrollRect.onValueChanged.AddListener(PlaySound);
        }
        
        public override void RemoveListener()
        {
            if (_scrollRect) _scrollRect.onValueChanged.RemoveListener(PlaySound);
        }

        private void PlaySound(Vector2 position)
        {
            _currentSnapDifference = (int)Mathf.Abs(_scrollRect.content.localPosition.y - _initSnapPos.y);
            _currentSlideDifference = (int)Mathf.Abs(_scrollRect.content.localPosition.y - _initSlidePos.y);
            _currentDifferenceToStart = Mathf.Abs(_scrollRect.content.localPosition.y - _startPos.y);

            //Debug.LogWarning("_currentSlideDifference： " + (_scrollRect.content.localPosition.y - _initSnapPos.y));

            if (_currentSnapDifference >= scrollDistance && 
                (((_currentDifferenceToStart - scrollDistance / 2)) % scrollDistance) == 0)
            {
                Postevent();
                _initSnapPos = _scrollRect.content.localPosition;
                DeviationCal();
                _currentSlideDifference = (int)Mathf.Abs(_scrollRect.content.localPosition.y - _initSlidePos.y);

                //Debug.LogWarning("Snap Sound");
            }
            if (_currentSlideDifference >= scrollDistance + 1)
            {
                Postevent();
                DeviationCal();
                /*
                Debug.LogWarning(deviation);
                Debug.LogWarning(_initSlidePos.y);
                Debug.LogWarning(_scrollRect.content.localPosition.y);
                Debug.LogWarning("Slide Sound");
                */
            }
        }

        private void DeviationCal()
        {
            var deviation = ((int)_scrollRect.content.localPosition.y - (scrollDistance / 2)) % scrollDistance;
            _initSlidePos.y =
                deviation > (scrollDistance / 2) ?
                (int)_scrollRect.content.localPosition.y + scrollDistance - deviation :
                (int)_scrollRect.content.localPosition.y - deviation;
        }

        private void Postevent()
        {
            if (_scrollRect.content.localPosition.y - _initSnapPos.y >= 0)
                downScrollEvent.Post(gameObject, AudioTriggerSource.ScrollSound);
            else
                upScrollEvent.Post(gameObject, AudioTriggerSource.ScrollSound);
        }

        public override bool IsValid()
        {
            return downScrollEvent.IsValid();
        }
    }   
}

