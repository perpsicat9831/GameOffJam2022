using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;


namespace Logic
{
    public class Timer 
    {
        //��ʱ��
        public float duration;
        //ʣ��ʱ��
        public float leftTime;
        //���� false�ᱻ����
        public bool isActive;
        //�Ƿ���ͣ
        public bool isPause;


        //�����¼�
        public Action onUpdateAct;
        //�˳��¼�
        public Action onEndAct;


        public void Run()
        {
            if (isActive && !isPause)
            {
                leftTime -= Time.deltaTime;
                if (leftTime <= 0)
                {
                    if (onEndAct != null)
                    {
                        onEndAct.Invoke();
                    }
                    isActive = false;
                }
                else
                {
                    if (onUpdateAct != null)
                        onUpdateAct.Invoke();
                }
            }
        }

        public void SetTimer(float _duration, Action callAction = null, Action updateAction = null)
        {
            Reset();
            duration = leftTime = _duration;
            onEndAct = callAction;
            onUpdateAct = updateAction;
        }
        //���ü�ʱ��
        public void Reset()
        {
            isActive = true;
            isPause = false;
            leftTime = duration;
        }

        /// <summary>
        /// ���ü�ʱ����ͣ
        /// </summary>
        public void SetPause(bool pause)
        {
            isPause = pause;
        }
        public void Clear()
        {
            //����
            isActive = false;
            onUpdateAct = null;
            onEndAct = null;
        }
    }
}