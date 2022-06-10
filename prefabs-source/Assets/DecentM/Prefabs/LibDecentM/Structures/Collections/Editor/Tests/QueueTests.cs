using NUnit.Framework;
using UnityEngine;

namespace DecentM.Collections.Tests
{
    public class QueueTests
    {
        [Test]
        public void Enqueue_Count()
        {
            Queue queue = new Queue();

            queue.Enqueue("a");

            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void Enqueue_Peek()
        {
            Queue queue = new Queue();

            queue.Enqueue("a");

            Assert.AreEqual("a", queue.Peek());
        }

        [Test]
        public void Shift_Count()
        {
            Queue queue = new Queue();

            queue.Shift("b");
            Assert.AreEqual(1, queue.Count);

            queue.Shift("a");
            Assert.AreEqual(2, queue.Count);
        }

        [Test]
        public void Shift_Peek()
        {
            Queue queue = new Queue();

            queue.Enqueue("a");
            queue.Shift("b");

            Assert.AreEqual("b", queue.Peek());
        }

        [Test]
        public void Dequeue_Count()
        {
            Queue queue = new Queue();

            queue.Enqueue("a");
            queue.Enqueue("b");

            Assert.AreEqual(2, queue.Count);
            Assert.AreEqual("a", queue.Dequeue());
            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual("b", queue.Dequeue());
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void Dequeue_Peek()
        {
            Queue queue = new Queue();

            queue.Enqueue("a");
            queue.Enqueue("b");

            Assert.AreEqual("a", queue.Dequeue());
            Assert.AreEqual("b", queue.Peek());
            Assert.AreEqual("b", queue.Dequeue());
            Assert.AreEqual(null, queue.Peek());
        }
    }
}
