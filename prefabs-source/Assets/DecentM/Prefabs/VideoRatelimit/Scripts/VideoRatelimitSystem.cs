using System.Collections.Generic;
using UnityEngine;

namespace DecentM.Prefabs.VideoRatelimit
{
    public class VideoRatelimitSystem : MonoBehaviour
    {
        public float ratelimitSeconds = 5.2f;
        public Queue<MonoBehaviour> queue = new Queue<MonoBehaviour>();

        public const string PlaybackWindowEvent = "OnPlaybackWindow";

        private float elapsed = 0f;
        private bool isWaiting = false;

        private void FixedUpdate()
        {
            if (!this.isWaiting && this.queue.Count != 0)
            {
                this.SendWindowEventToFirst();
                return;
            }

            this.elapsed += Time.fixedUnscaledDeltaTime;
            // Using lte is important here because we want to avoid the rate limit by guaranteeing that we
            // only fire events after it opened.
            if (this.elapsed <= this.ratelimitSeconds)
                return;

            this.elapsed = 0f;
            this.isWaiting = false;

            this.SendWindowEventToFirst();
        }

        // If a player starts loading a URL, we already used up the current window, nothing we can do about it,
        // so we just reset the rate limit counter to 0 to start over.
        public void OnPlayerLoad()
        {
            this.elapsed = 0f;
            this.isWaiting = true;
        }

        public void RequestPlaybackWindow(MonoBehaviour behaviour)
        {
            this.queue.Enqueue(behaviour);
        }

        private void SendWindowEventToFirst()
        {
            if (this.queue.Count <= 0)
                return;

            MonoBehaviour behaviour = this.queue.Dequeue();
            if (behaviour == null)
                return;

            behaviour.Invoke(PlaybackWindowEvent, 0);

            // pre-emptively start waiting, assuming that this event will cause a video to be loaded
            // if we don't do this, a delayed video load by this behaviour could cause a race
            // condition where we send the window event to the next behaviour and this one starts loading
            // after a small delay
            this.isWaiting = true;
        }
    }
}
