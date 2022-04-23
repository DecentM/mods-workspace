using System;
using UdonSharp;

namespace DecentM.Pubsub
{
    public abstract class PubsubHost<Messages> : UdonSharpBehaviour
    {
        private PubsubSubscriber<Messages>[] subscribers;

        public int Subscribe(PubsubSubscriber<Messages> behaviour)
        {
            bool initialised = this.subscribers != null;

            if (initialised)
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

        protected void BroadcastEvent(Messages eventName, object[] data)
        {
            if (this.subscribers == null) return;

            foreach (UdonSharpBehaviour subscriber in this.subscribers)
            {
                this.QueuePush(eventName, data, subscriber);
            }
        }

        // Holy shit I created a monster
        // I hope there's a better way of doing this and I just don't know about it
        protected void BroadcastEvent(Messages eventName, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            this.BroadcastEvent(eventName, new object[] { arg1, arg2, arg3, arg4, arg5 });
        }

        protected void BroadcastEvent(Messages eventName, object arg1, object arg2, object arg3, object arg4)
        {
            this.BroadcastEvent(eventName, new object[] { arg1, arg2, arg3, arg4 });
        }

        protected void BroadcastEvent(Messages eventName, object arg1, object arg2, object arg3)
        {
            this.BroadcastEvent(eventName, new object[] { arg1, arg2, arg3 });
        }

        protected void BroadcastEvent(Messages eventName, object arg1, object arg2)
        {
            this.BroadcastEvent(eventName, new object[] { arg1, arg2 });
        }

        protected void BroadcastEvent(Messages eventName, object arg1)
        {
            this.BroadcastEvent(eventName, new object[] { arg1 });
        }

        protected void BroadcastEvent(Messages eventName)
        {
            this.BroadcastEvent(eventName, null);
        }
    }
}
