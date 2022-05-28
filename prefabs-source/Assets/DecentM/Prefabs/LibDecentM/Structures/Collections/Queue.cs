using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Collections
{
    public class Queue : Collection
    {
        public void Enqueue(dynamic item)
        {
            if (this.value == null || this.value.Length == 0)
            {
                this.value = new dynamic[] { item };
                return;
            }

            dynamic[] tmp = new dynamic[value.Length + 1];
            Array.Copy(this.value, tmp, this.value.Length);
            tmp[tmp.Length - 1] = item;
            this.value = tmp;
        }

        public dynamic Peek()
        {
            if (this.value == null || this.value.Length == 0)
                return null;

            return this.value[0];
        }

        public dynamic Dequeue()
        {
            dynamic item = this.Peek();

            if (item == null)
                return null;

            dynamic[] tmp = new dynamic[value.Length - 1];
            Array.Copy(this.value, 1, tmp, 0, this.value.Length - 1);
            this.value = tmp;

            return item;
        }
    }
}
