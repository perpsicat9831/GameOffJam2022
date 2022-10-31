using System.Linq;
using UnityEngine;
using UnityEditor;

namespace AudioStudio.Tools
{
	public class AsSelectionCommands : EditorWindow
	{
        //[MenuItem("Assets/AudioStudio/Batch Rename")]
        //private static void BatchRename()
        //{
        //    var window = GetWindow<AsAssetBatchRenamer>();			
        //    window.position = new Rect(800, 400, 200, 180);						
        //    window.titleContent = new GUIContent("Batch Rename");
        //}	
        
        //[MenuItem("AudioStudio/Tools/Character Component Sync")]
        //public static void CharacterComponentSync()
        //{
        //    var window = GetWindow<CharacterComponentSync>();
        //    window.position = new Rect(500, 300, 300, 100);
        //    window.titleContent = new GUIContent("Character Component Sync");
        //}
        
        [MenuItem("Assets/AudioStudio/Implementation Backup/Save Selection")]
        public static void SaveSelectedAssets()
        {
            AsComponentBackup.Instance.SeparateXmlFiles = false;
            var selectedPaths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray();
            var prefabs = selectedPaths.Where(path => path.EndsWith(".prefab")).ToArray();
            if (prefabs.Length > 0)
                AsComponentBackup.Instance.SaveSelectedAssets(prefabs, AsComponentBackup.Instance.SaveSelectedComponents);
            
            var scenes = selectedPaths.Where(path => path.EndsWith(".unity")).ToArray();
            if (scenes.Length > 0)
                AsComponentBackup.Instance.SaveSelectedAssets(scenes, AsComponentBackup.Instance.SaveSelectedScenes);
            
            var clips = selectedPaths.Where(path => path.EndsWith(".anim")).ToArray();
            if (clips.Length > 0)
                AsAnimationEventBackup.Instance.SaveSelectedAssets(clips, AsAnimationEventBackup.Instance.ParseAnimation);
            
            var models = selectedPaths.Where(path => path.EndsWith(".FBX")).ToArray();
            if (models.Length > 0)
                AsAnimationEventBackup.Instance.SaveSelectedAssets(models, AsAnimationEventBackup.Instance.ParsePrefab);
            
            var controllers = selectedPaths.Where(path => path.EndsWith(".controller")).ToArray();
            if (controllers.Length > 0)
                AsAudioStateBackup.Instance.SaveSelectedAssets(controllers, AsAudioStateBackup.Instance.ParseAnimator);
            
            var timelineAssets = selectedPaths.Where(path => path.EndsWith(".playable")).ToArray();
            if (timelineAssets.Length > 0)
                AsTimelineAudioBackup.Instance.SaveSelectedAssets(timelineAssets, AsTimelineAudioBackup.Instance.ParseTimeline);
        }

        /*
        [MenuItem("Assets/AudioStudio/Implementation Backup/Export Selection")]
        public static void ExportSelectedAssets()
        {
            AsComponentBackup.Instance.SeparateXmlFiles = false;
            var selectedPaths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray();
            var prefabs = selectedPaths.Where(path => path.EndsWith(".prefab")).ToArray();
            if (prefabs.Length > 0)
                AsComponentBackup.Instance.ExportSelectedAssets(prefabs, AsComponentBackup.Instance.ParseSinglePrefab);
            
            var scenes = selectedPaths.Where(path => path.EndsWith(".unity")).ToArray();
            if (scenes.Length > 0)
                AsComponentBackup.Instance.ExportSelectedAssets(scenes, AsComponentBackup.Instance.ParseSingleScene);
            
            var clips = selectedPaths.Where(path => path.EndsWith(".anim")).ToArray();
            if (clips.Length > 0)
                AsAnimationEventBackup.Instance.ExportSelectedAssets(clips, AsAnimationEventBackup.Instance.ParseAnimation);
            
            var models = selectedPaths.Where(path => path.EndsWith(".FBX")).ToArray();
            if (models.Length > 0)
                AsAnimationEventBackup.Instance.ExportSelectedAssets(models, AsAnimationEventBackup.Instance.ParsePrefab);
            
            var controllers = selectedPaths.Where(path => path.EndsWith(".controller")).ToArray();
            if (controllers.Length > 0)
                AsAudioStateBackup.Instance.ExportSelectedAssets(controllers, AsAudioStateBackup.Instance.ParseAnimator);
            
            var timelineAssets = selectedPaths.Where(path => path.EndsWith(".playable")).ToArray();
            if (timelineAssets.Length > 0)
                AsTimelineAudioBackup.Instance.ExportSelectedAssets(timelineAssets, AsTimelineAudioBackup.Instance.ParseTimeline);
        }
        */

