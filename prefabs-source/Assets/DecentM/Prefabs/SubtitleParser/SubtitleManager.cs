
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class SubtitleManager : UdonSharpBehaviour
{
    public InstructionRunner runner;

    [UdonSynced, FieldChangeCallback(nameof(index))]
    private int _index = 0;
    public int index
    {
        get
        {
            return this._index;
        }

        set
        {
            this._index = value;
            this.UpdateInstructions();
        }
    }

    public TextAsset[] instructionsFiles;

    void Start()
    {
        if (this.instructionsFiles == null || this.instructionsFiles.Length == 0)
        {
            Debug.LogError("No instructions files were set. You must set at least one parsed .srt file TextAsset.");
            this.enabled = false;
            return;
        }
    }

    private void UpdateInstructions()
    {
        this.runner.instructionsFile = this.instructionsFiles[this.index];
    }
}
