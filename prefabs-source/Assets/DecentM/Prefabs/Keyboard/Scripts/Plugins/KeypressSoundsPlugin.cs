using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeypressSoundsPlugin : KeyboardPlugin
    {
        public AudioSource audioPlayer;

        private void PlayTypingSound(int index)
        {
            if (this.audioPlayer.isPlaying)
                return;

            this.audioPlayer.time = index;
            this.audioPlayer.Play();
            this.SendCustomEventDelayedSeconds(nameof(StopPlaying), 0.45f);
        }

        public void StopPlaying()
        {
            this.audioPlayer.Stop();
            this.audioPlayer.time = 0;
        }

        private void PlayRandomTypingNoise()
        {
            int typingSoundIndex = Random.Range(
                0,
                (Mathf.FloorToInt(this.audioPlayer.clip.length * 2) / 2) + 1
            );
            this.PlayTypingSound(typingSoundIndex);
        }

        protected override void OnKeyPressDown(KeyboardKey key)
        {
            this.PlayRandomTypingNoise();
        }

        protected override void OnKeyPressUp(KeyboardKey key)
        {
            this.PlayRandomTypingNoise();
        }
    }
}