        [MenuItem("Assets/AudioStudio/Implementation Backup/Revert Selection")]
        public static void RevertSelectedAssets()
        {
            var selectedPaths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray();
            var prefabs = selectedPaths.Where(path => path.EndsWith(".prefab")).ToArray();
            if (prefabs.Length > 0)
                AsComponentBackup.Instance.RevertSelectedAssets(prefabs, AsComponentBackup.Instance.RevertSelectedComponents);
            
            var scenes = selectedPaths.Where(path => path.EndsWith(".unity")).ToArray();
            if (scenes.Length > 0)
                AsComponentBackup.Instance.RevertSelectedAssets(scenes, AsComponentBackup.Instance.RevertSelectedScenes);
            
            //var clips = selectedPaths.Where(path => path.EndsWith(".anim")).ToArray();
            //if (clips.Length > 0)
            //    AsAnimationEventBackup.Instance.RevertSelectedAssets(clips, AsAnimationEventBackup.Instance.ImportClip);
            
            //var models = selectedPaths.Where(path => path.EndsWith(".FBX")).ToArray();
            //if (models.Length > 0)
            //    AsAnimationEventBackup.Instance.RevertSelectedAssets(models, AsAnimationEventBackup.Instance.ImportModel);
            
            //var controllers = selectedPaths.Where(path => path.EndsWith(".controller")).ToArray();
            //if (controllers.Length > 0)
            //    AsAudioStateBackup.Instance.RevertSelectedAssets(controllers, AsAudioStateBackup.Instance.ImportAnimator);
            
            var timelineAssets = selectedPaths.Where(path => path.EndsWith(".playable")).ToArray();
            if (timelineAssets.Length > 0)
                AsTimelineAudioBackup.Instance.RevertSelectedAssets(timelineAssets, AsTimelineAudioBackup.Instance.ImportTimelines);
        }
        
        [MenuItem("Assets/AudioStudio/Implementation Backup/Remove Selected")]
        public static void RemoveSelectedAssets()
        {
            var selectedPaths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray();
            var prefabs = selectedPaths.Where(path => path.EndsWith(".prefab")).ToArray();
            if (prefabs.Length > 0)
                AsComponentBackup.Instance.RemoveSelectedAssets(prefabs, AsComponentBackup.Instance.RemoveSelectedComponents);
            
            var scenes = selectedPaths.Where(path => path.EndsWith(".unity")).ToArray();
            if (scenes.Length > 0)
                AsComponentBackup.Instance.RemoveSelectedAssets(scenes, AsComponentBackup.Instance.RemoveSelectedScenes);
            
            var clips = selectedPaths.Where(path => path.EndsWith(".anim")).ToArray();
            if (clips.Length > 0)
                AsAnimationEventBackup.Instance.RemoveSelectedAssets(clips, AsAnimationEventBackup.Instance.RemoveClip);

            var models = selectedPaths.Where(path => path.EndsWith(".FBX")).ToArray();
            if (models.Length > 0)
                AsAnimationEventBackup.Instance.RemoveSelectedAssets(models, AsAnimationEventBackup.Instance.RemoveModel);
            
            var controllers = selectedPaths.Where(path => path.EndsWith(".controller")).ToArray();
            if (controllers.Length > 0)
                AsAudioStateBackup.Instance.RemoveSelectedAssets(controllers, AsAudioStateBackup.Instance.RemoveAllInAnimator);
            
            var timelineAssets = selectedPaths.Where(path => path.EndsWith(".playable")).ToArray();
            if (timelineAssets.Length > 0)
                AsTimelineAudioBackup.Instance.RemoveSelectedAssets(timelineAssets, AsTimelineAudioBackup.Instance.RemoveAllInTimeline);
        }
        
        /*
        [MenuItem("Assets/AudioStudio/Implementation Backup/Remove Unsaved")]
        public static void RemoveUnsavedComponents()
        {
            var selectedPaths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray();
            var prefabs = selectedPaths.Where(path => path.EndsWith(".prefab")).ToArray();
            if (prefabs.Length > 0)
                ; 
                //AsComponentBackup.Instance.RemoveSelectedAssets(prefabs, AsComponentBackup.Instance.RemoveUnsavedInPrefab);
            
            var scenes = selectedPaths.Where(path => path.EndsWith(".unity")).ToArray();
            if (scenes.Length > 0)
                ;
                //AsComponentBackup.Instance.RemoveSelectedAssets(scenes, AsComponentBackup.Instance.RemoveUnsavedInScene);

            var controllers = selectedPaths.Where(path => path.EndsWith(".controller")).ToArray();
            if (controllers.Length > 0)
                AsAudioStateBackup.Instance.RemoveSelectedAssets(controllers, AsAudioStateBackup.Instance.RemoveUnsavedInAnimator);
            
            var timelineAssets = selectedPaths.Where(path => path.EndsWith(".playable")).ToArray();
            if (timelineAssets.Length > 0)
                AsTimelineAudioBackup.Instance.RemoveSelectedAssets(timelineAssets, AsTimelineAudioBackup.Instance.RemoveUnsavedInTimeline);
        }
        */

        [MenuItem("Assets/AudioStudio/Preview Timeline")]
        public static void PreviewTimeline()
        {
            AsTimelinePlayer.PreviewTimeline(Selection.activeGameObject);
        }
	}
}