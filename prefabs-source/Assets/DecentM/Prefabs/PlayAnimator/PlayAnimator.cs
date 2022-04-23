
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayAnimator : UdonSharpBehaviour
{
    [Header("Settings")]
    [Tooltip("A list of Animators to control")]
    public Animator[] targets;
    [Tooltip("Which animation to target in the controller")]
    public int layerIndex = 0;
    [Tooltip("Float between 0 and 1 - We will offset the animation by this much to count for network latency\nDoes nothing when `global` is unchecked")]
    public float latencyOffset = 0.01f;

    private void PlayTarget(Animator animator)
    {
        animator.enabled = true;
        int hash = animator.GetCurrentAnimatorStateInfo(this.layerIndex).fullPathHash;
        animator.Play(hash, this.layerIndex, this.latencyOffset);
    }

    private void StopTarget(Animator animator)
    {
        animator.enabled = false;
    }

    public void Play()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            this.PlayTarget(targets[i]);
        }
    }

    public void Stop()
    {
        for (int i = 0; i < this.targets.Length; i++)
        {
            this.StopTarget(targets[i]);
        }
    }
}
