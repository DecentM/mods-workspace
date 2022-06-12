using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DecentM.Collections
{
    public class DoubleLinkedList : Collection
    {
        /*
         * Value structure:
         * new object[] { prev, value, next };
         */

        private object[] CreateItem(object prev, object value, object next)
        {
            return new object[] { prev, value, next };
        }

        private object[] CreateItem(int prev, object value)
        {
            return new object[] { prev, value, -1 };
        }

        private object[] CreateItem(object value)
        {
            return new object[] { -1, value, -1 };
        }

        private int FindWithNullIndex(int nullIndex)
        {
            if (nullIndex < 0)
                return -1;

            for (int i = 0; i < this.value.Length; i++)
            {
                object[] item = (object[])this.value[i];
                if (item == null || nullIndex >= item.Length || item[nullIndex] == null)
                    continue;

                if ((int)item[nullIndex] == -1)
                    return i;
            }

            return -1;
        }

        public int FirstIndex
        {
            get { return this.FindWithNullIndex(0); }
        }

        public int LastIndex
        {
            get { return this.FindWithNullIndex(2); }
        }

        public object First
        {
            get { return this.ElementAt(this.FirstIndex); }
        }

        public object Last
        {
            get { return this.ElementAt(this.LastIndex); }
        }

        public int Next(int index)
        {
            object[] item = (object[])this.ElementAt(this.value, index);

            if (item == null || item.Length != 3 || item[2] == null)
                return -1;

            return (int)item[2];
        }

        public int Prev(int index)
        {
            object[] item = (object[])this.ElementAt(this.value, index);

            if (item == null || item.Length != 3 || item[0] == null)
                return -1;

            return (int)item[0];
        }

        private bool ValidateIndex(int index)
        {
            return this.value.Length > index && index >= 0;
        }

        private void UpdateItemNext(int index, int next)
        {
            if (!this.ValidateIndex(index))
                return;

            object itemOrNull = this.ElementAt(this.value, index);

            if (itemOrNull == null)
                return;

            object[] item = (object[])itemOrNull;

            this.value[index] = this.CreateItem(item[0], item[1], next);
        }

        private void UpdateItemPrev(int index, int prev)
        {
            if (!this.ValidateIndex(index))
                return;

            object itemOrNull = this.ElementAt(this.value, index);

            if (itemOrNull == null)
                return;

            object[] item = (object[])itemOrNull;
            this.value[index] = this.CreateItem(prev, item[1], item[2]);
        }

        public object[] Values
        {
            get
            {
                object[] result = new object[this.value.Length];

                for (int i = 0; i < this.value.Length; i++)
                {
                    result[i] = ((object[])this.value[i])[1];
                }

                return result;
            }
        }

        public object ElementAt(int index)
        {
            return this.ElementAt(this.Values, index);
        }

        private int[] GetBoundaries(object[] item)
        {
            int prev = -1;
            int next = -1;

            if (item[0] != null)
                prev = (int)item[0];
            if (item[2] != null)
                next = (int)item[2];

            return new int[] { prev, next };
        }

        public int[] BoundariesForIndex(int index)
        {
            object[] item = (object[])this.ElementAt(this.value, index);

            if (item == null)
                return new int[] { -1, -1 };

            return this.GetBoundaries(item);
        }

        public bool Add(object item)
        {
            int lastItemIndex = this.LastIndex;
            int length = this.value.Length;

            // If there's an item with no "next" property, we need to add the new item as the next one.
            // The "next" property of the current item is always null, as we're always adding the newest item.
            if (lastItemIndex >= 0)
            {
                this.value = this.Add(this.value, this.CreateItem(lastItemIndex, item));
                this.UpdateItemNext(lastItemIndex, this.value.Length - 1);
            }
            // Otherwise, we're adding the first item, so we create it with no "prev" property.
            else
            {
                this.value = this.Add(this.value, this.CreateItem(item));
            }

            return this.value.Length == length + 1;
        }

        public bool Remove(int index)
        {
            int length = this.value.Length;
            object[] item = (object[])this.ElementAt(this.value, index);
            int prev = (int)item[0];
            int next = (int)item[2];

            if (next >= 0)
                this.UpdateItemNext(prev, next);
            if (prev >= 0)
                this.UpdateItemPrev(next, prev);

            // Go through all items and adjust the prev and next values if they're after the index,
            // to compensate for the array indexes changing after removing an item
            for (int i = 0; i < this.value.Length; i++)
            {
                object[] updateItem = (object[])this.value[i];
                if (updateItem == null)
                    continue;

                int updateItemPrev = (int)((object[])updateItem[i])[0];
                int updateItemNext = (int)((object[])updateItem[i])[2];

                if (updateItemPrev > index)
                    this.UpdateItemPrev(i, updateItemPrev - 1);

                if (updateItemNext > index)
                    this.UpdateItemNext(i, updateItemNext - 1);
            }

            this.RemoveAt(this.value, index);

            return this.value.Length == length - 1;
        }
    }
}
