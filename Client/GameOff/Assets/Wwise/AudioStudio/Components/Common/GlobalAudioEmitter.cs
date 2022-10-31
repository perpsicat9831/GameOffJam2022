using UnityEngine;

namespace AudioStudio.Components
{
    /// <summary>
    /// A game object that stays for the entire game, used to play all 2D sounds and music. 
    /// It's also the listener for all the 2D sounds.
    /// </summary>
    /// 
    [DisallowMultipleComponent]
    public class GlobalAudioEmitter : MonoBehaviour
    {
        public static GameObject GameObject;

        private void Awake()
        {
            if (GameObject)
                DestroyImmediate(GameObject);

            GameObject = gameObject;
            AudioStudioWrapper.RegisterGameObj(gameObject, gameObject.name);
            AudioStudioWrapper.AddDefaultListener(gameObject);
        }

        private void Update()
        {
            EmitterManager.RefreshPositions();
            EmitterManager.RefreshEnvironments();
            ListenerManager.UpdateListenerPositions();
        }
    }
}