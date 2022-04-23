
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRCAudioLink;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AudioLinkHapticsVolume : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("The AudioLink game object in the world")]
    public AudioLink audioLink;

    [Header("Haptics settings (values range from 0 to 1)")]
    [Tooltip("How long to vibrate each note for. This should be low as the vibration command is send with every frame.")]
    public float duration = .05f;
    [Tooltip("How intense the vibration is")]
    public float amplitude = 1f;
    [Tooltip("How quick the vibration motor spins inside the controller")]
    public float frequency = 1f;
    [Space]
    [Header("Haptics customisations")]
    [Tooltip("Defines how much influence band 0 has in the vibration intensity")]
    public float band0Amplitude = 2f;
    [Tooltip("Defines how much influence band 1 has in the vibration intensity")]
    public float band1Amplitude = 1.5f;
    [Tooltip("Defines how much influence band 2 has in the vibration intensity")]
    public float band2Amplitude = 1f;
    [Tooltip("Defines how much influence band 3 has in the vibration intensity")]
    public float band3Amplitude = .5f;
    [Space]
    [Tooltip("Defines how much influence band 0 has in the vibration speed")]
    public float band0Frequency = 1.3f;
    [Tooltip("Defines how much influence band 1 has in the vibration speed")]
    public float band1Frequency = 1f;
    [Tooltip("Defines how much influence band 2 has in the vibration speed")]
    public float band2Frequency = .7f;
    [Tooltip("Defines how much influence band 3 has in the vibration speed")]
    public float band3Frequency = .5f;

    private void DoHaptics(VRC_Pickup.PickupHand hand, float bandValue, float threshold, float amplitude, float frequency)
    {
        if (bandValue > threshold)
        {
            float overshoot = bandValue - threshold;
            Networking.LocalPlayer.PlayHapticEventInHand(hand, this.duration, this.amplitude * amplitude * overshoot, this.frequency * frequency * overshoot);
        }
    }

    private void Update()
    {
        Material alMat = this.audioLink.audioMaterial;

        // The _Samples** float arrays contain the AudioLink output.
        // Each index is one tick delayed
        float[] samples0R = alMat.GetFloatArray("_Samples0R");
        float[] samples1R = alMat.GetFloatArray("_Samples1R");
        float[] samples2R = alMat.GetFloatArray("_Samples2R");
        float[] samples3R = alMat.GetFloatArray("_Samples3R");

        float[] samples0L = alMat.GetFloatArray("_Samples0L");
        float[] samples1L = alMat.GetFloatArray("_Samples1L");
        float[] samples2L = alMat.GetFloatArray("_Samples2L");
        float[] samples3L = alMat.GetFloatArray("_Samples3L");

        // If for some reason, AudioLink didn't write its float arrays, we bail here to avoid crashing
        if (
            samples0R == null
            || samples1R == null
            || samples2R == null
            || samples3R == null
            || samples0L == null
            || samples1L == null
            || samples2L == null
            || samples3L == null
           )
        {
            return;
        }

        float band0R = samples0R[0] * this.audioLink.bass * this.audioLink.gain;
        float band1R = samples1R[0] * this.audioLink.bass * this.audioLink.gain;
        float band2R = samples2R[0] * this.audioLink.treble * this.audioLink.gain;
        float band3R = samples3R[0] * this.audioLink.treble * this.audioLink.gain;

        float band0L = samples0L[0] * this.audioLink.bass * this.audioLink.gain;
        float band1L = samples1L[0] * this.audioLink.bass * this.audioLink.gain;
        float band2L = samples2L[0] * this.audioLink.treble * this.audioLink.gain;
        float band3L = samples3L[0] * this.audioLink.treble * this.audioLink.gain;

        this.DoHaptics(VRC_Pickup.PickupHand.Left, band0L, this.audioLink.threshold0, this.band0Amplitude, this.band0Frequency);
        this.DoHaptics(VRC_Pickup.PickupHand.Left, band1L, this.audioLink.threshold1, this.band1Amplitude, this.band1Frequency);
        this.DoHaptics(VRC_Pickup.PickupHand.Left, band2L, this.audioLink.threshold2, this.band2Amplitude, this.band2Frequency);
        this.DoHaptics(VRC_Pickup.PickupHand.Left, band3L, this.audioLink.threshold3, this.band3Amplitude, this.band3Frequency);

        this.DoHaptics(VRC_Pickup.PickupHand.Right, band0R, this.audioLink.threshold0, this.band0Amplitude, this.band0Frequency);
        this.DoHaptics(VRC_Pickup.PickupHand.Right, band1R, this.audioLink.threshold1, this.band1Amplitude, this.band1Frequency);
        this.DoHaptics(VRC_Pickup.PickupHand.Right, band2R, this.audioLink.threshold2, this.band2Amplitude, this.band2Frequency);
        this.DoHaptics(VRC_Pickup.PickupHand.Right, band3R, this.audioLink.threshold3, this.band3Amplitude, this.band3Frequency);
    }
}
