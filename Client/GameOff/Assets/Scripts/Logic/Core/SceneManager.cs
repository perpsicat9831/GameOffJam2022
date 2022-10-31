using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Logic
{
    public static class SceneManager
    {

        public static void LoadScene(string scenePath)
        {
            SceneLoader.Load(scenePath);
        }

        public static void UnloadPreScene()
        {
            SceneLoader.UnloadPreScene();
        }
    }
}
