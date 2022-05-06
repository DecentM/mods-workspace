
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class SubtitlesPlugin : VideoPlayerPlugin
    {
        private string[][] currentSubtitles;

        protected override void OnPlaybackStart(float timestamp)
        {
            if (timestamp < 10000) this.JumpToStart();
            else this.SeekToTimestamp(Mathf.FloorToInt(timestamp));
        }

        protected override void OnUnload()
        {
            this.Reset();
        }

        protected override void OnMetadataChange(string title, string uploader, string siteName, int viewCount, int likeCount, string resolution, int fps, string description, string duration, string[][] subtitles)
        {
            this.currentSubtitles = subtitles;
            string[] langs = new string[subtitles.Length];

            for (int i = 0; i < subtitles.Length; i++)
            {
                langs[i] = subtitles[i][0];
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

            for (int i = 0; i < currentSubtitles.Length; i++)
            {
                string lang = currentSubtitles[i][0];
                string content = currentSubtitles[i][1];

                if (language == lang)
                {
                    this.SetInstructions(content);
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

                // Skip clearing the canvas if the next instruction is less than .2 seconds away
                // Commented out because this is now handled by the compiler in the editor instead of runtime
                /* if (type == 2 && timestamp - lastTimestamp < 200 && timestamp - lastTimestamp > -200)
                {
                    return;
                } */

                string text = parts[2].Replace(NewlineDelimeter, '\n');

                object[] instruction = this.CreateInstruction(type, timestamp, text);

                lastTimestamp = timestamp;
                this.instructions[i] = instruction;
            }
        }

        private float elapsed = 0;

        private void FixedUpdate()
        {
            if (!this.system.IsPlaying()) return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            bool seeking = this.seekDirection != 0;
            if (seeking || this.elapsed >= 0.1)
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

        public void SeekToTimestamp(int timestamp)
        {
            this.SearchForInstructionIndex(timestamp, instructionIndex);
        }

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

            // Search for an instruction **behind** the needed one by about 10 seconds.
            // Accuracy of 10 seconds, because the default behaviour is that
            // the instructions tick forward quickly if their timestamps are in the past.
            while (diff > -5000 && loop < 10)
            {
                // If we're smaller that the needed timestamp, we search forward.
                if (currentTimestamp < timestamp - 5000)
                {
                    cursor = cursor + ((this.instructions.Length - cursor) / 2);
                }
                else if (currentTimestamp > timestamp) // We search backwards
                {
                    cursor = cursor / 2;
                }

                currentInstruction = this.instructions[cursor];
                currentTimestamp = (int)currentInstruction[1];

                diff = timestamp - currentTimestamp;

                loop++;
            }

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
            if (this.instructions == null || this.instructionIndex >= this.instructions.Length)
            {
                return;
            }

            int timeMillis = Mathf.RoundToInt(this.system.GetTime() * 1000) + Mathf.RoundToInt(this.subtitleOffset);
            object[] instruction = this.instructions[this.instructionIndex];

            if (instruction == null)
            {
                return;
            }

            int diff = timeMillis - (int)instruction[1];

            /*
              this.debug.text = $"index {this.instructionIndex}\n" +
                $"system type {instruction}\n" +
                $"type {((int)instruction[0] == 1 ? "write" : "clear")}\n" +
                $"timestamp {instruction[1]}\n" +
                $"current time {timeMillis}\n" +
                $"diff: {diff}\n" +
                $"seeking: {this.seekDirection}";
            */

            // if we're behind, start seeking forward
            if (diff > 10000)
            {
                this.seekDirection = 1;
                // if we're ahead, start seeking backward
            }
            else if (diff < -10000)
            {
                this.seekDirection = -1;
                // stop seeking if we're about right
            }
            else if (this.seekDirection != 0)
            {
                this.events.OnSubtitleClear();
                this.seekDirection = 0;
            }

            this.instructionIndex = Mathf.Max(this.instructionIndex + this.seekDirection, 0);

            // If we're seeking, we make a progress report to the player
            //if (this.seekDirection != 0)
            //{
            //    this.text.text = $"Seeking... ({(diff < 0 ? diff * -1 : diff)})";
            //}

            // If the timestamp of the current instruction is in the past, it means we should be displaying it
            if ((int)instruction[1] < timeMillis)
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
                    this.events.OnSubtitleRender(value);
                    break;

                case 2:
                    this.events.OnSubtitleClear();
                    break;
            }
        }
    }
}
