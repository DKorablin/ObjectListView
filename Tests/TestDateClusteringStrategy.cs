﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{

	[TestClass]
	public class TestDateClusteringStrategy
	{

		readonly DateTime DATE1 = new DateTime(1998, 11, 30, 22, 23, 24);
		readonly DateTime DATE2 = new DateTime(1999, 12, 31, 22, 23, 24);

		[TestMethod]
		public void Test_Construction_Empty()
		{
			DateTimeClusteringStrategy strategy = new DateTimeClusteringStrategy();
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return DATE2; };
			Object result = strategy.GetClusterKey(null);

			Assert.AreEqual(new DateTime(1999, 12, 1), result);
		}

		[TestMethod]
		public void Test_Construction_WithPortions()
		{
			DateTimeClusteringStrategy strategy = new DateTimeClusteringStrategy(DateTimePortion.Hour | DateTimePortion.Minute, "HH:mm");
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return DATE1; };
			Object result = strategy.GetClusterKey(null);

			Assert.AreEqual(new DateTime(1, 1, 1, 22, 23, 0), result);
		}

		[TestMethod]
		public void Test_Extracting_FromNull()
		{
			DateTimeClusteringStrategy strategy = new DateTimeClusteringStrategy(DateTimePortion.Hour | DateTimePortion.Minute, "HH:mm");
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return null; };
			Object result = strategy.GetClusterKey(null);
			Assert.IsNull(result);
		}

		[TestMethod]
		public void Test_GetClusterDisplayLabel_Plural()
		{
			DateTimeClusteringStrategy strategy = new DateTimeClusteringStrategy(DateTimePortion.Hour | DateTimePortion.Minute, "HH:mm");
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return DATE1; };
			ICluster cluster = new Cluster(strategy.GetClusterKey(null));
			cluster.Count = 2;
			String result = strategy.GetClusterDisplayLabel(cluster);
			Assert.AreEqual("22:23 (2 items)", result);
		}

		[TestMethod]
		public void Test_GetClusterDisplayLabel_Singular()
		{
			DateTimeClusteringStrategy strategy = new DateTimeClusteringStrategy(DateTimePortion.Year | DateTimePortion.Month, "MM-yy");
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return DATE1; };
			ICluster cluster = new Cluster(strategy.GetClusterKey(null));
			cluster.Count = 1;
			String result = strategy.GetClusterDisplayLabel(cluster);
			Assert.AreEqual("11-98 (1 item)", result);
		}

		[TestMethod]
		public void Test_GetClusterDisplayLabel_NullValue()
		{
			DateTimeClusteringStrategy strategy = new DateTimeClusteringStrategy(DateTimePortion.Year | DateTimePortion.Month, "HH:mm");
			strategy.Column = new OLVColumn();
			strategy.Column.AspectGetter = delegate (Object x) { return DATE1; };
			ICluster cluster = new Cluster(null);
			cluster.Count = 1;
			String result = strategy.GetClusterDisplayLabel(cluster);
			Assert.AreEqual(ClusteringStrategy.NULL_LABEL + " (1 item)", result);
		}
	}
}