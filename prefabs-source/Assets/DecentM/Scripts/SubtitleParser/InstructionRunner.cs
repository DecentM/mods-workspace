
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
    // TODO: make this private, and use udon from a central manager object to set srtFile
    public TextAsset _instructionsFile;

    public TextAsset instructionsFile
    {
        get {
            return this._instructionsFile;
        }

        set
        {
            this._instructionsFile = value;
            this.Reset();
        }
    }
    public float precision = 0.1f;
    public TextMeshProUGUI text;
    public TextMeshProUGUI debug;
    public USharpVideoPlayer player;

    private bool initialised = false;
    private float fixedUpdateRate;

    private void Start()
    {
        this.fixedUpdateRate = 1 / Time.fixedDeltaTime;
        this.initialised = true;
        this.Reset();
    }

    private int clock = 0;
    private bool running = true;

    private void Reset()
    {
        this.clock = 0;
        this.running = true;
        this.instructionIndex = 0;
        this.instructions = new object[0][];
    }

    private ReadInstructions()
    {
        string[] lines = this.instructionsFile.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string[] instructionParts = lines[i].Split(null, 3);
        }
    }

    private void FixedUpdate()
    {
        if (!this.initialised || !this.running)
        {
            return;
        }

        this.clock++;
        if (this.clock >= (this.fixedUpdateRate / 10))
        {
            this.TickInstruction();
            this.clock = 0;
        }
    }

    private object[][] instructions;
    private int instructionIndex = 0;

    private void TickInstruction()
    {
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
        
        // We've reached the end of the instructions, stop processing more
        if (instructionIndex >= this.instructions.Length)
        {
            this.running = false;
            return;
        }

        int timeMillis = Mathf.RoundToInt(this.player.GetVideoManager().GetTime() * 1000);
        object[] instruction = this.instructions[this.instructionIndex];

        this.debug.text = $"index {this.instructionIndex}\n" +
            $"system type {instruction}\n" +
            $"type {instruction[0]}\n" +
            $"timestamp {instruction[1]}\n" +
            $"current time {timeMillis}";

        if (instruction == null)
        {
            this.debug.text = "Instruction is null";
            return;
        }

        if ((int) instruction[1] < timeMillis)
        {
            this.debug.text = (int) instruction[0] == 1 ? "Writing text" : "Clearing canvas";
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
