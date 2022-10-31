using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioStudio.Components
{
    public class MusicPlayer : AudioEmitter3D
    {
        // Start is called before the first frame update
        protected override void Start()
        {
            AudioManager.RegistMusicPlayer(gameObject);
            //if (startmusic.IsValid())
            //{
            //    startmusic.Post(gameObject);
            //}
        }

        protected override void OnDestroy()
        {
            AudioStudioWrapper.StopAll(gameObject);
            AudioManager.UnRegistMusicPlayer();
            base.OnDestroy();
        }
    }
}
