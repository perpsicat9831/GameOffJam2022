using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;


public class AkSetVolume : MonoBehaviour
{
    public Slider thisSlider;
    public float masterVolume;
    public float soundVolume;
    public float musicVolume;

    public void SetBusVolume(string busName)
    {
        float sliderValue = thisSlider.value;

        if (busName == "Master")
        {
            masterVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("Volume_Master", masterVolume);
        }

        if (busName == "Sound")
        {
            soundVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("Volume_Sound", soundVolume);
        }

        if (busName == "Music")
        {
            musicVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("Volume_Music", musicVolume);
        }

    }

}
