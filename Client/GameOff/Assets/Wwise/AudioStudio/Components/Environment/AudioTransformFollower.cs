using UnityEngine;

namespace AudioStudio.Components
{
    /// <summary>
    /// A tool to link audio config game object with scene objects.
    /// </summary>
    public class AudioTransformFollower : AsComponent
    {
        public GameObject HostGameObject;

        protected override void Awake()
        {
            DestroyImmediate(this);
        }

        /// <summary>
        /// Set position of this game object based on the current location of the linked scene game object. 
        /// </summary>
        /// <returns></returns>
        public bool UpdatePosition()
        {
            if (!HostGameObject) return false;
            var changed = false;
            if (transform.position != HostGameObject.transform.position)
            {
                transform.position = HostGameObject.transform.position;
                changed = true;
            }

            if (transform.rotation != HostGameObject.transform.rotation)
            {
                transform.rotation = HostGameObject.transform.rotation;
                changed = true;
            }
            return changed;
        }
    }
}