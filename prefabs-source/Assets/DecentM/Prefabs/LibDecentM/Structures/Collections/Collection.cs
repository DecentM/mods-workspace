using System;
using UdonSharp;
using JetBrains.Annotations;

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
            if (array == null)
                return -1;

            for (int i = 0; i < array.Length; i++)
            {
                if (Equals(array[i], item))
                    return i;
            }

            return -1;
        }

        protected object ElementAt(object[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
                return null;

            return array[index];
        }

        protected object[] RemoveAt(object[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
                return array;

            object[] tmp = new object[array.Length - 1];
            Array.Copy(array, tmp, index);
            Array.Copy(array, index + 1, tmp, index, array.Length - index - 1);

            return tmp;
        }

        protected object[] RemoveRange(object[] array, int startIndex, int endIndex)
        {
            if (array == null || startIndex > endIndex)
                return array;

            object[] tmp = new object[array.Length - (endIndex - startIndex + 1)];
            Array.Copy(array, 0, tmp, 0, startIndex);
            Array.Copy(array, endIndex + 1, tmp, startIndex, array.Length - endIndex - 1);

            return tmp;
        }

        protected object[] Add(object[] array, object item)
        {
            if (array == null)
                return new object[] { item };

            return this.Insert(array, array.Length, item);
        }

        protected object[] Insert(object[] array, int index, object item)
        {
            if (array == null)
                return new object[] { item };

            object[] tmp = new object[array.Length + 1];
            Array.Copy(array, 0, tmp, 0, index);
            Array.Copy(array, index, tmp, index + 1, array.Length - index);
            tmp[index] = item;

            return tmp;
        }

        [PublicAPI]
        public virtual bool Contains(object item)
        {
            return this.Contains(this.value, item);
        }

        [PublicAPI]
        public virtual int Count
        {
            get { return this.value.Length; }
        }

        [PublicAPI]
        public virtual object[] ToArray()
        {
            return this.value;
        }

        [PublicAPI]
        public virtual void FromArray(object[] newValue)
        {
            this.value = newValue;
        }

        [PublicAPI]
        public void FromCollection(Collection collection)
        {
            this.FromArray(collection.ToArray());
        }

        [PublicAPI]
        public virtual void Clear()
        {
            this.value = new object[0];
        }
    }
}
