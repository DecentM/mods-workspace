
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace DecentM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Scheduling : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("If checked, disabled UdonBehaviours will be auto-unsubscribed from events")]
        public bool cleanupInactive = false;

        public readonly string OnSecondPassedEvent = "OnSecondPassed";

        private int clock = 0;
        private float fixedUpdateRate;

        [Header("Internals")]
        [Tooltip("The list of components that are currently receiving events")]
        public Component[] secondSubscribers = new Component[0];

        private void Start()
        {
            this.fixedUpdateRate = 1 / Time.fixedDeltaTime;
        }

        private void FixedUpdate()
        {
            this.clock++;

            if (this.clock >= this.fixedUpdateRate)
            {
                this.clock = 0;
                this.BroadcastSecondPassed();
            }
        }

        public void OnEverySecond(UdonBehaviour behaviour)
        {
            Component[] newSecondSubscribers = new Component[this.secondSubscribers.Length + 1];
            Array.Copy(this.secondSubscribers, newSecondSubscribers, this.secondSubscribers.Length);

            newSecondSubscribers[newSecondSubscribers.Length - 1] = behaviour;

            this.secondSubscribers = newSecondSubscribers;
        }

        public void OffEverySecond(UdonBehaviour behaviour)
        {
            Component[] newSecondSubscribers = new Component[this.secondSubscribers.Length - 1];

            int j = 0;

            // Copy all subscribers from the current list into the new one, except for the one given.
            // This will effectively remove the subscriber from the list.
            for (int i = 0; i < this.secondSubscribers.Length; i++)
            {
                if (this.secondSubscribers[i] != behaviour)
                {
                    newSecondSubscribers[j] = this.secondSubscribers[i];
                    j++;
                }
            }

            this.secondSubscribers = newSecondSubscribers;
        }

        private void BroadcastSecondPassed()
        {
            Debug.Log("BroadcastSecondPassed()");
            // Tell subscribers that a second has passed
            for (int i = 0; i < this.secondSubscribers.Length; i++)
            {
                UdonBehaviour subscriber = (UdonBehaviour) this.secondSubscribers[i];

                if (subscriber.enabled && subscriber.gameObject.activeSelf)
                {
                    subscriber.SendCustomEvent(this.OnSecondPassedEvent);
                } else if (this.cleanupInactive == true)
                {
                    // Automatically unsubscribe a subscriber if it has been deactivated
                    this.OffEverySecond(subscriber);
                }
            }
        }
    }
}
