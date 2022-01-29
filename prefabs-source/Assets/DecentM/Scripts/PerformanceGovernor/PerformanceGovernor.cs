
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PerformanceGovernor : UdonSharpBehaviour
{
    private const int CheckEverySeconds = 10;

    private float updateRate;
    private int fixedClock = 0;
    private int frameCounter = 0;
    private int fpsAverage = 0;

    /**
     * 0 = High performance
     * 1 = Medium performance
     * 2 = Low performance
     **/
    private int currentMode = 0;

    // Hidden in inspector, because our custom inspector already draws a UI for these.
    [HideInInspector]
    public int high = 60;
    [HideInInspector]
    public int low = 30;

    [Header("Settings")]
    [Tooltip("To prevent a race condition where high FPS from our improvements causes objects being re-enabled too soon, re-enabling a higher performance mode requires higher FPS than going downwards. The higher this value is, the less likely it is to cause the race condition. Best to leave it at default.")]
    public int headroom = 20;
    [Tooltip("Referenced UdonBehaviours will receive this event when performance is high")]
    public string eventNameHigh = "OnPerformanceHigh";
    [Tooltip("Referenced UdonBehaviours will receive this event when performance is medium")]
    public string eventNameMedium = "OnPerformanceMedium";
    [Tooltip("Referenced UdonBehaviours will receive this event when performance is low")]
    public string eventNameLow = "OnPerformanceLow";

    [Header("References")]
    [Tooltip("A list of UdonBehaviours that handle events (by turning laggy objects off for example)")]
    public UdonSharpBehaviour[] behaviours;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object from the world")]
    public LibDecentM lib;

    [Header("Debugging")]
    [Tooltip("If debugging is enabled in LibDecentM, we will write the current FPS and FPS counter to this TextMeshPro object.")]
    public TextMeshProUGUI debug;

    private void Start()
    {
        this.updateRate = 1 / Time.fixedDeltaTime;
    }

    public int Subscribe(UdonSharpBehaviour behaviour)
    {
        UdonSharpBehaviour[] tmp = new UdonSharpBehaviour[this.behaviours.Length + 1];
        this.behaviours.CopyTo(tmp, 0);

        tmp[tmp.Length - 1] = behaviour;
        this.behaviours = tmp;

        // return the index so that it's possible to unsubscribe
        return this.behaviours.Length - 1;
    }

    public void Unsubscribe(int index)
    {
        UdonSharpBehaviour[] result = new UdonSharpBehaviour[this.behaviours.Length - 1];

        int i = 0;

        foreach (UdonSharpBehaviour item in this.behaviours)
        {
            // Ignore the requested index
            if (i == index)
            {
                i++;
                continue;
            }

            result[i] = item;

            i++;
        }
    }

    private void FixedUpdate()
    {
        this.fixedClock++;
    
        // If we've reached one second, the current frameCounter will be our FPS value
        if (this.fixedClock % this.updateRate == 0)
        {
            // This average will get more and more accurate as time approaches `this.updateRate * CheckEverySeconds`,
            // it will start on 0. This is as intended, as we only sample the average value when it gets the most
            // accurate.
            this.fpsAverage = (this.frameCounter + this.fpsAverage) / 2;
            this.frameCounter = 0;
        }

        // Every ten seconds, take a sample from this.averageFrames and send events as needed
        if (this.fixedClock % (this.updateRate * CheckEverySeconds) == 0)
        {
            this.CheckFrames();
            this.fixedClock = 0;
            this.fpsAverage = 0;
        }

        if (this.lib.debugging.isDebugging && this.debug != null && this.fixedClock % (this.updateRate / 10) == 0)
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
        this.debug.text += $"Mode: {this.currentMode}\n";
    }

    private void CheckFrames()
    {
        // Check the current framerate and increase of decrease the level based on the current FPS + some headroom
        // to prevent switching back and forth quickly
        switch (this.currentMode)
        {
            default:
            // 0 = High
            case 0:
                // Switch to medium mode if the FPS is lower than "high"
                if (this.fpsAverage < this.high)
                {
                    this.currentMode = 1;
                    this.OnModeChange();
                }
                break;

            // 1 = Medium
            case 1:
                // Switch to low mode if the FPS is lower than "low"
                if (this.fpsAverage < this.low)
                {
                    this.currentMode = 2;
                    this.OnModeChange();
                }

                // Switch to high mode if the FPS is higher than "high" plus headroom
                if (this.fpsAverage > this.high + this.headroom)
                {
                    this.currentMode = 0;
                    this.OnModeChange();
                }
                break;

            // 2 = Low
            case 2:
                // Switch to medium mode if the FPS is higher than "medium" plus headroom
                if (this.fpsAverage > this.low + this.headroom)
                {
                    this.currentMode = 1;
                    this.OnModeChange();
                }
                break;
        }
    }

    private void OnModeChange()
    {
        // Send events
        for (int i = 0; i < this.behaviours.Length; i++)
        {
            Component component = this.behaviours[i];

            // Ignore non-UdonBehaviours
            if (component.GetType() != typeof(UdonBehaviour))
            {
                continue;
            }

            UdonBehaviour behaviour = (UdonBehaviour)component;

            switch (this.currentMode)
            {
                default:
                case 0:
                    behaviour.SendCustomEvent(this.eventNameHigh);
                    break;
                case 1:
                    behaviour.SendCustomEvent(this.eventNameMedium);
                    break;
                case 2:
                    behaviour.SendCustomEvent(this.eventNameLow);
                    break;
            }
        }
    }
}
