using NUnit.Framework;
using UnityEngine;

namespace DecentM.Collections.Tests
{
    public class DoubleLinkedListTests
    {
        private DoubleLinkedList Prepare()
        {
            DoubleLinkedList dll = new DoubleLinkedList();

            dll.Add("a");
            dll.Add("b");
            dll.Add("c");

            this.Dump(dll);

            return dll;
        }

        private bool VerifyIntegrity(DoubleLinkedList dll)
        {
            int processed = 0;

            void Traverse(int index)
            {
                int next = dll.Next(index);
                processed++;

                if (next >= 0)
                {
                    Traverse(next);
                    return;
                }
            }

            int firstIndex = dll.FirstIndex;
            Traverse(firstIndex);

            Debug.Log($"processed: {processed}, count: {dll.Count}, firstIndex: {firstIndex}");

            return processed == dll.Count;
        }

        private void Dump(DoubleLinkedList dll)
        {
            for (int i = 0; i < dll.Count; i++)
            {
                object item = dll.ElementAt(i);
                int prev = dll.Prev(i);
                int next = dll.Next(i);

                Debug.Log($"item {i} - prev: {prev}, item: {(string)item}, next: {next}");
            }
        }

        [Test]
        public void Add_Count()
        {
            DoubleLinkedList dll = this.Prepare();

            Assert.AreEqual(3, dll.Count);
        }

        [Test]
        public void Add_Integrity()
        {
            DoubleLinkedList dll = this.Prepare();

            Assert.IsTrue(this.VerifyIntegrity(dll));
        }
    }
}
