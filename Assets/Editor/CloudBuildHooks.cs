// Must live in Assets/Editor: Unity Build Automation looks for pre-export methods here.
namespace ShadowFire.EditorTools
{
    public static class CloudBuildHooks
    {
        /// Set this as "Pre-Export Method" in the Build Automation target's Advanced settings:
        /// ShadowFire.EditorTools.CloudBuildHooks.PreExport
        public static void PreExport()
        {
            CIBuild.Prepare();
        }
    }
}
