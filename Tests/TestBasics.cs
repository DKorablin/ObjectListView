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
	public class TestOlvBasics
	{
		protected ObjectListView _olv;
		protected MainForm _mainForm;

		[TestInitialize]
		public void SetupTest()
		{
			PersonDb.Reset();
			this._mainForm = new MainForm()
			{
				Size = new Size(),
			};

			this._mainForm.Show();
			this._olv = this.GetObjectListView();
		}

		protected virtual ObjectListView GetObjectListView()
			=> this._mainForm.objectListView1;

		[TestCleanup]
		public void TearDownTest()
			=> this._mainForm.Close();

		[TestMethod]
		public void Test_SetObjects_All()
		{
			this._olv.SetObjects(PersonDb.All);
			Assert.AreEqual(PersonDb.All.Count, this._olv.GetItemCount());
		}

		[TestMethod]
		public void Test_SetObjects_Null()
		{
			this._olv.SetObjects(null);
			Assert.AreEqual(0, this._olv.GetItemCount());
		}

		[TestMethod]
		public void Test_GetModelObject()
		{
			this._olv.SetObjects(PersonDb.All);
			for(Int32 i = 0; i < PersonDb.All.Count; i++)
				Assert.AreEqual(PersonDb.All[i], this._olv.GetModelObject(i));
		}

		[TestMethod]
		public void Test_AddObject()
		{
			this._olv.SetObjects(null);
			this._olv.AddObject(PersonDb.All[0]);
			Assert.AreNotEqual(-1, this._olv.IndexOf(PersonDb.All[0]));
			this._olv.AddObject(PersonDb.All[1]);
			Assert.AreNotEqual(-1, this._olv.IndexOf(PersonDb.All[1]));
			Assert.AreEqual(2, this._olv.GetItemCount());
		}

		[TestMethod]
		public void Test_AddObjects()
		{
			this._olv.SetObjects(null);
			this._olv.AddObjects(PersonDb.All);
			foreach(Object x in PersonDb.All)
				Assert.AreNotEqual(-1, this._olv.IndexOf(x));
			Assert.AreEqual(PersonDb.All.Count, this._olv.GetItemCount());
		}

		[TestMethod]
		public virtual void Test_AddObject_ModelFilter()
		{
			ArrayList somePeople = new ArrayList(PersonDb.All);
			Person first = PersonDb.All[0];
			somePeople.Remove(first);
			this._olv.SetObjects(somePeople);

			this._olv.UseFiltering = true;
			this._olv.ModelFilter = new ModelFilter(x => false);

			this._olv.AddObject(first);

			this._olv.UseFiltering = false;
			ArrayList contents = ObjectListView.EnumerableToArray(this._olv.Objects, false);
			Assert.HasCount(somePeople.Count + 1, contents);

			// The added Object should have been added at the end of all contents,
			// not at the end of the filtered contents
			Assert.AreEqual(somePeople.Count, this._olv.IndexOf(first));
		}

		[TestMethod]
		public void Test_RemoveObject()
		{
			this._olv.SetObjects(PersonDb.All);
			this._olv.RemoveObject(PersonDb.All[1]);
			Assert.AreEqual(-1, this._olv.IndexOf(PersonDb.All[1]));
			Assert.AreEqual(PersonDb.All.Count - 1, this._olv.GetItemCount());
		}

		[TestMethod]
		public void Test_RemoveObjects()
		{
			this._olv.SetObjects(PersonDb.All);
			List<Person> toRemove = new List<Person>();
			toRemove.Add(PersonDb.All[1]);
			toRemove.Add(PersonDb.All[2]);
			toRemove.Add(PersonDb.All[5]);
			this._olv.RemoveObjects(toRemove);
			foreach(Person x in toRemove)
				Assert.AreEqual(-1, this._olv.IndexOf(x));
			Assert.AreEqual(PersonDb.All.Count - toRemove.Count, this._olv.GetItemCount());
		}

		[TestMethod]
		public void Test_EffectiveRowHeight()
		{
			this._olv.RowHeight = 32;
			Assert.AreEqual(32, this._olv.RowHeightEffective);
			this._olv.RowHeight = -1;
		}

		[TestMethod]
		public void Test_RefreshObject()
		{
			this._olv.SetObjects(PersonDb.All);
			Person another = new Person(PersonDb.All[1].Name);
			another.Occupation = "a new occupation";

			OLVListItem item = this._olv.ModelToItem(another);
			Assert.IsNotNull(item);
			Assert.AreNotEqual(another.Occupation, item.SubItems[1].Text);

			this._olv.RefreshObject(another);

			OLVListItem item2 = this._olv.ModelToItem(another);
			Assert.IsNotNull(item2);
			Assert.AreEqual(another.Occupation, item2.SubItems[1].Text);
		}

		[TestMethod]
		public void Test_UpdateObject_AddsWhenNew()
		{
			List<Person> people = new List<Person>();
			people.Add(PersonDb.All[0]);
			people.Add(PersonDb.All[1]);
			people.Add(PersonDb.All[2]);
			this._olv.SetObjects(people);

			Person newGuy = PersonDb.All[3];
			OLVListItem item = this._olv.ModelToItem(newGuy);
			Assert.IsNull(item);

			this._olv.UpdateObject(newGuy);

			OLVListItem item2 = this._olv.ModelToItem(newGuy);
			Assert.IsNotNull(item2);
			Assert.AreEqual(newGuy.Occupation, item2.SubItems[1].Text);
		}

		[TestMethod]
		public void Test_UpdateObject_AddsWhenNew_WithFilterInstalled()
		{
			List<Person> people = new List<Person>
			{
				PersonDb.All[0],
				PersonDb.All[1],
				PersonDb.All[2]
			};
			this._olv.SetObjects(people);

			Person newGuy = PersonDb.All[3];
			this._olv.UseFiltering = true;
			this._olv.ModelFilter = new ModelFilter(x => ((Person)x).Name == newGuy.Name);
			Assert.AreEqual(0, this._olv.GetItemCount());

			this._olv.UpdateObject(newGuy);

			OLVListItem item2 = this._olv.ModelToItem(newGuy);
			Assert.IsNotNull(item2);
			Assert.AreEqual(newGuy.Occupation, item2.SubItems[1].Text);
		}
	}

	[TestClass]
	public class TestFastOlvBasics : TestOlvBasics
	{
		protected override ObjectListView GetObjectListView()
			=> this._mainForm.fastObjectListView1;
	}

	[TestClass]
	public class TestTreeListViewBasics : TestOlvBasics
	{
		protected override ObjectListView GetObjectListView()
			=> this._mainForm.treeListView1;
	}
}