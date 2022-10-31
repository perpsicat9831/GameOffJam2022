using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
/// <summary>
/// 一些配置
/// </summary>
public class AppConfig
{
    public static string ProjectName = "ImbalanceLand";

    /// <summary>
    /// 表格数据位置
    /// </summary>
    public static string CsvDataPath = "datas";//Assets/Resources/datas
    /// <summary>
    /// 编辑器下默认不打印硬件设备的信息，真机写入到日志中
    /// </summary>
    public static bool IsLogDeviceInfo = false;
    /// <summary>
    /// 是否打印ab加载的日志
    /// </summary>
    public static bool IsLogAbInfo = false;
    /// <summary>
    /// 是否打印ab加载耗时
    /// </summary>
    public static bool IsLogAbLoadCost = false;
    /// <summary>
    /// 是否创建AssetDebugger
    /// </summary>
    public static bool UseAssetDebugger = Application.isEditor;
    /// <summary>
    /// 是否记录到文件中，包括：UI的ab加载耗时，UI函数执行耗时
    /// </summary>
    public static bool IsSaveCostToFile = false;
    /// <summary>
    /// 仅对Editor有效，Editor下加载资源默认从磁盘的相对目录读取，如果需要从Aplication.streamingAssets则设置为true
    /// </summary>
    public static bool ReadStreamFromEditor;
    /// <summary>
    /// 打包ab文件的后缀
    /// </summary>
    public const string AssetBundleExt = ".ab";
    /// <summary>
    /// Product目录的相对路径
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
    /// UI设计的分辨率
    /// </summary>
    public static Vector2 UIResolution = new Vector2(1280, 720);

    public static string UseABPrefsKey = "UseAB";
}
