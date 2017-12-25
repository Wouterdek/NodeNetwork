using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.Utilities;

namespace NodeNetworkTests
{
    [TestClass]
    public class LCSTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestOldNull()
        {
            LongestCommonSubsequence.GetChanges(null, new List<int>()).Count();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNewNull()
        {
            LongestCommonSubsequence.GetChanges(new List<int>(), null).Count();
        }

        [TestMethod]
        public void TestEmptyAndEmpty()
        {
            Assert.AreEqual(0, LongestCommonSubsequence.GetChanges(new List<int>(), new List<int>()).Count());
        }

        [TestMethod]
        public void TestEmptyAndNonEmpty()
        {
            Assert.IsTrue(
                LongestCommonSubsequence.GetChanges(new List<int>(), new []{ 9 })
                    .SequenceEqual(new []{
                        (0, 9, LongestCommonSubsequence.ChangeType.Added)
                    })
            );
        }

        [TestMethod]
        public void TestNonEmptyAndEmpty()
        {
            Assert.IsTrue(
                LongestCommonSubsequence.GetChanges(new[] { 9 }, new List<int>())
                    .SequenceEqual(new[]{
                        (0, 9, LongestCommonSubsequence.ChangeType.Removed)
                    })
            );
        }

        [TestMethod]
        public void Test123_123()
        {
            Assert.AreEqual(0, LongestCommonSubsequence.GetChanges(new[] { 1, 2, 3 }, new []{ 1, 2, 3 }).Count());
        }

        [TestMethod]
        public void Test13_123()
        {
            Assert.IsTrue(
                LongestCommonSubsequence.GetChanges(new[] { 1, 3 }, new []{ 1, 2, 3 })
                    .SequenceEqual(new[]{
                        (1, 2, LongestCommonSubsequence.ChangeType.Added)
                    })
            );
        }

        [TestMethod]
        public void Test123_13()
        {
            Assert.IsTrue(
                LongestCommonSubsequence.GetChanges(new[] { 1, 2, 3 }, new[] { 1, 3 })
                    .SequenceEqual(new[]{
                        (1, 2, LongestCommonSubsequence.ChangeType.Removed)
                    })
            );
        }

        [TestMethod]
        public void Test134_123()
        {
            Assert.IsTrue(
                LongestCommonSubsequence.GetChanges(new[] { 1, 3, 4 }, new[] { 1, 2, 3 })
                    .SequenceEqual(new[]{
                        (2, 4, LongestCommonSubsequence.ChangeType.Removed),
                        (1, 2, LongestCommonSubsequence.ChangeType.Added)
                    })
            );
        }

        [TestMethod]
        public void Test123_134()
        {
            Assert.IsTrue(
                LongestCommonSubsequence.GetChanges(new[] { 1, 2, 3 }, new[] { 1, 3, 4 })
                    .SequenceEqual(new[]{
                        (1, 2, LongestCommonSubsequence.ChangeType.Removed),
                        (2, 4, LongestCommonSubsequence.ChangeType.Added)
                    })
            );
        }
    }
}
