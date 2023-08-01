using UnityEditor;

public class BuildScript
{
    [MenuItem("ind3x/Build/Build All")]
    static void BuildAll()
    {
        // Mobile
        BuildIos();
        BuildAndroid();

        // Desktop
        BuildWindows();
        BuildLinux();
        BuildOSX();

        // Web
        BuildWebGL();
    }

    #region Mobile
    [MenuItem("ind3x/Build/Build iOS")]
    static void BuildIos()
    {
        string projectName = PlayerSettings.productName;
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./builds/ios/{projectName}", BuildTarget.iOS, BuildOptions.None);
    }

    [MenuItem("ind3x/Build/Build Android")]
    static void BuildAndroid()
    {
        string projectName = PlayerSettings.productName;
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./builds/android/{projectName}.apk", BuildTarget.Android, BuildOptions.None);
    }
    #endregion

    #region Desktop
    [MenuItem("ind3x/Build/Build Windows")]
    static void BuildWindows()
    {
        string projectName = PlayerSettings.productName;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 0);
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./builds/windows/{projectName}.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    [MenuItem("ind3x/Build/Build Linux")]
    static void BuildLinux()
    {
        string projectName = PlayerSettings.productName;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./builds/linux/{projectName}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.None);
    }

    [MenuItem("ind3x/Build/Build OS X")]
    static void BuildOSX()
    {
        string projectName = PlayerSettings.productName;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, (int)AppleMobileArchitecture.ARM64);
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./builds/osx/{projectName}", BuildTarget.StandaloneOSX, BuildOptions.None);
    }
    #endregion

    #region Web
    [MenuItem("ind3x/Build/Build WebGL")]
    static void BuildWebGL()
    {
        string projectName = PlayerSettings.productName;
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./builds/webgl", BuildTarget.WebGL, BuildOptions.None);
    }
    #endregion

    static void PerformAssetBundleBuild()
    {
        // BuildPipeline.BuildAssetBundles("../AssetBundles/", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneLinux64);
    }
}