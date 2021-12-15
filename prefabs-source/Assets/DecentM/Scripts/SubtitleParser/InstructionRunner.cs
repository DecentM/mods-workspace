
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
    public float subtitleOffset = 0;

    private TextAsset _instructionsFile;

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

    private void FixedUpdate()
    {
        if (!this.initialised || !this.running)
        {
            return;
        }

        this.clock++;
        bool seeking = this.seekDirection != 0;
        if (seeking || this.clock >= (this.fixedUpdateRate / 10))
        {
            this.TickInstruction();
            this.clock = 0;
        }
    }

    private object[][] instructions = new object[0][];
    private int instructionIndex = 0;

    private int SearchForInstructionIndex(int timestamp, int startIndex)
    {
        int cursor = startIndex;
        object[] currentInstruction = this.instructions[cursor];
        int currentTimestamp = (int)currentInstruction[1];
        int diff = timestamp - currentTimestamp;
        int loop = 0;

        if (diff < 0)
        {
            diff = diff * -1;
        }

        // Binary search for an instruction **behind** the needed one by about 10 seconds.
        // Accuracy of 10 seconds, because the default behaviour is that
        // the instructions tick forward quickly if their timestamps are in the past.
        while (diff > -5000 && loop < 10)
        {
            // If we're smaller that the needed timestamp, we search forward.
            if (currentTimestamp < timestamp - 5000)
            {
                cursor = cursor + ((this.instructions.Length - cursor) / 2);
            } else if (currentTimestamp > timestamp) // We search backwards
            {
                cursor = cursor / 2;
            }

            currentInstruction = this.instructions[cursor];
            currentTimestamp = (int)currentInstruction[1];

            diff = timestamp - currentTimestamp;

            loop++;
        }

        Debug.Log($"Found index at {cursor} with offset {currentTimestamp - timestamp}");

        return cursor;
    }

    // -1, 0, or 1
    private int seekDirection = 0;

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
        if (this.instructionsFile == null || this.instructions == null || this.instructionIndex >= this.instructions.Length)
        {
            this.running = false;
            return;
        }

        int timeMillis = Mathf.RoundToInt(this.player.GetVideoManager().GetTime() * 1000) + Mathf.RoundToInt(this.subtitleOffset);
        object[] instruction = this.instructions[this.instructionIndex];

        if (instruction == null)
        {
            return;
        }

        int diff = timeMillis - (int)instruction[1];

        this.debug.text = $"index {this.instructionIndex}\n" +
            $"system type {instruction}\n" +
            $"type {((int)instruction[0] == 1 ? "write" : "clear")}\n" +
            $"timestamp {instruction[1]}\n" +
            $"current time {timeMillis}\n" +
            $"diff: {diff}\n" +
            $"seeking: {this.seekDirection}";

        // if we're behind, start seeking forward
        if (diff > 10000)
        {
            this.seekDirection = 1;
        // if we're ahead, start seeking backward
        } else if (diff < -10000)
        {
            this.seekDirection = -1;
        // stop seeking if we're about right
        } else if (this.seekDirection != 0)
        {
            this.text.text = "";
            this.seekDirection = 0;
        }

        this.instructionIndex = Mathf.Max(this.instructionIndex + this.seekDirection, 0);

        // If we're seeking, we make a progress report to the player
        //if (this.seekDirection != 0)
        //{
        //    this.text.text = $"Seeking... ({(diff < 0 ? diff * -1 : diff)})";
        //}

        // If the timestamp of the current instruction is in the past, it means we should be displaying it
        if ((int) instruction[1] < timeMillis)
        {
            // Prevent writing to the screen while seeking
            if (this.seekDirection == 0)
            {
                this.ExecuteInstruction((int)instruction[0], (string)instruction[2]);
            }

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
