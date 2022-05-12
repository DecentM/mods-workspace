using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Drumstick : UdonSharpBehaviour
    {
        public VRC_Pickup.PickupHand hand;

        public void PlayHapticFeedback()
        {
            Networking.LocalPlayer.PlayHapticEventInHand(this.hand, 0.2f, .5f, 1f);
        }
    }
}
