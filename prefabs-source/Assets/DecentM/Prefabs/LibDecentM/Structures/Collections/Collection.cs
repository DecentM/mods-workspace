using System;
using UdonSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.Collections
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class Collection : UdonSharpBehaviour
    {
        protected object[] value = new object[0];

        protected bool Contains(object[] array, object item)
        {
            return this.IndexOf(array, item) >= 0;
        }

        protected int IndexOf(object[] array, object item)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == item)
                    return i;
            }

            return -1;
        }

        protected object ElementAt(object[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return null;

            return array[index];
        }

        protected object[] RemoveAt(object[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return array;

            object[] tmp = new object[array.Length - 1];
            Array.Copy(array, tmp, index);
            Array.Copy(array, index + 1, tmp, index, array.Length - index - 1);

            return tmp;
        }

        protected object[] Add(object[] array, object item)
        {
            if (array == null || this.Contains(array, item))
                return array;

            object[] tmp = new object[array.Length + 1];
            Array.Copy(array, tmp, array.Length);
            tmp[tmp.Length - 1] = item;

            return tmp;
        }

        public abstract int Count { get; }

        public abstract object[] ToArray();

        public abstract void FromArray(object[] newValue);

        public virtual void Clear()
        {
            this.value = new object[0];
        }
    }
}
