using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using DecentM.Subtitles;

public class CompileSubtitlesBuildHook : IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => 99;
    private Compiler compiler = new Compiler();

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        if (requestedBuildType == VRCSDKRequestedBuildType.Avatar)
        {
            return true;
        }

        compiler.Compile();

        return true;
    }
}
