using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Collections
{
    public class Queue : Collection
    {
        public void Enqueue(object item)
        {
            if (this.value == null || this.value.Length == 0)
            {
                this.value = new object[] { item };
                return;
            }

            object[] tmp = new object[value.Length + 1];
            Array.Copy(this.value, tmp, this.value.Length);
            tmp[tmp.Length - 1] = item;
            this.value = tmp;
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

            object[] tmp = new object[value.Length - 1];
            Array.Copy(this.value, 1, tmp, 0, this.value.Length - 1);
            this.value = tmp;

            return item;
        }
    }
}
