
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PerformanceGovernor : UdonSharpBehaviour
{
    private const int CheckEverySeconds = 10;

    private float updateRate;
    private int fixedClock = 0;
    private int frameCounter = 0;
    private int fpsAverage = 0;

    [HideInInspector]
    public int high = 60;
    [HideInInspector]
    public int low = 30;
    public TextMeshProUGUI debug;

    private void Start()
    {
        this.updateRate = 1 / Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        this.fixedClock++;
    
        // If we've reached one second, the current frameCounter will be our FPS value
        if (this.fixedClock % this.updateRate == 0)
        {
            // int secondsSinceLastCheck = Mathf.RoundToInt(this.fixedClock / this.updateRate);
            this.fpsAverage = (this.frameCounter + this.fpsAverage) / 2;
            this.frameCounter = 0;
        }

        // Every ten seconds, check frames and send events as needed
        if (this.fixedClock % (this.updateRate * CheckEverySeconds) == 0)
        {
            this.CheckFrames();
            this.fixedClock = 0;
            this.fpsAverage = 0;
        }

        if (this.debug != null && this.fixedClock % (this.updateRate / 10) == 0)
        {
            this.PrintDebugText();
        }
    }

    private void LateUpdate()
    {
        this.frameCounter++;
    }

    private void PrintDebugText()
    {
        this.debug.text = "";
        this.debug.text += $"Average FPS: {this.fpsAverage}\n";
        this.debug.text += $"Frame counter: {this.frameCounter}\n";
    }

    private void CheckFrames()
    {
        // Check the current framerate and increase of decrease the level based on the current FPS + some headroom
        // to prevent switching back and forth quickly
    }
}
