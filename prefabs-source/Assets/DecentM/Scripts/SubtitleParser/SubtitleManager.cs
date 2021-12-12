
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SubtitleManager : UdonSharpBehaviour
{
    public InstructionRunner runner;
    public int defaultIndex = 0;

    public TextAsset[] instructionsFiles;

    void Start()
    {
        if (this.instructionsFiles == null || this.instructionsFiles.Length == 0)
        {
            return;
        }

        this.runner.instructionsFile = this.instructionsFiles[this.defaultIndex];
    }
}
