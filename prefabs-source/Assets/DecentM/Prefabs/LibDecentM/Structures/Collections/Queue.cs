using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Collections
{
    public class Queue : Collection
    {
        public override int Count
        {
            get { return this.value.Length; }
        }

        public override object[] ToArray()
        {
            return this.value;
        }

        public override void FromArray(object[] newValue)
        {
            this.value = newValue;
        }

        public void Enqueue(object item)
        {
            if (this.value == null || this.value.Length == 0)
            {
                this.value = new object[] { item };
                return;
            }

            this.value = this.Add(this.value, item);
        }

        public object Peek()
        {
            if (this.value == null || this.value.Length == 0)
                return null;

            return this.value[0];
        }

        public object Dequeue()
        {
            object item = this.Peek();

            if (item == null)
                return null;

            this.value = this.RemoveAt(this.value, 0);

            return item;
        }

        public void Shift(object item)
        {
            if (this.value == null || this.value.Length == 0)
            {
                this.value = new object[] { item };
                return;
            }

            this.value = this.Insert(this.value, 0, item);
        }
    }
}
