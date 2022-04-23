
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using DecentM.Pubsub;

namespace DecentM
{
    public enum PerformanceGovernorMode
    {
        High,
        Medium,
        Low,
    }

    public enum PerformanceGovernorEvent
    {
        OnPerformanceHigh,
        OnPerformanceMedium,
        OnPerformanceLow,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PerformanceGovernor : PubsubHost<PerformanceGovernorEvent>
    {
        private PerformanceGovernorMode currentMode = PerformanceGovernorMode.High;

        // Hidden in inspector, because our custom inspector already draws a UI for these.
        [HideInInspector]
        public int high = 60;
        [HideInInspector]
        public int low = 30;

        [Header("Settings")]
        [Tooltip("To prevent a race condition where high FPS from our improvements causes objects being re-enabled too soon, re-enabling a higher performance mode requires higher FPS than going downwards. The higher this value is, the less likely it is to cause the race condition. Best to leave it at default.")]
        public int headroom = 20;

        public float checkEverySeconds = 10;

        private int frameCounter = 0;
        private float elapsed = 0;

        private void LateUpdate()
        {
            this.frameCounter++;
            this.elapsed += Time.unscaledDeltaTime;

            // Tally up the number of frames we had over the last x seconds, then reset
            if (this.elapsed > this.checkEverySeconds)
            {
                this.CheckFrames();
                this.elapsed = 0;
            }
        }

        private void CheckFrames()
        {
            float fpsAverage = this.frameCounter / this.checkEverySeconds;

            // Check the current framerate and increase of decrease the level based on the current FPS + some headroom
            // to prevent switching back and forth quickly
            switch (this.currentMode)
            {
                default:
                case PerformanceGovernorMode.High:
                    // Switch to medium mode if the FPS is lower than "high"
                    if (fpsAverage < this.high)
                    {
                        this.currentMode = PerformanceGovernorMode.Medium;
                        this.OnModeChange();
                    }
                    break;

                case PerformanceGovernorMode.Medium:
                    // Switch to low mode if the FPS is lower than "low"
                    if (fpsAverage < this.low)
                    {
                        this.currentMode = PerformanceGovernorMode.Low;
                        this.OnModeChange();
                    }

                    // Switch to high mode if the FPS is higher than "high" plus headroom
                    if (fpsAverage > this.high + this.headroom)
                    {
                        this.currentMode = PerformanceGovernorMode.High;
                        this.OnModeChange();
                    }
                    break;

                case PerformanceGovernorMode.Low:
                    // Switch to medium mode if the FPS is higher than "medium" plus headroom
                    if (fpsAverage > this.low + this.headroom)
                    {
                        this.currentMode = PerformanceGovernorMode.Medium;
                        this.OnModeChange();
                    }
                    break;
            }
        }

        private void OnModeChange()
        {
            switch (this.currentMode)
            {
                default:
                case PerformanceGovernorMode.High:
                    this.BroadcastEvent(PerformanceGovernorEvent.OnPerformanceHigh);
                    break;
                case PerformanceGovernorMode.Medium:
                    this.BroadcastEvent(PerformanceGovernorEvent.OnPerformanceMedium);
                    break;
                case PerformanceGovernorMode.Low:
                    this.BroadcastEvent(PerformanceGovernorEvent.OnPerformanceLow);
                    break;
            }
        }
    }
}
