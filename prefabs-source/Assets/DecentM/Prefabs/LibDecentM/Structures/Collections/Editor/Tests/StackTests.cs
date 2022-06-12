using NUnit.Framework;
using UnityEngine;

namespace DecentM.Collections.Tests
{
    public class StackTests
    {
        private Stack Prepare()
        {
            Stack stack = new Stack();

            stack.Push("a");
            stack.Push("b");
            stack.Push("c");

            return stack;
        }

        [Test]
        public void Push_Count()
        {
            Stack stack = this.Prepare();

            Assert.AreEqual(3, stack.Count);
        }

        [Test]
        public void Push_Duplicate()
        {
            Stack stack = this.Prepare();

            Assert.IsTrue(stack.Push("a"));
            Assert.AreEqual(4, stack.Count);
        }

        [Test]
        public void Peek()
        {
            Stack stack = this.Prepare();

            Assert.AreEqual("c", stack.Peek());
        }

        [Test]
        public void Pop()
        {
            Stack stack = this.Prepare();

            Assert.AreEqual("c", stack.Pop());
            Assert.AreEqual("b", stack.Pop());
            Assert.AreEqual("a", stack.Pop());
        }
    }
}
