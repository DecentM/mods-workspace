using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AnimatorSync : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("The animator component to control")]
    public Animator animator;
    [Tooltip("Which animation to target in the controller")]
    public int layerIndex = 0;
    [Tooltip("Float between 0 and 1 - We will offset the animation by this much to count for network latency")]
    public float latencyOffset = 0.01f;

    private float lastFiredHandshake = 0f;
    private bool handshakeFiredThisLoop = false;

    // Stop the UDON Behaviour if the animator isn't set as that'd cause a crash in the first
    // FixedUpdate anyway
    void Start()
    {
        if (this.animator == null)
        {
            Debug.LogError("Setting the animator is required");
            this.gameObject.SetActive(false);
        }
    }

    // Called from a network event from the master, only non-masters run this, as the master is
    // the source of truth.
    public void OnHandshake ()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        // The master doesn't need to accept handshakes from itself
        if (localPlayer.isMaster)
        {
            return;
        }

        int hash = this.animator.GetCurrentAnimatorStateInfo(this.layerIndex).fullPathHash;

        // Calling Play() will reset the animation's progress to the value of the third argument,
        // even if the animation is already playing. Handshakes happen at point 0 when the
        // animation loops, but we gotta count for network latency, so we reset a little bit
        // ahead of 0.
        this.animator.Play(hash, this.layerIndex, this.latencyOffset);
    }

    // Private function called only by the master
    private void DoHandshake ()
    {
        AnimatorStateInfo info = this.animator.GetCurrentAnimatorStateInfo(this.layerIndex);
        this.lastFiredHandshake = info.normalizedTime;

        this.handshakeFiredThisLoop = true;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnHandshake");
    }

    private void FixedUpdate()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        // Only the master needs to check the state of the animation, as it's the source of truth
        if (!localPlayer.isMaster)
        {
            return;
        }

        // Send a handshake if we never did one before (lastFiredHandshake is 0f by default), or if we haven't
        // done one this loop yet (most likely in the beginning of the loop).
        if (this.lastFiredHandshake == 0f || !this.handshakeFiredThisLoop)
        {
            this.DoHandshake();
            return;
        }

        AnimatorStateInfo info = this.animator.GetCurrentAnimatorStateInfo(this.layerIndex);

        int currentLoop = Mathf.FloorToInt(info.normalizedTime);
        float remainder = info.normalizedTime - currentLoop;

        // Reset the handshake flag when the remainder from the saved timestamp is larger than the current remainder
        // (meaning that we're now in the next loop)
        int lastHandshakeLoop = Mathf.FloorToInt(this.lastFiredHandshake);
        float lastHandshakeRemainder = this.lastFiredHandshake - lastHandshakeLoop;

        if (lastHandshakeRemainder > remainder)
        {
            this.handshakeFiredThisLoop = false;
        }
    }
}
