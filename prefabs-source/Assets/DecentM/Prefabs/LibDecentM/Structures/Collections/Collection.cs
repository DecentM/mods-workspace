using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.Collections
{
#if COMPILER_UDONSHARP
    public abstract class Collection : UdonSharpBehaviour
#else
    public abstract class Collection
#endif
    {
        protected dynamic[] value = new dynamic[0];

        public bool Contains(dynamic item)
        {
            bool result = false;

            foreach (dynamic valueItem in this.value)
            {
                if (valueItem == item)
                    return true;
            }

            return result;
        }

        public int Count
        {
            get { return this.value.Length; }
        }

        public dynamic[] ToArray()
        {
            return this.value;
        }

        public void FromArray(dynamic[] newValue)
        {
            this.value = newValue;
        }

        public void Clear()
        {
            this.value = new dynamic[0];
        }
    }
}
