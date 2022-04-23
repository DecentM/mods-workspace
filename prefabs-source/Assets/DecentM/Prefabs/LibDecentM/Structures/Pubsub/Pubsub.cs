using System;
using UdonSharp;

namespace DecentM.Pubsub
{
    public abstract class PubsubHost<Messages> : UdonSharpBehaviour
    {
        private PubsubSubscriber<Messages>[] subscribers;

        public int Subscribe(PubsubSubscriber<Messages> behaviour)
        {
            if (this.subscribers != null)
            {
                PubsubSubscriber<Messages>[] tmp = new PubsubSubscriber<Messages>[this.subscribers.Length + 1];
                Array.Copy(this.subscribers, 0, tmp, 0, this.subscribers.Length);
                tmp[tmp.Length - 1] = behaviour;
                this.subscribers = tmp;
            }
            else
            {
                PubsubSubscriber<Messages>[] tmp = new PubsubSubscriber<Messages>[1];
                tmp[0] = behaviour;
                this.subscribers = tmp;
            }

            return this.subscribers.Length - 1;
        }

        public bool Unsubscribe(int index)
        {
            if (this.subscribers == null || this.subscribers.Length == 0 || index < 0 || index >= this.subscribers.Length) return false;

            PubsubSubscriber<Messages>[] tmp = new PubsubSubscriber<Messages>[subscribers.Length + 1];
            Array.Copy(this.subscribers, 0, tmp, 0, index);
            Array.Copy(this.subscribers, index + 1, tmp, index, this.subscribers.Length - 1 - index);
            this.subscribers = tmp;

            return true;
        }

        private object[][] queue;

        private void QueuePush(object eventName, object[] data, UdonSharpBehaviour behaviour)
        {
            bool initialised = this.queue != null;

            if (!initialised)
            {
                this.queue = new object[0][];
            }

            object[] queueItem = new object[] { eventName, data, behaviour };

            object[][] tmp = new object[this.queue.Length + 1][];
            Array.Copy(this.queue, 0, tmp, 0, this.queue.Length);
            tmp[tmp.Length - 1] = queueItem;
            this.queue = tmp;
        }

        private object[] QueuePop()
        {
            if (this.queue == null || this.queue.Length == 0) return null;

            object[] result = this.queue[0];

            object[][] tmp = new object[this.queue.Length - 1][];
            Array.Copy(this.queue, 1, tmp, 0, this.queue.Length);
            this.queue = tmp;

            return result;
        }

        private void FixedUpdate()
        {
            // Get the first item from the queue and also remove it from the queue
            object[] queueItem = this.QueuePop();

            if (queueItem == null) return;

            string eventName = (string)queueItem[0];
            object data = queueItem[1];
            UdonSharpBehaviour behaviour = (UdonSharpBehaviour)queueItem[2];

            behaviour.SetProgramVariable($"OnPubsubEvent_name", eventName);
            behaviour.SetProgramVariable($"OnPubsubEvent_data", data);
            behaviour.SendCustomEvent($"OnPubsubEvent");
        }

        protected void BroadcastEvent(Messages eventName, params object[] data)
        {
            if (this.subscribers == null) return;

            foreach (UdonSharpBehaviour subscriber in this.subscribers)
            {
                this.QueuePush(eventName, data, subscriber);
            }
        }
    }
}
