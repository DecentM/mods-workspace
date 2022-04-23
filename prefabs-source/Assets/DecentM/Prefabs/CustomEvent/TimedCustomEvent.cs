
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DecentM;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TimedCustomEvent : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("The UdonBehaviour to send the custom event to")]
    public UdonBehaviour target;

    [Header("Settings")]
    [Tooltip("The name of the event to send / public function name to call")]
    public string eventName;
    [Tooltip("How many times per second to send the event (min 1, max <1 / Time.fixedDeltaTime>, usually 50)")]
    public float timesPerSecond = 1;

    [Header("LibDecentM")]
    [Tooltip("The LibDecentM object")]
    public LibDecentM lib;

    private int clock = 0;

    private void FixedUpdate()
    {
        this.clock++;

        float realTimesPerSecond = Mathf.Clamp(this.timesPerSecond, 1, lib.scheduling.fixedUpdateRate);

        if (this.clock > lib.scheduling.fixedUpdateRate / realTimesPerSecond)
        {
            this.clock = 0;
            this.SendLocal();
        }
    }

    private void SendLocal()
    {
        this.target.SendCustomEvent(this.eventName);
    }
}
