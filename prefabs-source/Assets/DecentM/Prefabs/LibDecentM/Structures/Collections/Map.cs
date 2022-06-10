using System;

namespace DecentM.Collections
{
    public class Map : Collection
    {
        private object[] keys = new object[0];

        public bool Add(object key, object value)
        {
            if (this.Contains(key))
                return false;

            int kLength = this.keys.Length;
            int vLength = this.value.Length;

            this.keys = base.Add(this.keys, key);
            this.value = base.Add(this.value, value);

            return this.keys.Length == kLength + 1 && this.value.Length == vLength + 1;
        }

        public bool Remove(object key)
        {
            int index = this.IndexOf(this.keys, key);
            if (index == -1)
                return false;

            int kLength = this.keys.Length;
            int vLength = this.value.Length;

            this.keys = this.RemoveAt(this.keys, index);
            this.value = this.RemoveAt(this.value, index);

            return this.keys.Length + 1 == kLength && this.value.Length + 1 == vLength;
        }

        public object Get(object key)
        {
            int index = this.IndexOf(this.keys, key);

            if (index == -1)
                return null;

            return this.ElementAt(this.value, index);
        }

        public object KeyOf(object value)
        {
            int index = this.IndexOf(this.value, value);

            if (index == -1)
                return null;

            return this.ElementAt(this.keys, index);
        }

        public object[] Keys
        {
            get { return this.keys; }
        }

        public object[] Values
        {
            get { return this.value; }
        }

        public bool Contains(object item)
        {
            return this.Contains(this.keys, item);
        }

        public override void Clear()
        {
            this.value = new object[0];
            this.keys = new object[0];
        }

        public override int Count
        {
            get { return this.keys.Length; }
        }

        public override object[] ToArray()
        {
            object[][] result = new object[this.keys.Length][];

            for (int i = 0; i < this.keys.Length; i++)
            {
                result[i] = new object[] { this.keys[i], this.value[i] };
            }

            return result;
        }

        public override void FromArray(object[] newValue)
        {
            this.value = new object[newValue.Length];
            this.keys = new object[newValue.Length];

            for (int i = 0; i < newValue.Length; i++)
            {
                object[] item = (object[])newValue[i];

                if (item.Length != 2)
                {
                    this.keys[i] = null;
                    this.value[i] = null;
                    continue;
                }

                this.keys[i] = item[0];
                this.value[i] = item[1];
            }
        }
    }
}
