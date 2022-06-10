using System;

namespace DecentM.Collections
{
    public class List : Collection
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

        public object ElementAt(int index)
        {
            return this.ElementAt(this.value, index);
        }

        public bool Add(object item)
        {
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

        public bool Contains(object item)
        {
            return this.Contains(this.value, item);
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
