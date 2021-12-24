
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RaycastHeightAdjuster : UdonSharpBehaviour
{
    public Transform target;
    public int raycastMaxDistance = 10;
    public LayerMask raycastLayerMask = 0;

    public bool lockX = false;
    public bool lockY = false;

    public bool lookAtPlayer = true;

    private void FixedUpdate()
    {
        VRCPlayerApi.TrackingData head = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        VRCPlayerApi.TrackingData rightHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
        VRCPlayerApi.TrackingData leftHand = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);

        VRCPlayerApi.TrackingData hand = rightHand.position.y > leftHand.position.y ? rightHand : leftHand;

        Vector3 direction = hand.position - head.position;

        RaycastHit hit;

        if (!Physics.Raycast(head.position, direction, out hit, this.raycastMaxDistance, this.raycastLayerMask))
        {
            return;
        }

        if (this.target == null)
        {
            return;
        }

        this.target.transform.position = new Vector3(
            this.lockX ? this.target.transform.position.x : hit.point.x,
            this.lockY ? this.target.transform.position.y : hit.point.y,
            this.target.transform.position.z
        );

        if (this.lookAtPlayer)
        {
            this.target.transform.forward = new Vector3(this.target.transform.forward.x, direction.y, this.target.transform.forward.z);
        }
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        if (value == true)
        {
            this.enabled = false;
            return;
        }
    }
}
