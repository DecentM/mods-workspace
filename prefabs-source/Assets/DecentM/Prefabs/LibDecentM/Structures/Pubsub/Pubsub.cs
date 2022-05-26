using System;
using UdonSharp;
using UnityEngine;

namespace DecentM.Pubsub
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class PubsubHost : UdonSharpBehaviour
    {
        public int batchSize = 10;

        [NonSerialized]
        public PubsubSubscriber[] subscribers;

        public int Subscribe(PubsubSubscriber behaviour)
        {
            if (this.subscribers != null)
            {
                PubsubSubscriber[] tmp = new PubsubSubscriber[this.subscribers.Length + 1];
                Array.Copy(this.subscribers, 0, tmp, 0, this.subscribers.Length);
                tmp[tmp.Length - 1] = behaviour;
                this.subscribers = tmp;
            }
            else
            {
                PubsubSubscriber[] tmp = new PubsubSubscriber[1];
                tmp[0] = behaviour;
                this.subscribers = tmp;
            }

            return this.subscribers.Length - 1;
        }

        public bool Unsubscribe(int index)
        {
            if (
                this.subscribers == null
                || this.subscribers.Length == 0
                || index < 0
                || index >= this.subscribers.Length
            )
                return false;

            PubsubSubscriber[] tmp = new PubsubSubscriber[subscribers.Length + 1];
            Array.Copy(this.subscribers, 0, tmp, 0, index);
            Array.Copy(
                this.subscribers,
                index + 1,
                tmp,
                index,
                this.subscribers.Length - 1 - index
            );
            this.subscribers = tmp;

            return true;
        }

        private object[][] queue;

        private void QueuePush(object eventName, object[] data, PubsubSubscriber behaviour)
        {
            if (this.queue == null)
                this.queue = new object[0][];

            object[] queueItem = new object[] { eventName, data, behaviour };

            object[][] tmp = new object[this.queue.Length + 1][];
            Array.Copy(this.queue, 0, tmp, 0, this.queue.Length);
            tmp[tmp.Length - 1] = queueItem;
            this.queue = tmp;
        }

        private object[] QueuePop()
        {
            if (this.queue == null || this.queue.Length == 0)
                return null;

            object[] result = this.queue[0];

            object[][] tmp = new object[this.queue.Length - 1][];
            Array.Copy(this.queue, 1, tmp, 0, this.queue.Length - 1);
            this.queue = tmp;

            return result;
        }

        private void FixedUpdate()
        {
            if (this.queue == null || this.queue.Length == 0)
                return;

            int processedCount = 0;

            // Process a batch of items from the queue
            while (processedCount < this.batchSize)
            {
                // Get the first item from the queue and also remove it from the queue
                object[] queueItem = this.QueuePop();

                if (queueItem == null)
                {
                    processedCount++;
                    break;
                }

                string eventName = (string)queueItem[0];
                object[] data = (object[])queueItem[1];
                PubsubSubscriber subscriber = (PubsubSubscriber)queueItem[2];

                subscriber.OnPubsubEvent(eventName, data);

                processedCount++;
            }
        }

        protected void BroadcastEvent(object eventName, params object[] data)
        {
            if (this.subscribers == null || this.subscribers.Length == 0)
                return;

            foreach (PubsubSubscriber subscriber in this.subscribers)
            {
                this.QueuePush(eventName, data, subscriber);
            }
        }
    }
}
