using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;
using DecentM.Metrics;

public class MetricsBuildHook : IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => 7;

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        if (requestedBuildType != VRCSDKRequestedBuildType.Scene)
            return true;

        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            MetricsUI ui = root.GetComponentInChildren<MetricsUI>();

            if (ui == null)
                continue;

            URLStore urlStore = ui.GetComponentInChildren<URLStore>();

            MetricsUrlGenerator.SaveUrls(ui, urlStore);
        }

        return true;
    }
}
