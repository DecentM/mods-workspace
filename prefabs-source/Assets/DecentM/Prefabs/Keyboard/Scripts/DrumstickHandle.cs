
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DrumstickHandle : UdonSharpBehaviour
    {
        public KeyboardEvents events;
        public Animator animator;
        public Transform resetPosition;

        private new Rigidbody rigidbody;

        private void Start()
        {
            this.rigidbody = this.GetComponent<Rigidbody>();
            this.ResetAndStop();
        }

        private bool holding = false;

        public void OnEnable()
        {
            this.ResetAndStop();
        }

        public override void OnDrop()
        {
            this.holding = false;
            this.animator.SetBool("FlashlightOn", false);
            this.events.OnCommandClear();
            this.SendCustomEventDelayedSeconds(nameof(this.ResetAndStop), 5);
        }

        public void ResetAndStop()
        {
            if (this.holding) return;

            this.ResetAndStopDeferred();
            this.SendCustomEventDelayedSeconds(nameof(this.ResetAndStopDeferred), 0.1f);
        }

        public void ResetAndStopDeferred()
        {
            this.gameObject.transform.SetPositionAndRotation(this.resetPosition.position, this.resetPosition.rotation);

            if (this.rigidbody == null) return;

            this.rigidbody.velocity = Vector3.zero;
            this.rigidbody.angularVelocity = Vector3.zero;
        }

        public override void OnPickup()
        {
            this.holding = true;
            this.animator.SetBool("FlashlightOn", true);
        }
    }
}
