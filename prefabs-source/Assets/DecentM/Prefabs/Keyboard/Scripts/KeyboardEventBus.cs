using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Keyboard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KeyboardEventBus : UdonSharpBehaviour
    {
        public UdonSharpBehaviour[] subscribers;

        private void Start()
        {
            if (this.subscribers == null)
                this.subscribers = new UdonSharpBehaviour[0];
        }

        public int Subscribe(UdonSharpBehaviour behaviour)
        {
            bool initialised = this.subscribers != null;

            if (initialised)
            {
                UdonSharpBehaviour[] tmp = new UdonSharpBehaviour[this.subscribers.Length + 1];
                Array.Copy(this.subscribers, 0, tmp, 0, this.subscribers.Length);
                tmp[tmp.Length - 1] = behaviour;
                this.subscribers = tmp;
            }
            else
            {
                UdonSharpBehaviour[] tmp = new UdonSharpBehaviour[1];
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

            UdonSharpBehaviour[] tmp = new UdonSharpBehaviour[subscribers.Length + 1];
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

        private void BroadcastEvent(string eventName, object[] data)
        {
            foreach (UdonSharpBehaviour subscriber in this.subscribers)
            {
                subscriber.SetProgramVariable($"OnBusEvent_name", eventName);
                subscriber.SetProgramVariable($"OnBusEvent_data", data);
                subscriber.SendCustomEvent("OnBusEvent");
            }
        }
    }
}
