
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.VideoPlayer.Plugins
{
    public class SubtitlesPlugin : VideoPlayerPlugin
    {
        public InstructionRunner instructionRunner;

        private string[][] currentSubtitles;

        protected override void OnPlaybackStart(float timestamp)
        {
            if (timestamp < 10000) this.instructionRunner.JumpToStart();
            else this.instructionRunner.SeekToTimestamp(Mathf.FloorToInt(timestamp));
        }

        protected override void OnLoadRequested(VRCUrl url)
        {
            this.instructionRunner.Reset();
        }

        protected override void OnUnload()
        {
            this.instructionRunner.Reset();
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
                this.instructionRunner.Reset();
                return;
            }

            for (int i = 0; i < currentSubtitles.Length; i++)
            {
                string lang = currentSubtitles[i][0];
                string content = currentSubtitles[i][1];

                if (language == lang)
                {
                    this.instructionRunner.SetInstructions(content);
                    break;
                }
            }
        }
    }
}
