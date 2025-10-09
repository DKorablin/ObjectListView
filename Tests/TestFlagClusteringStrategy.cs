using System;
using System.Collections;
using System.Collections.Generic;
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
			Assert.HasCount(4, strategy.Values);
			Assert.Contains(2, strategy.Values);
			Assert.Contains(4, strategy.Values);
			Assert.Contains(8, strategy.Values);
			Assert.Contains(16, strategy.Values);
		}

		[TestMethod]
		public void Test_EnumConstruction_Labels()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			Assert.HasCount(4, strategy.Labels);
			Assert.Contains("FlagValue1", strategy.Labels);
			Assert.Contains("FlagValue2", strategy.Labels);
			Assert.Contains("FlagValue3", strategy.Labels);
			Assert.Contains("FlagValue4", strategy.Labels);
		}

		[TestMethod]
		public void Test_GetClusterKey()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn()
			{
				AspectGetter = (x) => TestFlagEnum.FlagValue1 | TestFlagEnum.FlagValue4,
			};
			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.HasCount(2, (ICollection)result);

			Assert.Contains((Int64)TestFlagEnum.FlagValue1, (ICollection<Int64>)result);
			Assert.Contains((Int64)TestFlagEnum.FlagValue4, (ICollection<Int64>)result);
		}

		[TestMethod]
		public void Test_GetClusterKey_ZeroValue()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn()
			{
				AspectGetter = (x) => 0,
			};

			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.IsEmpty((IEnumerable)result);
		}

		[TestMethod]
		public void Test_GetClusterKey_NonNumericValue()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn()
			{
				AspectGetter = (x) => "not number",
			};

			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.IsEmpty(((ICollection)result));
		}

		[TestMethod]
		public void Test_GetClusterKey_NonConvertibleValue()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			strategy.Column = new OLVColumn()
			{
				AspectGetter = (x) => new Object(),
			};

			Object result = strategy.GetClusterKey(null);
			Assert.IsInstanceOfType<IEnumerable>(result);
			Assert.IsEmpty((ICollection)result);
		}

		[TestMethod]
		public void Test_GetClusterDisplayLabel()
		{
			FlagClusteringStrategy strategy = new FlagClusteringStrategy(typeof(TestFlagEnum));
			ICluster cluster = new Cluster(TestFlagEnum.FlagValue2)
			{
				Count = 2,
			};

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