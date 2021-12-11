
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using UdonSharp.Video;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class InstructionRunner : UdonSharpBehaviour
{
    public TextAsset srtFile;
    public SRTParser parser;
    public float precision = 0.1f;
    public TextMeshProUGUI text;
    public USharpVideoPlayer player;

    private bool initialised = false;

    private void Start()
    {
        this.instructions = this.parser.ParseToInstructions(this.srtFile.text);
        this.initialised = true;
    }

    private int clock = 0;

    private void FixedUpdate()
    {
        if (!this.initialised)
        {
            return;
        }

        clock++;
        if (clock > 1 / Time.fixedDeltaTime / precision)
        {
            clock = 0;
            this.Run();
        }
    }

    private object[][] instructions;
    private int instructionIndex = 0;

    private void Run()
    {
        Debug.Log("Run()");

        /** Index map
         * 0 - type
         * 1 - timestamp
         * 2 - value
         **/

        /**
         * Types:
         * 0 - unknown / transform error
         * 1 - RenderText
         * 2 - Clear
         **/

        int timeMillis = Mathf.RoundToInt(this.player.GetVideoManager().GetTime() / 1000);
        object[] instruction = this.instructions[this.instructionIndex];

        if (instruction == null)
        {
            return;
        }

        if ((int)instruction[1] < timeMillis)
        {
            this.ExecuteInstruction((int) instruction[0], (string) instruction[2]);
            this.instructionIndex++;
        }
    }

    private void ExecuteInstruction(int type, string value)
    {
        switch (type)
        {
            case 1:
                this.text.text = value;
                break;

            case 2:
                this.text.text = "";
                break;
        }
    }
}
