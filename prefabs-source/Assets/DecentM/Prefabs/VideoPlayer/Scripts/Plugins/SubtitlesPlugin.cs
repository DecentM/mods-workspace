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
            if (timestamp < 10000)
                this.JumpToStart();
            else
            {
                int index = this.SearchForInstructionIndex(Mathf.FloorToInt(timestamp));
                if (index >= 0)
                    this.instructionIndex = index;
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

        public static string GetLanguageIdFromFilename(string filename)
        {
            string[] parts = filename.Split('.');
            return parts[parts.Length - 1];
        }

        public static string GetLanguageFromFilename(string filename)
        {
            string language = GetLanguageIdFromFilename(filename);
            if (language.EndsWith("-ar"))
                language = language.Substring(0, language.Length - 3);
            if (language.EndsWith("-en"))
                language = language.Substring(0, language.Length - 3);

            switch (language.ToLower())
            {
                case "af":
                    return "Afrikaans";
                case "sq":
                    return "Albanian";
                case "ar":
                    return "Arabic";
                case "ar-dz":
                    return "Arabic (Algeria)";
                case "ar-bh":
                    return "Arabic (Bahrain)";
                case "ar-eg":
                    return "Arabic (Egypt)";
                case "ar-iq":
                    return "Arabic (Iraq)";
                case "ar-jo":
                    return "Arabic (Jordan)";
                case "ar-kw":
                    return "Arabic (Kuwait)";
                case "ar-lb":
                    return "Arabic (Lebanon)";
                case "ar-ly":
                    return "Arabic (Libya)";
                case "ar-ma":
                    return "Arabic (Morocco)";
                case "ar-om":
                    return "Arabic (Oman)";
                case "ar-qa":
                    return "Arabic (Qatar)";
                case "ar-sa":
                    return "Arabic (Saudi Arabia)";
                case "ar-sy":
                    return "Arabic (Syria)";
                case "ar-tn":
                    return "Arabic (Tunisia)";
                case "ar-ae":
                    return "Arabic (U.A.E.)";
                case "ar-ye":
                    return "Arabic (Yemen)";
                case "eu":
                    return "Basque";
                case "be":
                    return "Belarusian";
                case "bg":
                    return "Bulgarian";
                case "ca":
                    return "Catalan";
                case "zh":
                    return "Chinese";
                case "zh-hant":
                    return "Chinese (Traditional)";
                case "zh-hans":
                    return "Chinese (Simplified)";
                case "zh-hk":
                    return "Chinese (Hong Kong)";
                case "zh-cn":
                    return "Chinese (PRC)";
                case "zh-sg":
                    return "Chinese (Singapore)";
                case "zh-tw":
                    return "Chinese (Taiwan)";
                case "hr":
                    return "Croatian";
                case "cs":
                    return "Czech";
                case "da":
                    return "Danish";
                case "nl-be":
                    return "Dutch (Belgium)";
                case "nl":
                    return "Dutch";
                case "en":
                    return "English";
                case "en-au":
                    return "English (Australia)";
                case "en-bz":
                    return "English (Belize)";
                case "en-ca":
                    return "English (Canada)";
                case "en-ie":
                    return "English (Ireland)";
                case "en-jm":
                    return "English (Jamaica)";
                case "en-nz":
                    return "English (New Zealand)";
                case "en-za":
                    return "English (South Africa)";
                case "en-tt":
                    return "English (Trinidad)";
                case "en-gb":
                    return "English (United Kingdom)";
                case "en-us":
                    return "English (United States)";
                case "et":
                    return "Estonian";
                case "fo":
                    return "Faeroese";
                case "fa":
                    return "Farsi";
                case "fi":
                    return "Finnish";
                case "fr-be":
                    return "French (Belgium)";
                case "fr-ca":
                    return "French (Canada)";
                case "fr-lu":
                    return "French (Luxembourg)";
                case "fr":
                    return "French";
                case "fr-ch":
                    return "French (Switzerland)";
                case "gd":
                    return "Gaelic (Scotland)";
                case "de-at":
                    return "German (Austria)";
                case "de-li":
                    return "German (Liechtenstein)";
                case "de-lu":
                    return "German (Luxembourg)";
                case "de":
                    return "German";
                case "de-ch":
                    return "German (Switzerland)";
                case "el":
                    return "Greek";
                case "he":
                    return "Hebrew";
                case "hi":
                    return "Hindi";
                case "hu":
                    return "Hungarian";
                case "is":
                    return "Icelandic";
                case "id":
                    return "Indonesian";
                case "ga":
                    return "Irish";
                case "it":
                    return "Italian";
                case "it-ch":
                    return "Italian (Switzerland)";
                case "ja":
                    return "Japanese";
                case "ko":
                    return "Korean";
                case "ku":
                    return "Kurdish";
                case "lv":
                    return "Latvian";
                case "lt":
                    return "Lithuanian";
                case "mk":
                    return "Macedonian (FYROM)";
                case "ml":
                    return "Malayalam";
                case "ms":
                    return "Malaysian";
                case "mt":
                    return "Maltese";
                case "no":
                    return "Norwegian";
                case "nb":
                    return "Norwegian (Bokmål)";
                case "nn":
                    return "Norwegian (Nynorsk)";
                case "pl":
                    return "Polish";
                case "pt-br":
                    return "Portuguese (Brazil)";
                case "pt":
                    return "Portuguese";
                case "pa":
                    return "Punjabi";
                case "rm":
                    return "Rhaeto-Romanic";
                case "ro":
                    return "Romanian";
                case "ro-md":
                    return "Romanian (Republic of Moldova)";
                case "ru":
                    return "Russian";
                case "ru-md":
                    return "Russian (Republic of Moldova)";
                case "sr":
                    return "Serbian";
                case "sk":
                    return "Slovak";
                case "sl":
                    return "Slovenian";
                case "sb":
                    return "Sorbian";
                case "es-ar":
                    return "Spanish (Argentina)";
                case "es-bo":
                    return "Spanish (Bolivia)";
                case "es-cl":
                    return "Spanish (Chile)";
                case "es-co":
                    return "Spanish (Colombia)";
                case "es-cr":
                    return "Spanish (Costa Rica)";
                case "es-do":
                    return "Spanish (Dominican Republic)";
                case "es-ec":
                    return "Spanish (Ecuador)";
                case "es-sv":
                    return "Spanish (El Salvador)";
                case "es-gt":
                    return "Spanish (Guatemala)";
                case "es-hn":
                    return "Spanish (Honduras)";
                case "es-mx":
                    return "Spanish (Mexico)";
                case "es-ni":
                    return "Spanish (Nicaragua)";
                case "es-pa":
                    return "Spanish (Panama)";
                case "es-py":
                    return "Spanish (Paraguay)";
                case "es-pe":
                    return "Spanish (Peru)";
                case "es-pr":
                    return "Spanish (Puerto Rico)";
                case "es":
                    return "Spanish";
                case "es-uy":
                    return "Spanish (Uruguay)";
                case "es-ve":
                    return "Spanish (Venezuela)";
                case "sv":
                    return "Swedish";
                case "sv-fi":
                    return "Swedish (Finland)";
                case "th":
                    return "Thai";
                case "ts":
                    return "Tsonga";
                case "tn":
                    return "Tswana";
                case "tr":
                    return "Turkish";
                case "ua":
                    return "Ukrainian";
                case "ur":
                    return "Urdu";
                case "ve":
                    return "Venda";
                case "vi":
                    return "Vietnamese";
                case "cy":
                    return "Welsh";
                case "xh":
                    return "Xhosa";
                case "ji":
                    return "Yiddish";
                case "zu":
                    return "Zulu";

                default:
                    return language;
            }
        }

        protected override void OnMetadataChange(
            string title,
            string uploader,
            string siteName,
            int viewCount,
            int likeCount,
            string resolution,
            int fps,
            string description,
            string duration,
            TextAsset[] subtitles
        )
        {
            this.currentSubtitles = subtitles;
            string[][] langs = new string[subtitles.Length][];

            for (int i = 0; i < subtitles.Length; i++)
            {
                langs[i] = new string[]
                {
                    GetLanguageIdFromFilename(subtitles[i].name),
                    GetLanguageFromFilename(subtitles[i].name)
                };
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

                if (language == GetLanguageIdFromFilename(filename))
                {
                    this.SetInstructions(content);
                    this.instructionIndex = this.SearchForInstructionIndex(
                        Mathf.FloorToInt(this.system.GetTime())
                    );
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

        private const char NewlineDelimeter = '˟';

        public static object[][] ParseInstructions(string instructions)
        {
            string[] lines = instructions.Split('\n');
            object[][] result = new object[lines.Length][];

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

                object[] instruction = new object[3];

                instruction[0] = type;
                instruction[1] = timestamp;
                instruction[2] = text;

                result[i] = instruction;
            }

            return result;
        }

        public void SetInstructions(string newInstructions)
        {
            this.instructions = ParseInstructions(newInstructions);
        }

        private float elapsed = 0;
        public float tickInterval = 0.1f;

        private void FixedUpdate()
        {
            if (!this.system.IsPlaying())
                return;

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
            if (this.instructions == null || this.instructions.Length == 0)
                return null;
            if (index < 0)
                return null;
            if (index >= this.instructions.Length)
                return this.instructions[this.instructions.Length - 1];

            return this.instructions[index];
        }

        private int SearchForInstructionIndex(int targetTimestamp)
        {
            float lowestDiff = float.PositiveInfinity;

            for (int i = 0; i < this.instructions.Length; i++)
            {
                object[] instruction = this.GetInstructionAtIndex(i);
                if (instruction == null || instruction.Length == 0)
                    continue;

                int timestamp = (int)instruction[1];
                int diff = Mathf.Abs(timestamp - targetTimestamp);

                if (diff <= lowestDiff)
                    lowestDiff = diff;
                else
                    return i - 1;
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
                if (this.debugSlot != null)
                    this.debugSlot.text = "No subtitles set.";
                return;
            }

            // We've reached the end of the instructions, stop processing more
            if (this.instructionIndex >= this.instructions.Length)
            {
                if (this.debugSlot != null)
                    this.debugSlot.text = "End of subtitles reached.";
                return;
            }

            int timeMillis =
                Mathf.RoundToInt(this.system.GetTime() * 1000)
                + Mathf.RoundToInt(this.subtitleOffset);
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
                    if (index >= 0)
                        this.instructionIndex = index;
                    return;
                }
            }

            if (this.debugSlot != null)
                this.debugSlot.text =
                    $"Instructions: {this.instructions.Length}\n"
                    + $"Next instruction index: {this.instructionIndex}\n"
                    + $"Next instruction type: {((int)instruction[0] == 1 ? "write" : "clear")}\n"
                    + $"Next instruction timestamp: {instruction[1]}\n"
                    + $"Distance from next instruction: {diff}\n"
                    + $"Current playback time {timeMillis}\n";

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
