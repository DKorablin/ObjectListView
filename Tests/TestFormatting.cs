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
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightIdeasSoftware.Tests
{
	[TestClass]
	public class TestOlvFormatting
	{
		protected ObjectListView _olv;

		public TestOlvFormatting()
			=> this._olv = MyGlobals.mainForm.objectListView1;

		[TestInitialize]
		public void InitEachTest()
		{
			this._olv.UseAlternatingBackColors = false;
			this._olv.UseHyperlinks = false;
			this._olv.RowFormatter = null;
			this._olv.HyperlinkStyle = null;
			foreach(OLVColumn column in this._olv.Columns)
				column.Hyperlink = false;
		}

		[TestMethod]
		public void TestNoFormatting()
		{
			this._olv.HyperlinkStyle = null;
			this._olv.SetObjects(PersonDb.All);
			for(Int32 i = 0; i < this._olv.GetItemCount(); i++)
			{
				Assert.AreEqual(this._olv.ForeColor, this._olv.GetItem(i).ForeColor);
				Assert.AreEqual(this._olv.BackColor, this._olv.GetItem(i).BackColor);
			}
		}

		[TestMethod]
		public void TestAlternateBackColors()
		{
			this._olv.UseAlternatingBackColors = true;
			this._olv.AlternateRowBackColor = Color.Pink;

			this._olv.SetObjects(PersonDb.All);
			for(Int32 i = 0; i < this._olv.GetItemCount(); i++)
			{
				if((i % 2) == 0)
					Assert.AreEqual(this._olv.BackColor, this._olv.GetItem(i).BackColor);
				else
					Assert.AreEqual(this._olv.AlternateRowBackColor, this._olv.GetItem(i).BackColor);
			}
		}

		[TestMethod]
		public void TestRowFormatter()
		{
			Color testForeColor = Color.Yellow;
			Color testBackColor = Color.Violet;

			this._olv.RowFormatter = delegate (OLVListItem olvi)
			{
				olvi.ForeColor = testForeColor;
				olvi.BackColor = testBackColor;
			};
			this._olv.SetObjects(PersonDb.All);

			for(Int32 i = 0; i < this._olv.GetItemCount(); i++)
			{
				Assert.AreEqual(testForeColor, this._olv.GetItem(i).ForeColor);
				Assert.AreEqual(testBackColor, this._olv.GetItem(i).BackColor);
			}
		}

		[TestMethod]
		public void TestFormatRowEvent()
		{
			this._olv.FormatRow += new EventHandler<FormatRowEventArgs>(olv_FormatRow1);
			this._olv.SetObjects(PersonDb.All);

			for(Int32 i = 0; i < this._olv.GetItemCount(); i++)
			{
				Assert.AreEqual(this.testForeColor, this._olv.GetItem(i).ForeColor);
				Assert.AreEqual(this.testBackColor, this._olv.GetItem(i).BackColor);
			}
			this._olv.FormatRow -= new EventHandler<FormatRowEventArgs>(olv_FormatRow1);
		}
		Color testForeColor = Color.Yellow;
		Color testBackColor = Color.Violet;

		void olv_FormatRow1(Object sender, FormatRowEventArgs e)
		{
			e.Item.ForeColor = this.testForeColor;
			e.Item.BackColor = this.testBackColor;
		}

		[TestMethod]
		public void TestFormatCellEvent()
		{
			this._olv.FormatRow += new EventHandler<FormatRowEventArgs>(olv_FormatRow2);
			this._olv.FormatCell += new EventHandler<FormatCellEventArgs>(olv_FormatCell);
			this._olv.SetObjects(PersonDb.All);
			for(int i = 0; i < this._olv.GetItemCount(); i++)
			{
				if(i % 2 == 0)
				{
					Assert.IsFalse(this._olv.GetItem(i).UseItemStyleForSubItems);
					for(int j = 0; j < this._olv.Columns.Count; j++)
					{
						Assert.AreEqual(this.testCellForeColor, this._olv.GetItem(i).SubItems[j].ForeColor);
						Assert.AreEqual(this.testCellBackColor, this._olv.GetItem(i).SubItems[j].BackColor);
					}
				} else
				{
					Assert.IsTrue(this._olv.GetItem(i).UseItemStyleForSubItems);
					Assert.AreEqual(this._olv.ForeColor, this._olv.GetItem(i).ForeColor);
					Assert.AreEqual(this._olv.BackColor, this._olv.GetItem(i).BackColor);
				}
			}
			this._olv.FormatRow -= new EventHandler<FormatRowEventArgs>(olv_FormatRow2);
			this._olv.FormatCell -= new EventHandler<FormatCellEventArgs>(olv_FormatCell);
		}
		Color testCellForeColor = Color.Aquamarine;
		Color testCellBackColor = Color.BlanchedAlmond;

		void olv_FormatCell(Object sender, FormatCellEventArgs e)
		{
			e.SubItem.ForeColor = this.testCellForeColor;
			e.SubItem.BackColor = this.testCellBackColor;
		}

		private void olv_FormatRow2(Object sender, FormatRowEventArgs e)
			=> e.UseCellFormatEvents = (e.RowIndex % 2 == 0);

		[TestMethod]
		public void TestFormatRowAndCell_CellTakesPriority()
		{
			Color formatCellForeground = Color.PaleGoldenrod;
			Color formatRowForeground = Color.Pink;
			Color formatRowBackground = Color.Fuchsia;

			EventHandler<FormatRowEventArgs> olvOnFormatRow = delegate (Object sender, FormatRowEventArgs args)
			{
				args.Item.ForeColor = formatRowForeground;
				args.Item.BackColor = formatRowBackground;
				args.UseCellFormatEvents = true;
			};
			EventHandler<FormatCellEventArgs> olvOnFormatCell = delegate (Object sender, FormatCellEventArgs args)
			{
				args.SubItem.ForeColor = formatCellForeground;
			};

			this._olv.FormatRow += olvOnFormatRow;
			this._olv.FormatCell += olvOnFormatCell;

			this._olv.SetObjects(PersonDb.All);

			for(Int32 i = 0; i < this._olv.GetItemCount(); i++)
			{
				for(Int32 j = 0; j < this._olv.Columns.Count; j++)
				{
					Assert.AreEqual(formatCellForeground, this._olv.GetItem(i).SubItems[j].ForeColor);
					Assert.AreEqual(formatRowBackground, this._olv.GetItem(i).SubItems[j].BackColor);
				}
			}
			this._olv.FormatRow -= olvOnFormatRow;
			this._olv.FormatCell -= olvOnFormatCell;
		}


		[TestMethod]
		public void TestHyperlinks()
		{
			this._olv.UseHyperlinks = true;
			this._olv.HyperlinkStyle = new HyperlinkStyle();
			this._olv.HyperlinkStyle.Normal.ForeColor = Color.Thistle;
			this._olv.HyperlinkStyle.Normal.BackColor = Color.SpringGreen;
			this._olv.HyperlinkStyle.Normal.FontStyle = FontStyle.Bold;

			foreach(OLVColumn column in this._olv.Columns)
				column.Hyperlink = (column.Index < 2);

			this._olv.SetObjects(PersonDb.All);
			for(Int32 j = 0; j < this._olv.GetItemCount(); j++)
			{
				OLVListItem item = this._olv.GetItem(j);
				for(Int32 i = 0; i < this._olv.Columns.Count; i++)
				{
					OLVColumn column = this._olv.GetColumn(i);
					if(column.Hyperlink)
					{
						Assert.AreEqual(this._olv.HyperlinkStyle.Normal.ForeColor, item.SubItems[i].ForeColor);
						Assert.AreEqual(this._olv.HyperlinkStyle.Normal.BackColor, item.SubItems[i].BackColor);
						Assert.IsTrue(item.SubItems[i].Font.Bold);
					} else
					{
						Assert.AreEqual(this._olv.ForeColor, item.SubItems[i].ForeColor);
						Assert.AreEqual(this._olv.BackColor, item.SubItems[i].BackColor);
						Assert.IsFalse(item.SubItems[i].Font.Bold);
					}
				}
			}
		}

		[TestMethod]
		public void TestHyperlinksAndFormatCell_HyperlinkHasPriority()
		{
			Color hyperlinkForeground = Color.Thistle;
			Color formatCellForeground = Color.PaleGoldenrod;
			Color formatCellBackground = Color.DarkKhaki;

			this._olv.UseCellFormatEvents = true;
			EventHandler<FormatCellEventArgs> olvOnFormatCell = delegate (Object sender, FormatCellEventArgs args)
			{
				args.SubItem.ForeColor = formatCellForeground;
				args.SubItem.BackColor = formatCellBackground;
			};
			this._olv.FormatCell += olvOnFormatCell;

			this._olv.UseHyperlinks = true;
			this._olv.HyperlinkStyle = new HyperlinkStyle();
			this._olv.HyperlinkStyle.Normal.ForeColor = hyperlinkForeground;

			foreach(OLVColumn column in this._olv.Columns)
				column.Hyperlink = column.Index < 2;

			this._olv.SetObjects(PersonDb.All);

			try
			{
				for(Int32 j = 0; j < this._olv.GetItemCount(); j++)
				{
					OLVListItem item = this._olv.GetItem(j);
					for(Int32 i = 0; i < this._olv.Columns.Count; i++)
					{
						OLVColumn column = this._olv.GetColumn(i);
						if(column.Hyperlink)
							Assert.AreEqual(hyperlinkForeground, item.SubItems[i].ForeColor);
						else
							Assert.AreEqual(formatCellForeground, item.SubItems[i].ForeColor);

						Assert.AreEqual(formatCellBackground, item.SubItems[i].BackColor);
					}
				}
			} finally
			{
				this._olv.FormatCell -= olvOnFormatCell;
			}
		}
	}

	[TestClass]
	public class TestFastOlvFormatting : TestOlvFormatting
	{
		public TestFastOlvFormatting()
			=> this._olv = MyGlobals.mainForm.fastObjectListView1;
	}

	[TestClass]
	public class TestTreeListViewFormatting : TestOlvFormatting
	{
		public TestTreeListViewFormatting()
			=> this._olv = MyGlobals.mainForm.treeListView1;
	}
}