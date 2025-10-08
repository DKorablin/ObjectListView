using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	[TestClass]
	public class TestTypedListView
	{
		private TypedObjectListView<Person> tolv;

		[TestInitialize]
		public void Init()
		{
			// MyGlobals.mainForm should be initialized in a test assembly initializer
			this.tolv = new TypedObjectListView<Person>(MyGlobals.mainForm.objectListView1);
		}

		[TestCleanup]
		public void TestTearDown()
		{
			if(this.tolv?.ListView != null)
				for(Int32 i = 0; i < this.tolv.ListView.Columns.Count; i++)
					this.tolv.ListView.GetColumn(i).AspectGetter = null;
		}

		[TestMethod]
		public void Test_Objects_All()
		{
			this.tolv.Objects = PersonDb.All;
			Assert.HasCount(PersonDb.All.Count, this.tolv.Objects);
		}

		[TestMethod]
		public void Test_GenerateAspectGetters_ExtractsData()
		{
			this.tolv.GenerateAspectGetters();
			this.tolv.Objects = PersonDb.All;
			Person p = this.tolv.ListView.GetItem(0).RowObject as Person;
			Assert.AreEqual(p.Name, this.tolv.ListView.Items[0].SubItems[0].Text);
		}

		[TestMethod]
		public void Test_GenerateAspectGetters_NullDataObject()
		{
			this.tolv.GenerateAspectGetters();
			List<Person> list = new List<Person>
			{
				null
			};
			this.tolv.Objects = list;
			Assert.HasCount(list.Count, this.tolv.Objects);
		}

		[TestMethod]
		public void Test_GenerateAspectGetters_ClearObjects()
		{
			this.tolv.GenerateAspectGetters();
			this.tolv.Objects = PersonDb.All;
			this.tolv.ListView.ClearObjects();
			Assert.IsEmpty(this.tolv.Objects);

			this.tolv.Objects = PersonDb.All;
			Assert.HasCount(PersonDb.All.Count, this.tolv.Objects);
		}
	}
}