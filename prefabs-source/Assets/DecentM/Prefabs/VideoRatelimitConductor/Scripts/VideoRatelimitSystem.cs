using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using DecentM.Collections;

namespace DecentM.VideoRatelimit
{
    public class VideoRatelimitSystem : UdonSharpBehaviour
    {
        public float ratelimitSeconds = 5f;

        private float elapsed = 0;

        private void FixedUpdate()
        {
            this.elapsed += Time.fixedUnscaledDeltaTime;
            // Using lte is important here because we want to avoid the rate limit by guaranteeing that we
            // only fire events after it opened.
            if (this.elapsed <= this.ratelimitSeconds)
                return;
            this.elapsed = 0;

            Debug.Log("Rate limit elapsed");
        }
    }
}
