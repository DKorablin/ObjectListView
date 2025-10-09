﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	class GeneratorTestModelEmpty
	{
	}

	class GeneratorTestModel1
	{
		[OLVColumn("Primary",
			AspectToStringFormat = "AspectToStringFormat",
			CheckBoxes = true,
			DisplayIndex = 0,
			FillsFreeSpace = true,
			FreeSpaceProportion = 98,
			GroupWithItemCountFormat = "GroupWithItemCountFormat",
			GroupWithItemCountSingularFormat = "GroupWithItemCountSingularFormat",
			Hyperlink = true,
			ImageAspectName = "ImageAspectName",
			IsEditable = true,
			IsTileViewColumn = true,
			IsVisible = true,
			MaximumWidth = 1000,
			MinimumWidth = 100,
			Name = "ColumnName",
			Tag = "Tag",
			TextAlign = HorizontalAlignment.Right,
			ToolTipText = "ToolTipText",
			TriStateCheckBoxes = true,
			UseInitialLetterForGroup = true,
			Width = 500)]
		public String OLVPrimaryColumn
		{
			get => String.Empty;
			set => _ = value;
		}

		public String NotUsedProperty
		{
			get => String.Empty;
			set => _ = value;
		}

		[OLVColumn("Secondary",
			AspectToStringFormat = "!AspectToStringFormat",
			CheckBoxes = false,
			DisplayIndex = 1,
			FillsFreeSpace = false,
			GroupWithItemCountFormat = "!GroupWithItemCountFormat",
			GroupWithItemCountSingularFormat = "!GroupWithItemCountSingularFormat",
			Hyperlink = false,
			ImageAspectName = "!ImageAspectName",
			IsEditable = false,
			IsTileViewColumn = false,
			IsVisible = false,
			MaximumWidth = -1,
			MinimumWidth = -1,
			Tag = "!Tag",
			TextAlign = HorizontalAlignment.Left,
			ToolTipText = "!ToolTipText",
			TriStateCheckBoxes = false,
			UseInitialLetterForGroup = false,
			Width = -1)]
		public String OLVSecondaryColumn
		{
			get => String.Empty;
			set => _ = value;
		}

		[DisplayName(nameof(DisplayNameColumn))]
		[OLVColumn]
		public String DisplayNameColumn
		{
			get => String.Empty;
			set => _ = value;
		}
	}

	class GeneratorTestModelSorting
	{
		[OLVColumn("Secondary", DisplayIndex = 1)]
		public String OLVSecondaryColumn
		{
			get => String.Empty;
			set => _ = value;
		}

		[OLVColumn("Primary", DisplayIndex = 0)]
		public String OLVShouldBeFirst
		{
			get => String.Empty;
			set => _ = value;
		}

		[OLVColumn("SecondLast", DisplayIndex = 3)]
		public String OLVSecondLast
		{
			get => String.Empty;
			set => _ = value;
		}

		[OLVColumn("Last")]
		public String OLVMustBeLast
		{
			get => String.Empty;
			set => _ = value;
		}

		[OLVColumn("Hidden", DisplayIndex = 2, IsVisible = false)]
		public String OLVMustNotBeVisible
		{
			get => String.Empty;
			set => _ = value;
		}
	}

	class GeneratorTestModelGroupies
	{
		[OLVColumn("Secondary",
			DisplayIndex = 1,
			GroupCutoffs = new Object[] { 10, 20, 30 },
			GroupDescriptions = new String[] { "Ten", "Twenty", "Thirty", "Above thirty" })
		]
		public Int32 OLVGroupy { get; set; }
	}

	class GeneratorTestPropertiesWithoutOlvColumnAttribute
	{
		public Int32 Property1 { get; set; }

		public String Property2
		{
			get => this._property2;
		}
#pragma warning disable 649
		private String _property2;
#pragma warning restore 649

		public bool CheckBoxProperty { get; set; }

		public Boolean? TriStateCheckBox { get; set; }
	}

	class ClassWithChildren
	{
		public Int32 Property1 { get; set; }

		public String Property2 { get; } = "value";

		[OLVChildren]
		public IList<ClassWithChildren> MyChildren { get; set; }
	}

	class ClassWithUntypedChildren
	{
		public Int32 Property1 { get; set; }

		public String Property2 { get; } = "property2";

		[OLVChildren]
		public ArrayList UntypedChildList { get; set; }

		[OLVChildren]
		public IList<ClassWithChildren> MyNotUsedChildren { get; set; }
	}

	class ClassWithIgnoredProperties
	{

		[OLVIgnore]
		public String PublicProperty { get; set; }
	}

	[TestClass]
	public class TestGenerator
	{
		public TestGenerator()
			=> this.olv = MyGlobals.mainForm.objectListView2;

		protected ObjectListView olv;

		[TestInitialize]
		public void InitEachTest()
			=> this.olv.Clear();

		[TestMethod]
		public void TestEmpty()
		{
			IList<OLVColumn> columns = Generator.GenerateColumns(typeof(GeneratorTestModelEmpty));
			Assert.IsEmpty(columns);
		}

		[TestMethod]
		public void TestBasics()
		{
			IList<OLVColumn> columns = Generator.GenerateColumns(typeof(GeneratorTestModel1));
			Assert.HasCount(3, columns);
			Assert.AreEqual("OLVPrimaryColumn", columns[0].AspectName);
			Assert.AreEqual("AspectToStringFormat", columns[0].AspectToStringFormat);
			Assert.IsTrue(columns[0].CheckBoxes);
			Assert.AreEqual(0, columns[0].DisplayIndex);
			Assert.IsTrue(columns[0].FillsFreeSpace);
			Assert.AreEqual(98, columns[0].FreeSpaceProportion);
			Assert.AreEqual("GroupWithItemCountFormat", columns[0].GroupWithItemCountFormat);
			Assert.AreEqual("GroupWithItemCountSingularFormat", columns[0].GroupWithItemCountSingularFormat);
			Assert.IsTrue(columns[0].Hyperlink);
			Assert.AreEqual("ImageAspectName", columns[0].ImageAspectName);
			Assert.IsTrue(columns[0].IsEditable);
			Assert.IsTrue(columns[0].IsTileViewColumn);
			Assert.IsTrue(columns[0].IsVisible);
			Assert.AreEqual(1000, columns[0].MaximumWidth);
			Assert.AreEqual(100, columns[0].MinimumWidth);
			Assert.AreEqual("ColumnName", columns[0].Name);
			Assert.AreEqual("Primary", columns[0].Text);
			Assert.AreEqual("Tag", columns[0].Tag);
			Assert.AreEqual(HorizontalAlignment.Right, columns[0].TextAlign);
			Assert.AreEqual("ToolTipText", columns[0].ToolTipText);
			Assert.IsTrue(columns[0].TriStateCheckBoxes);
			Assert.IsTrue(columns[0].UseInitialLetterForGroup);
			Assert.AreEqual(500, columns[0].Width);

			Assert.AreEqual("OLVSecondaryColumn", columns[1].AspectName);
			Assert.AreEqual("!AspectToStringFormat", columns[1].AspectToStringFormat);
			Assert.IsFalse(columns[1].CheckBoxes);
			Assert.AreEqual(1, columns[1].DisplayIndex);
			Assert.IsFalse(columns[1].FillsFreeSpace);
			Assert.AreEqual("!GroupWithItemCountFormat", columns[1].GroupWithItemCountFormat);
			Assert.AreEqual("!GroupWithItemCountSingularFormat", columns[1].GroupWithItemCountSingularFormat);
			Assert.IsFalse(columns[1].Hyperlink);
			Assert.AreEqual("!ImageAspectName", columns[1].ImageAspectName);
			Assert.IsFalse(columns[1].IsEditable);
			Assert.IsFalse(columns[1].IsTileViewColumn);
			Assert.IsFalse(columns[1].IsVisible);
			Assert.AreEqual(-1, columns[1].MaximumWidth);
			Assert.AreEqual(-1, columns[1].MinimumWidth);
			Assert.AreEqual("OLVSecondaryColumn", columns[1].Name);
			Assert.AreEqual("Secondary", columns[1].Text);
			Assert.AreEqual("!Tag", columns[1].Tag);
			Assert.AreEqual(HorizontalAlignment.Left, columns[1].TextAlign);
			Assert.AreEqual("!ToolTipText", columns[1].ToolTipText);
			Assert.IsFalse(columns[1].TriStateCheckBoxes);
			Assert.IsFalse(columns[1].UseInitialLetterForGroup);
			Assert.AreEqual(-1, columns[1].Width);

			Assert.AreEqual(nameof(GeneratorTestModel1.DisplayNameColumn), columns[2].Text);
		}

		[TestMethod]
		public void TestSorting()
		{
			IList<OLVColumn> columns = Generator.GenerateColumns(typeof(GeneratorTestModelSorting));
			Assert.HasCount(5, columns);
			Assert.AreEqual("OLVShouldBeFirst", columns[0].AspectName);
			Assert.AreEqual("OLVSecondaryColumn", columns[1].AspectName);
			Assert.AreEqual("OLVMustNotBeVisible", columns[2].AspectName);
			Assert.AreEqual("OLVSecondLast", columns[3].AspectName);
			Assert.AreEqual("OLVMustBeLast", columns[4].AspectName);
		}

		[TestMethod]
		public void TestBuilding()
		{
			Assert.IsEmpty(this.olv.Columns);
			Generator.GenerateColumns(this.olv, typeof(GeneratorTestModelSorting));
			Assert.HasCount(4, this.olv.Columns);
			Assert.HasCount(5, this.olv.AllColumns);
		}

		[TestMethod]
		public void TestGroupies()
		{
			IList<OLVColumn> columns = Generator.GenerateColumns(typeof(GeneratorTestModelGroupies));
			Assert.HasCount(1, columns);
			GeneratorTestModelGroupies model = new GeneratorTestModelGroupies();
			model.OLVGroupy = 5;
			Assert.AreEqual(0, columns[0].GetGroupKey(model));
			model.OLVGroupy = 35;
			Assert.AreEqual(3, columns[0].GetGroupKey(model));
			Assert.AreEqual("Above thirty", columns[0].ConvertGroupKeyToTitle(3));
		}

		[TestMethod]
		public void TestGenerateClearSorting()
		{
			Generator.GenerateColumns(this.olv, typeof(GeneratorTestModelSorting));
			this.olv.PrimarySortColumn = this.olv.GetColumn(0);

			Generator.GenerateColumns(this.olv, typeof(GeneratorTestModelGroupies));

			Assert.IsNull(this.olv.PrimarySortColumn);
		}

		[TestMethod]
		public void TestEmptyCollection()
		{
			this.olv.Columns.Add(new OLVColumn("not used", "NoAttribute"));
			Assert.AreNotEqual(0, this.olv.Columns.Count);
			ArrayList models = new ArrayList();
			Generator.GenerateColumns(this.olv, models);
			Assert.IsEmpty(this.olv.Columns);
		}

		[TestMethod]
		public void TestNonEmptyCollection()
		{
			this.olv.Columns.Add(new OLVColumn("not used", "NoAttribute"));
			Assert.HasCount(1, this.olv.Columns);
			ArrayList models = new ArrayList();
			models.Add(new GeneratorTestModelSorting());
			models.Add(new GeneratorTestModelSorting());
			Generator.GenerateColumns(this.olv, models);
			Assert.HasCount(5, this.olv.AllColumns);
			Assert.HasCount(4, this.olv.Columns);
		}

		[TestMethod]
		public void TestPropertiesWithoutAttributes_Ignored()
		{
			Generator.GenerateColumns(this.olv, typeof(GeneratorTestPropertiesWithoutOlvColumnAttribute));
			Assert.IsEmpty(this.olv.AllColumns);
		}

		[TestMethod]
		public void TestPropertiesWithoutAttributes_Basics()
		{
			Generator.GenerateColumns(this.olv, typeof(GeneratorTestPropertiesWithoutOlvColumnAttribute), true);
			Assert.HasCount(4, this.olv.AllColumns);
			Assert.HasCount(4, this.olv.Columns);
			Assert.AreEqual("Property1", this.olv.GetColumn(0).Text);
			Assert.AreEqual("Property1", this.olv.GetColumn(0).AspectName);
			Assert.AreEqual("Property2", this.olv.GetColumn(1).Text);
			Assert.AreEqual("Property2", this.olv.GetColumn(1).AspectName);
			Assert.AreEqual("Check Box Property", this.olv.GetColumn(2).Text);
			Assert.AreEqual("CheckBoxProperty", this.olv.GetColumn(2).AspectName);
		}

		[TestMethod]
		public void TestPropertiesWithoutAttributes_Editable()
		{
			Generator.GenerateColumns(this.olv, typeof(GeneratorTestPropertiesWithoutOlvColumnAttribute), true);
			Assert.IsTrue(this.olv.GetColumn("Property1").IsEditable);
			Assert.IsFalse(this.olv.GetColumn("Property2").IsEditable);
		}

		[TestMethod]
		public void TestPropertiesWithoutAttributes_CheckBox()
		{
			Generator.GenerateColumns(this.olv, typeof(GeneratorTestPropertiesWithoutOlvColumnAttribute), true);
			Assert.IsFalse(this.olv.GetColumn("Property1").CheckBoxes);
			Assert.IsTrue(this.olv.GetColumn("Check Box Property").CheckBoxes);
			Assert.IsTrue(this.olv.GetColumn("Tri State Check Box").TriStateCheckBoxes);
		}

		[TestMethod]
		public void TestPropertiesWithoutAttributes_DisplayIndexSetCorrectly()
		{
			Generator.GenerateColumns(this.olv, typeof(GeneratorTestPropertiesWithoutOlvColumnAttribute), true);
			for(Int32 i = 0; i < this.olv.AllColumns.Count; i++)
				Assert.AreEqual(i, this.olv.GetColumn(i).DisplayIndex);
		}

		[TestMethod]
		public void TestIgnoreAttribute_NoColumnCreated()
		{
			Generator.GenerateColumns(this.olv, typeof(ClassWithIgnoredProperties), true);
			Assert.IsEmpty(this.olv.AllColumns);
		}
	}


	[TestClass]
	public class TestColumnBuildingForTreeListView
	{
		protected TreeListView _tolv;

		public TestColumnBuildingForTreeListView()
			=> this._tolv = MyGlobals.mainForm.treeListView1;

		[TestInitialize]
		public void InitEachTest()
			=> this._tolv.Reset();

		[TestMethod]
		public void TestDelegatesCreated()
		{
			Generator.GenerateColumns(this._tolv, typeof(ClassWithChildren), true);
			Assert.IsNotNull(this._tolv.CanExpandGetter);
			Assert.IsNotNull(this._tolv.ChildrenGetter);
		}

		[TestMethod]
		public void TestCanExpandDelegateWorks()
		{
			Generator.GenerateColumns(this._tolv, typeof(ClassWithChildren), true);
			ClassWithChildren parent = new ClassWithChildren
			{
				MyChildren = new List<ClassWithChildren>()
				{
					new ClassWithChildren(),
				}
			};

			List<ClassWithChildren> roots = new List<ClassWithChildren>
			{
				parent
			};
			this._tolv.Objects = roots;

			Assert.IsTrue(this._tolv.CanExpand(parent));
		}

		[TestMethod]
		public void TestGetChildrenDelegateWorks()
		{
			Generator.GenerateColumns(this._tolv, typeof(ClassWithChildren), true);
			ClassWithChildren parent = new ClassWithChildren();
			parent.MyChildren = new List<ClassWithChildren>();
			parent.MyChildren.Add(new ClassWithChildren());

			List<ClassWithChildren> roots = new List<ClassWithChildren>();
			roots.Add(parent);
			this._tolv.Objects = roots;
			this._tolv.ExpandAll();

			Assert.AreEqual(2, this._tolv.GetItemCount());
		}

		[TestMethod]
		public void TestGetChildrenDelegateWorksWithUntypedChildren_NestedChildren()
		{
			Generator.GenerateColumns(this._tolv, typeof(ClassWithUntypedChildren), true);
			ClassWithUntypedChildren parent = new ClassWithUntypedChildren();
			ClassWithUntypedChildren child1 = new ClassWithUntypedChildren();
			ClassWithUntypedChildren child2 = new ClassWithUntypedChildren();
			parent.UntypedChildList = new ArrayList();
			parent.UntypedChildList.Add(child1);
			parent.UntypedChildList.Add(child2);
			child1.UntypedChildList = new ArrayList();
			child1.UntypedChildList.Add(new ClassWithUntypedChildren());
			child2.UntypedChildList = new ArrayList();
			child2.UntypedChildList.Add(new ClassWithUntypedChildren());

			ArrayList roots = new ArrayList();
			roots.Add(parent);
			this._tolv.Objects = roots;
			this._tolv.ExpandAll();

			Assert.AreEqual(5, this._tolv.GetItemCount());
		}

		[TestMethod]
		public void TestGetChildrenDelegateWorksWithUntypedChildren_WrongTypes()
		{
			Generator.GenerateColumns(this._tolv, typeof(ClassWithUntypedChildren), true);
			ClassWithUntypedChildren parent = new ClassWithUntypedChildren();
			parent.UntypedChildList = new ArrayList
			{
				"String",
				1
			};

			ArrayList roots = new ArrayList();
			roots.Add(parent);
			this._tolv.Objects = roots;
			this._tolv.ExpandAll();

			Assert.AreEqual(3, this._tolv.GetItemCount());
		}
	}
}