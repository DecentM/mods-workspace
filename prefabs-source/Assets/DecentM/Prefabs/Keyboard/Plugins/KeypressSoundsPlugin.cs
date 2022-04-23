
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeypressSoundsPlugin : UdonSharpBehaviour
    {
        public KeyboardEvents events;
        public AudioSource audioPlayer;

        private void Start()
        {
            this.events.Subscribe(this);
        }

        private void PlayTypingSound(int index)
        {
            if (this.audioPlayer.isPlaying) return;

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
            int typingSoundIndex = Random.Range(0, (Mathf.FloorToInt(this.audioPlayer.clip.length * 2) / 2) + 1);
            this.PlayTypingSound(typingSoundIndex);
        }

        private string OnKeyboardEvent_name;
        private object OnKeyboardEvent_data;
        public void OnKeyboardEvent()
        {
            switch (OnKeyboardEvent_name)
            {
                // Play sounds on both key down and key up
                case nameof(this.events.OnKeyPressDown):
                case nameof(this.events.OnKeyPressUp):
                    this.PlayRandomTypingNoise();
                    break;

                default:
                    break;
            }
        }
    }
}
