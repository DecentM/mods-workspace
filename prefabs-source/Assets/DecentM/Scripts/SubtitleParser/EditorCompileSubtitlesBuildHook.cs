using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

public class CompileSubtitlesBuildHook : IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => 99;

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        if (requestedBuildType == VRCSDKRequestedBuildType.Avatar)
            return true;

        return true;
    }
}
