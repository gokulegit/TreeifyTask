using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.TaskTree;
using System.Linq;

namespace Octopus.Test
{
    [TestClass]
    public class AsyncTaskTest
    {
        [TestMethod]
        [ExpectedException(typeof(AsyncTasksCycleDetectedException))]
        public void TestCycleDetection()
        {
            var t1 = new AsyncTask("t1");
            var t2 = new AsyncTask("t2");
            var t3 = new AsyncTask("t3");
            var t4 = new AsyncTask("t4");
            t1.AddChild(t2);
            t2.AddChild(t3);
            t2.AddChild(t4);
            t3.AddChild(t4);
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncTasksCycleDetectedException))]
        public void TestCycleDetectionUsingParent()
        {
            var t1 = new AsyncTask("t1");
            var t2 = new AsyncTask("t2");
            var t3 = new AsyncTask("t3");
            var t4 = new AsyncTask("t4");
            t1.AddChild(t2);
            t2.AddChild(t3);
            t2.AddChild(t4);
            // Create a loop by adding root as t4's child. 
            t4.AddChild(t1);
        }

        [TestMethod]
        public void TestFlatList()
        {
            var t1 = new AsyncTask("t1");
            var t2 = new AsyncTask("t2");
            var t3 = new AsyncTask("t3");
            var t4 = new AsyncTask("t4");
            t2.AddChild(t1);
            t1.AddChild(t3);
            t3.AddChild(t4);

            var fromT2 = t2.ToFlatList().ToArray();
            Assert.IsNotNull(fromT2);
            Assert.AreEqual(4, fromT2.Length);

            var fromT1 = t1.ToFlatList().ToArray();
            Assert.IsNotNull(fromT1);
            Assert.AreEqual(3, fromT1.Length);
        }
    }
}
