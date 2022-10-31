using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public class SoundQueueData
    {
        public int soundId;
        public int curNum;
        public float deltaTime;
    }
    public class SoundManager : BaseManager<SoundManager>, IManagerUpdateModule
    {
        private GameObject audioRootGo;
        /// <summary>
        /// AudioSource缓存池
        /// </summary>
        private Queue<AudioSource> cacheAudioSource;

        private Dictionary<uint, AudioPlayHandler> dicAudio;

        /// <summary>
        /// 队列 soundId-队列数据
        /// </summary>
        private Dictionary<int, SoundQueueData> dicQueue = new Dictionary<int, SoundQueueData>();
        private List<int> listQueueKey = new List<int>();
        /// <summary>
        /// 单个队列最大长度
        /// </summary>
        private readonly int QueueMaxNum = 6;
        /// <summary>
        /// 队列执行的间隔时间
        /// </summary>
        private readonly float QueueDicTime = 0.1f;
        //private int volumeBgm = 1;
        private int volumeAudio = 1;
        private uint curBgmId = 0;
        private uint curAudioId = 1;


        #region lifeCycle
        public override void OnInit()
        {
            cacheAudioSource = new Queue<AudioSource>(5);
            dicAudio = new Dictionary<uint, AudioPlayHandler>();
            volumeAudio = 1;
            curBgmId = 0;
            curAudioId = 1;
            audioRootGo = new GameObject("AudioRoot");
            GameObject.DontDestroyOnLoad(audioRootGo);
            audioRootGo.AddComponent<AudioListener>();
        }
        public override void OnDestroy()
        {
            cacheAudioSource?.Clear();
            dicAudio.Clear();
            curBgmId = 0;
            curAudioId = 1;
            GameObject.Destroy(audioRootGo);
            audioRootGo = null;
        }
        public void OnUpData()
        {
            for (int i = 0; i < listQueueKey.Count; i++)
            {
                var key = listQueueKey[i];
                if (dicQueue.TryGetValue(key, out SoundQueueData data) && data.curNum > 0)
                {
                    data.deltaTime += Time.deltaTime;
                    if(data.deltaTime > QueueDicTime)
                    {
                        data.deltaTime -= QueueDicTime;
                        data.curNum--;
                        //执行一次
                        Play(data.soundId);
                    }
                }
            }
        }
        #endregion

        #region func
        public uint Play(int soundId, bool useQueue = false)
        {
            var soundData = CSVManager.CSVData.TbSound.GetOrDefault(soundId);
            if (soundData == null)
            {
                Log.Error("未找到TbSound中id " + soundId + "对应的数据");
                return 0;
            }
            if (useQueue)
            {
                //使用队列，这边不再做其他操作
                AddQueue(soundId);
                return 0;
            }
            var path = soundData.Path;
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Log.Error("can not find the clip: " + soundId + " path: " + path);
                return 0;
            }
            AudioSource source = GetAudioSource();
            source.clip = clip;
            source.loop = soundData.IsLoop;
            source.volume = volumeAudio;
            source.spatialBlend = soundData.SpaticalBlend;
            var clipName = Tool.GetFileNameByPath(path);
            source.gameObject.name = clipName;
            source.Play();

            var audioId = curAudioId++;

            var audioHandler = new AudioPlayHandler()
            {
                clipName = clipName,
                id = audioId,
                audioSource = source,
            };

            dicAudio.Add(audioId, audioHandler);

            //非循环播放的主动停止
            if (!source.loop)
            {
                TimeManager.Instance.RegisterTimer(source.clip.length, () =>
                {
                    StopPlayAudio(audioId);
                });
            }
            return audioId;
            //Log.LogInfo($"play audio, audio name:{clipName}, loop:{source.loop}");
        }

        public void StopPlayAudio(uint soundId)
        {
            Debug.Log("StopPlayAudio " + soundId);
            //正在播放
            if (dicAudio.ContainsKey(soundId))
            {
                var audioSource = dicAudio[soundId].audioSource;
                //未在缓存池里
                if (!cacheAudioSource.Contains(audioSource))
                {
                    audioSource.Stop();
                    //audioSource.gameObject.SetActive(false);
                    cacheAudioSource.Enqueue(audioSource);
                }
                dicAudio.Remove(soundId);
            }
        }

        public void PlayBgm(int soundId)
        {
            StopBgm();
            curBgmId = Play(soundId);
            Debug.Log("PlayBgm " + curBgmId);
        }
        public void StopBgm()
        {
            if (curBgmId != 0)
            {
                StopPlayAudio(curBgmId);
                curBgmId = 0;
            }
        }

        private AudioSource GetAudioSource()
        {
            AudioSource source;

            //优先从缓存池里取
            if (cacheAudioSource.Count > 0)
            {
                source = cacheAudioSource.Dequeue();
                //source.gameObject.SetActive(true);
            }
            else
            {
                GameObject gameObject = new GameObject("audio");
                gameObject.transform.parent = audioRootGo.transform;

                source = gameObject.AddComponent<AudioSource>();
                source.spatialBlend = 1f;
                source.loop = false;
            }
            return source;
        }

        private void AddQueue(int soundId)
        {
            if(dicQueue.TryGetValue(soundId,out SoundQueueData data))
            {
                if (data.curNum <= QueueMaxNum)
                {
                    data.curNum++;
                }
            }
            else
            {
                SoundQueueData queueData = new SoundQueueData();
                queueData.soundId = soundId;
                queueData.curNum = 1;
                queueData.deltaTime = 0;
                dicQueue.Add(soundId, queueData);
                listQueueKey.Add(soundId);
            }
        }
        #endregion

        #region event
        #endregion

        #region Class
        public class AudioPlayHandler
        {
            /// <summary>
            /// handler的Id,唯一ID
            /// </summary>
            public uint id;
            /// <summary>
            /// 音频表ID
            /// </summary>
            public uint soundId;
            /// <summary>
            /// 音频名字
            /// </summary>
            public string clipName;
            /// <summary>
            /// 音频对象
            /// </summary>
            public AudioSource audioSource;
        }
        #endregion

    }
}
