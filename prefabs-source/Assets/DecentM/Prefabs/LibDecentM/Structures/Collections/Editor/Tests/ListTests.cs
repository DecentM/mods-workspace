using NUnit.Framework;
using UnityEngine;

namespace DecentM.Collections.Tests
{
    public class ListTests
    {
        [Test]
        public void Add_Count()
        {
            List list = new List();
            Assert.AreEqual(0, list.Count);

            list.Add("a");
            Assert.AreEqual(1, list.Count);

            list.Add("b");
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void Add_Duplicates()
        {
            List list = new List();

            Assert.True(list.Add("a"));
            Assert.True(list.Add("b"));
            Assert.False(list.Add("a"));
        }

        [Test]
        public void AddRange_Count()
        {
            List list = new List();
            Assert.AreEqual(list.Count, 0);

            list.AddRange(new string[] { "a", "b" });
            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        public void AddRange_Duplicates()
        {
            List list = new List();

            list.AddRange(new string[] { "a", "b" });
            Assert.False(list.Add("b"));
        }

        [Test]
        public void Contains()
        {
            List list = new List();

            list.Add("a");
            list.Add("b");

            Assert.True(list.Contains("a"));
            Assert.True(list.Contains("b"));
            Assert.False(list.Contains("c"));
        }

        [Test]
        public void ElementAt()
        {
            List list = new List();

            list.Add("a");
            list.Add("b");

            Assert.AreEqual("b", list.ElementAt(1));
        }

        [Test]
        public void Insert_Count()
        {
            List list = new List();

            list.Add("a");
            list.Add("b");
            list.Add("c");

            Assert.True(list.Insert(1, "d"));
        }

        [Test]
        public void Insert_Duplicates()
        {
            List list = new List();

            list.Add("a");
            list.Add("b");

            Assert.True(list.Insert(1, "c"));
            Assert.False(list.Insert(1, "a"));
        }

        [Test]
        public void InsertRange_Count()
        {
            List list = new List();

            list.Add("a");
            list.Add("b");

            list.InsertRange(1, new string[] { "c", "d" });

            Assert.AreEqual("a", list.ElementAt(0));
            Assert.AreEqual("c", list.ElementAt(1));
            Assert.AreEqual("d", list.ElementAt(2));
            Assert.AreEqual("b", list.ElementAt(3));
        }

        [Test]
        public void InsertRange_Duplicates()
        {
            List list = new List();

            list.Add("a");
            list.Add("b");

            list.InsertRange(1, new string[] { "c", "d" });

            Assert.True(list.Add("e"));
            Assert.False(list.Add("c"));
        }

        [Test]
        public void IndexOf()
        {
            List list = new List();

            list.Add("a");
            list.Add("b");

            list.InsertRange(1, new string[] { "c", "d" });

            Assert.AreEqual(0, list.IndexOf("a"));
            Assert.AreEqual(1, list.IndexOf("c"));
            Assert.AreEqual(3, list.IndexOf("b"));
        }

        [Test]
        public void RemoveAt()
        {
            List list = new List();

            list.AddRange(new string[] { "a", "b", "c" });
            list.RemoveAt(0);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(0, list.IndexOf("b"));
            Assert.AreEqual(1, list.IndexOf("c"));
        }

        [Test]
        public void Remove()
        {
            List list = new List();

            list.AddRange(new string[] { "a", "b", "c" });
            list.Remove("b");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(0, list.IndexOf("a"));
            Assert.AreEqual(1, list.IndexOf("c"));
        }

        [Test]
        public void RemoveRange()
        {
            List list = new List();

            list.AddRange(new string[] { "a", "b", "c", "d", "e", "f" });
            list.RemoveRange(1, 2);

            Assert.AreEqual(list.Count, 4);
            Assert.AreEqual(0, list.IndexOf("a"));
            Assert.AreEqual(1, list.IndexOf("d"));
            Assert.AreEqual(2, list.IndexOf("e"));
            Assert.AreEqual(3, list.IndexOf("f"));
        }

        [Test]
        public void Reverse()
        {
            List list = new List();

            list.AddRange(new string[] { "a", "b", "c" });
            list.Reverse();

            Assert.AreEqual("a", list.ElementAt(2));
            Assert.AreEqual("b", list.ElementAt(1));
            Assert.AreEqual("c", list.ElementAt(0));
        }
    }
}
