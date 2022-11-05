using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Framework
{
    public enum PanelType
    {
        MainUI, //main panel ui 
        NormalUI, //normal panel ui
        HeadInfoUI //hud ui
    }

    [AddComponentMenu("Framework/UIWindowAsset")]
    public class UIWindowAsset : MonoBehaviour
    {
        public string StringArgument;
        public PanelType PanelType = PanelType.NormalUI;
        public bool IsUIEditor = true;
        public string atals_arr = "";
#if UNITY_EDITOR
        private void Awake()
        {
            if (IsUIEditor)
            {
                SpriteAtlasManager.atlasRequested += this.RequestAtlas;
            }
        }

        private void OnDestroy()
        {
            if (IsUIEditor)
            {
                SpriteAtlasManager.atlasRequested -= this.RequestAtlas;
            }
        }

        private void RequestAtlas(string tag, Action<SpriteAtlas> callback)
        {
            var findAssets = AssetDatabase.FindAssets("t:SpriteAtlas", new string[] { "Assets\\" + AppDef.ResourcesBuildDir });
            if (findAssets != null && findAssets.Length >= 1)
            {
                string find_path = null;
                foreach (string guid in findAssets)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.Contains(tag))
                    {
                        find_path = assetPath;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(find_path))
                {
                    var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(find_path);
                    callback?.Invoke(atlas);
                }
                else
                {
                    Log.Error($"找不到名字为{tag}的SpriteAtlas");
                }
            }
            else
            {
                Log.Error($"找不到名字为{tag}的SpriteAtlas");
            }
        }
#endif
    }
}
