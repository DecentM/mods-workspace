using System;

namespace DecentM.Collections
{
    public class List : Collection
    {
        public object ElementAt(int index)
        {
            return this.ElementAt(this.value, index);
        }

        public bool Add(object item)
        {
            if (this.Contains(item))
                return false;

            int length = this.value.Length;
            this.value = this.Add(this.value, item);

            return this.value.Length == length + 1;
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

            int length = this.value.Length;
            this.value = this.Insert(this.value, index, item);

            return this.value.Length == length + 1;
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
            return this.IndexOf(this.value, item);
        }

        public bool RemoveAt(int index)
        {
            int length = this.value.Length;
            this.value = this.RemoveAt(this.value, index);

            // Return success if the result array is one smaller than the previous value
            return this.value.Length == length - 1;
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

            int length = this.value.Length;
            this.value = this.RemoveRange(this.value, startIndex, endIndex);

            return this.value.Length == length - (endIndex - startIndex);
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
