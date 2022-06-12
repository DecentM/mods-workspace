using NUnit.Framework;
using UnityEngine;

namespace DecentM.Collections.Tests
{
    public class MapTests
    {
        private Map Prepare()
        {
            Map map = new Map();

            map.Add("a", 1);
            map.Add("b", 2);
            map.Add("c", 3);

            map.Add(4, "q");
            map.Add(5, "w");
            map.Add(6, "e");

            return map;
        }

        [Test]
        public void Add_Count()
        {
            Map map = this.Prepare();

            Assert.AreEqual(6, map.Count);
        }

        [Test]
        public void Add_Duplicates()
        {
            Map map = this.Prepare();

            Assert.IsFalse(map.Add("a", 2));

            Assert.AreEqual(6, map.Count);
        }

        [Test]
        public void Remove_Count()
        {
            Map map = this.Prepare();

            Assert.IsTrue(map.Remove("a"));
            Assert.IsFalse(map.Remove("a"));

            Assert.AreEqual(5, map.Count);
        }

        [Test]
        public void Get()
        {
            Map map = this.Prepare();

            Assert.AreEqual(2, map.Get("b"));
            Assert.IsNull(map.Get("y"));
        }

        [Test]
        public void KeyOf()
        {
            Map map = this.Prepare();

            Assert.AreEqual(4, map.KeyOf("q"));
            Assert.AreEqual("b", map.KeyOf(2));
            Assert.IsNull(map.KeyOf(4));
        }
    }
}
