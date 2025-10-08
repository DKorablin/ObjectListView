﻿using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	[TestClass]
	public class TestFlagClusteringStrategy
	{

		[TestMethod]
		public void Test_EnumConstruction_Values()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			Assert.AreEqual(4, strategy.Values.Length);
			Assert.Contains(2, strategy.Values);
			Assert.Contains(4, strategy.Values);
			Assert.Contains(8, strategy.Values);
			Assert.Contains(16, strategy.Values);
		}

		[TestMethod]
		public void Test_EnumConstruction_Labels()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			Assert.AreEqual(4, strategy.Labels.Length);
			Assert.Contains("FlagValue1", strategy.Labels);
			Assert.Contains("FlagValue2", strategy.Labels);
			Assert.Contains("FlagValue3", strategy.Labels);
			Assert.Contains("FlagValue4", strategy.Labels);
		}

		[TestMethod]
		public void Test_GetClusterKey()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return TestFlagEnum.FlagValue1 | TestFlagEnum.FlagValue4; };
			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.AreEqual(2, ((ICollection)result).Count);
			Assert.Contains((ulong)TestFlagEnum.FlagValue1, result as ICollection);
			Assert.Contains((ulong)TestFlagEnum.FlagValue4, result as ICollection);
		}

		[TestMethod]
		public void Test_GetClusterKey_ZeroValue()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return 0; };
			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.AreEqual(0, ((ICollection)result).Count);
		}

		[TestMethod]
		public void Test_GetClusterKey_NonNumericValue()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return "not number"; };
			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.AreEqual(0, ((ICollection)result).Count);
		}

		[TestMethod]
		public void Test_GetClusterKey_NonConvertibleValue()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return new Object(); };
			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.AreEqual(0, ((ICollection)result).Count);
		}

		[TestMethod]
		public void Test_GetClusterDisplayLabel()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			ICluster cluster = new Cluster(TestFlagEnum.FlagValue2);
			cluster.Count = 2;
			String result = strategy.GetClusterDisplayLabel(cluster);
			Assert.AreEqual("FlagValue2 (2 items)", result);
		}

		[Flags]
		private enum TestFlagEnum
		{
			FlagValue1 = 2,
			FlagValue2 = 4,
			FlagValue3 = 8,
			FlagValue4 = 16
		}
	}
}