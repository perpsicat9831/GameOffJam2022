using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Framework
{
    public class EditorMenuExtend
    {
        [MenuItem("Framework/OpenMainScene",priority = -1)]
        public static void OpenMainScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string mainScenePath = "Assets/ResourseAB/Scenes/GameStart.unity";
                EditorSceneManager.OpenScene(mainScenePath);
            }
        }
    }
}
