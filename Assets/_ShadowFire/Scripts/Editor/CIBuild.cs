using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace ShadowFire.EditorTools
{
    /// Shared by GitHub (GameCI) and Unity Build Automation.
    public static class CIBuild
    {
        /// Generates scenes + applies Android player settings. No player build.
        public static void Prepare()
        {
            GreyboxBuilder.BuildAll();
            PlayerSettings.companyName = "ShadowFire";
            PlayerSettings.productName = "Shadow Fire";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.shadowfire.game");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            EditorUserBuildSettings.buildAppBundle = false;
        }

        /// GameCI entry: prepare + build APK to build/Android/ShadowFire.apk
        public static void Android()
        {
            Prepare();
            var opts = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "build/Android/ShadowFire.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var rep = BuildPipeline.BuildPlayer(opts);
            if (rep.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
