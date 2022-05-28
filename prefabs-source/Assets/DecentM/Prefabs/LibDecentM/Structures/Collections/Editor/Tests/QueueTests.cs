using NUnit.Framework;
using UnityEngine;

namespace DecentM.Collections.Tests
{
    public class QueueTests
    {
        [Test]
        public void Enqueue()
        {
            Queue queue = new Queue();

            queue.Enqueue("a");

            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual("a", queue.Peek());
        }

        [Test]
        public void Shift()
        {
            Queue queue = new Queue();

            queue.Enqueue("a");
            queue.Shift("b");

            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual("a", queue.Peek());
        }

        [Test]
        public void Dequeue()
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
    }
}
