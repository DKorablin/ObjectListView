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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	[TestClass]
	public class TestTreeView
	{
		[TestInitialize]
		public void InitEachTest()
		{
			PersonDb.Reset();

			mainForm = new MainForm();
			mainForm.Size = new Size();
			mainForm.Show();
			this.olv = mainForm.treeListView1;

			this.SetupDelegates();

			this.olv.UseFiltering = false;
			this.olv.ModelFilter = null;
			this.olv.HierarchicalCheckboxes = false;
			this.olv.Roots = PersonDb.All.GetRange(0, NumberOfRoots);
			this.olv.DiscardAllState();
		}

		protected virtual void SetupDelegates()
		{
			this.olv.CanExpandGetter = delegate (Object x) { return ((Person)x).Children.Count > 0; };
			this.olv.ChildrenGetter = delegate (Object x) { return ((Person)x).Children; };
			// this is only used when HierarchicalCheckboxes is true
			this.olv.ParentGetter = delegate (Object child) { return ((Person)child).Parent; };
		}

		private const Int32 NumberOfRoots = 2;
		protected TreeListView olv;
		private MainForm mainForm;

		[TestCleanup]
		public void TearDownEachTest()
			=> mainForm.Close();

		[TestMethod]
		public void TestInitialConditions()
		{
			Assert.AreEqual(NumberOfRoots, this.olv.GetItemCount());
			Int32 i = 0;
			foreach(Object x in this.olv.Roots)
			{
				Assert.AreEqual(PersonDb.All[i], x);
				Assert.IsFalse(this.olv.IsExpanded(x));
				i++;
			}
		}

		[TestMethod]
		public void TestCollapseAll()
		{
			this.olv.ExpandAll();
			this.olv.CollapseAll();
			Assert.AreEqual(NumberOfRoots, this.olv.GetItemCount());
		}

		[TestMethod]
		public void TestExpandAll()
		{
			this.olv.ExpandAll();
			Assert.AreEqual(PersonDb.All.Count, this.olv.GetItemCount());
		}

		[TestMethod]
		public void TestExpand()
		{
			this.olv.ExpandAll();
			this.olv.CollapseAll();

			Assert.AreEqual(NumberOfRoots, this.olv.GetItemCount());
			Int32 expectedCount = NumberOfRoots + PersonDb.All[0].Children.Count;
			this.olv.Expand(PersonDb.All[0]);
			Assert.AreEqual(expectedCount, this.olv.GetItemCount());

			Int32 expectedCount2 = NumberOfRoots + PersonDb.All[0].Children.Count + PersonDb.All[1].Children.Count;
			this.olv.Expand(PersonDb.All[1]);
			Assert.AreEqual(expectedCount2, this.olv.GetItemCount());
		}

		[TestMethod]
		public void TestClearAll()
		{
			this.olv.ClearObjects();
			Assert.AreEqual(0, this.olv.GetItemCount());
		}

		[TestMethod]
		public void TestCollapse()
		{
			Int32 originalCount = this.olv.GetItemCount();
			this.olv.Expand(PersonDb.All[0]);
			this.olv.Expand(PersonDb.All[1]);

			this.olv.Collapse(PersonDb.All[1]);
			this.olv.Collapse(PersonDb.All[0]);
			Assert.AreEqual(originalCount, this.olv.GetItemCount());
		}

		[TestMethod]
		public void TestRefreshOfHiddenItem()
		{
			this.olv.ExpandAll();
			this.olv.Collapse(PersonDb.All[1]);

			Int32 count = this.olv.GetItemCount();

			// This should do nothing since its parent is collapsed
			this.olv.RefreshObject(PersonDb.All[1].Children[0]);
			Assert.AreEqual(count, this.olv.GetItemCount());
		}

		[TestMethod]
		public void TestNullReferences()
		{
			this.olv.Expand(null);
			this.olv.Collapse(null);
			this.olv.RefreshObject(null);
		}

		[TestMethod]
		public void TestNonExistentObjects()
		{
			this.olv.Expand(new Person("name1"));
			this.olv.Collapse(new Person("name2"));
			this.olv.RefreshObject(1);
		}

		[TestMethod]
		public void TestGetParentRoot()
		{
			Assert.IsNull(this.olv.GetParent(PersonDb.All[0]));
		}

		[TestMethod]
		public void TestGetParentBeforeExpand()
		{
			Person p = PersonDb.All[0];
			Assert.IsNull(this.olv.GetParent(p.Children[0]));
		}

		[TestMethod]
		public void TestGetParent()
		{
			Person p = PersonDb.All[0];
			this.olv.Expand(p);
			Assert.AreEqual(p, this.olv.GetParent(p.Children[0]));
		}

		[TestMethod]
		public void TestGetChildrenLeaf()
		{
			Person p = PersonDb.All[0];
			Assert.IsEmpty((IList)this.olv.GetChildren(p.Children[0]));
		}

		[TestMethod]
		public void TestGetChildren()
		{
			Person p = PersonDb.All[0];
			IEnumerable kids = this.olv.GetChildren(p);
			Int32 i = 0;
			foreach(Person x in kids)
			{
				Assert.AreEqual(x, p.Children[i]);
				i++;
			}

			Assert.HasCount(i, p.Children);
		}

		[TestMethod]
		public void TestPreserveSelection()
		{
			this.olv.SelectedObject = PersonDb.All[1];
			this.olv.Expand(PersonDb.All[0]);
			Assert.AreEqual(PersonDb.All[1], this.olv.SelectedObject);
			this.olv.Collapse(PersonDb.All[0]);
			Assert.AreEqual(PersonDb.All[1], this.olv.SelectedObject);
		}

		[TestMethod]
		public void TestExpandedObjects()
		{
			this.olv.ExpandedObjects = new Person[] { PersonDb.All[1] };
			Assert.Contains(PersonDb.All[1], this.olv.ExpandedObjects as ICollection);
			this.olv.ExpandedObjects = null;
			Assert.IsEmpty(this.olv.ExpandedObjects as ICollection);
		}

		[TestMethod]
		public void TestPreserveExpansion()
		{
			this.olv.Expand(PersonDb.All[1]);
			Assert.Contains(PersonDb.All[1], this.olv.ExpandedObjects as ICollection);
			this.olv.Collapse(PersonDb.All[1]);
			Assert.IsEmpty(this.olv.ExpandedObjects as ICollection);
		}

		[TestMethod]
		public void TestRebuildAllWithPreserve()
		{
			this.olv.CheckBoxes = true;
			this.olv.SelectedObject = PersonDb.All[0];
			this.olv.Expand(PersonDb.All[1]);
			this.olv.CheckedObjects = new Person[] { PersonDb.All[0] };
			this.olv.RebuildAll(true);
			Assert.AreEqual(PersonDb.All[0], this.olv.SelectedObject);
			Assert.Contains(PersonDb.All[1], this.olv.ExpandedObjects as ICollection);
			Assert.Contains(PersonDb.All[0], this.olv.CheckedObjects as ICollection);
			this.olv.CheckBoxes = false;
		}

		[TestMethod]
		public void TestModelFilterNestedMatchParentsIncluded()
		{
			this.olv.ExpandAll();

			this.olv.UseFiltering = true;
			this.olv.ModelFilter = new TextMatchFilter(this.olv, PersonDb.LastAlphabeticalName.ToLowerInvariant());

			// After filtering the list should contain the one item that matched the filter and its ancestors
			Assert.AreEqual(4, this.olv.GetItemCount());
		}

		[TestMethod]
		public void Test_ModelFilter_HandlesColumnChanges()
		{
			this.olv.ExpandAll();

			OLVColumn commentColumn = new OLVColumn("Comment field", "Comments");
			this.olv.AllColumns.Add(commentColumn);
			this.olv.RebuildColumns();

			this.olv.UseFiltering = true;
			this.olv.ModelFilter = new TextMatchFilter(this.olv, PersonDb.LastComment);

			// After filtering the list should contain the one item that matched the filter and its ancestors
			Assert.AreEqual(4, this.olv.GetItemCount());

			commentColumn.IsVisible = false;
			this.olv.RebuildColumns();
			Assert.AreEqual(0, this.olv.VirtualListSize);

			commentColumn.IsVisible = true;
			this.olv.RebuildColumns();
			Assert.AreEqual(4, this.olv.GetItemCount());
			Assert.AreEqual(PersonDb.LastComment, ((Person)this.olv.GetItem(3).RowObject).Comments);
		}

		[TestMethod]
		public void TestRefreshWhenModelChanges()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();
			this.olv.Expand(firstRoot);
			Assert.HasCount(2, ObjectListView.EnumerableToArray(this.olv.GetChildren(firstRoot), false));

			IList<Person> originalChildren = firstRoot.Children;
			firstRoot.Children = new List<Person>();
			firstRoot.Children.Add(originalChildren[0]);
			Assert.HasCount(2, ObjectListView.EnumerableToArray(this.olv.GetChildren(firstRoot), false));
			this.olv.RefreshObject(firstRoot);
			Assert.HasCount(1, ObjectListView.EnumerableToArray(this.olv.GetChildren(firstRoot), false));

			firstRoot.Children = originalChildren;
		}

		[TestMethod]
		public void Test_RefreshObject_ExpansionUnchanged()
		{
			this.olv.CollapseAll();
			this.olv.ExpandAll();
			Int32 count = this.olv.GetItemCount();
			ArrayList expanded = ObjectListView.EnumerableToArray(this.olv.ExpandedObjects, false);
			Person lastExpanded = (Person)expanded[expanded.Count - 1];
			Object parentOfLastExpanded = this.olv.GetParent(lastExpanded);
			this.olv.RefreshObject(parentOfLastExpanded);
			Assert.IsTrue(this.olv.IsExpanded(lastExpanded));
			Assert.AreEqual(count, this.olv.GetItemCount());
		}

		[TestMethod]
		public void Test_CheckedObjects_CheckingVisibleObjects()
		{
			this.olv.CheckBoxes = true;
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();
			this.olv.Expand(firstRoot);
			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.CheckedObjects = ObjectListView.EnumerableToArray(firstRoot.Children, true);

			ArrayList checkedObjects = new ArrayList(this.olv.CheckedObjects);
			Assert.HasCount(firstRoot.Children.Count, checkedObjects);
		}

		[TestMethod]
		public void Test_CheckedObjects_CheckingHiddenObjects()
		{
			this.olv.CheckBoxes = true;
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.CheckedObjects = ObjectListView.EnumerableToArray(firstRoot.Children, true);

			ArrayList checkedObjects = new ArrayList(this.olv.CheckedObjects);
			Assert.HasCount(firstRoot.Children.Count, checkedObjects);
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_Unrolled_CheckingParent_ChecksChildren()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();
			this.olv.Expand(firstRoot);

			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.HierarchicalCheckboxes = true;
			this.olv.CheckObject(firstRoot);

			foreach(Person child in firstRoot.Children)
				Assert.IsTrue(this.olv.IsChecked(child));

			this.olv.UncheckObject(firstRoot);

			foreach(Person child in firstRoot.Children)
				Assert.IsFalse(this.olv.IsChecked(child));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_Rolled_CheckingParent_ChecksChildren()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			this.olv.HierarchicalCheckboxes = true;
			this.olv.CheckObject(firstRoot);
			this.olv.Expand(firstRoot);

			foreach(Person child in firstRoot.Children)
				Assert.IsTrue(this.olv.IsChecked(child));

			this.olv.UncheckObject(firstRoot);

			foreach(Person child in firstRoot.Children)
				Assert.IsFalse(this.olv.IsChecked(child));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckAllChildren_ParentIsChecked()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();
			this.olv.Expand(firstRoot);

			this.olv.HierarchicalCheckboxes = true;

			Assert.IsFalse(this.olv.IsChecked(firstRoot));

			foreach(Person child in firstRoot.Children)
				this.olv.CheckObject(child);

			Assert.IsTrue(this.olv.IsChecked(firstRoot));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckSomeChildren_ParentIsIndeterminate()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();
			this.olv.Expand(firstRoot);

			this.olv.HierarchicalCheckboxes = true;

			Assert.IsFalse(this.olv.IsChecked(firstRoot));

			foreach(Person child in firstRoot.Children)
				this.olv.CheckObject(child);
			this.olv.UncheckObject(firstRoot.Children[0]);

			Assert.IsTrue(this.olv.IsCheckedIndeterminate(firstRoot));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_UncheckAllChildren_ParentIsUnchecked()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();
			this.olv.Expand(firstRoot);

			this.olv.HierarchicalCheckboxes = true;

			this.olv.CheckObject(firstRoot);

			foreach(Person child in firstRoot.Children)
				this.olv.UncheckObject(child);

			Assert.IsFalse(this.olv.IsChecked(firstRoot));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_ChildrenInheritCheckedness()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			this.olv.HierarchicalCheckboxes = true;

			this.olv.CheckObject(firstRoot);
			Assert.IsTrue(this.olv.IsChecked(firstRoot));

			this.olv.Expand(firstRoot);
			Assert.IsTrue(this.olv.IsChecked(firstRoot));

			foreach(Person child in firstRoot.Children)
				Assert.IsTrue(this.olv.IsChecked(child));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.HierarchicalCheckboxes = true;
			this.olv.CheckObject(firstRoot);

			foreach(Person child in this.olv.GetChildren(firstRoot))
				Assert.IsTrue(this.olv.IsChecked(child));

			this.olv.UncheckObject(firstRoot);

			foreach(Person child in firstRoot.Children)
				Assert.IsFalse(this.olv.IsChecked(child));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckedObjects_Get()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.HierarchicalCheckboxes = true;
			this.olv.CheckObject(firstRoot);

			ArrayList checkedObjects = new ArrayList(this.olv.CheckedObjects);
			Assert.HasCount(1, checkedObjects);
			Assert.Contains(firstRoot, checkedObjects);
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckedObjects_Get_IncludesExpandedChildren()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.HierarchicalCheckboxes = true;
			this.olv.Expand(firstRoot);
			this.olv.CheckObject(firstRoot);

			ArrayList checkedObjects = new ArrayList(this.olv.CheckedObjects);
			Assert.HasCount(1 + firstRoot.Children.Count, checkedObjects);
			Assert.Contains(firstRoot, checkedObjects);
			foreach(Person child in firstRoot.Children)
				Assert.Contains(child, checkedObjects);
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckedObjects_Get_IncludesCheckedObjectsNotInControl()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.HierarchicalCheckboxes = true;
			this.olv.Expand(firstRoot);

			Person newGuy = new Person("someone new");
			this.olv.CheckObject(newGuy);

			ArrayList checkedObjects = new ArrayList(this.olv.CheckedObjects);
			Assert.HasCount(1, checkedObjects);
			Assert.AreEqual(((Person)checkedObjects[0]).Name, newGuy.Name);
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckedObjects_Set_RecalculatesParent()
		{
			Person firstRoot = PersonDb.All[0];
			this.olv.CollapseAll();

			Assert.IsEmpty(this.olv.CheckedObjects);

			this.olv.HierarchicalCheckboxes = true;

			this.olv.CheckedObjects = ObjectListView.EnumerableToArray(firstRoot.Children, true);

			ArrayList checkedObjects = new ArrayList(this.olv.CheckedObjects);
			Assert.HasCount(1 + firstRoot.Children.Count, checkedObjects);
			Assert.Contains(firstRoot, checkedObjects);
			foreach(Person child in firstRoot.Children)
				Assert.Contains(child, checkedObjects);
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckedObjects_Set_DeeplyNestedObject()
		{
			this.olv.HierarchicalCheckboxes = true;
			Assert.IsEmpty(this.olv.CheckedObjects);

			// The tree structure is:
			// GGP1
			// +-- GP1
			// +-- GP2 
			//      +-- P1
			//          +-- last
			// So checking "last" will also check P1 and GP2, and will make GGP1 indeterminate.

			Person last = PersonDb.All[PersonDb.All.Count - 1];
			Person P1 = last.Parent;
			Person GP2 = last.Parent.Parent;
			Person GGP1 = last.Parent.Parent.Parent;
			Person GP1 = GGP1.Children[0];

			ArrayList toBeChecked = new ArrayList();
			toBeChecked.Add(last);
			this.olv.CheckedObjects = toBeChecked;

			ArrayList checkedObjects = new ArrayList(this.olv.CheckedObjects);
			Assert.HasCount(3, checkedObjects);
			Assert.Contains(last, checkedObjects);
			Assert.Contains(P1, checkedObjects);
			Assert.Contains(GP2, checkedObjects);
			Assert.DoesNotContain(GGP1, checkedObjects);

			Assert.IsTrue(this.olv.IsChecked(last));
			Assert.IsTrue(this.olv.IsChecked(P1));
			Assert.IsTrue(this.olv.IsChecked(GP2));
			Assert.IsTrue(this.olv.IsCheckedIndeterminate(GGP1));

			// When GP1 is checked, GGP1 should also become checked.
			this.olv.CheckObject(GP1);
			Assert.IsTrue(this.olv.IsChecked(GP1));
			Assert.IsTrue(this.olv.IsChecked(GGP1));
		}

		[TestMethod]
		public void Test_HierarchicalCheckBoxes_CheckedObjects_Set_ClearsPreviousChecks()
		{
			Person firstRoot = PersonDb.All[0];
			Person secondRoot = PersonDb.All[1];

			this.olv.HierarchicalCheckboxes = true;
			this.olv.CheckObject(secondRoot);

			this.olv.CheckedObjects = ObjectListView.EnumerableToArray(firstRoot.Children, true);

			Assert.IsFalse(this.olv.IsChecked(secondRoot));
		}

		[TestMethod]
		public void Test_RefreshObject_NonRoot_AfterRemoveChild()
		{
			this.olv.ClearObjects();
			Assert.AreEqual(0, this.olv.GetItemCount());

			Person root = new Person("root");
			Person child = new Person("child");
			Person grandchild = new Person("grandchild");

			root.AddChild(child);
			child.AddChild(grandchild);
			grandchild.AddChild(new Person("ggrandchild1"));
			grandchild.AddChild(new Person("ggrandchild2"));
			grandchild.AddChild(new Person("ggrandchild3"));

			this.olv.Roots = new ArrayList(new Object[] { root });
			this.olv.ExpandAll();

			Assert.AreEqual(3, ObjectListView.EnumerableCount(this.olv.GetChildren(grandchild)));

			child.Children.RemoveAt(0);

			this.olv.RefreshObject(child);
			Assert.AreEqual(2, this.olv.GetItemCount());
		}
	}

	[TestClass]
	public class TestTreeViewViaInterface : TestTreeView
	{
		protected override void SetupDelegates()
		{
			// Don't setup delegates. This forces TreeListView to use the ITreeModel interface
		}
	}
}