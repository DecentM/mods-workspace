using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections;

namespace DecentM.Collections
{
    public class List : Collection
    {
        public object ElementAt(int index)
        {
            if (index < 0 || index >= this.value.Length)
                return null;

            return this.value[index];
        }

        public bool Add(object item)
        {
            if (this.Contains(item))
                return false;

            if (this.value == null || this.value.Length == 0)
            {
                this.value = new object[] { item };
                return true;
            }

            object[] tmp = new object[value.Length + 1];
            Array.Copy(this.value, tmp, this.value.Length);
            tmp[tmp.Length - 1] = item;
            this.value = tmp;

            return true;
        }

        public void AddRange(object[] items)
        {
            foreach (object item in items)
            {
                // `Add` ignores duplicate items, this way every item will be
                // added without duplicating any
                this.Add(item);
            }
        }

        public bool Insert(int index, object item)
        {
            if (this.Contains(item))
                return false;

            object[] tmp = new object[value.Length + 1];
            Array.Copy(this.value, 0, tmp, 0, index);
            Array.Copy(this.value, index, tmp, index + 1, this.value.Length - index);
            tmp[index] = item;
            this.value = tmp;

            return true;
        }

        public void InsertRange(int startIndex, object[] items)
        {
            for (int i = startIndex; i < startIndex + items.Length; i++)
            {
                // `Insert` ignores duplicate items, this way every item will be
                // inserted without duplicating any
                this.Insert(i, items[i - startIndex]);
            }
        }

        public int IndexOf(object item)
        {
            for (int i = 0; i < this.value.Length; i++)
            {
                object valueItem = this.value[i];
                if (valueItem == item)
                    return i;
            }

            return -1;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= this.value.Length)
                return false;

            object[] tmp = new object[value.Length - 1];
            Array.Copy(this.value, tmp, index);
            Array.Copy(this.value, index + 1, tmp, index, this.value.Length - index - 1);
            this.value = tmp;

            return true;
        }

        public bool Remove(object item)
        {
            int index = this.IndexOf(item);

            if (index == -1)
                return false;

            return this.RemoveAt(index);
        }

        public bool RemoveRange(int startIndex, int endIndex)
        {
            if (startIndex > endIndex)
                return false;

            object[] tmp = new object[this.value.Length - (endIndex - startIndex + 1)];
            Array.Copy(this.value, 0, tmp, 0, startIndex);
            Array.Copy(this.value, endIndex + 1, tmp, startIndex, this.value.Length - endIndex - 1);
            this.value = tmp;

            return true;
        }

        public void Reverse()
        {
            object[] tmp = new object[this.value.Length];

            for (int i = 0; i < this.value.Length; i++)
            {
                tmp[i] = this.value[this.value.Length - 1 - i];
            }

            this.value = tmp;
        }
    }
}
