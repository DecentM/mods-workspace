using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace DecentM.VideoPlayer.Plugins
{
    public class SubtitlesPlugin : VideoPlayerPlugin
    {
        public TextMeshProUGUI debugSlot;

        private TextAsset[] currentSubtitles;

        protected override void OnPlaybackStart(float timestamp)
        {
            if (timestamp < 10000) this.JumpToStart();
            else
            {
                int index = this.SearchForInstructionIndex(Mathf.FloorToInt(timestamp));
                if (index >= 0) this.instructionIndex = index;
            }
        }

        protected override void OnUnload()
        {
            this.Reset();
        }

        protected override void OnLoadApproved(VRCUrl url)
        {
            this.Reset();
        }

        private string GetLanguageFromFilename(string filename)
        {
            string[] parts = filename.Split('.');

            return parts[parts.Length - 1];
        }

        protected override void OnMetadataChange(string title, string uploader, string siteName, int viewCount, int likeCount, string resolution, int fps, string description, string duration, TextAsset[] subtitles)
        {
            this.currentSubtitles = subtitles;
            string[] langs = new string[subtitles.Length];

            for (int i = 0; i < subtitles.Length; i++)
            {
                langs[i] = this.GetLanguageFromFilename(subtitles[i].name);
            }

            this.events.OnSubtitleLanguageOptionsChange(langs);
        }

        protected override void OnSubtitleLanguageRequested(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                this.Reset();
                return;
            }

            this.events.OnSubtitleClear();

            for (int i = 0; i < currentSubtitles.Length; i++)
            {
                string filename = currentSubtitles[i].name;
                string content = currentSubtitles[i].text;

                if (language == this.GetLanguageFromFilename(filename))
                {
                    this.SetInstructions(content);
                    this.instructionIndex = this.SearchForInstructionIndex(Mathf.FloorToInt(this.system.GetTime()));
                    break;
                }
            }
        }

        public float subtitleOffset = 0;

        protected override void _Start()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.elapsed = 0;
            this.instructionIndex = 0;
            this.events.OnSubtitleClear();
            this.SetInstructions("");
        }

        private object[] CreateInstruction(int type = 0, int timestamp = 0, string value = "")
        {
            object[] instruction = new object[3];

            instruction[0] = type;
            instruction[1] = timestamp;
            instruction[2] = value;

            return instruction;
        }

        private const char NewlineDelimeter = '˟';

        public void SetInstructions(string newInstructions)
        {
            string[] lines = newInstructions.Split('\n');
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

                string text = parts[2].Replace(NewlineDelimeter, '\n');

                object[] instruction = this.CreateInstruction(type, timestamp, text);

                lastTimestamp = timestamp;
                this.instructions[i] = instruction;
            }
        }

        private float elapsed = 0;
        public float tickInterval = 0.1f;

        private void FixedUpdate()
        {
            if (!this.system.IsPlaying()) return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            // bool seeking = this.seekDirection != 0;
            if (this.elapsed >= this.tickInterval)
            {
                this.TickInstruction();
                this.elapsed = 0;
            }
        }

        private object[][] instructions = new object[0][];
        private int instructionIndex = 0;

        public void JumpToStart()
        {
            this.instructionIndex = 0;
        }

        public int timestampSeekAccuracy = 10000;

        private object[] GetInstructionAtIndex(int index)
        {
            if (this.instructions == null || this.instructions.Length == 0) return null;
            if (index < 0) return null;
            if (index >= this.instructions.Length) return this.instructions[this.instructions.Length - 1];

            return this.instructions[index];
        }

        private int SearchForInstructionIndex(int targetTimestamp)
        {
            float lowestDiff = float.PositiveInfinity;

            for (int i = 0; i < this.instructions.Length; i++)
            {
                object[] instruction = this.GetInstructionAtIndex(i);
                if (instruction == null || instruction.Length == 0) continue;

                int timestamp = (int)instruction[1];
                int diff = Mathf.Abs(timestamp - targetTimestamp);

                if (diff <= lowestDiff) lowestDiff = diff;
                else return i - 1;
            }

            return -1;
        }

        // -1, 0, or 1
        // private int seekDirection = 0;

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

            if (this.instructions == null || this.instructions.Length == 0)
            {
                if (this.debugSlot != null) this.debugSlot.text = "No subtitles set.";
                return;
            }

            // We've reached the end of the instructions, stop processing more
            if (this.instructionIndex >= this.instructions.Length)
            {
                if (this.debugSlot != null) this.debugSlot.text = "End of subtitles reached.";
                return;
            }

            int timeMillis = Mathf.RoundToInt(this.system.GetTime() * 1000) + Mathf.RoundToInt(this.subtitleOffset);
            object[] instruction = this.GetInstructionAtIndex(this.instructionIndex);
            object[] previousInstruction = this.GetInstructionAtIndex(this.instructionIndex - 1);

            if (instruction == null)
            {
                this.instructionIndex++;
                return;
            }

            int instructionTimestamp = (int)instruction[1];
            int diff = timeMillis - instructionTimestamp;

            if (previousInstruction != null)
            {
                int previousInstructionTimestamp = (int)previousInstruction[1];
                int previousDiff = timeMillis - previousInstructionTimestamp;

                // If we're far away from the next instruction, check if we need to seek
                // Seek if the previous instruction is in the future OR of the next instruction is in the past
                if (Mathf.Abs(diff) > this.timestampSeekAccuracy && (previousDiff < 0 || diff > 0))
                {
                    this.events.OnSubtitleClear();
                    int index = this.SearchForInstructionIndex(timeMillis);
                    if (index >= 0) this.instructionIndex = index;
                    return;
                }
            }

            if (this.debugSlot != null) this.debugSlot.text =
                $"Instructions: {this.instructions.Length}\n" +
                $"Next instruction index: {this.instructionIndex}\n" +
                $"Next instruction type: {((int)instruction[0] == 1 ? "write" : "clear")}\n" +
                $"Next instruction timestamp: {instruction[1]}\n" +
                $"Distance from next instruction: {diff}\n" +
                $"Current playback time {timeMillis}\n";

            // If the timestamp of the current instruction is in the past, it means we should be displaying it
            if ((int)instruction[1] < timeMillis)
            {
                this.ExecuteInstruction((int)instruction[0], (string)instruction[2]);
                this.instructionIndex++;
            }
        }

        private void ExecuteInstruction(int type, string value)
        {
            switch (type)
            {
                case 1:
                    this.events.OnSubtitleRender(value);
                    break;

                case 2:
                    this.events.OnSubtitleClear();
                    break;
            }
        }
    }
}
