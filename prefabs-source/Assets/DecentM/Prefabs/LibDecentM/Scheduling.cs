using UnityEngine;
using System;

namespace DecentM.Prefabs
{
    public class Scheduling : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("If checked, disabled UdonBehaviours will be auto-unsubscribed from events")]
        public bool cleanupInactive = false;

        public readonly string OnSecondPassedEvent = "OnSecondPassed";

        [Header("Internals")]
        [Tooltip("The list of components that are currently receiving events")]
        public Component[] secondSubscribers = new Component[0];

        [Tooltip("The current state of the internal clock used to count time")]
        public int clock = 0;

        [Tooltip(
            "Calculated from Unity's value, this shows how many times FixedUpdate events are sent per second"
        )]
        public float fixedUpdateRate;

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

        public void OnEverySecond(MonoBehaviour behaviour)
        {
            Component[] newSecondSubscribers = new Component[this.secondSubscribers.Length + 1];
            Array.Copy(this.secondSubscribers, newSecondSubscribers, this.secondSubscribers.Length);

            newSecondSubscribers[newSecondSubscribers.Length - 1] = behaviour;

            this.secondSubscribers = newSecondSubscribers;
        }

        public void OffEverySecond(MonoBehaviour behaviour)
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
            // Tell subscribers that a second has passed
            for (int i = 0; i < this.secondSubscribers.Length; i++)
            {
                MonoBehaviour subscriber = (MonoBehaviour)this.secondSubscribers[i];

                if (subscriber.enabled && subscriber.gameObject.activeSelf)
                {
                    subscriber.Invoke(this.OnSecondPassedEvent, 0);
                }
                else if (this.cleanupInactive == true)
                {
                    // Automatically unsubscribe a subscriber if it has been deactivated
                    this.OffEverySecond(subscriber);
                }
            }
        }
    }
}
