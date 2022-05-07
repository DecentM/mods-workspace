
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BeginAdjustButton : UdonSharpBehaviour
{
    public UdonBehaviour target;

    public void OnInteract()
    {
        this.target.enabled = true;
    }
}
