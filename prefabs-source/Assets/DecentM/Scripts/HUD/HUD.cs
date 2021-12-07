
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class HUD : UdonSharpBehaviour
{
    private void LateUpdate()
    {
        VRCPlayerApi.TrackingData trackingData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        this.transform.SetPositionAndRotation(trackingData.position, trackingData.rotation);
    }
}
