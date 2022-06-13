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

            void Traverse(int id, DLLTraversalDirection direction)
            {
                if (dll.ElementById(id) == null)
                    return;

                int[] boundaries = dll.Boundaries(id);
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

            Traverse(dll.FirstId, DLLTraversalDirection.Forward);
            Traverse(dll.LastId, DLLTraversalDirection.Backward);

            bool success = processed == dll.Count * 2;

            if (!success)
                this.Dump(dll);

            return success;
        }

        private void Dump(DoubleLinkedList dll)
        {
            for (int i = 0; i < dll.Count; i++)
            {
                int id = dll.IdByIndex(i);
                object item = dll.ElementById(id);
                int[] boundaries = dll.Boundaries(id);

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

            Assert.IsTrue(dll.Remove(1));

            Assert.AreEqual(2, dll.Count);
        }

        [Test]
        public void Remove_Integrity()
        {
            DoubleLinkedList dll = this.Prepare();

            Assert.IsTrue(dll.Remove(1));

            Assert.IsTrue(this.VerifyIntegrity(dll));
        }

        [Test]
        public void Id_Integrity()
        {
            DoubleLinkedList dll = this.Prepare();

            int id = dll.Add("d");

            Assert.AreEqual("d", dll.ElementById(id));
            Assert.IsTrue(this.VerifyIntegrity(dll));
        }
    }
}
