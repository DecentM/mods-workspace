using System.Collections.Generic;
using UnityEngine;

namespace DecentM.Prefabs.Pubsub
{
    struct QueueItem
    {
        public QueueItem(object eventName, object[] data, PubsubSubscriber subscriber)
        {
            this.eventName = eventName;
            this.data = data;
            this.subscriber = subscriber;
        }

        public object eventName;
        public object[] data;
        public PubsubSubscriber subscriber;
    }

    public abstract class PubsubHost : MonoBehaviour
    {
        public int batchSize = 10;

        private Queue<QueueItem> queue = new Queue<QueueItem>();
        public List<PubsubSubscriber> subscribers = new List<PubsubSubscriber>();

        public int Subscribe(PubsubSubscriber behaviour)
        {
            if (this.subscribers.Contains(behaviour))
                return -1;

            this.subscribers.Add(behaviour);

            return this.subscribers.Count - 1;
        }

        public void Unsubscribe(int index)
        {
            this.subscribers.RemoveAt(index);
        }

        public void FixedUpdate()
        {
            if (this.queue == null || this.queue.Count == 0)
                return;

            int processedCount = 0;

            // Process a batch of items from the queue
            while (processedCount < this.batchSize && this.queue.Count > 0)
            {
                // Get the first item from the queue and also remove it from the queue
                QueueItem? queueItemOrNull = this.queue.Dequeue();

                if (queueItemOrNull == null)
                {
                    processedCount++;
                    break;
                }

                QueueItem queueItem = (QueueItem)queueItemOrNull;

                queueItem.subscriber.OnPubsubEvent(queueItem.eventName, queueItem.data);

                processedCount++;
            }
        }

        protected void BroadcastEvent(object eventName, params object[] data)
        {
            if (this.subscribers.Count == 0)
                return;

            foreach (PubsubSubscriber subscriber in this.subscribers.ToArray())
            {
                this.queue.Enqueue(new QueueItem(eventName, data, subscriber));
            }
        }
    }
}
