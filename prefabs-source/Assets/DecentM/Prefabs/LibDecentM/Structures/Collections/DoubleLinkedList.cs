using JetBrains.Annotations;

namespace DecentM.Collections
{
    public class DoubleLinkedList : Collection
    {
        /*
         * Value structure:
         * new object[] { prev, id, value, next };
         */

        [PublicAPI]
        public override bool Contains(object value)
        {
            return this.Contains(this.Values, value);
        }

        [PublicAPI]
        public bool Contains(int id)
        {
            return this.Contains(this.Ids, id);
        }

        [PublicAPI]
        public override object[] ToArray()
        {
            object[] result = new object[0];
            int currentId = this.FirstId;

            while (true)
            {
                int[] boundaries = this.Boundaries(currentId);
                result = this.Add(result, this.ElementById(currentId));

                if (boundaries[1] == -1)
                    break;

                currentId = boundaries[1];
            }

            return result;
        }

        [PublicAPI]
        public override void FromArray(object[] newValue)
        {
            this.Clear();
            this.AddRange(newValue);
        }

        [PublicAPI]
        public int IdByIndex(int index)
        {
            object[] item = (object[])this.ElementAt(this.value, index);

            if (item == null || item[1] == null)
                return -1;

            return (int)item[1];
        }

        [PublicAPI]
        public int IndexById(int id)
        {
            for (int i = 0; i < this.value.Length; i++)
            {
                if (this.IdByIndex(i) == id)
                    return i;
            }

            return -1;
        }

        private int nextItemId = 0;

        private object[] CreateItem(object prev, object id, object value, object next)
        {
            return new object[] { prev, id, value, next };
        }

        private object[] CreateItem(object prev, object value, object next)
        {
            return new object[] { prev, this.nextItemId++, value, next };
        }

        private object[] CreateItem(object value)
        {
            return new object[] { -1, this.nextItemId++, value, -1 };
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

        [PublicAPI]
        public int FirstId
        {
            get { return this.IdByIndex(this.FindWithNegativeIndex(0)); }
        }

        [PublicAPI]
        public int LastId
        {
            get { return this.IdByIndex(this.FindWithNegativeIndex(3)); }
        }

        [PublicAPI]
        public object First
        {
            get { return this.ElementById(this.FirstId); }
        }

        [PublicAPI]
        public object Last
        {
            get { return this.ElementById(this.LastId); }
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

        private object[] Ids
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

        [PublicAPI]
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

        [PublicAPI]
        public int[] Boundaries(int id)
        {
            object[] item = (object[])this.ElementAt(this.value, this.IndexById(id));

            if (item == null)
                return new int[] { -1, -1 };

            return this.GetBoundaries(item);
        }

        [PublicAPI]
        public int Prev(int id)
        {
            return this.Boundaries(id)[0];
        }

        [PublicAPI]
        public int Next(int id)
        {
            return this.Boundaries(id)[1];
        }

        [PublicAPI]
        public int Add(object item)
        {
            int lastItemId = this.LastId;

            // If there's an item with no "next" property, we need to add the new item as the next one.
            // The "next" property of the current item is always null, as we're always adding the newest item.
            if (lastItemId >= 0)
            {
                this.value = this.Add(this.value, this.CreateItem(item));

                this.UpdateItemPrev(this.IdByIndex(this.value.Length - 1), lastItemId);
                this.UpdateItemNext(lastItemId, this.IdByIndex(this.value.Length - 1));
            }
            // Otherwise, we're adding the first item, so we create it with no "prev" property.
            else
            {
                this.value = this.Add(this.value, this.CreateItem(item));
            }

            return this.IdByIndex(this.value.Length - 1);
        }

        [PublicAPI]
        public int AddAfter(int id, object item)
        {
            if (!this.Contains(id))
                return -1;

            int[] bounds = this.Boundaries(id);

            // Add the item to the end of the array and connect it to the upper bounds of the
            // item before
            this.value = this.Add(this.value, this.CreateItem(id, item, bounds[1]));

            int newId = this.IdByIndex(this.value.Length - 1);

            // update the "next" property of the item specified
            this.UpdateItemNext(id, newId);
            // update the "prev" property of the item after the item specified
            this.UpdateItemPrev(bounds[1], newId);

            return newId;
        }

        [PublicAPI]
        public int[] AddRange(object[] items)
        {
            int[] result = new int[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                result[i] = this.Add(items[i]);
            }

            return result;
        }

        [PublicAPI]
        public bool Remove(int id)
        {
            int index = this.IndexById(id);
            int[] bounds = this.Boundaries(id);
            int length = this.value.Length;

            this.value = this.RemoveAt(this.value, index);

            this.UpdateItemPrev(bounds[1], bounds[0]);
            this.UpdateItemNext(bounds[0], bounds[1]);

            return this.value.Length == length - 1;
        }
    }
}
