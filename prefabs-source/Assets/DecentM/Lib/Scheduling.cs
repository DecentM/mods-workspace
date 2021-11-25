
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
        public readonly string OnSecondPassedEvent = "OnSecondPassed";

        private int clock = 0;
        private float fixedUpdateRate;

        private Component[] secondSubscribers = new Component[0];

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
            
            // Copy all subscribers from the current list into the new one, except for the one given.
            // This will effectively remove the subscriber from the list.
            for (int i = 0; i < this.secondSubscribers.Length;)
            {
                if (this.secondSubscribers[i] != behaviour)
                {
                    newSecondSubscribers[i] = this.secondSubscribers[i];
                    i++;
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

                Debug.Log($"subscriber.SendCustomEvent() - {subscriber.name}");

                if (subscriber.enabled && subscriber.gameObject.activeSelf)
                {
                    subscriber.SendCustomEvent(this.OnSecondPassedEvent);
                } else
                {
                    // Automatically unsubscribe a subscriber if it has been deactivated
                    this.OffEverySecond(subscriber);
                }
            }
        }
    }
}
