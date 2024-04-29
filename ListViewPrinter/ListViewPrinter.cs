/*
 * ListViewPrinterBase - A helper class to easily print an ListView 
 *
 * User: Phillip Piper (phillip.piper@gmail.com)
 * Date: 2007-11-01 11:15 AM
 *
 * Change log:
 * 2018-10-06  JPP  - Fix rendering so that OLVColumn.WordWrap works when using the ListViewPrinter
 * 2009-02-24  JPP  - Correctly use new renderer scheme :)
 * 2009-01-26  JPP  - Use new renderer scheme
 *                  - Removed ugly hack about BarRenderer when printing.
 * 2009-01-19  JPP  - Use IsPrinting property on BaseRenderer
 * v2.0.1
 * 2008-12-16  JPP  - Hide all obsolete properties from the code generator
 * v2.0
 * The interaction with the IDE was completely rewritten in this version.
 * Old code should still work, but the IDE will not recognise the old configurations.
 * 
 * 2008-11-23  JPP  - Put back some obsolete methods to make transition easier.
 * 2008-11-15  JPP  - Use BrushData and PenData objects to ease IDE interactions.
 *                  - [BREAK] Removed obsolete methods.
 *                  - Changed license to GPL v3, to be consistent with ObjectListView.
 * v1.2
 * 2008-04-13  JPP  - Made the instance variables 'groupHeaderFormat' and 'listHeaderFormat'
 *                    private, like they always should have been. Use their corresponding
 *                    properties instead.
 * 2008-01-16  JPP  - Made all classes public so they can be accessed from a DLL
 *                  - Corrected initial value bugs
 * 2007-11-29  JPP  - Made list cells able to wrap, rather than always ellipsing.
 *                  - Handle ListViewItems having less sub items than there are columns.
 * 2007-11-21  JPP  - Cell images are no longer erased by a non-transparent cell backgrounds.
 * v1.1
 * 2007-11-10  JPP  - Made to work with virtual lists (if using ObjectListView)
 *                  - Make the list view header be able to show on each page
 * 2007-11-06  JPP  - Changed to use Pens internally in BlockFormat
 *                  - Fixed bug where group + following row would overprint footer
 * v1.0
 * 2007-11-05  JPP  - Vastly improved integration with IDE
 *                  - Added support for page ranges, and printing images
 * 2007-11-03  JPP  Added support for groups
 * 2007-10-31  JPP  Initial version
 * 
 * To Do:
 * 
 * Copyright (C) 2006-2018 Phillip Piper
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip.piper@gmail.com.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
	/// <summary>
	/// A ListViewPrinterBase prints or print previews an ListView.
	/// </summary>
	/// <remarks>
	/// <para>The format of the page header/footer, list header and list rows can all be customised.</para>
	/// <para>This class works best with ObjectListView class, but still works fine with normal ListViews.
	/// If you don't have ObjectListView class in your project, you must define WITHOUT_OBJECTLISTVIEW as one
	/// of the conditional compilation symbols on your projects properties.</para>
	/// <para>
	/// If you do use ObjectListView, and specifically the "Fast" flavours, printing groups will not work --
	/// the list will be printed as if groups were off. This is because the "Fast" flavours of ObjectListView 
	/// are virtual listviews, and MS doesn't allow virtual listviews to have groups. To make them work in
	/// OLV, I had to use undocumented features and avoid the standard Groups mechanism completely. 
	/// This printer was written to work against a standard ListView, so uses the standard Groups mechanism,
	/// which for virtual lists is always empty. So, no printing of groups for FastObjectListViews.
	/// </para>
	/// </remarks>
	public class ListViewPrinterBase : PrintDocument
	{
		#region Constructors

		/// <summary>Make a new ListViewPrinterBase</summary>
		public ListViewPrinterBase()
		{
			// Give the report a reasonable set of default values
			this.HeaderFormat = BlockFormat.Header();
			this.ListHeaderFormat = BlockFormat.ListHeader();
			this.CellFormat = BlockFormat.DefaultCell();
			this.GroupHeaderFormat = BlockFormat.GroupHeader();
			this.FooterFormat = BlockFormat.Footer();
		}

		/// <summary>Make a new ListViewPrinterBase that will print the given ListView</summary>
		public ListViewPrinterBase(ListView lv)
			: this()
			=> this.ListView = lv;

		#endregion

		#region Control Properties

		/// <summary>This is the ListView that will be printed</summary>
		[Category("Behaviour")]
		[Description("Which listview will be printed by this printer?")]
		[DefaultValue(null)]
		public ListView ListView { get; set; }

		/// <summary>Should this report use text only?</summary>
		[Category("Behaviour")]
		[Description("Should this report use text only? If this is false, images on the primary column will be included.")]
		[DefaultValue(false)]
		public Boolean IsTextOnly { get; set; } = false;

		/// <summary>Should this report be shrunk to fit into the width of a page?</summary>
		[Category("Behaviour")]
		[Description("Should this report be shrunk to fit into the width of a page?")]
		[DefaultValue(true)]
		public Boolean IsShrinkToFit { get; set; } = true;

		/// <summary>Should this report only include the selected rows in the listview?</summary>
		[Category("Behaviour")]
		[Description("Should this report only include the selected rows in the listview?")]
		[DefaultValue(false)]
		public Boolean IsPrintSelectionOnly { get; set; } = false;

		/// <summary>
		/// Should this report use the column order as the user sees them? With this enabled,
		/// the report will match the order of column as the user has arranged them.
		/// </summary>
		[Category("Behaviour")]
		[Description("Should this report use the column order as the user sees them? With this enabled, the report will match the order of column as the user has arranged them.")]
		[DefaultValue(true)]
		public Boolean UseColumnDisplayOrder { get; set; } = true;

		/// <summary>Should column headings always be centered, even if on the control itself, they are aligned to the left or right?</summary>
		[Category("Behaviour")]
		[Description("Should column headings always be centered or should they follow the alignment on the control itself?")]
		[DefaultValue(true)]
		public Boolean AlwaysCenterListHeader { get; set; } = true;

		/// <summary>Should listview headings be printed at the top of each page, or just at the top of the list?</summary>
		[Category("Behaviour")]
		[Description("Should listview headings be printed at the top of each page, or just at the top of the list?")]
		[DefaultValue(true)]
		public Boolean IsListHeaderOnEachPage { get; set; } = true;

		/// <summary>Return the index of the first page of the report that should be printed</summary>
		[Category("Behaviour")]
		[Description("Return the first page of the report that should be printed")]
		[DefaultValue(0)]
		public Int32 FirstPage { get; set; } = 0;

		/// <summary>Return the index of the last page of the report that should be printed</summary>
		[Category("Behaviour")]
		[Description("Return the last page of the report that should be printed")]
		[DefaultValue(9999)]
		public Int32 LastPage { get; set; } = 9999;

		/// <summary>Return the number of the page that is currently being printed.</summary>
		[Browsable(false)]
		public Int32 PageNumber
		{
			get => this.pageNumber;
		}

		/// <summary>Is this report showing groups?</summary>
		/// <remarks>Groups can't be shown when we are printing selected rows only.</remarks>
		[Browsable(false)]
		public Boolean IsShowingGroups
		{
			get => (this.ListView != null && this.ListView.ShowGroups && !this.IsPrintSelectionOnly && this.ListView.Groups.Count > 0);
		}

		#endregion

		#region Formatting Properties

		/// <summary>How should the page header be formatted? null means no page header will be printed</summary>
		[Category("Appearance - Formatting")]
		[Description("How will the page header be formatted? ")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public BlockFormat HeaderFormat { get; set; }

		/// <summary>How should the list header be formatted? null means no list header will be printed</summary>
		[Category("Appearance - Formatting")]
		[Description("How will the header of the list be formatted? ")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public BlockFormat ListHeaderFormat { get; set; }

		/// <summary>How should the grouping header be formatted? null means revert to reasonable default</summary>
		[Category("Appearance - Formatting")]
		[Description("How will the group headers be formatted?")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public BlockFormat GroupHeaderFormat
		{
			get => this._groupHeaderFormat ?? (this._groupHeaderFormat = BlockFormat.GroupHeader());// The group header format cannot be null
			set => this._groupHeaderFormat = value;
		}
		private BlockFormat _groupHeaderFormat;

		/// <summary>How should the list cells be formatted? null means revert to default</summary>
		[Category("Appearance - Formatting")]
		[Description("How will the list cells be formatted? ")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public BlockFormat CellFormat
		{
			get => this._cellFormat ?? (this._cellFormat = BlockFormat.DefaultCell());// The cell format cannot be null
			set => this._cellFormat = value;
		}
		private BlockFormat _cellFormat;

		/// <summary>How should the page footer be formatted? null means no footer will be printed</summary>
		[Category("Appearance - Formatting")]
		[Description("How will the page footer be formatted?")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public BlockFormat FooterFormat { get; set; }

		/// <summary>What font will be used to draw the text of the list?</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font ListFont
		{
			get => this.CellFormat.Font;
			set => this.CellFormat.Font = value;
		}

		/// <summary>
		/// What pen will be used to draw the cells within the list?
		/// If this is null, no grid will be drawn
		/// </summary>
		/// <remarks>This is just a convenience wrapper around CellFormat.SetBorderPen</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Pen ListGridPen
		{
			get => this.CellFormat.GetBorderPen(Sides.Top);
			set => this.CellFormat.SetBorderPen(Sides.All, value);
		}

		/// <summary>What String will be written at the top of each page of the report?</summary>
		/// <remarks><para>The header can be divided into three parts: left aligned, 
		/// centered, and right aligned. If the given String contains Tab characters,
		/// everything before the first tab will be left aligned, everything between
		/// the first and second tabs will be centered and everything after the second
		/// tab will be right aligned.</para>
		/// <para>Within each part, the following substitutions are possible:</para>
		/// <list>
		/// <item>{0} - The page number</item>
		/// <item>{1} - The current date/time</item>
		/// </list>
		/// </remarks>
		[Category("Appearance")]
		[Description("The String that will be written at the top of each page. Use '\\t' characters to separate left, centre, and right parts of the header.")]
		[DefaultValue(null)]
		public String Header
		{
			get => this._header;
			set => this._header = value?.Replace("\\t", "\t");
		}
		private String _header;

		/// <summary>What String will be written at the bottom of each page of the report?</summary>
		/// <remarks>The footer, like the header, can have three parts, and behaves
		/// in the same way as described as Header.</remarks>
		[Category("Appearance")]
		[Description("The String that will be written at the bottom of each page. Use '\\t' characters to separate left, centre, and right parts of the footer.")]
		[DefaultValue(null)]
		public String Footer
		{
			get => this._footer;
			set => this._footer = value?.Replace("\\t", "\t");
		}
		private String _footer;

		//-----------------------------------------------------------------------
		// Watermark

		/// <summary>The watermark will be printed translucently over the report itself</summary>
		[Category("Appearance - Watermark")]
		[Description("The watermark will be printed translucently over the report itself?")]
		[DefaultValue(null)]
		public String Watermark { get; set; }

		/// <summary>What font should be used to print the watermark</summary>
		[Category("Appearance - Watermark")]
		[Description("What font should be used to print the watermark?")]
		[DefaultValue(null)]
		public Font WatermarkFont { get; set; }

		/// <summary>Return the watermark font or a reasonable default</summary>
		[Browsable(false)]
		public Font WatermarkFontOrDefault
		{
			get => this.WatermarkFont ?? new Font("Tahoma", 72);
		}

		/// <summary>What color should be used for the watermark?</summary>
		[Category("Appearance - Watermark")]
		[Description("What foregroundColor should be used for the watermark?")]
		[DefaultValue(typeof(Color), "")]
		public Color WatermarkColor { get; set; } = Color.Empty;

		/// <summary>Return the color of the watermark or a reasonable default</summary>
		[Browsable(false)]
		public Color WatermarkColorOrDefault
		{
			get => this.WatermarkColor == Color.Empty ? Color.Gray : this.WatermarkColor;
		}

		/// <summary>How transparent should the watermark be? &lt;= 0 is transparent, &gt;= 100 is completely opaque.</summary>
		[Category("Appearance - Watermark")]
		[Description("How transparent should the watermark be? 0 is transparent, 100 is completely opaque.")]
		[DefaultValue(50)]
		public Int32 WatermarkTransparency
		{
			get => this._watermarkTransparency;
			set => this._watermarkTransparency = Math.Max(0, Math.Min(value, 100));
		}
		private Int32 _watermarkTransparency = 50;

		#endregion

		#region Accessing

		/// <summary>Return the number of rows that this printer is going to print</summary>
		/// <param name="lv">The listview that is being printed</param>
		/// <returns>The number of rows that will be displayed</returns>
		protected Int32 GetRowCount(ListView lv)
		{
			if(this.IsPrintSelectionOnly)
				return lv.SelectedIndices.Count;
			else
				return lv.VirtualMode
					? lv.VirtualListSize
					: lv.Items.Count;
		}

		/// <summary>Return the n'th row that will be printed</summary>
		/// <param name="lv">The listview that is being printed</param>
		/// <param name="n">The index of the row to be printed</param>
		/// <returns>A ListViewItem</returns>
		protected ListViewItem GetRow(ListView lv, Int32 n)
		{
			if(this.IsPrintSelectionOnly)
				return lv.VirtualMode
					? this.GetVirtualItem(lv, lv.SelectedIndices[n])
					: lv.SelectedItems[n];

			if(!this.IsShowingGroups)
				return lv.VirtualMode
					? this.GetVirtualItem(lv, n)
					: lv.Items[n];

			// If we are showing groups, things are more complicated. The n'th
			// row of the report doesn't directly correspond to existing list.
			// The best we can do is figure out which group the n'th item belongs to
			// and then figure out which item it is within the groups items.
			Int32 i;
			for(i = this.groupStartPositions.Count - 1; i >= 0; i--)
				if(n >= this.groupStartPositions[i])
					break;
			Int32 indexInList = n - this.groupStartPositions[i];
			return lv.Groups[i].Items[indexInList];
		}

		/// <summary>Get the nth item from the given listview, which is in virtual mode.</summary>
		/// <param name="lv">The ListView in virtual mode</param>
		/// <param name="n">index of item to get</param>
		/// <returns>the item</returns>
		protected virtual ListViewItem GetVirtualItem(ListView lv, Int32 n)
			=> throw new ApplicationException("Virtual list items cannot be retrieved. Use an ObjectListView instead.");

		/// <summary>Return the i'th subitem of the given row, in the order that columns are presented in the report</summary>
		/// <param name="lvi">The row from which a subitem is to be fetched</param>
		/// <param name="i">The index of the subitem in display order</param>
		/// <returns>A SubItem</returns>
		protected ListViewItem.ListViewSubItem GetSubItem(ListViewItem lvi, Int32 i)
			=> i < lvi.SubItems.Count
				? lvi.SubItems[this.GetColumn(i).Index]
				: new ListViewItem.ListViewSubItem();

		/// <summary>Return the number of columns to be printed in the report</summary>
		/// <returns>An int</returns>
		protected Int32 GetColumnCount()
			=> this.sortedColumns.Count;

		/// <summary>Return the n'th ColumnHeader (ordered as they should be displayed in the report)</summary>
		/// <param name="i">Which column</param>
		/// <returns>A ColumnHeader</returns>
		protected ColumnHeader GetColumn(Int32 i)
			=> this.sortedColumns[i];

		/// <summary>
		/// Return the index of group that starts at the given position.
		/// Return -1 if no group starts at that position
		/// </summary>
		/// <param name="n">The row position in the list</param>
		/// <returns>The group index</returns>
		protected Int32 GetGroupAtPosition(Int32 n)
			=> this.groupStartPositions.IndexOf(n);

		#endregion

		#region Commands

		/// <summary>Show a Page Setup dialog to customize the printing of this document</summary>
		public void PageSetup()
		{
			PageSetupDialog dlg = new PageSetupDialog
			{
				Document = this,
				EnableMetric = true
			};
			dlg.ShowDialog();
		}

		/// <summary>Show a Print Preview of this document</summary>
		public void PrintPreview()
		{
			PrintPreviewDialog dlg = new PrintPreviewDialog
			{
				UseAntiAlias = true,
				Document = this
			};
			dlg.ShowDialog();
		}

		/// <summary>Print this document after showing a confirmation dialog</summary>
		public void PrintWithDialog()
		{
			PrintDialog dlg = new PrintDialog
			{
				Document = this,
				AllowSelection = this.ListView.SelectedIndices.Count > 0,
				AllowSomePages = true
			};

			// Show the standard print dialog box, that lets the user select a printer
			// and change the settings for that printer.
			if(dlg.ShowDialog() == DialogResult.OK)
			{
				this.IsPrintSelectionOnly = (dlg.PrinterSettings.PrintRange == PrintRange.Selection);
				if(dlg.PrinterSettings.PrintRange == PrintRange.SomePages)
				{
					this.FirstPage = dlg.PrinterSettings.FromPage;
					this.LastPage = dlg.PrinterSettings.ToPage;
				} else
				{
					this.FirstPage = 1;
					this.LastPage = 999999;
				}
				this.Print();
			}
		}

		#endregion

		#region Event handlers

		/// <summary>A print job is about to be printed</summary>
		/// <param name="e"></param>
		override protected void OnBeginPrint(PrintEventArgs e)
		{
			base.OnBeginPrint(e);

			// Initialize our state information
			this.rowIndex = -1;
			this.indexLeftColumn = -1;
			this.indexRightColumn = -1;
			this.pageNumber = 0;

			// Initialize our caches
			this.sortedColumns = new SortedList<Int32, ColumnHeader>();
			this.groupStartPositions = new List<Int32>();

			this.PreparePrint();
		}

		/// <summary>Print a given page</summary>
		/// <param name="e"></param>
		override protected void OnPrintPage(PrintPageEventArgs e)
		{
			if(this.ListView == null || this.ListView.View != View.Details)
				return;

			base.OnPrintPage(e);

			this.pageNumber++;

			// Ignore all pages before the first requested page
			// Have to allow for weird cases where the last page is before the first page
			// and where we run out of things to print before reaching the first requested page.
			Int32 pageToStop = Math.Min(this.FirstPage, this.LastPage + 1);
			if(this.pageNumber < pageToStop)
			{
				e.HasMorePages = true;
				while(this.pageNumber < pageToStop && e.HasMorePages)
				{
					e.HasMorePages = this.PrintOnePage(e);
					this.pageNumber++;
				}

				// Remove anything drawn
				e.Graphics.Clear(Color.White);

				// If we ran out of pages before reaching the first page, simply return
				if(!e.HasMorePages)
					return;
			}

			// If we haven't reached the end of the requested pages, print one.
			if(this.pageNumber <= this.LastPage)
			{
				e.HasMorePages = this.PrintOnePage(e);
				e.HasMorePages = e.HasMorePages && (this.pageNumber < this.LastPage);
			} else
				e.HasMorePages = false;
		}

		#endregion

		#region List printing

		/// <summary>Prepare some precalculated fields used when printing</summary>
		protected void PreparePrint()
		{
			if(this.ListView == null)
				return;

			// Build sortedColumn so it holds the column in the order they should be printed
			foreach(ColumnHeader column in this.ListView.Columns)
			{
				if(this.UseColumnDisplayOrder)
					this.sortedColumns.Add(column.DisplayIndex, column);
				else
					this.sortedColumns.Add(column.Index, column);
			}

			// If the listview is grouped, build an array to holds the start
			// position of each group. The way to understand this array is that
			// the index of the first member of group n is found at groupStartPositions[n].
			Int32 itemCount = 0;
			foreach(ListViewGroup lvg in this.ListView.Groups)
			{
				this.groupStartPositions.Add(itemCount);
				itemCount += lvg.Items.Count;
			}
		}

		/// <summary>Do the actual work of printing on page</summary>
		/// <param name="e"></param>
		protected Boolean PrintOnePage(PrintPageEventArgs e)
		{
			this.CalculateBounds(e);
			this.CalculatePrintParameters(this.ListView);
			this.PrintHeaderFooter(e.Graphics);
			this.ApplyScaling(e.Graphics);
			Boolean continuePrinting = this.PrintList(e.Graphics, this.ListView);
			this.PrintWatermark(e.Graphics);
			return continuePrinting;
		}

		/// <summary>Figure out the page bounds and the boundaries for the list</summary>
		/// <param name="e"></param>
		protected void CalculateBounds(PrintPageEventArgs e)
		{
			// Printing to a real printer doesn't take the printers hard margins into account
			if(this.PrintController.IsPreview)
				this.pageBounds = (RectangleF)e.MarginBounds;
			else
				this.pageBounds = new RectangleF(e.MarginBounds.X - e.PageSettings.HardMarginX,
					e.MarginBounds.Y - e.PageSettings.HardMarginY, e.MarginBounds.Width, e.MarginBounds.Height);

			this.listBounds = this.pageBounds;
		}

		/// <summary>Figure out the boundaries for various aspects of the report</summary>
		/// <param name="lv">The listview to be printed</param>
		protected void CalculatePrintParameters(ListView lv)
		{
			// If we are in the middle of printing a listview, don't change the parameters
			if(this.rowIndex >= 0 && this.rowIndex < this.GetRowCount(lv))
				return;

			this.rowIndex = 0;

			// If we are shrinking the report to fit on the page...
			if(this.IsShrinkToFit)
			{

				// ...we print all the columns, but we need to figure how much to shrink
				// them so that they will fit onto the page
				this.indexLeftColumn = 0;
				this.indexRightColumn = this.GetColumnCount() - 1;

				Int32 totalWidth = 0;
				for(Int32 i = 0; i < this.GetColumnCount(); i++)
					totalWidth += this.GetColumn(i).Width;
				this.scaleFactor = Math.Min(this.listBounds.Width / totalWidth, 1.0f);
			} else
			{
				// ...otherwise, we print unscaled but have to figure out which columns
				// will fit on the current page
				this.scaleFactor = 1.0f;
				this.indexLeftColumn = ++this.indexRightColumn;

				// Iterate the columns until we find a column that won't fit on the page
				Int32 width = 0;
				for(Int32 i = this.indexLeftColumn; i < this.GetColumnCount() && (width += this.GetColumn(i).Width) < this.listBounds.Width; i++)
					this.indexRightColumn = i;
			}
		}

		/// <summary>Apply any scaling that is required to the report</summary>
		/// <param name="g"></param>
		protected void ApplyScaling(Graphics g)
		{
			if(this.scaleFactor >= 1.0f)
				return;

			g.ScaleTransform(this.scaleFactor, this.scaleFactor);

			Single inverse = 1.0f / this.scaleFactor;
			this.listBounds = new RectangleF(this.listBounds.X * inverse, this.listBounds.Y * inverse, this.listBounds.Width * inverse, this.listBounds.Height * inverse);
		}

		/// <summary>Print our watermark on the given Graphic</summary>
		/// <param name="g"></param>
		protected void PrintWatermark(Graphics g)
		{
			if(String.IsNullOrEmpty(this.Watermark))
				return;

			StringFormat strFormat = new StringFormat
			{
				LineAlignment = StringAlignment.Center,
				Alignment = StringAlignment.Center
			};

			// THINK: Do we want this to be a property?
			Int32 watermarkRotation = -30;

			// Setup a rotation transform on the Graphic so we can write the watermark at an angle
			g.ResetTransform();
			Matrix m = new Matrix();
			m.RotateAt(watermarkRotation, new PointF(this.pageBounds.X + this.pageBounds.Width / 2, this.pageBounds.Y + this.pageBounds.Height / 2));
			g.Transform = m;

			// Calculate the semi-transparent pen required to print the watermark
			Int32 alpha = (Int32)(255.0f * (Single)this.WatermarkTransparency / 100.0f);
			Brush brush = new SolidBrush(Color.FromArgb(alpha, this.WatermarkColorOrDefault));

			// Finally draw the watermark
			g.DrawString(this.Watermark, this.WatermarkFontOrDefault, brush, this.pageBounds, strFormat);
			g.ResetTransform();
		}

		/// <summary>Do the work of printing the list into 'listBounds'</summary>
		/// <param name="g">The graphic used for drawing</param>
		/// <param name="lv">The listview to be printed</param>
		/// <returns>Return true if there are still more pages in the report</returns>
		protected Boolean PrintList(Graphics g, ListView lv)
		{
			this.currentOrigin = this.listBounds.Location;

			if(this.rowIndex == 0 || this.IsListHeaderOnEachPage)
				this.PrintListHeader(g, lv);

			this.PrintRows(g, lv);

			// We continue to print pages when we have more rows or more columns remaining
			return (this.rowIndex < this.GetRowCount(lv) || this.indexRightColumn + 1 < this.GetColumnCount());
		}

		/// <summary>Print the header of the listview</summary>
		/// <param name="g">The graphic used for drawing</param>
		/// <param name="lv">The listview to be printed</param>
		protected void PrintListHeader(Graphics g, ListView lv)
		{
			// If there is no format for the header, we don't draw it
			BlockFormat fmt = this.ListHeaderFormat;
			if(fmt == null)
				return;

			// Calculate the height of the list header
			Single height = 0;
			for(Int32 i = 0; i < this.GetColumnCount(); i++)
			{
				ColumnHeader col = this.GetColumn(i);
				height = Math.Max(height, fmt.CalculateHeight(g, col.Text, col.Width));
			}

			// Draw the header one cell at a time
			RectangleF cell = new RectangleF(this.currentOrigin.X, this.currentOrigin.Y, 0, height);
			for(Int32 i = this.indexLeftColumn; i <= this.indexRightColumn; i++)
			{
				ColumnHeader col = this.GetColumn(i);
				cell.Width = col.Width;
				fmt.Draw(g, cell, col.Text, (this.AlwaysCenterListHeader ? HorizontalAlignment.Center : col.TextAlign));
				cell.Offset(cell.Width, 0);
			}

			this.currentOrigin.Y += cell.Height;
		}

		/// <summary>Print the rows of the listview</summary>
		/// <param name="g">The graphic used for drawing</param>
		/// <param name="lv">The listview to be printed</param>
		protected void PrintRows(Graphics g, ListView lv)
		{
			while(this.rowIndex < this.GetRowCount(lv))
			{

				// Will this row fit before the end of page?
				Single rowHeight = this.CalculateRowHeight(g, lv, this.rowIndex);
				if(this.currentOrigin.Y + rowHeight > this.listBounds.Bottom)
					break;

				// If we are printing group and there is a group beginning at the current position,
				// print it so long as the group header and at least one following row will fit on the page
				if(this.IsShowingGroups)
				{
					Int32 groupIndex = this.GetGroupAtPosition(this.rowIndex);
					if(groupIndex != -1)
					{
						Single groupHeaderHeight = this.GroupHeaderFormat.CalculateHeight(g);
						if(this.currentOrigin.Y + groupHeaderHeight + rowHeight < this.listBounds.Bottom)
							this.PrintGroupHeader(g, lv, groupIndex);
						else
						{
							this.currentOrigin.Y = this.listBounds.Bottom;
							break;
						}
					}
				}
				this.PrintRow(g, lv, this.rowIndex, rowHeight);
				this.rowIndex++;
			}
		}

		/// <summary>Calculate how high the given row of the report should be.</summary>
		/// <param name="g">The graphic used for drawing</param>
		/// <param name="lv">The listview to be printed</param>
		/// <param name="n">The index of the row whose height is to be calculated</param>
		/// <returns>The height of one row in pixels</returns>
		virtual protected Single CalculateRowHeight(Graphics g, ListView lv, Int32 n)
		{
			// If we're including graphics in the report, we need to allow for the height of a small image
			if(!this.IsTextOnly && lv.SmallImageList != null)
				this.CellFormat.MinimumTextHeight = lv.SmallImageList.ImageSize.Height;

			// If the cell lines can't wrap, calculate the generic height of the row
			if(!this.CellFormat.CanWrap)
				return this.CellFormat.CalculateHeight(g);

			// If the cell lines can wrap, calculate the height of the tallest cell
			Single height = 0f;
			ListViewItem lvi = this.GetRow(lv, n);
			for(Int32 i = 0; i < this.GetColumnCount(); i++)
			{
				ColumnHeader column = this.GetColumn(i);
				Int32 colWidth = column.Width;
				if(!this.IsTextOnly && column.Index == 0 && lv.SmallImageList != null && lvi.ImageIndex != -1)
					colWidth -= lv.SmallImageList.ImageSize.Width;

				// If we are using an specialized renderer in an ObjectListView, it could do anything
				// with the Text value (e.g. it could be a BLOB that is presented as an Image).
				// So we ignore it, and hope that the height of the row can be calculated from
				// the other cells in the row.
				OLVColumn olvc = column as OLVColumn;
				if(olvc == null || !(olvc.Renderer is BaseRenderer))
					height = Math.Max(height, this.CellFormat.CalculateHeight(g, this.GetSubItem(lvi, i).Text, colWidth));
			}
			return height;
		}

		/// <summary>Print a group header</summary>
		/// <param name="g">The graphic used for drawing</param>
		/// <param name="lv">The listview to be printed</param>
		/// <param name="groupIndex">The index of the group header to be printed</param>
		protected void PrintGroupHeader(Graphics g, ListView lv, Int32 groupIndex)
		{
			ListViewGroup lvg = lv.Groups[groupIndex];
			BlockFormat fmt = this.GroupHeaderFormat;
			Single height = fmt.CalculateHeight(g);
			RectangleF r = new RectangleF(this.currentOrigin.X, this.currentOrigin.Y, this.listBounds.Width, height);
			fmt.Draw(g, r, lvg.Header, lvg.HeaderAlignment);
			this.currentOrigin.Y += height;
		}

		/// <summary>Print one row of the listview</summary>
		/// <param name="g"></param>
		/// <param name="lv"></param>
		/// <param name="row"></param>
		/// <param name="rowHeight"></param>
		protected void PrintRow(Graphics g, ListView lv, Int32 row, Single rowHeight)
		{
			ListViewItem lvi = this.GetRow(lv, row);

			// Print the row cell by cell. We only print the cells that are in the range
			// of columns that are chosen for this page
			RectangleF cell = new RectangleF(this.currentOrigin, new SizeF(0, rowHeight));
			for(Int32 i = this.indexLeftColumn; i <= this.indexRightColumn; i++)
			{
				ColumnHeader col = this.GetColumn(i);
				cell.Width = col.Width;
				this.PrintCell(g, lv, lvi, row, i, cell);
				cell.Offset(cell.Width, 0);
			}
			this.currentOrigin.Y += rowHeight;
		}

		/// <summary>Print one cell of the listview</summary>
		/// <param name="g"></param>
		/// <param name="lv"></param>
		/// <param name="lvi"></param>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="cell"></param>
		virtual protected void PrintCell(Graphics g, ListView lv, ListViewItem lvi, Int32 row, Int32 column, RectangleF cell)
		{
			BlockFormat fmt = this.CellFormat;
			ColumnHeader ch = this.GetColumn(column);

			// Are we going to print an icon in this cell? We print an image if it
			// isn't a text only report AND it is a primary column AND the cell has an image and a image list.
			if(!this.IsTextOnly && ch.Index == 0 && lvi.ImageIndex != -1 && lv.SmallImageList != null)
			{
				// Trick the block format into indenting the text so it doesn't write the text into where the image is going to be drawn
				const Int32 gapBetweenImageAndText = 3;
				Single textInsetCorrection = lv.SmallImageList.ImageSize.Width + gapBetweenImageAndText;
				fmt.SetTextInset(Sides.Left, fmt.GetTextInset(Sides.Left) + textInsetCorrection);
				fmt.Draw(g, cell, this.GetSubItem(lvi, column).Text, ch.TextAlign);
				fmt.SetTextInset(Sides.Left, fmt.GetTextInset(Sides.Left) - textInsetCorrection);

				// Now draw the image into the area reserved for it
				RectangleF r = fmt.CalculatePaddedTextBox(cell);
				if(lv.SmallImageList.ImageSize.Height < r.Height)
					r.Y += (r.Height - lv.SmallImageList.ImageSize.Height) / 2;
				g.DrawImage(lv.SmallImageList.Images[lvi.ImageIndex], r.Location);
			} else// No image to draw. SImply draw the text
				fmt.Draw(g, cell, this.GetSubItem(lvi, column).Text, ch.TextAlign);
		}

		/// <summary>Print the page header and page footer</summary>
		/// <param name="g"></param>
		protected void PrintHeaderFooter(Graphics g)
		{
			if(!String.IsNullOrEmpty(this.Header))
				PrintPageHeader(g);

			if(!String.IsNullOrEmpty(this.Footer))
				PrintPageFooter(g);
		}

		/// <summary>Print the page header</summary>
		/// <param name="g"></param>
		protected void PrintPageHeader(Graphics g)
		{
			BlockFormat fmt = this.HeaderFormat;
			if(fmt == null)
				return;

			Single height = fmt.CalculateHeight(g);
			RectangleF headerRect = new RectangleF(this.listBounds.X, this.listBounds.Y, this.listBounds.Width, height);
			fmt.Draw(g, headerRect, this.SplitAndFormat(this.Header));

			// Move down the top of the area available for the list
			this.listBounds.Y += height;
			this.listBounds.Height -= height;
		}

		/// <summary>Print the page footer</summary>
		/// <param name="g"></param>
		protected void PrintPageFooter(Graphics g)
		{
			BlockFormat fmt = this.FooterFormat;
			if(fmt == null)
				return;

			Single height = fmt.CalculateHeight(g);
			RectangleF r = new RectangleF(this.listBounds.X, this.listBounds.Bottom - height, this.listBounds.Width, height);
			fmt.Draw(g, r, this.SplitAndFormat(this.Footer));

			// Decrease the area available for the list
			this.listBounds.Height -= height;
		}

		/// <summary>
		/// Split the given String into at most three parts, using Tab as the divider. 
		/// Perform any substitutions required
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private String[] SplitAndFormat(String text)
		{
			String s = String.Format(text, this.pageNumber, DateTime.Now);
			return s.Split(new Char[] { '\x09' }, 3);
		}

		#endregion

		#region Compatibility

		/// <summary>What color will all the borders be drawn in?</summary>
		/// <remarks>This is just a convenience wrapper around ListGridPen</remarks>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Obsolete("Use ListGridPen instead")]
		public Color ListGridColor
		{
			get => this.ListGridPen?.Color ?? Color.Empty;
			set => this.ListGridPen = new Pen(new SolidBrush(value), 0.5f);
		}

		#endregion

		#region Private variables

		// These are our state variables.
		private Int32 rowIndex;
		private Int32 indexLeftColumn;
		private Int32 indexRightColumn;
		private Int32 pageNumber;

		// Cached values
		private SortedList<Int32, ColumnHeader> sortedColumns;
		private List<Int32> groupStartPositions;

		// Per-page variables
		private RectangleF pageBounds;
		private RectangleF listBounds;
		private PointF currentOrigin;
		private Single scaleFactor;

		#endregion
	}

	/// <summary>This ListViewPrinterBase handles only normal ListViews, while this class knows about the specifics of ObjectListViews</summary>
	public class ListViewPrinter : ListViewPrinterBase
	{
		/// <summary>Make a ListViewPrinter</summary>
		public ListViewPrinter()
		{
		}

		/// <summary>Get the nth item from the given listview, which is in virtual mode.</summary>
		/// <param name="lv">The ListView in virtual mode</param>
		/// <param name="n">index of item to get</param>
		/// <returns>the item</returns>
		override protected ListViewItem GetVirtualItem(ListView lv, Int32 n)
			=> ((VirtualObjectListView)lv).MakeListViewItem(n);

		/// <summary>Calculate how high each row of the report should be.</summary>
		/// <param name="g">The graphic used for drawing</param>
		/// <param name="lv">The listview to be printed</param>
		/// <param name="n"></param>
		/// <returns>The height of one row in pixels</returns>
		override protected Single CalculateRowHeight(Graphics g, ListView lv, Int32 n)
		{
			Single height = base.CalculateRowHeight(g, lv, n);
			if(lv is ObjectListView olv)
				height = Math.Max(height, olv.RowHeightEffective);
			return height;
		}

		/// <summary>If the given BlockFormat doesn't specify a background, take it from the SubItem or the ListItem.</summary>
		protected Boolean ApplyCellSpecificBackground(BlockFormat fmt, ListViewItem lvi, ListViewItem.ListViewSubItem lvsi)
		{
			if(fmt.BackgroundBrush != null)
				return false;

			fmt.BackgroundBrush = lvi.UseItemStyleForSubItems
				? new SolidBrush(lvi.BackColor)
				: (Brush)new SolidBrush(lvsi.BackColor);

			return true;
		}

		/// <summary>Print one cell of the ListView</summary>
		/// <param name="g"></param>
		/// <param name="lv"></param>
		/// <param name="lvi"></param>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="cell"></param>
		protected override void PrintCell(Graphics g, ListView lv, ListViewItem lvi, Int32 row, Int32 column, RectangleF cell)
		{
			if(this.IsTextOnly || !(lv is ObjectListView))
			{
				base.PrintCell(g, lv, lvi, row, column, cell);
				return;
			}

			// Decide which renderer should be used for drawing the cell
			ObjectListView listView = (ObjectListView)lv;
			OLVColumn olvc = (OLVColumn)this.GetColumn(column);
			OLVListItem olvItem = (OLVListItem)lvi;
			BaseRenderer renderer = listView.GetCellRenderer(olvItem.RowObject, olvc) as BaseRenderer;

			// We couldn't find a renderer we could use. Just use the default rendering
			if(renderer == null)
			{
				base.PrintCell(g, lv, lvi, row, column, cell);
				return;
			}

			// Store renderer state
			Font originalFont = renderer.Font;
			Boolean? originalCanWrap = renderer.CanWrap;

			try
			{
				// Configure the renderer
				renderer.IsPrinting = true;
				renderer.Font = this.CellFormat.Font;
				renderer.TextBrush = this.CellFormat.TextBrush;
				renderer.CanWrap = this.CellFormat.CanWrap;
				renderer.Aspect = null;
				renderer.Column = olvc;
				renderer.IsItemSelected = false;
				renderer.ListItem = olvItem;
				renderer.ListView = listView;
				renderer.RowObject = olvItem.RowObject;
				renderer.SubItem = (OLVListSubItem)this.GetSubItem(lvi, column);

				// Use the cell block format to draw the background and border of the cell
				Boolean bkChanged = this.ApplyCellSpecificBackground(this.CellFormat, renderer.ListItem, renderer.SubItem);
				this.CellFormat.Draw(g, cell, "", "", "");
				if(bkChanged)
					this.CellFormat.BackgroundBrush = null;

				// The renderer draws into the text area of the block. Unfortunately, the renderer uses Rectangle's 
				// rather than RectangleF's, so we have to convert, trying to prevent rounding errors
				RectangleF r = this.CellFormat.CalculatePaddedTextBox(cell);
				Rectangle r2 = new Rectangle((Int32)r.X + 1, (Int32)r.Y + 1, (Int32)r.Width - 1, (Int32)r.Height - 1);
				renderer.Render(g, r2);
			} finally
			{
				// Restore the original state
				renderer.IsPrinting = false;
				renderer.Font = originalFont;
				renderer.CanWrap = originalCanWrap;
			}
		}
	}

	/// <summary>Specify which sides of a block will be operated on</summary>
	public enum Sides
	{
		/// <summary>Left</summary>
		Left = 0,

		/// <summary>Top</summary>
		Top = 1,

		/// <summary>Right</summary>
		Right = 2,

		/// <summary>Bottom</summary>
		Bottom = 3,

		/// <summary>All</summary>
		All = 4
	}

	/// <summary>A BlockFormat controls the formatting and style of a single part (block) of a ListViewPrinter output.
	/// </summary>
	public class BlockFormat : Component
	{
		#region Public properties

		/// <summary>In what font should the text of the block be drawn? If this is null, the font from the listview will be used</summary>
		[Category("Appearance")]
		[Description("What font should this block be drawn in?")]
		[DefaultValue(null)]
		public Font Font { get; set; }

		/// <summary>Return the font that should be used for the text of this block or a reasonable default</summary>
		[Browsable(false)]
		public Font FontOrDefault
		{
			get => this.Font ?? new Font("Ms Sans Serif", 12);
		}

		/// <summary>What brush will be used to draw the text?</summary>
		/// <remarks>
		/// <para>If this format is used for cells and this is null AND an ObjectListView is being printed, 
		/// then the text color from the listview will be used.
		/// This is useful when you have setup specific colors on a RowFormatter delegate, for example.
		/// </para>
		/// </remarks>
		public Brush TextBrush;

		/// <summary>This Object is used by the IDE to set the text brush.</summary>
		[Category("Appearance")]
		[DisplayName("TextBrush")]
		[DefaultValue(null)]
		public IBrushData TextBrushData
		{
			get => this._textBrushData;
			set
			{
				this._textBrushData = value;
				if(value != null)
					this.TextBrush = value.GetBrush();
			}
		}
		private IBrushData _textBrushData;

		/// <summary>Return the brush that will be used to draw the text or a reasonable default</summary>
		[Browsable(false)]
		public Brush TextBrushOrDefault
		{
			get => this.TextBrush ?? Brushes.Black;
		}

		/// <summary>What brush will be used to paint the background?</summary>
		[Browsable(false)]
		public Brush BackgroundBrush;

		/// <summary>This Object is used by the IDE to set the background brush.</summary>
		[Category("Appearance")]
		[DisplayName("BackgroundBrush")]
		[DefaultValue(null)]
		public IBrushData BackgroundBrushData
		{
			get => this._backgroundBrushData;
			set
			{
				this._backgroundBrushData = value;
				if(value != null)
					this.BackgroundBrush = value.GetBrush();
			}
		}
		private IBrushData _backgroundBrushData;

		/// <summary>When laying out our header can the text be wrapped?</summary>
		[Category("Appearance")]
		[Description("When laying out our header can the text be wrapped?")]
		[DefaultValue(false)]
		public Boolean CanWrap { get; set; } = false;

		/// <summary>
		/// If this is set, at least this much vertical space will be reserved for the text,
		/// even if the text is smaller.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Single MinimumTextHeight { get; set; } = 0;

		//----------------------------------------------------------------------------------
		// All of these attributes are solely to make them appear in the IDE
		// When programming by hand, use Get/SetBorderPen() 
		// rather than these methods.

		/// <summary>
		/// Set the TopBorder
		/// </summary>
		[Category("Appearance")]
		[DisplayName("Border - Top")]
		[DefaultValue(null)]
		public PenData TopBorderPenData
		{
			get => this._topBorderPenData;
			set
			{
				this._topBorderPenData = value;
				if(value != null)
					this.SetBorderPen(Sides.Top, value.GetPen());
			}
		}
		private PenData _topBorderPenData;

		/// <summary>Set the LeftBorder</summary>
		[Category("Appearance")]
		[DisplayName("Border - Left")]
		[DefaultValue(null)]
		public PenData LeftBorderPenData
		{
			get => this._leftBorderPenData;
			set
			{
				this._leftBorderPenData = value;
				if(value != null)
					this.SetBorderPen(Sides.Left, value.GetPen());
			}
		}
		private PenData _leftBorderPenData;

		/// <summary>Set the BottomBorder</summary>
		[Category("Appearance")]
		[DisplayName("Border - Bottom")]
		[DefaultValue(null)]
		public PenData BottomBorderPenData
		{
			get => this._bottomBorderPenData;
			set
			{
				this._bottomBorderPenData = value;
				if(value != null)
					this.SetBorderPen(Sides.Bottom, value.GetPen());
			}
		}
		private PenData _bottomBorderPenData;

		/// <summary>Set the RightBorder</summary>
		[Category("Appearance")]
		[DisplayName("Border - Right")]
		[DefaultValue(null)]
		public PenData RightBorderPenData
		{
			get => this._rightBorderPenData;
			set
			{
				this._rightBorderPenData = value;
				if(value != null)
					this.SetBorderPen(Sides.Right, value.GetPen());
			}
		}
		private PenData _rightBorderPenData;

		/// <summary>Set the RightBorder</summary>
		[Category("Appearance")]
		[DisplayName("Border - All")]
		[DefaultValue(null)]
		public PenData AllBorderPenData
		{
			get
			{
				if(this._leftBorderPenData == this._topBorderPenData &&
					this._leftBorderPenData == this._rightBorderPenData &&
					this._leftBorderPenData == this._bottomBorderPenData)
					return this._leftBorderPenData;
				else
					return null;
			}
			set
			{
				this.LeftBorderPenData = value;
				this.TopBorderPenData = value;
				this.RightBorderPenData = value;
				this.BottomBorderPenData = value;
			}
		}

		#endregion

		#region Compatibility
		/// <summary>
		/// What color will be used to draw the text?
		/// This is a convenience method. Programmers should call TextBrush directly.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color TextColor
		{
			get => this.TextBrush is SolidBrush s
					? s.Color
					: Color.Empty;
			set => this.TextBrush = value.IsEmpty
					? null
					: new SolidBrush(value);
		}

		//----------------------------------------------------------------------------------
		// All of these attributes are solely to make them appear in the IDE
		// When programming by hand, use Get/SetBorderPen() rather than these methods.

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Single TopBorderWidth
		{
			get => this.GetBorderWidth(Sides.Top);
			set => this.SetBorder(Sides.Top, value, this.GetBorderBrush(Sides.Top));
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Single LeftBorderWidth
		{
			get => this.GetBorderWidth(Sides.Left);
			set => this.SetBorder(Sides.Left, value, this.GetBorderBrush(Sides.Left));
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Single BottomBorderWidth
		{
			get => this.GetBorderWidth(Sides.Bottom);
			set => this.SetBorder(Sides.Bottom, value, this.GetBorderBrush(Sides.Bottom));
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Single RightBorderWidth
		{
			get => this.GetBorderWidth(Sides.Right);
			set => this.SetBorder(Sides.Right, value, this.GetBorderBrush(Sides.Right));
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Color TopBorderColor
		{
			get => this.GetSolidBorderColor(Sides.Top);
			set => this.SetBorder(Sides.Top, this.GetBorderWidth(Sides.Top), new SolidBrush(value));
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Color LeftBorderColor
		{
			get => this.GetSolidBorderColor(Sides.Left);
			set => this.SetBorder(Sides.Left, this.GetBorderWidth(Sides.Left), new SolidBrush(value));
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Color BottomBorderColor
		{
			get => this.GetSolidBorderColor(Sides.Bottom);
			set => this.SetBorder(Sides.Bottom, this.GetBorderWidth(Sides.Bottom), new SolidBrush(value));
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("Use Get/SetBorderPen() instead")]
		public Color RightBorderColor
		{
			get => this.GetSolidBorderColor(Sides.Right);
			set => this.SetBorder(Sides.Right, this.GetBorderWidth(Sides.Right), new SolidBrush(value));
		}

		private Color GetSolidBorderColor(Sides side)
		{
			Brush b = this.GetBorderBrush(side);
			return b is SolidBrush s
				? s.Color
				: Color.Empty;
		}

		#endregion

		#region Accessing

		/// <summary>
		/// Get the padding for a particular side. 0 means no padding on that side.
		/// Padding appears before the border does.
		/// </summary>
		/// <param name="side">Which side</param>
		/// <returns>The width of the padding</returns>
		public Single GetPadding(Sides side)
			=> this._Padding.ContainsKey(side)
				? this._Padding[side]
				: 0.0f;

		/// <summary>Set the padding for a particular side. 0 means no padding on that side.</summary>
		/// <param name="side">Which side</param>
		/// <param name="value">How much padding</param>
		public void SetPadding(Sides side, Single value)
		{
			if(side == Sides.All)
			{
				this._Padding[Sides.Left] = value;
				this._Padding[Sides.Top] = value;
				this._Padding[Sides.Right] = value;
				this._Padding[Sides.Bottom] = value;
			} else
				this._Padding[side] = value;
		}

		/// <summary>Get the width of the border on a particular side. 0 means no border on that side.</summary>
		/// <param name="side">Which side</param>
		/// <returns>The width of the border</returns>
		public Brush GetBorderBrush(Sides side)
		{
			Pen p = this.GetBorderPen(side);
			return p?.Brush;
		}

		/// <summary>Get the pen of the border on a particular side.</summary>
		/// <param name="side">Which side</param>
		/// <returns>The pen of the border</returns>
		public Pen GetBorderPen(Sides side)
			=> this._BorderPen.TryGetValue(side, out Pen p)
				? p
				: null;

		/// <summary>Get the width of the border on a particular side. 0 means no border on that side.</summary>
		/// <param name="side">Which side</param>
		/// <returns>The width of the border</returns>
		public Single GetBorderWidth(Sides side)
		{
			Pen p = this.GetBorderPen(side);
			return p?.Width ?? 0;
		}

		/// <summary>Change the brush and width of the border on a particular side. 0 means no border on that side.</summary>
		/// <param name="side">Which side</param>
		/// <param name="width">How wide should it be?</param>
		/// <param name="brush">What brush should be used to paint it</param>
		public void SetBorder(Sides side, Single width, Brush brush)
			=> this.SetBorderPen(side, new Pen(brush, width));

		/// <summary>Change the pen of the border on a particular side.</summary>
		/// <param name="side">Which side</param>
		/// <param name="p">What pen should be used to draw it</param>
		public void SetBorderPen(Sides side, Pen p)
		{
			if(side == Sides.All)
			{
				this._areSideBorderEqual = true;
				this._BorderPen[Sides.Left] = p;
				this._BorderPen[Sides.Top] = p;
				this._BorderPen[Sides.Right] = p;
				this._BorderPen[Sides.Bottom] = p;
			} else
			{
				this._areSideBorderEqual = false;
				this._BorderPen[side] = p;
			}
		}
		private Boolean _areSideBorderEqual = false;

		/// <summary>Get the distance that the text should be inset from the border on a given side</summary>
		/// <param name="side">Which side</param>
		/// <returns>Distance of text inset</returns>
		public Single GetTextInset(Sides side)
			=> GetKeyOrDefault(this._TextInset, side, 0f);

		/// <summary>Set the distance that the text should be inset from the border on a given side</summary>
		/// <param name="side">Which side</param>
		/// <param name="value">Distance of text inset</param>
		public void SetTextInset(Sides side, Single value)
		{
			if(side == Sides.All)
			{
				this._TextInset[Sides.Left] = value;
				this._TextInset[Sides.Top] = value;
				this._TextInset[Sides.Right] = value;
				this._TextInset[Sides.Bottom] = value;
			} else
				this._TextInset[side] = value;
		}

		// I hate the fact that Dictionary doesn't have a method like this!
		private ValueT GetKeyOrDefault<KeyT, ValueT>(Dictionary<KeyT, ValueT> map, KeyT key, ValueT defaultValue)
			=> map.TryGetValue(key, out ValueT v)
				? v
				: defaultValue;

		private Dictionary<Sides, Pen> _BorderPen = new Dictionary<Sides, Pen>();
		private Dictionary<Sides, PenData> _BorderPenData = new Dictionary<Sides, PenData>();
		private Dictionary<Sides, Single> _TextInset = new Dictionary<Sides, Single>();
		private Dictionary<Sides, Single> _Padding = new Dictionary<Sides, Single>();

		#endregion

		#region Calculating

		/// <summary>Calculate how height this block will be when its printed on one line</summary>
		/// <param name="g">The Graphic to use for rendering</param>
		/// <returns></returns>
		public Single CalculateHeight(Graphics g)
			=> this.CalculateHeight(g, "Wy", 9999999);

		/// <summary>Calculate how height this block will be when it prints the given string to a maximum of the given width</summary>
		/// <param name="g">The Graphic to use for rendering</param>
		/// <param name="s">The String to be considered</param>
		/// <param name="width">The max width for the rendering</param>
		/// <returns>The height that will be used</returns>
		public Single CalculateHeight(Graphics g, String s, Int32 width)
		{
			width -= (Int32)(this.GetTextInset(Sides.Left) + this.GetTextInset(Sides.Right) + 0.5f);
			StringFormat fmt = new StringFormat
			{
				Trimming = StringTrimming.EllipsisCharacter
			};

			if(!this.CanWrap)
				fmt.FormatFlags = StringFormatFlags.NoWrap;
			Single height = g.MeasureString(s, this.FontOrDefault, width, fmt).Height;
			height = Math.Max(height, this.MinimumTextHeight);
			height += this.GetPadding(Sides.Top);
			height += this.GetPadding(Sides.Bottom);
			height += this.GetBorderWidth(Sides.Top);
			height += this.GetBorderWidth(Sides.Bottom);
			height += this.GetTextInset(Sides.Top);
			height += this.GetTextInset(Sides.Bottom);
			return height;
		}

		private RectangleF ApplyInsets(RectangleF cell, Single left, Single top, Single right, Single bottom)
		{
			return new RectangleF(cell.X + left,
				cell.Y + top,
				cell.Width - (left + right),
				cell.Height - (top + bottom));
		}

		/// <summary>Given a bounding box return the box after applying the padding factors</summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public RectangleF CalculatePaddedBox(RectangleF cell)
			=> this.ApplyInsets(cell,
				this.GetPadding(Sides.Left),
				this.GetPadding(Sides.Top),
				this.GetPadding(Sides.Right),
				this.GetPadding(Sides.Bottom));

		/// <summary>Given an already padded box, return the box into which the text will be drawn.</summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public RectangleF CalculateBorderedBox(RectangleF cell)
			=> this.ApplyInsets(cell,
				this.GetBorderWidth(Sides.Left),
				this.GetBorderWidth(Sides.Top),
				this.GetBorderWidth(Sides.Right),
				this.GetBorderWidth(Sides.Bottom));

		/// <summary>Given an already padded and bordered box, return the box into which the text will be drawn.</summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public RectangleF CalculateTextBox(RectangleF cell)
			=> this.ApplyInsets(cell,
				this.GetTextInset(Sides.Left),
				this.GetTextInset(Sides.Top),
				this.GetTextInset(Sides.Right),
				this.GetTextInset(Sides.Bottom));

		/// <summary>Apply padding and text insets to the given rectangle</summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public RectangleF CalculatePaddedTextBox(RectangleF cell)
			=> this.CalculateTextBox(this.CalculateBorderedBox(this.CalculatePaddedBox(cell)));

		#endregion

		#region Rendering

		/// <summary>Draw the given String aligned within the given cell</summary>
		/// <param name="g">Graphics to draw on</param>
		/// <param name="r">Cell into which the text is to be drawn</param>
		/// <param name="s">The String to be drawn</param>
		/// <param name="align">How should the String be aligned</param>
		public void Draw(Graphics g, RectangleF r, String s, HorizontalAlignment align)
		{
			switch(align)
			{
			case HorizontalAlignment.Center:
				this.Draw(g, r, null, s, null);
				break;
			case HorizontalAlignment.Left:
				this.Draw(g, r, s, null, null);
				break;
			case HorizontalAlignment.Right:
				this.Draw(g, r, null, null, s);
				break;
			default:
				break;
			}
		}

		/// <summary>
		/// Draw the array of strings so that the first String is left aligned,
		/// the second is centered and the third is right aligned. All strings 
		/// are optional. Extra strings are ignored.
		/// </summary>
		/// <param name="g">Graphics to draw on</param>
		/// <param name="r">Cell into which the text is to be drawn</param>
		/// <param name="strings">Array of strings</param>
		public void Draw(Graphics g, RectangleF r, String[] strings)
		{
			String left = null, centre = null, right = null;

			if(strings.Length >= 1)
				left = strings[0];
			if(strings.Length >= 2)
				centre = strings[1];
			if(strings.Length >= 3)
				right = strings[2];

			this.Draw(g, r, left, centre, right);
		}

		/// <summary>Draw this block</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="left"></param>
		/// <param name="centre"></param>
		/// <param name="right"></param>
		public void Draw(Graphics g, RectangleF r, String left, String centre, String right)
		{
			RectangleF paddedRect = this.CalculatePaddedBox(r);
			RectangleF paddedBorderedRect = this.CalculateBorderedBox(paddedRect);
			this.DrawBackground(g, paddedBorderedRect);
			this.DrawText(g, paddedBorderedRect, left, centre, right);
			this.DrawBorder(g, paddedRect);
		}

		private void DrawBackground(Graphics g, RectangleF r)
		{
			if(this.BackgroundBrush != null)
			{
				// Enlarge the background area by half the border widths on each side
				RectangleF r2 = this.ApplyInsets(r,
					  this.GetBorderWidth(Sides.Left) / -2,
					  this.GetBorderWidth(Sides.Top) / -2,
					  this.GetBorderWidth(Sides.Right) / -2,
					  this.GetBorderWidth(Sides.Bottom) / -2);
				g.FillRectangle(PrepareBrushForDrawing(this.BackgroundBrush, r2), r2);
			}
		}

		private void DrawBorder(Graphics g, RectangleF r)
		{
			if(this._areSideBorderEqual && this.GetBorderPen(Sides.Top) != null)
			{
				Pen p = this.GetBorderPen(Sides.Top);
				this.DrawOneBorder(g, Sides.Top, r.X, r.Y, r.Width, r.Height, true);
			} else
			{
				this.DrawOneBorder(g, Sides.Top, r.X, r.Y, r.Right, r.Y, false);
				this.DrawOneBorder(g, Sides.Bottom, r.X, r.Bottom, r.Right, r.Bottom, false);
				this.DrawOneBorder(g, Sides.Left, r.X, r.Y, r.X, r.Bottom, false);
				this.DrawOneBorder(g, Sides.Right, r.Right, r.Y, r.Right, r.Bottom, false);
			}
		}

		static public Brush PrepareBrushForDrawing(Brush value, RectangleF r)
		{
			LinearGradientBrush lgb = value as LinearGradientBrush;
			if(lgb == null)
				return value;

			// We really just want to change the bounds of the gradient, but there is no way to do that
			// so we have to make a new brush and copy across the information we can

			//lgb.Rectangle.X = r.X;
			//lgb.Rectangle.Y = r.Y;
			//lgb.Rectangle.Width = r.Width;
			//lgb.Rectangle.Height = r.Height;

			LinearGradientBrush lgb2 = new LinearGradientBrush(r, lgb.LinearColors[0], lgb.LinearColors[1], 0.0)
			{
				Blend = lgb.Blend,
				//lgb2.InterpolationColors = lgb.InterpolationColors;
				WrapMode = lgb.WrapMode,
				Transform = lgb.Transform
			};
			return lgb2;
		}

		static public Pen PreparePenForDrawing(Pen value, RectangleF r)
		{
			if(r.Height == 0)
				r.Height = value.Width;
			if(r.Width == 0)
				r.Width = value.Width;
			value.Brush = BlockFormat.PrepareBrushForDrawing(value.Brush, r);
			return value;
		}

		static public Brush PrepareBrushForDrawing(Brush value, Rectangle r)
		{
			LinearGradientBrush lgb = value as LinearGradientBrush;
			if(lgb == null)
				return value;

			// We really just want to change the bounds of the gradient, but there is no way to do that
			// so we have to make a new brush and copy across the information we can

			//lgb.Rectangle.X = r.X;
			//lgb.Rectangle.Y = r.Y;
			//lgb.Rectangle.Width = r.Width;
			//lgb.Rectangle.Height = r.Height;

			LinearGradientBrush lgb2 = new LinearGradientBrush(r, lgb.LinearColors[0], lgb.LinearColors[1], 0.0)
			{
				Blend = lgb.Blend,
				//lgb2.InterpolationColors = lgb.InterpolationColors;
				WrapMode = lgb.WrapMode,
				Transform = lgb.Transform
			};
			return lgb2;
		}

		static public Pen PreparePenForDrawing(Pen value, Rectangle r)
		{
			value.Brush = BlockFormat.PrepareBrushForDrawing(value.Brush, r);
			return value;
		}

		private void DrawOneBorder(Graphics g, Sides side, Single x1, Single y1, Single x2, Single y2, Boolean isRectangle)
		{
			Pen p = this.GetBorderPen(side);

			if(p == null)
				return;

			PreparePenForDrawing(p, new RectangleF(x1, y1, x2 - x1, y2 - y1));

			if(isRectangle)
				g.DrawRectangle(p, x1, y1, x2, y2);
			else
				g.DrawLine(p, x1, y1, x2, y2);
		}

		private void DrawText(Graphics g, RectangleF r, String left, String centre, String right)
		{
			RectangleF textRect = this.CalculateTextBox(r);
			Font font = this.FontOrDefault;
			Brush textBrush = this.TextBrushOrDefault;

			StringFormat fmt = new StringFormat();
			if(!this.CanWrap)
				fmt.FormatFlags = StringFormatFlags.NoWrap;
			fmt.LineAlignment = StringAlignment.Center;
			fmt.Trimming = StringTrimming.EllipsisCharacter;

			if(!String.IsNullOrEmpty(left))
			{
				fmt.Alignment = StringAlignment.Near;
				g.DrawString(left, font, textBrush, textRect, fmt);
			}

			if(!String.IsNullOrEmpty(centre))
			{
				fmt.Alignment = StringAlignment.Center;
				g.DrawString(centre, font, textBrush, textRect, fmt);
			}

			if(!String.IsNullOrEmpty(right))
			{
				fmt.Alignment = StringAlignment.Far;
				g.DrawString(right, font, textBrush, textRect, fmt);
			}
			//g.DrawRectangle(new Pen(Color.Red, 0.5f), textRect.X, textRect.Y, textRect.Width, textRect.Height);
			//g.FillRectangle(Brushes.Red, r);
		}

		#endregion

		#region Standard formatting styles

		/// <summary>Return the default style for cells</summary>
		static public BlockFormat DefaultCell()
		{
			BlockFormat fmt = new BlockFormat
			{
				Font = new Font("MS Sans Serif", 9)
			};
			//fmt.TextBrush = Brushes.Black;
			fmt.SetBorderPen(Sides.All, new Pen(Color.Blue, 0.5f));
			fmt.SetTextInset(Sides.All, 2);
			fmt.CanWrap = true;

			return fmt;
		}

		/// <summary>Return a minimal set of formatting values.</summary>
		static public BlockFormat Minimal()
			=> BlockFormat.Minimal(new Font("Times New Roman", 12));

		/// <summary>Return a minimal set of formatting values.</summary>
		static public BlockFormat Minimal(Font f)
		{
			BlockFormat fmt = new BlockFormat
			{
				Font = f,
				TextBrush = Brushes.Black
			};
			fmt.SetBorderPen(Sides.All, new Pen(Color.Gray, 0.5f));
			fmt.SetTextInset(Sides.All, 3.0f);

			return fmt;
		}

		/// <summary>Return a set of formatting values that draws boxes</summary>
		static public BlockFormat Box()
			=> BlockFormat.Box(new Font("Verdana", 24));

		/// <summary>Return a set of formatting values that draws boxes</summary>
		static public BlockFormat Box(Font f)
		{
			BlockFormat fmt = new BlockFormat
			{
				Font = f,
				TextBrush = Brushes.Black
			};
			fmt.SetBorderPen(Sides.All, new Pen(Color.Black, 0.5f));
			fmt.BackgroundBrush = Brushes.LightBlue;
			fmt.SetTextInset(Sides.All, 3.0f);

			return fmt;
		}

		/// <summary>Return a format that will nicely print headers.</summary>
		static public BlockFormat Header()
			=> BlockFormat.Header(new Font("Verdana", 24));

		/// <summary>Return a format that will nicely print headers.</summary>
		static public BlockFormat Header(Font f)
		{
			BlockFormat fmt = new BlockFormat
			{
				Font = f,
				TextBrush = Brushes.WhiteSmoke,
				BackgroundBrush = new LinearGradientBrush(new Point(1, 1), new Point(2, 2), Color.DarkBlue, Color.WhiteSmoke)
			};
			fmt.SetTextInset(Sides.All, 3.0f);
			fmt.SetPadding(Sides.Bottom, 10);

			return fmt;
		}

		/// <summary>Return a format that will nicely print report footers.</summary>
		static public BlockFormat Footer()
			=> BlockFormat.Footer(new Font("Verdana", 10, FontStyle.Italic));

		/// <summary>Return a format that will nicely print report footers.</summary>
		static public BlockFormat Footer(Font f)
		{
			BlockFormat fmt = new BlockFormat
			{
				Font = f,
				TextBrush = Brushes.Black
			};
			fmt.SetPadding(Sides.Top, 10);
			fmt.SetBorderPen(Sides.Top, new Pen(Color.Gray, 0.5f));
			fmt.SetTextInset(Sides.All, 3.0f);

			return fmt;
		}

		/// <summary>Return a format that will nicely print list headers.</summary>
		static public BlockFormat ListHeader()
			=> BlockFormat.ListHeader(new Font("Verdana", 12));

		/// <summary>Return a format that will nicely print list headers.</summary>
		static public BlockFormat ListHeader(Font f)
		{
			BlockFormat fmt = new BlockFormat
			{
				Font = f,
				TextBrush = Brushes.Black,
				BackgroundBrush = Brushes.LightGray
			};
			fmt.SetBorderPen(Sides.All, new Pen(Color.DarkGray, 1.5f));
			fmt.SetTextInset(Sides.All, 1.0f);

			fmt.CanWrap = true;

			return fmt;
		}

		/// <summary>Return a format that will nicely print group headers.</summary>
		static public BlockFormat GroupHeader()
			=> BlockFormat.GroupHeader(new Font("Verdana", 10, FontStyle.Bold));

		/// <summary>Return a format that will nicely print group headers.</summary>
		static public BlockFormat GroupHeader(Font f)
		{
			BlockFormat fmt = new BlockFormat
			{
				Font = f,
				TextBrush = Brushes.Black
			};
			fmt.SetPadding(Sides.Top, f.Height / 2);
			fmt.SetPadding(Sides.Bottom, f.Height / 2);
			fmt.SetBorder(Sides.Bottom, 3f, new LinearGradientBrush(new Point(1, 1), new Point(2, 2), Color.DarkBlue, Color.White));
			fmt.SetTextInset(Sides.All, 1.0f);

			return fmt;
		}

		#endregion
	}
}