using JetBrains.Annotations;

namespace DecentM.Collections
{
    public class Map : Collection
    {
        // Value structure: new object[]
        // {
        //      new object[] { key, value },
        //      new object[] { key, value },
        //      ...
        // };

        [PublicAPI]
        public bool Add(object key, object value)
        {
            if (this.Contains(key))
                return false;

            int length = this.value.Length;
            this.value = base.Add(this.value, new object[] { key, value });

            return this.value.Length == length + 1;
        }

        [PublicAPI]
        public bool Set(object key, object value)
        {
            this.Remove(key);

            int length = this.value.Length;
            this.value = base.Add(this.value, new object[] { key, value });

            return this.value.Length == length + 1;
        }

        [PublicAPI]
        public bool Remove(object key)
        {
            int index = this.IndexOf(this.Keys, key);

            if (index == -1)
                return false;

            int length = this.value.Length;
            this.value = this.RemoveAt(this.value, index);

            return this.value.Length == length - 1;
        }

        [PublicAPI]
        public object Get(object key)
        {
            int index = this.IndexOf(this.Keys, key);

            if (index == -1)
                return null;

            return this.ElementAt(this.Values, index);
        }

        [PublicAPI]
        public object KeyOf(object value)
        {
            int index = this.IndexOf(this.Values, value);

            if (index == -1)
                return null;

            return this.ElementAt(this.Keys, index);
        }

        [PublicAPI]
        public object[] Keys
        {
            get
            {
                object[] result = new object[this.value.Length];

                for (int i = 0; i < this.value.Length; i++)
                {
                    result[i] = ((object[])this.value[i])[0];
                }

                return result;
            }
        }

        [PublicAPI]
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

        [PublicAPI]
        public override bool Contains(object key)
        {
            return this.Contains(this.Keys, key);
        }
    }
}
