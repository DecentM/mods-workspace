using JetBrains.Annotations;

namespace DecentM.Collections
{
    public class Stack : Collection
    {
        [PublicAPI]
        public bool Push(object item)
        {
            int length = this.value.Length;
            this.value = this.Insert(this.value, 0, item);

            return this.value.Length == length + 1;
        }

        [PublicAPI]
        public object Peek()
        {
            return this.ElementAt(this.value, 0);
        }

        [PublicAPI]
        public object Pop()
        {
            object item = this.Peek();

            if (item == null)
                return null;

            this.value = this.RemoveAt(this.value, 0);

            return item;
        }
    }
}
