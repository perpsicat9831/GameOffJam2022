using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
/// <summary>
/// һЩ����
/// </summary>
public class AppConfig
{
    public static string ProjectName = "ImbalanceLand";

    /// <summary>
    /// �������λ��
    /// </summary>
    public static string CsvDataPath = "datas";//Assets/Resources/datas
    /// <summary>
    /// �༭����Ĭ�ϲ���ӡӲ���豸����Ϣ�����д�뵽��־��
    /// </summary>
    public static bool IsLogDeviceInfo = false;
    /// <summary>
    /// �Ƿ��ӡab���ص���־
    /// </summary>
    public static bool IsLogAbInfo = false;
    /// <summary>
    /// �Ƿ��ӡab���غ�ʱ
    /// </summary>
    public static bool IsLogAbLoadCost = false;
    /// <summary>
    /// �Ƿ񴴽�AssetDebugger
    /// </summary>
    public static bool UseAssetDebugger = Application.isEditor;
    /// <summary>
    /// �Ƿ��¼���ļ��У�������UI��ab���غ�ʱ��UI����ִ�к�ʱ
    /// </summary>
    public static bool IsSaveCostToFile = false;
    /// <summary>
    /// ����Editor��Ч��Editor�¼�����ԴĬ�ϴӴ��̵����Ŀ¼��ȡ�������Ҫ��Aplication.streamingAssets������Ϊtrue
    /// </summary>
    public static bool ReadStreamFromEditor;
    /// <summary>
    /// ���ab�ļ��ĺ�׺
    /// </summary>
    public const string AssetBundleExt = ".ab";
    /// <summary>
    /// ProductĿ¼�����·��
    /// </summary>
    public const string ProductRelPath = "Product";

    public const string AssetBundleBuildRelPath = "Product/Bundles";
    public const string StreamingBundlesFolderName = "Bundles";
    public const string LuaPath = "Lua";
    public const string SettingResourcesPath = "Setting";

    public static bool IsLoadAssetBundle = true;
    //whether use assetdata.loadassetatpath insead of load asset bundle, editor only
    public static bool IsEditorLoadAsset = false;


    public static bool IsDownloadRes = false;

    public static string VersionTextPath
    {
        get { return ResourceModule.AppBasePath + "/" + VersionTxtName; }
    }
    public static string VersionTxtName
    {
        get { return ResourceModule.GetBuildPlatformName() + "-version.txt"; }
    }

    /// <summary>
    /// UI��Ƶķֱ���
    /// </summary>
    public static Vector2 UIResolution = new Vector2(1280, 720);

    public static string UseABPrefsKey = "UseAB";
}
