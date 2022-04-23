using UnityEngine;
using UdonSharp;

namespace DecentM.Pubsub
{
    public abstract class PubsubSubscriber<Messages> : UdonSharpBehaviour
    {
        public PubsubHost<Messages>[] pubsubHosts;
        private int[] subscriptions;

        private void Start()
        {
            if (this.pubsubHosts == null)
            {
                Debug.LogError($"no pubsub host object is attached to {this.name}, this subscriber will not receive events");
                this.enabled = false;
                return;
            }

            this.subscriptions = new int[this.pubsubHosts.Length];

            if (this.pubsubHosts.Length == 0)
            {
                Debug.LogWarning($"no pubsub host object is attached to {this.name}, this subscriber will not reveive events until a host is attached");
            }

            this.SubscribeAll();
        }

        private void SubscribeAll()
        {
            for (int i = 0; i < this.pubsubHosts.Length; i++)
            {
                if (this.pubsubHosts[i] == null) continue;

                int subscription = this.pubsubHosts[i].Subscribe(this);

                this.subscriptions[i] = subscription;
            }
        }

        private void UnsubscribeAll()
        {
            for (int i = 0; i < this.pubsubHosts.Length; i++)
            {
                if (this.pubsubHosts[i] == null) continue;
                int subscription = this.subscriptions[i];

                this.pubsubHosts[i].Unsubscribe(subscription);
            }
        }

        // We expect inheriting behaviours to run this if they change our list of pubsub hosts
        protected void ResubscribeAll()
        {
            this.UnsubscribeAll();

            // Reset the subscription store to the new length
            this.subscriptions = new int[this.pubsubHosts.Length];

            this.SubscribeAll();
        }

        protected abstract void OnPubsubEvent(object name, object[] data);

        private object OnPubsubEvent_name;
        private object[] OnPubsubEvent_data;
        public void OnPubsubEvent()
        {
            this.OnPubsubEvent(OnPubsubEvent_name, OnPubsubEvent_data);
        }
    }
}
