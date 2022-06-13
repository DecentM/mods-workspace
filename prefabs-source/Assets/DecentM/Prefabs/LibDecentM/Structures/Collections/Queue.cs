using JetBrains.Annotations;

namespace DecentM.Collections
{
    public class Queue : Collection
    {
        [PublicAPI]
        public void Enqueue(object item)
        {
            this.value = this.Add(this.value, item);
        }

        [PublicAPI]
        public object Peek()
        {
            return this.ElementAt(this.value, 0);
        }

        [PublicAPI]
        public object Dequeue()
        {
            object item = this.Peek();

            if (item == null)
                return null;

            this.value = this.RemoveAt(this.value, 0);

            return item;
        }

        [PublicAPI]
        public void Shift(object item)
        {
            this.value = this.Insert(this.value, 0, item);
        }
    }
}
