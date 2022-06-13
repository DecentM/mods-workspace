using NUnit.Framework;
using UnityEngine;

namespace DecentM.Collections.Tests
{
    public enum DLLTraversalDirection
    {
        Forward,
        Backward,
    }

    public class DoubleLinkedListTests
    {
        private DoubleLinkedList Prepare()
        {
            DoubleLinkedList dll = new DoubleLinkedList();

            dll.Add("a");
            dll.Add("b");
            dll.Add("c");

            return dll;
        }

        #region Utils

        private bool VerifyIntegrity(DoubleLinkedList dll)
        {
            int processed = 0;

            void Traverse(int index, DLLTraversalDirection direction)
            {
                int[] boundaries = dll.Boundaries(index);
                processed++;

                switch (direction)
                {
                    case DLLTraversalDirection.Forward:

                        {
                            if (boundaries[1] >= 0)
                            {
                                Traverse(boundaries[1], direction);
                                return;
                            }
                        }
                        break;

                    case DLLTraversalDirection.Backward:

                        {
                            if (boundaries[0] >= 0)
                            {
                                Traverse(boundaries[0], direction);
                                return;
                            }
                        }
                        break;
                }
            }

            Traverse(dll.FirstIndex, DLLTraversalDirection.Forward);
            Traverse(dll.LastIndex, DLLTraversalDirection.Backward);

            Debug.Log(
                $"processed: {processed} (expected: {dll.Count * 2}), count: {dll.Count}, firstIndex: {dll.FirstIndex}"
            );

            return processed == dll.Count * 2;
        }

        private void Dump(DoubleLinkedList dll)
        {
            for (int i = 0; i < dll.Count; i++)
            {
                object item = dll.Values[i];
                int[] boundaries = dll.Boundaries(i);

                Debug.Log(
                    $"prev: {boundaries[0]}, id: {dll.IdByIndex(i)}, item: {(string)item}, next: {boundaries[1]}"
                );
            }
        }

        #endregion

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

        [Test]
        public void Add()
        {
            DoubleLinkedList dll = this.Prepare();

            Assert.AreEqual(3, dll.Add("d"));
            Assert.AreEqual("d", dll.ElementById(3));
            Assert.IsNull(dll.ElementById(4));
        }

        [Test]
        public void AddAfter_Integrity()
        {
            DoubleLinkedList dll = this.Prepare();

            dll.AddAfter(1, "x");

            Assert.IsTrue(this.VerifyIntegrity(dll));
        }

        [Test]
        public void Remove_Count()
        {
            DoubleLinkedList dll = this.Prepare();

            dll.Remove(1);

            Assert.AreEqual(2, dll.Count);
        }

        [Test]
        public void Remove_Integrity()
        {
            DoubleLinkedList dll = this.Prepare();

            dll.Remove(1);

            Assert.IsTrue(this.VerifyIntegrity(dll));
        }

        [Test]
        public void Id_Integrity()
        {
            DoubleLinkedList dll = this.Prepare();

            this.Dump(dll);

            int id = dll.Add("d");
            dll.Remove(1);

            Debug.Log("=================");
            this.Dump(dll);

            Assert.AreEqual("d", dll.ElementById(id));
        }
    }
}
