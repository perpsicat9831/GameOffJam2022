using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace Framework
{
    public enum KResourceQuality
    {
        Hd = 1,
        Sd = 2,
        Ld = 4,
    }
    public class ResourceModule : MonoBehaviour
    {
        /// <summary>
        /// ����GetResourceFullPath���������ص������ж�
        /// </summary>
        public enum GetResourceFullPathType
        {
            /// <summary>
            /// ����Դ
            /// </summary>
            Invalid,

            /// <summary>
            /// ��װ����
            /// </summary>
            InApp,

            /// <summary>
            /// �ȸ���Ŀ¼
            /// </summary>
            InDocument,
        }

        public static KResourceQuality Quality = KResourceQuality.Sd;

        public static float TextureScale
        {
            get { return 1f / (float)Quality; }
        }

        public static bool LoadByQueue = false;

        #region Init

        private static ResourceModule _Instance;

        public static ResourceModule Instance
        {
            get
            {
                if (_Instance == null)
                {
                    GameObject resMgr = GameObject.Find("_ResourceModule_");
                    if (resMgr == null)
                    {
                        resMgr = new GameObject("_ResourceModule_");
                        GameObject.DontDestroyOnLoad(resMgr);
                    }

                    _Instance = resMgr.AddComponent<ResourceModule>();
                }

                return _Instance;
            }
        }

        static ResourceModule()
        {
            InitResourcePath();
        }

        /// <summary>
        /// Initialize the path of AssetBundles store place ( Maybe in PersitentDataPath or StreamingAssetsPath )
        /// </summary>
        /// <returns></returns>
        static void InitResourcePath()
        {
            string editorProductPath = EditorProductFullPath;
            BundlesPathRelative = string.Format("{0}/{1}/", AppConfig.StreamingBundlesFolderName, GetBuildPlatformName());
            string fileProtocol = GetFileProtocol;
            AppDataPathWithProtocol = fileProtocol + AppDataPath;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    {
                        if (AppConfig.ReadStreamFromEditor)
                        {
                            AppBasePath = Application.streamingAssetsPath + "/";
                            AppBasePathWithProtocol = fileProtocol + AppBasePath;
                        }
                        else
                        {
                            AppBasePath = editorProductPath + "/";
                            AppBasePathWithProtocol = fileProtocol + AppBasePath;
                        }
                    }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    {
                        string path = Application.streamingAssetsPath.Replace('\\', '/');
                        AppBasePath = path + "/";
                        AppBasePathWithProtocol = fileProtocol + AppBasePath;
                    }
                    break;
                case RuntimePlatform.Android:
                    {
                        //�ĵ���https://docs.unity3d.com/Manual/StreamingAssets.html
                        //ע�⣬StramingAsset��Androidƽ̨����apk�У��޷�ͨ��File��ȡ��ʹ��LoadAssetsSync�����Ҫͬ����ȡab��ʹ��GetAbFullPath
                        AppBasePath = Application.dataPath + "!/assets/";
                        AppBasePathWithProtocol = fileProtocol + AppBasePath;
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    {
                        // MacOSX�£����ո���ļ��У��ո��ַ���Ҫת���%20
                        // only iPhone need to Escape the fucking Url!!! other platform works without it!!!
                        AppBasePath = System.Uri.EscapeUriString(Application.dataPath + "/Raw");
                        AppBasePathWithProtocol = fileProtocol + AppBasePath;
                    }
                    break;
                default:
                    {
                        Debuger.Assert(false);
                    }
                    break;
            }
        }

        #endregion

        #region Path Def

        /**·��˵��
         * Editor��ģ��������Դ��
         *     AppData:C:\xxx\xxx\Appdata
         *     StreamAsset:C:\KSFramrwork\Product
         * �����
         *     AppData:Android\data\com.xxx.xxx\files\
         *     StreamAsset:apk��
         */
        private static string editorProductFullPath;

        /// <summary>
        /// Product Folder Full Path , Default: C:\KSFramework\Product
        /// </summary>
        public static string EditorProductFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(editorProductFullPath))
                    editorProductFullPath = Path.GetFullPath(AppConfig.ProductRelPath);
                return editorProductFullPath;
            }
        }


        /// <summary>
        /// ��װ���ڵ�·�����ƶ�ƽ̨Ϊֻ��Ȩ�ޣ����Application.streamingAssetsPath���ж�ƽ̨������/��β
        /// </summary>
        public static string AppBasePath { get; private set; }

        /// <summary>
        /// WWW�Ķ�ȡ��Ҫfile://ǰ׺
        /// </summary>
        public static string AppBasePathWithProtocol { get; private set; }


        private static string appDataPath = null;
        /// <summary>
        /// app������Ŀ¼���ж�дȨ�ޣ�ʵ����Application.persistentDataPath����/��β
        /// </summary>
        public static string AppDataPath
        {
            get
            {
                if (appDataPath == null) appDataPath = Application.persistentDataPath + "/";
                return appDataPath;
            }
        }

        /// <summary>
        /// file://+Application.persistentDataPath
        /// </summary>
        public static string AppDataPathWithProtocol;

        /// <summary>
        /// Bundles/Android/ etc... no prefix for streamingAssets
        /// </summary>
        public static string BundlesPathRelative { get; private set; }

        /// <summary>
        /// On Windows, file protocol has a strange rule that has one more slash
        /// </summary>
        /// <returns>string, file protocol string</returns>
        public static string GetFileProtocol
        {
            get
            {
                string fileProtocol = "file://";
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer
#if UNITY_5 || UNITY_4
                || Application.platform == RuntimePlatform.WindowsWebPlayer
#endif
                )
                    fileProtocol = "file:///";

                return fileProtocol;
            }
        }

        /// <summary>
        /// Unity Editor load AssetBundle directly from the Asset Bundle Path,
        /// whth file:// protocol
        /// </summary>
        public static string EditorAssetBundleFullPath
        {
            get
            {
                string editorAssetBundlePath = Path.GetFullPath(AppConfig.AssetBundleBuildRelPath); // for editoronly
                return editorAssetBundlePath;
            }
        }

        #endregion

        /// <summary>
        /// ��ȡab�ļ�������·�������Ĵ��������ab��ʽ�ĺ�׺���������apk������������jar:file://ǰ׺
        /// </summary>
        /// <param name="path">���·��</param>
        /// <returns></returns>
        public static string GetAbFullPath(string path)
        {
            if (!path.EndsWith(AppConfig.AssetBundleExt)) path = path + AppConfig.AssetBundleExt;
            var _fullUrl = GetResourceFullPath(BundlesPathRelative + path, false);
            if (!string.IsNullOrEmpty(_fullUrl))
            {
                if (Application.platform == RuntimePlatform.Android && _fullUrl.StartsWith("/data/app"))
                {
                    return "jar:file://" + _fullUrl;//���apk�������ǰ׺��������unity2019.3.7f1+android6.0����Appbase��Ч
                }
            }

            return _fullUrl;
        }

        /// <summary>
        /// ��Դ�Ƿ����
        /// </summary>
        /// <param name="url">���·��</param>
        /// <param name="raiseError">�ļ������ڴ�ӡError</param>
        /// <returns></returns>
        public static bool IsResourceExist(string url, bool raiseError = true)
        {
            var pathType = GetResourceFullPath(url, false, out string fullPath, raiseError);
            return pathType != GetResourceFullPathType.Invalid;
        }

        /// <summary>
        /// ����·�������ȼ����ȸ�Ŀ¼->��װ��
        /// ��·����Product
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">�Ƿ����file://ǰ׺</param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public static string GetResourceFullPath(string url, bool withFileProtocol = false, bool raiseError = true)
        {
            string fullPath;
            if (GetResourceFullPath(url, withFileProtocol, out fullPath, raiseError) != GetResourceFullPathType.Invalid)
                return fullPath;
            return null;
        }

        /// <summary>
        /// �������·������ȡ������·�������ȴ����d�YԴĿ¼�ң�û�оͶ������YԴĿ� 
        /// ��·����Product
        /// </summary>
        /// <param name="url">���·��</param>
        /// <param name="withFileProtocol"></param>
        /// <param name="fullPath">����·��</param>
        /// <param name="raiseError">�ļ������ڴ�ӡError</param>
        /// <returns></returns>
        public static GetResourceFullPathType GetResourceFullPath(string url, bool withFileProtocol, out string fullPath, bool raiseError = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.Error("���Ի�ȡһ���յ���Դ·����");
                fullPath = null;
                return GetResourceFullPathType.Invalid;
            }
            string docUrl;
            bool hasDocUrl = TryGetAppDataUrl(url, withFileProtocol, out docUrl);
            if (hasDocUrl)
            {
                fullPath = docUrl;
                return GetResourceFullPathType.InDocument;
            }

            string inAppUrl;
            bool hasInAppUrl = TryGetInAppStreamingUrl(url, withFileProtocol, out inAppUrl);
            if (!hasInAppUrl) // ��������Դ��û�У�ֱ��ʧ�ܰ� ���� 
            {
                if (raiseError) Log.Error($"[Not Found] StreamingAssetsPath Url Resource: {url} ,fullPath:{inAppUrl}");
                fullPath = null;
                return GetResourceFullPathType.Invalid;
            }

            fullPath = inAppUrl; // ֱ��ʹ�ñ����YԴ��

            return GetResourceFullPathType.InApp;
        }

        /// <summary>
        /// ��ȡһ����Դ������·������apkѹ�����ڻ����ڿɶ�д·����
        /// </summary>
        /// <param name="fullPath"></param>
        public static GetResourceFullPathType GetResFullPathType(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                Log.Error("�޷�ʶ��һ���յ���Դ·����");
                return GetResourceFullPathType.Invalid;
            }

            if (Application.platform == RuntimePlatform.Android)
                return fullPath.StartsWith("/data/app") ? GetResourceFullPathType.InApp : GetResourceFullPathType.InDocument;
            return fullPath.StartsWith(AppDataPath) ? GetResourceFullPathType.InApp : GetResourceFullPathType.InDocument;
        }

        /// <summary>
        /// use AssetDatabase.LoadAssetAtPath insead of load asset bundle, editor only
        /// </summary>
        public static bool IsEditorLoadAsset
        {
            get { return Application.isEditor && AppConfig.IsEditorLoadAsset; }
        }

        /// <summary>
        /// �ɶ�д��Ŀ¼
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">�Ƿ����file://ǰ׺</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetAppDataUrl(string url, bool withFileProtocol, out string newUrl)
        {
            newUrl = Path.GetFullPath((withFileProtocol ? AppDataPathWithProtocol : AppDataPath) + url);
            return File.Exists(Path.GetFullPath(AppDataPath + url));
        }

        /// <summary>
        /// StreamingAssetsĿ¼
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">�Ƿ����file://ǰ׺</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetInAppStreamingUrl(string url, bool withFileProtocol, out string newUrl)
        {
            newUrl = Path.GetFullPath((withFileProtocol ? AppBasePathWithProtocol : AppBasePath) + url);

            if (Application.isEditor)
            {
                // Editor�����ļ����
                if (!File.Exists(Path.GetFullPath(AppBasePath + url)))
                {
                    return false;
                }
            }

            // Windows/Editorƽ̨�£����д�С�����ж�
            if (Application.isEditor)
            {
                var result = FileExistsWithDifferentCase(AppBasePath + url);
                if (!result)
                {
                    Log.Error("[��Сд����]����һ����Դ {0}����Сд�������⣬��Windows���Զ�ȡ���ֻ����У���ı��޸ģ�", url);
                }
            }

            return true;
        }

        /// <summary>
        /// ��Сд���еؽ����ļ��ж�, Windows Only
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool FileExistsWithDifferentCase(string filePath)
        {
            if (File.Exists(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileTitle = Path.GetFileName(filePath);
                string[] files = Directory.GetFiles(directory, fileTitle);
                var realFilePath = files[0].Replace("\\", "/");
                filePath = filePath.Replace("\\", "/");
                filePath = filePath.Replace("//", "/");

                return String.CompareOrdinal(realFilePath, filePath) == 0;
            }

            return false;
        }

        private static string _unityEditorEditorUserBuildSettingsActiveBuildTarget;

        /// <summary>
        /// UnityEditor.EditorUserBuildSettings.activeBuildTarget, Can Run in any platform~
        /// </summary>
        public static string UnityEditor_EditorUserBuildSettings_activeBuildTarget
        {
            get
            {
                if (Application.isPlaying && !string.IsNullOrEmpty(_unityEditorEditorUserBuildSettingsActiveBuildTarget))
                {
                    return _unityEditorEditorUserBuildSettingsActiveBuildTarget;
                }

                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var a in assemblies)
                {
                    if (a.GetName().Name == "UnityEditor")
                    {
                        Type lockType = a.GetType("UnityEditor.EditorUserBuildSettings");
                        //var retObj = lockType.GetMethod(staticMethodName,
                        //    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                        //    .Invoke(null, args);
                        //return retObj;
                        var p = lockType.GetProperty("activeBuildTarget");

                        var em = p.GetGetMethod().Invoke(null, new object[] { }).ToString();
                        _unityEditorEditorUserBuildSettingsActiveBuildTarget = em;
                        return em;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Different platform's assetBundles is incompatible.// ex: IOS, Android, Windows
        /// KEngine put different platform's assetBundles in different folder.
        /// Here, get Platform name that represent the AssetBundles Folder.
        /// </summary>
        /// <returns>Platform folder Name</returns>
        public static string GetBuildPlatformName()
        {
            string buildPlatformName = "Windows"; // default

            if (Application.isEditor)
            {
                var buildTarget = UnityEditor_EditorUserBuildSettings_activeBuildTarget;
                //UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                switch (buildTarget)
                {
                    case "StandaloneOSXIntel":
                    case "StandaloneOSXIntel64":
                    case "StandaloneOSXUniversal":
                    case "StandaloneOSX":
                        buildPlatformName = "MacOS";
                        break;
                    case "StandaloneWindows": // UnityEditor.BuildTarget.StandaloneWindows:
                    case "StandaloneWindows64": // UnityEditor.BuildTarget.StandaloneWindows64:
                        buildPlatformName = "Windows";
                        break;
                    case "Android": // UnityEditor.BuildTarget.Android:
                        buildPlatformName = "Android";
                        break;
                    case "iPhone": // UnityEditor.BuildTarget.iPhone:
                    case "iOS":
                        buildPlatformName = "iOS";
                        break;
                    default:
                        Debuger.Assert(false);
                        break;
                }
            }
            else
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXPlayer:
                        buildPlatformName = "MacOS";
                        break;
                    case RuntimePlatform.Android:
                        buildPlatformName = "Android";
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        buildPlatformName = "iOS";
                        break;
                    case RuntimePlatform.WindowsPlayer:
#if !UNITY_5_4_OR_NEWER
                    case RuntimePlatform.WindowsWebPlayer:
#endif
                        buildPlatformName = "Windows";
                        break;
                    default:
                        Debuger.Assert(false);
                        break;
                }
            }

            if (Quality != KResourceQuality.Sd) // SD no need add
                buildPlatformName += Quality.ToString().ToUpper();
            return buildPlatformName;
        }

        /// <summary>
        /// Load file. On Android will use plugin to do that.
        /// </summary>
        /// <param name="path">relative path,  when file is "file:///android_asset/test.txt", the pat is "test.txt"</param>
        /// <returns></returns>
        public static byte[] LoadAssetsSync(string path)
        {
            string fullPath = GetResourceFullPath(path, false);
            if (string.IsNullOrEmpty(fullPath))
                return null;

            return ReadAllBytes(fullPath);
        }

        /// <summary>
        /// �������ļ���ֱ�Ӷ�bytes
        /// </summary>
        /// <param name="resPath"></param>
        public static byte[] ReadAllBytes(string resPath)
        {
            byte[] bytes;
            using (FileStream fs = File.Open(resPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
            }

            return bytes;
        }

        /// <summary>
        /// Collect all KEngine's resource unused loaders
        /// </summary>
        public static void Collect()
        {
            while (ABManager.UnUsesLoaders.Count > 0)
                ABManager.DoGarbageCollect();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        #region Unity����

        private void Awake()
        {
            if (_Instance != null)
                Debuger.Assert(_Instance == this);
            SpriteAtlasManager.atlasRequested += ABManager.RequestAtlas;
            if (AppConfig.IsLogDeviceInfo)
            {
                //���������⼸��·��
                Log.LogInfo("ResourceManager AppBasePath:{0} ,AppBasePathWithProtocol:{1}", AppBasePath, AppBasePathWithProtocol);
                Log.LogInfo("ResourceManager AppDataPath:{0} ,AppDataPathWithProtocol:{1}", AppDataPath, AppDataPathWithProtocol);
            }
        }

        private void Update()
        {
            //NOTE ��Unity2019���н���ʽGC�����˴��������GC.Collect���������Ѽ��ص�ab���м���Ƿ���ҪUnload
            ABManager.CheckGcCollect();
        }

        private void OnDestroy()
        {
            SpriteAtlasManager.atlasRequested -= ABManager.RequestAtlas;
        }

        #endregion
    }
}