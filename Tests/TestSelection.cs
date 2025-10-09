/*
 * [File purpose]
 * Author: Phillip Piper
 * Date: 10/25/2008 11:06 PM
 * 
 * CHANGE LOG:
 * when who what
 * 10/25/2008 JPP  Initial Version
 */

using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	[TestClass]
	public class TestOlvSelection
	{
		protected ObjectListView _olv;

		public TestOlvSelection()
			=> this._olv = MyGlobals.mainForm.objectListView1;

		[TestInitialize]
		public void InitEachTest()
		{
			this._olv.SetObjects(PersonDb.All);
			this._olv.SelectedObjects = null;
		}

		[TestMethod]
		public void TestSelectedObject()
		{
			Assert.IsNull(this._olv.SelectedObject);
			this._olv.SelectedObject = PersonDb.All[1];
			Assert.AreEqual(PersonDb.All[1], this._olv.SelectedObject);
		}

		[TestMethod]
		public void TestSelectedObjects()
		{
			Assert.IsEmpty(this._olv.SelectedObjects);
			this._olv.SelectedObjects = PersonDb.All;
			Assert.HasCount(PersonDb.All.Count, this._olv.SelectedObjects);
			this._olv.SelectedObjects = null;
			Assert.IsEmpty(this._olv.SelectedObjects);
		}

		[TestMethod]
		public void TestSelectAll()
		{
			this._olv.SelectAll();
			Assert.HasCount(PersonDb.All.Count, this._olv.SelectedObjects);
		}

		[TestMethod]
		public void TestDeselectAll()
		{
			this._olv.SelectedObject = PersonDb.All[1];
			Assert.IsNotEmpty(this._olv.SelectedObjects);
			this._olv.DeselectAll();
			Assert.IsEmpty(this._olv.SelectedObjects);

			this._olv.SelectAll();
			Assert.IsNotEmpty(this._olv.SelectedObjects);
			this._olv.DeselectAll();
			Assert.IsEmpty(this._olv.SelectedObjects);
		}

		[TestMethod]
		public void TestSortingShouldNotRaiseSelectionChangedEvents()
		{
			this._olv.SelectionChanged += new EventHandler(this.olv_SelectionChanged);
			this._olv.SelectedObjects = PersonDb.All;
			Application.RaiseIdle(new EventArgs());
			this._countSelectionChanged = 0;

			this._olv.Sort(this._olv.GetColumn(1));

			// Force an idle cycle so the selection changed event is processed
			Application.RaiseIdle(new EventArgs());

			Assert.AreEqual(0, this._countSelectionChanged);

			// Cleanup
			this._olv.SelectionChanged -= new EventHandler(this.olv_SelectionChanged);
		}

		[TestMethod]
		public void TestAddingAColumnShouldNotRaiseSelectionChangedEvents()
		{
			this._olv.SelectionChanged += new EventHandler(this.olv_SelectionChanged);
			this._olv.SelectedObjects = PersonDb.All;
			Application.RaiseIdle(new EventArgs());
			this._countSelectionChanged = 0;

			this._olv.RebuildColumns();

			// Force an idle cycle so the selection changed event is processed
			Application.RaiseIdle(new EventArgs());

			Assert.AreEqual(0, this._countSelectionChanged);

			// Cleanup
			this._olv.SelectionChanged -= new EventHandler(this.olv_SelectionChanged);
		}

		[TestMethod]
		public void TestSelectionEvents()
		{
			_countSelectedIndexChanged = 0;
			this._countSelectionChanged = 0;
			this._olv.SelectedIndexChanged += new EventHandler(this.olv_SelectedIndexChanged);
			this._olv.SelectionChanged += new EventHandler(this.olv_SelectionChanged);
			this._olv.SelectedObjects = PersonDb.All;
			this._olv.SelectedObjects = null;

			// Force an idle cycle so the selection changed event is processed
			Application.RaiseIdle(new EventArgs());

			// On a virtual list, deselecting everything only triggers one event, but on
			// normal lists, there should two selectedIndex events for each Object.
			// Regardless of anything, there should be only one selection changed event.
			if(this._olv.VirtualMode)
				Assert.AreEqual(PersonDb.All.Count + 1, this._countSelectedIndexChanged);
			else
				Assert.AreEqual(PersonDb.All.Count * 2, this._countSelectedIndexChanged);
			Assert.AreEqual(1, _countSelectionChanged);

			// Cleanup
			this._olv.SelectedIndexChanged -= new EventHandler(this.olv_SelectedIndexChanged);
			this._olv.SelectionChanged -= new EventHandler(this.olv_SelectionChanged);
		}

		private void olv_SelectedIndexChanged(Object sender, EventArgs e)
			=> this._countSelectedIndexChanged++;

		private Int32 _countSelectedIndexChanged;

		protected void olv_SelectionChanged(Object sender, EventArgs e)
			=> this._countSelectionChanged++;

		protected Int32 _countSelectionChanged;
	}

	[TestClass]
	public class TestFastOlvSelection : TestOlvSelection
	{
		public TestFastOlvSelection()
			=> this._olv = MyGlobals.mainForm.fastObjectListView1;
	}

	[TestClass]
	public class TestTreeListViewSelection : TestOlvSelection
	{
		public TestTreeListViewSelection()
		{
			this._olv = this._treeListView = MyGlobals.mainForm.treeListView1;
			this._treeListView.CanExpandGetter = (x) => ((Person)x).Children.Count > 0;
			this._treeListView.ChildrenGetter = (x) => ((Person)x).Children;
		}
		private readonly TreeListView _treeListView;

		[TestMethod]
		public void TestCollapseExpandShouldNotRaiseSelectionChangedEvents()
		{
			this._olv.SelectedObjects = PersonDb.All;
			Application.RaiseIdle(new EventArgs());
			this._olv.SelectionChanged += new EventHandler(this.olv_SelectionChanged);
			this._countSelectionChanged = 0;

			this._treeListView.Expand(PersonDb.All[0]);
			this._treeListView.ExpandAll();

			// Force an idle cycle so the selection changed event is processed
			Application.RaiseIdle(new EventArgs());

			Assert.AreEqual(0, this._countSelectionChanged);

			// Cleanup
			this._treeListView.CollapseAll();

			this._olv.SelectionChanged -= new EventHandler(this.olv_SelectionChanged);
			this._treeListView.CanExpandGetter = null;
			this._treeListView.ChildrenGetter = null;
		}
	}
}