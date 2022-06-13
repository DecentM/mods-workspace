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
         * new object[] { prev, id, value, next };
         */

        public override object[] ToArray()
        {
            return this.Values;
        }

        public override void FromArray(object[] newValue)
        {
            this.AddRange(newValue);
        }

        private int[] idIndex = new int[0];

        private void ReindexIds()
        {
            this.idIndex = new int[this.value.Length];

            for (int i = 0; i < this.value.Length; i++)
            {
                object[] item = (object[])this.ElementAt(this.value, i);

                if (item == null || item.Length != 4 || item[1] == null)
                    continue;

                this.idIndex[i] = (int)item[1];
            }
        }

        public int IdByIndex(int index)
        {
            if (index < 0 || index >= this.idIndex.Length)
                return -1;

            return this.idIndex[index];
        }

        public int IndexById(int id)
        {
            for (int i = 0; i < this.idIndex.Length; i++)
            {
                if (this.IdByIndex(i) == id)
                    return i;
            }

            return -1;
        }

        private int lastId = 0;

        private object[] CreateItem(object prev, object id, object value, object next)
        {
            return new object[] { prev, id, value, next };
        }

        private object[] CreateItem(object prev, object value, object next)
        {
            return new object[] { prev, this.lastId++, value, next };
        }

        private object[] CreateItem(object value)
        {
            return new object[] { -1, this.lastId++, value, -1 };
        }

        private int FindWithNegativeIndex(int nullIndex)
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
            get { return this.FindWithNegativeIndex(0); }
        }

        public int LastIndex
        {
            get { return this.FindWithNegativeIndex(3); }
        }

        public object First
        {
            get { return this.ElementAt(this.Values, this.FirstIndex); }
        }

        public object Last
        {
            get { return this.ElementAt(this.Values, this.LastIndex); }
        }

        private void UpdateItemNext(int id, int next)
        {
            int index = this.IndexById(id);
            object itemOrNull = this.ElementAt(this.value, index);

            if (itemOrNull == null)
                return;

            object[] item = (object[])itemOrNull;

            this.value[index] = this.CreateItem(item[0], item[1], item[2], next);
        }

        private void UpdateItemPrev(int id, int prev)
        {
            int index = this.IndexById(id);
            object itemOrNull = this.ElementAt(this.value, index);

            if (itemOrNull == null)
                return;

            object[] item = (object[])itemOrNull;
            this.value[index] = this.CreateItem(prev, item[1], item[2], item[3]);
        }

        private object[] Values
        {
            get
            {
                object[] result = new object[this.value.Length];

                for (int i = 0; i < this.value.Length; i++)
                {
                    result[i] = ((object[])this.value[i])[2];
                }

                return result;
            }
        }

        public object ElementById(int id)
        {
            return this.ElementAt(this.Values, this.IndexById(id));
        }

        private int[] GetBoundaries(object[] item)
        {
            int prev = -1;
            int next = -1;

            if (item[0] != null)
                prev = (int)item[0];
            if (item[3] != null)
                next = (int)item[3];

            return new int[] { prev, next };
        }

        public int[] Boundaries(int id)
        {
            object[] item = (object[])this.ElementAt(this.value, this.IndexById(id));

            if (item == null)
                return new int[] { -1, -1 };

            return this.GetBoundaries(item);
        }

        private int Add(object item, bool shouldReindex)
        {
            int lastItemIndex = this.LastIndex;

            // If there's an item with no "next" property, we need to add the new item as the next one.
            // The "next" property of the current item is always null, as we're always adding the newest item.
            if (lastItemIndex >= 0)
            {
                this.value = this.Add(this.value, this.CreateItem(item));

                if (shouldReindex)
                    this.ReindexIds();

                this.UpdateItemPrev(
                    this.IdByIndex(this.value.Length - 1),
                    this.IdByIndex(lastItemIndex)
                );

                this.UpdateItemNext(lastItemIndex, this.IdByIndex(this.value.Length - 1));
            }
            // Otherwise, we're adding the first item, so we create it with no "prev" property.
            else
            {
                this.value = this.Add(this.value, this.CreateItem(item));

                if (shouldReindex)
                    this.ReindexIds();
            }

            return this.IdByIndex(this.value.Length - 1);
        }

        public int Add(object item)
        {
            return this.Add(item, true);
        }

        public int AddAfter(int id, object item)
        {
            int[] bounds = this.Boundaries(id);

            // add the item onto the array at the end
            this.value = this.Add(this.value, this.CreateItem(id, item, bounds[1]));
            this.ReindexIds();

            int newId = this.IdByIndex(this.value.Length - 1);

            // update the "next" property of the item specified
            this.UpdateItemNext(id, newId);
            // update the "prev" property of the item after the item specified
            this.UpdateItemPrev(bounds[1], newId);

            return newId;
        }

        public int[] AddRange(object[] items)
        {
            int[] result = new int[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                result[i] = this.Add(items[i], false);
            }

            this.ReindexIds();

            return result;
        }

        public bool Remove(int id)
        {
            int index = this.IndexById(id);
            int[] bounds = this.Boundaries(id);
            int length = this.value.Length;

            this.value = this.RemoveAt(this.value, index);
            this.ReindexIds();

            this.UpdateItemPrev(bounds[1], bounds[0]);
            this.UpdateItemNext(bounds[0], bounds[1]);

            return this.value.Length == length - 1;
        }
    }
}
