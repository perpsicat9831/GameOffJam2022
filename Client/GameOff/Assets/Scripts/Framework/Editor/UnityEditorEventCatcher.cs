using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// ���ڲ�׽�༭������¼������������߼�����ֻ��¶���ַ�װ�¼�
    /// 
    /// ע�ⲻҪ�Ӿ����߼������namespace KAdminTools û�о����߼����������߼�����KGameAdminEditor��
    /// 
    /// �����߼�ʹ��ʱ��ע�����[InitializeOnLoad]
    /// </summary>
    [InitializeOnLoad]
    public class UnityEditorEventCatcher
    {
        /// <summary>
        /// Editor update�¼�
        /// </summary>
        private static System.Action _OnEditorUpdateEvent;

        public static System.Action OnEditorUpdateEvent
        {
            get { return _OnEditorUpdateEvent; }
            set
            {
                _OnEditorUpdateEvent = value;
                if (IsInited && _OnEditorUpdateEvent != null)
                    _OnEditorUpdateEvent();
            }
        }

        /// <summary>
        /// ��Ҫ������Ϸ�¼�
        /// </summary>
        private static System.Action _OnWillPlayEvent;

        public static System.Action OnWillPlayEvent
        {
            get { return _OnWillPlayEvent; }
            set
            {
                _OnWillPlayEvent = value;
                //if (IsInited && _OnWillPlayEvent != null)
                //    _OnWillPlayEvent();
            }
        }

        /// <summary>
        /// ���벥��ʱ���¼�
        /// </summary>
        private static System.Action _OnBeginPlayEvent;

        public static System.Action OnBeginPlayEvent
        {
            get { return _OnBeginPlayEvent; }
            set
            {
                _OnBeginPlayEvent = value;
                //if (IsInited && _OnBeginPlayEvent != null)
                //    _OnBeginPlayEvent();
            }
        }

        /// <summary>
        /// ��Ҫֹͣ��Ϸ (��������ͣŶ)
        /// </summary>
        private static System.Action _OnWillStopEvent;

        public static System.Action OnWillStopEvent
        {
            get { return _OnWillStopEvent; }
            set
            {
                _OnWillStopEvent = value;
                //if (IsInited && _OnWillStopEvent != null)
                //    _OnWillStopEvent();
            }
        }

        /// <summary>
        /// ���������¼����¼��п��Խ���DLL��ע���޸�
        /// </summary>
        private static System.Action _OnLockingAssembly;

        public static System.Action OnLockingAssembly
        {
            get { return _OnLockingAssembly; }
            set
            {
                _OnLockingAssembly = value;
                if (IsInited && _OnLockingAssembly != null)
                    _OnLockingAssembly();
            }
        }


        /// <summary>
        /// ����ǰ�¼����Ƚ�����Ĵ��������PostBuildProcess��PostBuildScene
        /// ���Դ�����������
        ///     build ab(���abδ�����ı��򲻻ᴥ��)
        ///     build app
        /// </summary>
        public static Action OnBeforeBuildPlayerEvent;

        /// <summary>
        /// before build app�¼���ֻ��ִ��build app�Żᴥ��
        /// </summary>
        public static Action OnBeforeBuildAppEvent;


        /// <summary>
        /// ������ɺ��¼�
        /// </summary>
        private static System.Action<BuildTarget, string> _OnPostBuildPlayerEvent;

        public static System.Action<BuildTarget, string> OnPostBuildPlayerEvent
        {
            get { return _OnPostBuildPlayerEvent; }
            set { _OnPostBuildPlayerEvent = value; }
        }

        /// <summary>
        /// Save Scene�¼�
        /// </summary>
        internal static System.Action _onSaveSceneEvent;

        public static System.Action OnSaveSceneEvent
        {
            get { return _onSaveSceneEvent; }
            set { _onSaveSceneEvent = value; }
        }

        /// <summary>
        /// �Ƿ�̬�������
        /// </summary>
        public static bool IsInited { get; private set; }

        static UnityEditorEventCatcher()
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            SceneView.onSceneGUIDelegate -= OnSceneViewGUI;
            SceneView.onSceneGUIDelegate += OnSceneViewGUI;

            EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
#endif
            if (OnLockingAssembly != null)
            {
                EditorApplication.LockReloadAssemblies();
                OnLockingAssembly();
                EditorApplication.UnlockReloadAssemblies();
            }

            IsInited = true;
        }

        /// <summary>
        /// For BeforeBuildEvent, Because in Unity:   PostProcessScene -> PostProcessScene ->.... PostProcessScene -> PostProcessBuild
        /// When true, waiting PostProcessBuild to revert to false
        /// </summary>
        private static bool _beforeBuildFlag = false;

        [PostProcessScene]
        private static void OnProcessScene()
        {

            if (!_beforeBuildFlag && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _beforeBuildFlag = true;

                if (OnBeforeBuildPlayerEvent != null)
                    OnBeforeBuildPlayerEvent();
                UnityEngine.Debug.Log("OnBeforeBuildPlayerEvent");
            }
        }

        /// <summary>
        /// Unity��׼Build������
        /// </summary>
        [PostProcessBuild()]
        private static void OnPostBuildPlayer(BuildTarget target, string pathToBuiltProject)
        {
            if (OnPostBuildPlayerEvent != null)
            {
                OnPostBuildPlayerEvent(target, pathToBuiltProject);
            }

            UnityEngine.Debug.Log(string.Format("Success Build ({0}) : {1}", target, pathToBuiltProject));
        }

        /// <summary>
        /// ����״̬�ı䣬����һЩ�����ԵĶ���, ���������ţ������ļ�������ű����������õ�
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            //AppEngine.IsAppPlaying = EditorApplication.isPlaying && !EditorApplication.isPaused;
            //Log.Info($"playModelChange isPlaying:{EditorApplication.isPlaying} ,isPaused:{EditorApplication.isPaused}");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!EditorApplication.isPlaying) // means Will Change to Playmode
                {
                    if (OnWillPlayEvent != null)
                    {
                        OnWillPlayEvent();
                    }
                }
                else
                {
                    if (OnBeginPlayEvent != null)
                    {
                        OnBeginPlayEvent();
                    }
                }
            }
            else
            {
                if (EditorApplication.isPlaying)
                {
                    if (OnWillStopEvent != null)
                    {
                        OnWillStopEvent();
                    }
                }
            }
        }

        /// <summary>
        /// ��׽��������С�ͬʱ������Ϸ��״̬��ǿ����ͣ���������г���
        /// </summary>
        /// <param name="view"></param>
        //static void OnSceneViewGUI(SceneView view)
        static void OnEditorUpdate()
        {
            CheckComplie();
            if (OnEditorUpdateEvent != null)
            {
                OnEditorUpdateEvent();
            }
        }

        private static void OnSceneViewGUI(SceneView sceneview)
        {
            CheckComplie();
        }

        // �������У�������ͣ��Ϸ
        static void CheckComplie()
        {
            //NOTE ��Unity2019������ΪRecompile After Finished Playing���޸Ĵ����������бȽ��ȶ��������޸Ĵ����ֹͣ���� 
            /*if (EditorApplication.isCompiling)
            {
                if (EditorApplication.isPlaying)
                {
                    UnityEngine.Debug.Log("Force Stop Play, because of Compiling.");
                    EditorApplication.isPlaying = false;
                }
            }*/
        }
    }

    internal class SaveSceneAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.Contains(".unity"))
                {
                    //scenePath = Path.GetDirectoryName(path);
                    //sceneName = Path.GetFileNameWithoutExtension(path);
                    UnityEditorEventCatcher._onSaveSceneEvent?.Invoke();
                }
            }

            return paths;
        }
    }

#if UNITY_2019_1_OR_NEWER
    class KBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        } //ԽС���ȼ�Խ��

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Before Build App");
            UnityEditorEventCatcher.OnBeforeBuildAppEvent?.Invoke();
        }
    }
#else
    class KBuildProcessor :   IPreprocessBuild
    {
        public int callbackOrder { get { return 0; } } //ԽС���ȼ�Խ��
      
        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            Debug.Log("Before Build App");
            KUnityEditorEventCatcher.OnBeforeBuildAppEvent?.Invoke();
        }
    }
#endif

}