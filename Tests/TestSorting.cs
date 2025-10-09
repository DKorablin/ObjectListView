/*
 * [File purpose]
 * Author: Phillip Piper
 * Date: 11/25/2008 11:06 PM
 * 
 * CHANGE LOG:
 * when who what
 * 11/25/2008 JPP  Initial Version
 */

using System;
using System.Collections;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	[TestClass]
	public class TestSorting
	{
		protected ObjectListView _olv;

		public TestSorting()
			=> this._olv = MyGlobals.mainForm.objectListView1;

		[TestInitialize]
		public void InitEachTest()
		{
			this._olv.LastSortColumn = null;
			this._olv.LastSortOrder = SortOrder.None;
			this._olv.SetObjects(PersonDb.All);
		}

		[TestCleanup]
		public void TearDownEachTest()
		{
		}

		[TestMethod]
		public void TestInitialConditions()
		{
		}

		[TestMethod]
		public void TestSecondarySorting()
		{
			this._olv.SecondarySortColumn = this._olv.GetColumn(0);
			this._olv.SecondarySortOrder = SortOrder.Descending;
			this._olv.Sort(this._olv.GetColumn(3), SortOrder.Ascending);
			Assert.AreEqual(PersonDb.LastAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);

			this._olv.SecondarySortColumn = this._olv.GetColumn(0);
			this._olv.SecondarySortOrder = SortOrder.Ascending;
			this._olv.Sort(this._olv.GetColumn(3), SortOrder.Ascending);
			Assert.AreEqual(PersonDb.FirstAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);
		}

		[TestMethod]
		public void TestSortingByStringColumn()
		{
			this._olv.Sort(this._olv.GetColumn(0), SortOrder.Ascending);
			Assert.AreEqual(PersonDb.FirstAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);
			this._olv.Sort(this._olv.GetColumn(0), SortOrder.Descending);
			Assert.AreEqual(PersonDb.LastAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);
		}

		[TestMethod]
		public void TestSortingByIntColumn()
		{
			OLVColumn columnToSort = this._olv.GetColumn(2);
			this._olv.Sort(columnToSort, SortOrder.Ascending);
			Assert.AreEqual(PersonDb.All[PersonDb.All.Count - 1].Name, ((Person)this._olv.GetModelObject(0)).Name);
			this._olv.Sort(columnToSort, SortOrder.Descending);
			Assert.AreEqual(PersonDb.All[0].Name, ((Person)this._olv.GetModelObject(0)).Name);
		}

		[TestMethod]
		public void TestNoSorting()
		{
			ArrayList beforeContents = this.GetContents();

			this._olv.Sort();

			Assert.HasCount(beforeContents.Count, this.GetContents());

			this._olv.LastSortColumn = this._olv.GetColumn(0);
			this._olv.LastSortOrder = SortOrder.Descending;
			this._olv.Sort();

			Assert.AreNotEqual(beforeContents, this.GetContents());
		}

		private ArrayList GetContents()
		{
			ArrayList contents = new ArrayList();
			for(Int32 i = 0; i < this._olv.GetItemCount(); i++)
				contents.Add(this._olv.GetModelObject(i));
			return contents;
		}

		[TestMethod]
		virtual public void TestCustomSorting()
		{
			this._olv.Sort(this._olv.GetColumn(0), SortOrder.Ascending);
			Assert.AreEqual(PersonDb.FirstAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);

			try
			{
				this._olv.CustomSorter = (column, order) => this._olv.ListViewItemSorter = new ColumnComparer(new OLVColumn("dummy", "BirthDate"), SortOrder.Descending);

				this._olv.Sort(this._olv.GetColumn(0), SortOrder.Ascending);
				Assert.AreNotEqual(PersonDb.FirstAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);
			} finally
			{
				this._olv.CustomSorter = null;
			}
		}

		[TestMethod]
		public void TestAfterSortingEvent()
		{
			Int32 afterSortingCount = 0;
			try
			{
				this._olv.AfterSorting += new EventHandler<AfterSortingEventArgs>(OnAfterSorting);
				this._olv.Sort(this._olv.GetColumn(0), SortOrder.Ascending);
				this._olv.Sort();
				this._olv.Sort(this._olv.GetColumn(0));
			} finally
			{
				this._olv.AfterSorting -= new EventHandler<AfterSortingEventArgs>(OnAfterSorting);
			}

			Assert.AreEqual(3, afterSortingCount);

			void OnAfterSorting(Object sender, AfterSortingEventArgs e)
			{
				afterSortingCount++;
			}
		}

		[TestMethod]
		public void TestBeforeSortingEvent()
		{
			try
			{
				this._olv.BeforeSorting += new EventHandler<BeforeSortingEventArgs>(OnBeforeSorting);
				this._olv.Sort(this._olv.GetColumn(2), SortOrder.Ascending);

				// The BeforeSorting event should have changed the sort to descending by name
				Assert.AreEqual(PersonDb.LastAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);
			} finally
			{
				this._olv.BeforeSorting -= new EventHandler<BeforeSortingEventArgs>(OnBeforeSorting);
			}

			void OnBeforeSorting(Object sender, BeforeSortingEventArgs e)
			{
				Assert.AreEqual(this._olv.GetColumn(2), e.ColumnToSort);
				Assert.AreEqual(SortOrder.Ascending, e.SortOrder);

				e.ColumnToSort = this._olv.GetColumn(0);
				e.SortOrder = SortOrder.Descending;
			}
		}

		[TestMethod]
		public void TestCancelSorting()
		{
			this._olv.Sort(this._olv.GetColumn(0), SortOrder.Descending);
			Assert.AreEqual(PersonDb.LastAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);

			try
			{
				this._olv.BeforeSorting += new EventHandler<BeforeSortingEventArgs>(OnBeforeSorting);
				this._olv.Sort(this._olv.GetColumn(2), SortOrder.Ascending);

				// The BeforeSorting event should have cancelled the sort so the second Sort() should not have had an effect
				Assert.AreEqual(PersonDb.LastAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);
			} finally
			{
				this._olv.BeforeSorting -= new EventHandler<BeforeSortingEventArgs>(OnBeforeSorting);
			}

			void OnBeforeSorting(Object sender, BeforeSortingEventArgs e)
			{
				Assert.AreEqual(this._olv.GetColumn(2), e.ColumnToSort);
				Assert.AreEqual(SortOrder.Ascending, e.SortOrder);

				e.Canceled = true;
			}
		}

		[TestMethod]
		public void TestPreserveSelection()
		{
			this._olv.SelectedObject = PersonDb.All[0];
			this._olv.Sort(this._olv.GetColumn(2), SortOrder.Ascending);
			Assert.AreEqual(PersonDb.All[0], this._olv.SelectedObject);
		}

		[TestMethod]
		public void TestPreserveSelectionMultiple()
		{
			this._olv.SelectedObjects = PersonDb.All;
			this._olv.Sort(this._olv.GetColumn(1), SortOrder.Ascending);
			Assert.HasCount(PersonDb.All.Count, this._olv.SelectedObjects);

			foreach(Object x in this._olv.SelectedObjects)
				Assert.Contains(x, PersonDb.All);
		}

		[TestMethod]
		virtual public void TestUnsort()
		{
			this._olv.Sort(this._olv.GetColumn(0), SortOrder.Ascending);
			Assert.AreEqual(PersonDb.FirstAlphabeticalName, ((Person)this._olv.GetModelObject(0)).Name);
			this._olv.Unsort();
			Assert.IsNull(this._olv.PrimarySortColumn);
			//Assert.AreEqual(SortOrder.None, this.olv.PrimarySortOrder);
			Assert.AreEqual(PersonDb.All[0].Name, ((Person)this._olv.GetModelObject(0)).Name);
		}
	}

	[TestClass]
	public class TestFastOlvSorting : TestSorting
	{
		public TestFastOlvSorting()
			=> this._olv = MyGlobals.mainForm.fastObjectListView1;

		[TestMethod]
		override public void TestCustomSorting()
		{
		}

		[TestMethod]
		override public void TestUnsort()
		{
			// FastObjectListViews don't really support Unsort()
		}
	}
}