
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using UdonSharp.Video;
using TMPro;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class InstructionRunner : UdonSharpBehaviour
{
    public LibDecentM lib;

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
    private bool running = false;

    private void Reset()
    {
        this.clock = 0;
        this.running = true;
        this.instructionIndex = 0;

        this.ReadInstructions();
    }

    private object[] CreateInstruction(int type = 0, int timestamp = 0, string value = "")
    {
        object[] instruction = new object[3];

        instruction[0] = type;
        instruction[1] = timestamp;
        instruction[2] = value;

        return instruction;
    }

    private void ReadInstructions()
    {
        if (this.instructionsFile == null)
        {
            return;
        }

        string[] lines = this.instructionsFile.text.Split('\n');
        this.instructions = new object[lines.Length][];
        int lastTimestamp = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(null, 3);

            // A line should always have 3 parts. If not, the line is corrupt and we skip over it
            if (parts.Length != 3)
            {
                continue;
            }

            int type = 0;
            int timestamp = 0;

            int.TryParse(parts[0], out type);
            int.TryParse(parts[1], out timestamp);

            // Skip clearing the canvas if the next instruction is less than .2 seconds away
            if (type == 2 && timestamp - lastTimestamp < 200 && timestamp - lastTimestamp > -200)
            {
                return;
            }

            string text = parts[2].Replace('ª', '\n');

            object[] instruction = this.CreateInstruction(type, timestamp, text);

            lastTimestamp = timestamp;
            this.instructions[i] = instruction;
        }
    }

    private bool isSeeking = false;

    private void FixedUpdate()
    {
        if (!this.initialised || !this.running)
        {
            return;
        }

        if (this.isSeeking)
        {
            this.TickInstruction();
            return;
        }

        this.clock++;
        if (this.clock >= (this.fixedUpdateRate / 10))
        {
            this.TickInstruction();
            this.clock = 0;
        }
    }

    private object[][] instructions = new object[0][];
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

        if (timeMillis - (int) instruction[1] > 10000 && this.instructionIndex < this.instructions.Length - 1)
        {
            this.isSeeking = true;
            this.debug.text = $"Seeking forward\n{timeMillis - (int)instruction[1]}";
            this.instructionIndex++;
            return;
        }

        if ((int)instruction[1] - timeMillis > 10000 && this.instructionIndex > 0)
        {
            this.isSeeking = true;
            this.debug.text = $"Seeking backwards\n{(int)instruction[1] - timeMillis}";
            this.instructionIndex--;
            return;
        }

        this.isSeeking = false;

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
