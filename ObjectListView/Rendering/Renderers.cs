/*
 * Renderers - A collection of useful renderers that are used to owner draw a cell in an ObjectListView
 *
 * Author: Phillip Piper
 * Date: 27/09/2008 9:15 AM
 *
 * Change log:
 * 2018-10-06   JPP  - Fix rendering so that OLVColumn.WordWrap works when using customised Renderers 
 * 2018-05-01   JPP  - Use ITextMatchFilter interface rather than TextMatchFilter concrete class.
 * v2.9.2
 * 2016-06-02   JPP  - CalculateImageWidth() no longer adds 2 to the image width
 * 2016-05-29   JPP  - Fix calculation of cell edit boundaries on TreeListView controls
 * v2.9
 * 2015-08-22   JPP  - Allow selected row back/fore colours to be specified for each row
 * 2015-06-23   JPP  - Added ColumnButtonRenderer plus general support for Buttons
 * 2015-06-22   JPP  - Added BaseRenderer.ConfigureItem() and ConfigureSubItem() to easily allow
 *                     other renderers to be chained for use within a primary renderer.
 *                   - Lots of tightening of hit tests and edit rectangles
 * 2015-05-15   JPP  - Handle rendering an Image when that Image is returned as an aspect.
 * v2.8
 * 2014-09-26   JPP  - Dispose of animation timer in a more robust fashion.
 * 2014-05-20   JPP  - Handle rendering disabled rows
 * v2.7
 * 2013-04-29   JPP  - Fixed bug where Images were not vertically aligned
 * v2.6
 * 2012-10-26   JPP  - Hit detection will no longer report check box hits on columns without checkboxes.
 * 2012-07-13   JPP  - [Breaking change] Added preferedSize parameter to IRenderer.GetEditRectangle().
 * v2.5.1
 * 2012-07-14   JPP  - Added CellPadding to various places. Replaced DescribedTaskRenderer.CellPadding.
 * 2012-07-11   JPP  - Added CellVerticalAlignment to various places allow cell contents to be vertically
 *                     aligned (rather than always being centered).
 * v2.5
 * 2010-08-24   JPP  - CheckBoxRenderer handles hot boxes and correctly vertically centers the box.
 * 2010-06-23   JPP  - Major rework of HighlightTextRenderer. Now uses TextMatchFilter directly.
 *                     Draw highlighting underneath text to improve legibility. Works with new
 *                     TextMatchFilter capabilities.
 * v2.4
 * 2009-10-30   JPP  - Plugged possible resource leak by using using() with CreateGraphics()
 * v2.3
 * 2009-09-28   JPP  - Added DescribedTaskRenderer
 * 2009-09-01   JPP  - Correctly handle an ImageRenderer's handling of an aspect that holds
 *                     the image to be displayed at Byte[].
 * 2009-08-29   JPP  - Fixed bug where some of a cell's background was not erased. 
 * 2009-08-15   JPP  - Correctly MeasureText() using the appropriate graphic context
 *                   - Handle translucent selection setting
 * v2.2.1
 * 2009-07-24   JPP  - Try to honour CanWrap setting when GDI rendering text.
 * 2009-07-11   JPP  - Correctly calculate edit rectangle for subitems of a tree view
 *                     (previously subitems were indented in the same way as the primary column)
 * v2.2
 * 2009-06-06   JPP  - Tweaked text rendering so that column 0 isn't ellipsed unnecessarily.
 * 2009-05-05   JPP  - Added Unfocused foreground and background colors 
 *                     (thanks to Christophe Hosten)
 * 2009-04-21   JPP  - Fixed off-by-1 error when calculating text widths. This caused
 *                     middle and right aligned columns to always wrap one character
 *                     when printed using ListViewPrinter (SF#2776634).
 * 2009-04-11   JPP  - Correctly renderer checkboxes when RowHeight is non-standard
 * 2009-04-06   JPP  - Allow for item indent when calculating edit rectangle
 * v2.1
 * 2009-02-24   JPP  - Work properly with ListViewPrinter again
 * 2009-01-26   JPP  - AUSTRALIA DAY (why aren't I on holidays!)
 *                   - Major overhaul of renderers. Now uses IRenderer interface.
 *                   - ImagesRenderer and FlagsRenderer<T> are now defunct.
 *                     The names are retained for backward compatibility.
 * 2009-01-23   JPP  - Align bitmap AND text according to column alignment (previously
 *                     only text was aligned and bitmap was always to the left).
 * 2009-01-21   JPP  - Changed to use TextRenderer rather than native GDI routines.
 * 2009-01-20   JPP  - Draw images directly from image list if possible. 30% faster!
 *                   - Tweaked some spacings to look more like native ListView
 *                   - Text highlight for non FullRowSelect is now the right color
 *                     when the control doesn't have focus.
 *                   - Commented out experimental animations. Still needs work.
 * 2009-01-19   JPP  - Changed to draw text using GDI routines. Looks more like
 *                     native control this way. Set UseGdiTextRendering to false to 
 *                     revert to previous behavior.
 * 2009-01-15   JPP  - Draw background correctly when control is disabled
 *                   - Render checkboxes using CheckBoxRenderer
 * v2.0.1
 * 2008-12-29   JPP  - Render text correctly when HideSelection is true.
 * 2008-12-26   JPP  - BaseRenderer now works correctly in all Views
 * 2008-12-23   JPP  - Fixed two small bugs in BarRenderer
 * v2.0
 * 2008-10-26   JPP  - Don't owner draw when in Design mode
 * 2008-09-27   JPP  - Separated from ObjectListView.cs
 * 
 * Copyright (C) 2006-2018 Phillip Piper
 * 
 * TO DO:
 * - Hit detection on renderers doesn't change the controls standard selection behavior
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Timer = System.Threading.Timer;

namespace BrightIdeasSoftware
{
	/// <summary>Renderers are the mechanism used for owner drawing cells.</summary>
	/// <remarks>As such, they can also handle hit detection and positioning of cell editing rectangles.</remarks>
	public interface IRenderer
	{
		/// <summary>Render the whole item within an ObjectListView. This is only used in non-Details views.</summary>
		/// <param name="e">The event</param>
		/// <param name="g">A Graphics for rendering</param>
		/// <param name="itemBounds">The bounds of the item</param>
		/// <param name="rowObject">The model Object to be drawn</param>
		/// <returns>Return true to indicate that the event was handled and no further processing is needed.</returns>
		Boolean RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, Object rowObject);

		/// <summary>Render one cell within an ObjectListView when it is in Details mode.</summary>
		/// <param name="e">The event</param>
		/// <param name="g">A Graphics for rendering</param>
		/// <param name="cellBounds">The bounds of the cell</param>
		/// <param name="rowObject">The model Object to be drawn</param>
		/// <returns>Return true to indicate that the event was handled and no further processing is needed.</returns>
		Boolean RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, Object rowObject);

		/// <summary>What is under the given point?</summary>
		/// <param name="hti"></param>
		/// <param name="x">x co-ordinate</param>
		/// <param name="y">y co-ordinate</param>
		/// <remarks>This method should only alter HitTestLocation and/or UserData.</remarks>
		void HitTest(OlvListViewHitTestInfo hti, Int32 x, Int32 y);

		/// <summary>When the value in the given cell is to be edited, where should the edit rectangle be placed?</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="item"></param>
		/// <param name="subItemIndex"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize);
	}

	/// <summary>Renderers that implement this interface will have the filter property updated, each time the filter on the ObjectListView is updated.</summary>
	public interface IFilterAwareRenderer
	{
		/// <summary>Gets or sets the filter that is currently active</summary>
		IModelFilter Filter { get; set; }
	}

	/// <summary>An AbstractRenderer is a do-nothing implementation of the IRenderer interface.</summary>
	[Browsable(true)]
	[ToolboxItem(false)]
	public class AbstractRenderer : Component, IRenderer
	{
		#region IRenderer Members

		/// <summary>Render the whole item within an ObjectListView. This is only used in non-Details views.</summary>
		/// <param name="e">The event</param>
		/// <param name="g">A Graphics for rendering</param>
		/// <param name="itemBounds">The bounds of the item</param>
		/// <param name="rowObject">The model Object to be drawn</param>
		/// <returns>Return true to indicate that the event was handled and no further processing is needed.</returns>
		public virtual Boolean RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, Object rowObject)
			=> true;

		/// <summary>Render one cell within an ObjectListView when it is in Details mode.</summary>
		/// <param name="e">The event</param>
		/// <param name="g">A Graphics for rendering</param>
		/// <param name="cellBounds">The bounds of the cell</param>
		/// <param name="rowObject">The model Object to be drawn</param>
		/// <returns>Return true to indicate that the event was handled and no further processing is needed.</returns>
		public virtual Boolean RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, Object rowObject)
			=> false;

		/// <summary>What is under the given point?</summary>
		/// <param name="hti"></param>
		/// <param name="x">x co-ordinate</param>
		/// <param name="y">y co-ordinate</param>
		/// <remarks>This method should only alter HitTestLocation and/or UserData.</remarks>
		public virtual void HitTest(OlvListViewHitTestInfo hti, Int32 x, Int32 y) { }

		/// <summary>When the value in the given cell is to be edited, where should the edit rectangle be placed?</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="item"></param>
		/// <param name="subItemIndex"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		public virtual Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize)
			=> cellBounds;

		#endregion
	}

	/// <summary>This class provides compatibility for v1 RendererDelegates</summary>
	[ToolboxItem(false)]
	internal class Version1Renderer : AbstractRenderer
	{
		public Version1Renderer(RenderDelegate renderDelegate)
			=> this.RenderDelegate = renderDelegate;

		/// <summary>The renderer delegate that this renderer wraps</summary>
		public RenderDelegate RenderDelegate;

		#region IRenderer Members

		public override Boolean RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, Object rowObject)
			=> this.RenderDelegate == null
				? base.RenderSubItem(e, g, cellBounds, rowObject)
				: this.RenderDelegate(e, g, cellBounds, rowObject);

		#endregion
	}

	/// <summary>A BaseRenderer provides useful base level functionality for any custom renderer.</summary>
	/// <remarks>
	/// <para>Subclasses will normally override the Render or OptionalRender method, and use the other methods as helper functions.</para>
	/// </remarks>
	[Browsable(true),
	 ToolboxItem(true)]
	public class BaseRenderer : AbstractRenderer
	{
		internal const TextFormatFlags NormalTextFormatFlags = TextFormatFlags.NoPrefix |
			TextFormatFlags.EndEllipsis |
			TextFormatFlags.PreserveGraphicsTranslateTransform;

		#region Configuration Properties

		/// <summary>Can the renderer wrap lines that do not fit completely within the cell?</summary>
		/// <remarks>
		/// <para>If this is not set specifically, the value will be taken from Column.WordWrap</para>
		/// <para>
		/// Wrapping text doesn't work with the GDI renderer, so if this set to true, GDI+ rendering will used.
		/// The difference between GDI and GDI+ rendering can give word wrapped columns a slight different appearance.
		/// </para>
		/// </remarks>
		[Category("Appearance")]
		[Description("Can the renderer wrap text that does not fit completely within the cell")]
		[DefaultValue(null)]
		public Boolean? CanWrap { get; set; }

		/// <summary>Get the actual value that should be used right now for CanWrap</summary>
		[Browsable(false)]
		protected Boolean CanWrapOrDefault => this.CanWrap ?? this.Column != null && this.Column.WordWrap;

		/// <summary>Gets or sets how many pixels will be left blank around this cell</summary>
		/// <remarks>
		/// <para>This setting only takes effect when the control is owner drawn.</para>
		/// <para><see cref="ObjectListView.CellPadding"/> for more details.</para>
		/// </remarks>
		[Category(Constants.ObjectListView)]
		[Description("The number of pixels that renderer will leave empty around the edge of the cell")]
		[DefaultValue(null)]
		public Rectangle? CellPadding { get; set; }

		/// <summary>Gets the horizontal alignment of the column</summary>
		[Browsable(false)]
		public HorizontalAlignment CellHorizontalAlignment => this.Column == null ? HorizontalAlignment.Left : this.Column.TextAlign;

		/// <summary>Gets or sets how cells drawn by this renderer will be vertically aligned.</summary>
		/// <remarks>
		/// <para>If this is not set, the value from the column or control itself will be used.</para>
		/// </remarks>
		[Category(Constants.ObjectListView)]
		[Description("How will cell values be vertically aligned?")]
		[DefaultValue(null)]
		public virtual StringAlignment? CellVerticalAlignment { get; set; }

		/// <summary>Gets the optional padding that this renderer should apply before drawing.</summary>
		/// <remarks>This property considers all possible sources of padding</remarks>
		[Browsable(false)]
		protected virtual Rectangle? EffectiveCellPadding
		{
			get
			{
				if(this.CellPadding.HasValue)
					return this.CellPadding.Value;

				if(this.OLVSubItem != null && this.OLVSubItem.CellPadding.HasValue)
					return this.OLVSubItem.CellPadding.Value;

				if(this.ListItem != null && this.ListItem.CellPadding.HasValue)
					return this.ListItem.CellPadding.Value;

				if(this.Column != null && this.Column.CellPadding.HasValue)
					return this.Column.CellPadding.Value;

				if(this.ListView != null && this.ListView.CellPadding.HasValue)
					return this.ListView.CellPadding.Value;

				return null;
			}
		}

		/// <summary>Gets the vertical cell alignment that should govern the rendering.</summary>
		/// <remarks>This property considers all possible sources.</remarks>
		[Browsable(false)]
		protected virtual StringAlignment EffectiveCellVerticalAlignment
		{
			get
			{
				if(this.CellVerticalAlignment.HasValue)
					return this.CellVerticalAlignment.Value;

				if(this.OLVSubItem != null && this.OLVSubItem.CellVerticalAlignment.HasValue)
					return this.OLVSubItem.CellVerticalAlignment.Value;

				if(this.ListItem != null && this.ListItem.CellVerticalAlignment.HasValue)
					return this.ListItem.CellVerticalAlignment.Value;

				if(this.Column != null && this.Column.CellVerticalAlignment.HasValue)
					return this.Column.CellVerticalAlignment.Value;

				if(this.ListView != null)
					return this.ListView.CellVerticalAlignment;

				return StringAlignment.Center;
			}
		}

		/// <summary>Gets or sets the image list from which keyed images will be fetched</summary>
		[Category("Appearance")]
		[Description("The image list from which keyed images will be fetched for drawing. If this is not given, the small ImageList from the ObjectListView will be used")]
		[DefaultValue(null)]
		public ImageList ImageList { get; set; }

		/// <summary>When rendering multiple images, how many pixels should be between each image?</summary>
		[Category("Appearance")]
		[Description("When rendering multiple images, how many pixels should be between each image?")]
		[DefaultValue(1)]
		public Int32 Spacing { get; set; } = 1;

		/// <summary>Should text be rendered using GDI routines? This makes the text look more like a native List view control.</summary>
		/// <remarks>Even if this is set to true, it will return false if the renderer
		/// is set to word wrap, since GDI doesn't handle wrapping.</remarks>
		[Category("Appearance")]
		[Description("Should text be rendered using GDI routines?")]
		[DefaultValue(true)]
		public virtual Boolean UseGdiTextRendering
		{
			get => !this.IsPrinting && !this.CanWrapOrDefault && useGdiTextRendering;// Can't use GDI routines on a GDI+ printer context or when word wrapping is required
			set => useGdiTextRendering = value;
		}
		private Boolean useGdiTextRendering = true;

		#endregion

		#region State Properties

		/// <summary>Get or set the aspect of the model Object that this renderer should draw</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Object Aspect
		{
			get => _aspect ?? (_aspect = this.Column.GetValue(this.RowObject));
			set => _aspect = value;
		}

		private Object _aspect;

		/// <summary>What are the bounds of the cell that is being drawn?</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle Bounds { get; set; }

		/// <summary>Get or set the OLVColumn that this renderer will draw</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public OLVColumn Column { get; set; }

		/// <summary>Get/set the event that caused this renderer to be called</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DrawListViewItemEventArgs DrawItemEvent { get; set; }

		/// <summary>Get/set the event that caused this renderer to be called</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DrawListViewSubItemEventArgs Event { get; set; }

		/// <summary>Gets or sets the font to be used for text in this cell</summary>
		/// <remarks>
		/// <para>
		/// In general, this property should be treated as internal.
		/// If you do set this, the given font will be used without any other consideration.
		/// All other factors -- selection state, hot item, hyperlinks -- will be ignored.
		/// </para>
		/// <para>A better way to set the font is to change either ListItem.Font or SubItem.Font</para>
		/// </remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font Font
		{
			get
			{
				if(this._font != null || this.ListItem == null)
					return this._font;

				if(this.SubItem == null || this.ListItem.UseItemStyleForSubItems)
					return this.ListItem.Font;

				return this.SubItem.Font;
			}
			set { this._font = value; }
		}

		private Font _font;

		/// <summary>Gets the image list from which keyed images will be fetched</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ImageList ImageListOrDefault => this.ImageList ?? this.ListView.SmallImageList;

		/// <summary>Should this renderer fill in the background before drawing?</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Boolean IsDrawBackground => !this.IsPrinting;

		/// <summary>Cache whether or not our item is selected</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Boolean IsItemSelected { get; set; }

		/// <summary>Is this renderer being used on a printer context?</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Boolean IsPrinting { get; set; }

		/// <summary>Get or set the listitem that this renderer will be drawing</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public OLVListItem ListItem { get; set; }

		/// <summary>Get/set the listview for which the drawing is to be done</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ObjectListView ListView { get; set; }

		/// <summary>Get the specialized OLVSubItem that this renderer is drawing</summary>
		/// <remarks>This returns null for column 0.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public OLVListSubItem OLVSubItem => this.SubItem as OLVListSubItem;

		/// <summary>Get or set the model Object that this renderer should draw</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Object RowObject { get; set; }

		/// <summary>Get or set the list subitem that this renderer will be drawing</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public OLVListSubItem SubItem { get; set; }

		/// <summary>The brush that will be used to paint the text</summary>
		/// <remarks>
		/// <para>In general, this property should be treated as internal. It will be reset after each render.</para>
		/// <para>In particular, don't set it to configure the color of the text on the control. That should be done via SubItem.ForeColor</para>
		/// </remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Brush TextBrush
		{
			get => _textBrush ?? new SolidBrush(this.GetForegroundColor());
			set => _textBrush = value;
		}

		private Brush _textBrush;

		/// <summary>Will this renderer use the custom images from the parent ObjectListView to draw the checkbox images.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If this is true, the renderer will use the images from the 
		/// StateImageList to represent checkboxes. 0 - unchecked, 1 - checked, 2 - indeterminate.
		/// </para>
		/// <para>If this is false (the default), then the renderer will use .NET's standard CheckBoxRenderer.</para>
		/// </remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Boolean UseCustomCheckboxImages { get; set; }

		private void ClearState()
		{
			this.Event = null;
			this.DrawItemEvent = null;
			this.Aspect = null;
			this.TextBrush = null;
		}

		#endregion

		#region Utilities

		/// <summary>Align the second rectangle with the first rectangle, according to the alignment of the column</summary>
		/// <param name="outer">The cell's bounds</param>
		/// <param name="inner">The rectangle to be aligned within the bounds</param>
		/// <returns>An aligned rectangle</returns>
		protected virtual Rectangle AlignRectangle(Rectangle outer, Rectangle inner)
		{
			Rectangle r = new Rectangle(outer.Location, inner.Size);

			// Align horizontally depending on the column alignment
			if(inner.Width < outer.Width)
				r.X = this.AlignHorizontally(outer, inner);

			// Align vertically too
			if(inner.Height < outer.Height)
				r.Y = this.AlignVertically(outer, inner);

			return r;
		}

		/// <summary>Calculate the left edge of the rectangle that aligns the outer rectangle with the inner one according to this renderer's horizontal alignment</summary>
		/// <param name="outer"></param>
		/// <param name="inner"></param>
		/// <returns></returns>
		protected Int32 AlignHorizontally(Rectangle outer, Rectangle inner)
		{
			HorizontalAlignment alignment = this.CellHorizontalAlignment;
			switch(alignment)
			{
			case HorizontalAlignment.Left:
				return outer.Left + 1;
			case HorizontalAlignment.Center:
				return outer.Left + ((outer.Width - inner.Width) / 2);
			case HorizontalAlignment.Right:
				return outer.Right - inner.Width - 1;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>Calculate the top of the rectangle that aligns the outer rectangle with the inner rectangle according to this renders vertical alignment</summary>
		/// <param name="outer"></param>
		/// <param name="inner"></param>
		/// <returns></returns>
		protected Int32 AlignVertically(Rectangle outer, Rectangle inner)
			=> this.AlignVertically(outer, inner.Height);

		/// <summary>
		/// Calculate the top of the rectangle that aligns the outer rectangle with a rectangle of the given height
		/// according to this renderer's vertical alignment
		/// </summary>
		/// <param name="outer"></param>
		/// <param name="innerHeight"></param>
		/// <returns></returns>
		protected Int32 AlignVertically(Rectangle outer, Int32 innerHeight)
		{
			switch(this.EffectiveCellVerticalAlignment)
			{
			case StringAlignment.Near:
				return outer.Top + 1;
			case StringAlignment.Center:
				return outer.Top + ((outer.Height - innerHeight) / 2);
			case StringAlignment.Far:
				return outer.Bottom - innerHeight - 1;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Calculate the space that our rendering will occupy and then align that space
		/// with the given rectangle, according to the Column alignment
		/// </summary>
		/// <param name="g"></param>
		/// <param name="r">Pre-padded bounds of the cell</param>
		/// <returns></returns>
		protected virtual Rectangle CalculateAlignedRectangle(Graphics g, Rectangle r)
		{
			if(this.Column == null)
				return r;

			Rectangle contentRectangle = new Rectangle(Point.Empty, this.CalculateContentSize(g, r));
			return this.AlignRectangle(r, contentRectangle);
		}

		/// <summary>Calculate the size of the content of this cell.</summary>
		/// <param name="g"></param>
		/// <param name="r">Pre-padded bounds of the cell</param>
		/// <returns>The width and height of the content</returns>
		protected virtual Size CalculateContentSize(Graphics g, Rectangle r)
		{
			Size checkBoxSize = this.CalculatePrimaryCheckBoxSize(g);
			Size imageSize = this.CalculateImageSize(g, this.GetImageSelector());
			Size textSize = this.CalculateTextSize(g, this.GetText(), r.Width - (checkBoxSize.Width + imageSize.Width));

			// If the combined width is greater than the whole cell,  we just use the cell itself

			Int32 width = Math.Min(r.Width, checkBoxSize.Width + imageSize.Width + textSize.Width);
			Int32 componentMaxHeight = Math.Max(checkBoxSize.Height, Math.Max(imageSize.Height, textSize.Height));
			Int32 height = Math.Min(r.Height, componentMaxHeight);

			return new Size(width, height);
		}

		/// <summary>Calculate the bounds of a checkbox given the (pre-padded) cell bounds</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds">Pre-padded cell bounds</param>
		/// <returns></returns>
		protected Rectangle CalculateCheckBoxBounds(Graphics g, Rectangle cellBounds)
		{
			Size checkBoxSize = this.CalculateCheckBoxSize(g);
			return this.AlignRectangle(cellBounds, new Rectangle(0, 0, checkBoxSize.Width, checkBoxSize.Height));
		}

		/// <summary>How much space will the check box for this cell occupy?</summary>
		/// <remarks>Only column 0 can have check boxes. Sub item checkboxes are treated as images</remarks>
		/// <param name="g"></param>
		/// <returns></returns>
		protected virtual Size CalculateCheckBoxSize(Graphics g)
		{
			if(this.UseCustomCheckboxImages && this.ListView.StateImageList != null)
				return this.ListView.StateImageList.ImageSize;

			return CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.UncheckedNormal);
		}

		/// <summary>How much space will the check box for this row occupy?</summary>
		/// <remarks>If the list doesn't have checkboxes, or this isn't the primary column, this returns an empty size.</remarks>
		/// <param name="g"></param>
		/// <returns></returns>
		protected virtual Size CalculatePrimaryCheckBoxSize(Graphics g)
		{
			if(!this.ListView.CheckBoxes || !this.ColumnIsPrimary)
				return Size.Empty;

			Size size = this.CalculateCheckBoxSize(g);
			size.Width += 6;
			return size;
		}

		/// <summary>How much horizontal space will the image of this cell occupy?</summary>
		/// <param name="g"></param>
		/// <param name="imageSelector"></param>
		/// <returns></returns>
		protected virtual Int32 CalculateImageWidth(Graphics g, Object imageSelector)
			=> this.CalculateImageSize(g, imageSelector).Width;

		/// <summary>How much vertical space will the image of this cell occupy?</summary>
		/// <param name="g"></param>
		/// <param name="imageSelector"></param>
		/// <returns></returns>
		protected virtual Int32 CalculateImageHeight(Graphics g, Object imageSelector)
			=> this.CalculateImageSize(g, imageSelector).Height;

		/// <summary>How much space will the image of this cell occupy?</summary>
		/// <param name="g"></param>
		/// <param name="imageSelector"></param>
		/// <returns></returns>
		protected virtual Size CalculateImageSize(Graphics g, Object imageSelector)
		{
			if(imageSelector == null || imageSelector == DBNull.Value)
				return Size.Empty;

			// Check for the image in the image list (most common case)
			ImageList il = this.ImageListOrDefault;
			if(il != null)
			{
				Int32 selectorAsInt;
				if(imageSelector is String selectorAsString)
					selectorAsInt = il.Images.IndexOfKey(selectorAsString);
				else if(imageSelector is Int32 i)
					selectorAsInt = i;
				else
					selectorAsInt = -1;

				if(selectorAsInt >= 0)
					return il.ImageSize;
			}

			// Is the selector actually an image?
			return imageSelector is Image image
				? image.Size
				: Size.Empty;
		}

		/// <summary>How much horizontal space will the text of this cell occupy?</summary>
		/// <param name="g"></param>
		/// <param name="txt"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		protected virtual Int32 CalculateTextWidth(Graphics g, String txt, Int32 width)
			=> String.IsNullOrEmpty(txt)
				? 0
				: this.CalculateTextSize(g, txt, width).Width;

		/// <summary>How much space will the text of this cell occupy?</summary>
		/// <param name="g"></param>
		/// <param name="txt"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		protected virtual Size CalculateTextSize(Graphics g, String txt, Int32 width)
		{
			if(String.IsNullOrEmpty(txt))
				return Size.Empty;

			if(this.UseGdiTextRendering)
			{
				Size proposedSize = new Size(width, Int32.MaxValue);
				return TextRenderer.MeasureText(g, txt, this.Font, proposedSize, NormalTextFormatFlags);
			}

			// Using GDI+ rendering
			using(StringFormat fmt = new StringFormat())
			{
				fmt.Trimming = StringTrimming.EllipsisCharacter;
				SizeF sizeF = g.MeasureString(txt, this.Font, width, fmt);
				return new Size(1 + (Int32)sizeF.Width, 1 + (Int32)sizeF.Height);
			}
		}

		/// <summary>Return the Color that is the background color for this item's cell</summary>
		/// <returns>The background color of the subitem</returns>
		public virtual Color GetBackgroundColor()
		{
			if(!this.ListView.Enabled)
				return SystemColors.Control;

			if(this.IsItemSelected && !this.ListView.UseTranslucentSelection && this.ListView.FullRowSelect)
				return this.GetSelectedBackgroundColor();

			if(this.SubItem == null || this.ListItem.UseItemStyleForSubItems)
				return this.ListItem.BackColor;

			return this.SubItem.BackColor;
		}

		/// <summary>Return the color of the background color when the item is selected</summary>
		/// <returns>The background color of the subitem</returns>
		public virtual Color GetSelectedBackgroundColor()
		{
			if(this.ListView.Focused)
				return this.ListItem.SelectedBackColor ?? this.ListView.SelectedBackColorOrDefault;

			if(!this.ListView.HideSelection)
				return this.ListView.UnfocusedSelectedBackColorOrDefault;

			return this.ListItem.BackColor;
		}

		/// <summary>Return the color to be used for text in this cell</summary>
		/// <returns>The text color of the subitem</returns>
		public virtual Color GetForegroundColor()
		{
			if(this.IsItemSelected &&
				!this.ListView.UseTranslucentSelection &&
				(this.ColumnIsPrimary || this.ListView.FullRowSelect))
				return this.GetSelectedForegroundColor();

			return this.SubItem == null || this.ListItem.UseItemStyleForSubItems ? this.ListItem.ForeColor : this.SubItem.ForeColor;
		}

		/// <summary>Return the color of the foreground color when the item is selected</summary>
		/// <returns>The foreground color of the subitem</returns>
		public virtual Color GetSelectedForegroundColor()
		{
			if(this.ListView.Focused)
				return this.ListItem.SelectedForeColor ?? this.ListView.SelectedForeColorOrDefault;

			if(!this.ListView.HideSelection)
				return this.ListView.UnfocusedSelectedForeColorOrDefault;

			return this.SubItem == null || this.ListItem.UseItemStyleForSubItems ? this.ListItem.ForeColor : this.SubItem.ForeColor;
		}

		/// <summary>Return the image that should be drawn against this subitem</summary>
		/// <returns>An Image or null if no image should be drawn.</returns>
		protected virtual Image GetImage()
			=> this.GetImage(this.GetImageSelector());

		/// <summary>
		/// Return the actual image that should be drawn when keyed by the given image selector.
		/// An image selector can be:
		/// <list type="bullet">
		/// <item><description>an int, giving the index into the image list</description></item>
		/// <item><description>a String, giving the image key into the image list</description></item>
		/// <item><description>an Image, being the image itself</description></item>
		/// </list>
		/// </summary>
		/// <param name="imageSelector">The value that indicates the image to be used</param>
		/// <returns>An Image or null</returns>
		protected virtual Image GetImage(Object imageSelector)
		{
			if(imageSelector == null || imageSelector == DBNull.Value)
				return null;

			ImageList il = this.ImageListOrDefault;
			if(il != null)
			{
				if(imageSelector is Int32 index)
					return index >= 0 && index < il.Images.Count
						? il.Images[index]
						: null;

				if(imageSelector is String str)
					return il.Images.ContainsKey(str)
						? il.Images[str]
						: null;
			}

			return imageSelector as Image;
		}

		/// <summary></summary>
		protected virtual Object GetImageSelector()
			=> this.ColumnIsPrimary ? this.ListItem.ImageSelector : this.OLVSubItem.ImageSelector;

		/// <summary>Return the String that should be drawn within this</summary>
		/// <returns></returns>
		protected virtual String GetText()
			=> this.SubItem == null ? this.ListItem.Text : this.SubItem.Text;
		#endregion

		#region IRenderer members

		/// <summary>Render the whole item in a non-details view.</summary>
		/// <param name="e"></param>
		/// <param name="g"></param>
		/// <param name="itemBounds"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		public override Boolean RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, Object model)
		{
			this.ConfigureItem(e, itemBounds, model);
			return this.OptionalRender(g, itemBounds);
		}

		/// <summary>Prepare this renderer to draw in response to the given event</summary>
		/// <param name="e"></param>
		/// <param name="itemBounds"></param>
		/// <param name="model"></param>
		/// <remarks>Use this if you want to chain a second renderer within a primary renderer.</remarks>
		public virtual void ConfigureItem(DrawListViewItemEventArgs e, Rectangle itemBounds, Object model)
		{
			this.ClearState();

			this.DrawItemEvent = e;
			this.ListItem = (OLVListItem)e.Item;
			this.SubItem = null;
			this.ListView = (ObjectListView)this.ListItem.ListView;
			this.Column = this.ListView.GetColumn(0);
			this.RowObject = model;
			this.Bounds = itemBounds;
			this.IsItemSelected = this.ListItem.Selected && this.ListItem.Enabled;
		}

		/// <summary>Render one cell</summary>
		/// <param name="e"></param>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		public override Boolean RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, Object model)
		{
			this.ConfigureSubItem(e, cellBounds, model);
			return this.OptionalRender(g, cellBounds);
		}

		/// <summary>Prepare this renderer to draw in response to the given event</summary>
		/// <param name="e"></param>
		/// <param name="cellBounds"></param>
		/// <param name="model"></param>
		/// <remarks>Use this if you want to chain a second renderer within a primary renderer.</remarks>
		public virtual void ConfigureSubItem(DrawListViewSubItemEventArgs e, Rectangle cellBounds, Object model)
		{
			this.ClearState();

			this.Event = e;
			this.ListItem = (OLVListItem)e.Item;
			this.SubItem = (OLVListSubItem)e.SubItem;
			this.ListView = (ObjectListView)this.ListItem.ListView;
			this.Column = (OLVColumn)e.Header;
			this.RowObject = model;
			this.Bounds = cellBounds;
			this.IsItemSelected = this.ListItem.Selected && this.ListItem.Enabled;
		}

		/// <summary>Calculate which part of this cell was hit</summary>
		/// <param name="hti"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public override void HitTest(OlvListViewHitTestInfo hti, Int32 x, Int32 y)
		{
			this.ClearState();

			this.ListView = hti.ListView;
			this.ListItem = hti.Item;
			this.SubItem = hti.SubItem;
			this.Column = hti.Column;
			this.RowObject = hti.RowObject;
			this.IsItemSelected = this.ListItem.Selected && this.ListItem.Enabled;
			this.Bounds = this.SubItem == null
				? this.ListItem.Bounds
				: this.ListItem.GetSubItemBounds(this.Column.Index);

			using(Graphics g = this.ListView.CreateGraphics())
				this.HandleHitTest(g, hti, x, y);
		}

		/// <summary>Calculate the edit rectangle</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="item"></param>
		/// <param name="subItemIndex"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		public override Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize)
		{
			this.ClearState();

			this.ListView = (ObjectListView)item.ListView;
			this.ListItem = item;
			this.SubItem = item.GetSubItem(subItemIndex);
			this.Column = this.ListView.GetColumn(subItemIndex);
			this.RowObject = item.RowObject;
			this.IsItemSelected = this.ListItem.Selected && this.ListItem.Enabled;
			this.Bounds = cellBounds;

			return this.HandleGetEditRectangle(g, cellBounds, item, subItemIndex, preferredSize);
		}

		#endregion

		#region IRenderer implementation

		// Subclasses will probably want to override these methods rather than the IRenderer
		// interface methods.

		/// <summary>Draw our data into the given rectangle using the given graphics context.</summary>
		/// <remarks>
		/// <para>Subclasses should override this method.</para></remarks>
		/// <param name="g">The graphics context that should be used for drawing</param>
		/// <param name="r">The bounds of the subitem cell</param>
		/// <returns>Returns whether the rendering has already taken place.
		/// If this returns false, the default processing will take over.
		/// </returns>
		public virtual Boolean OptionalRender(Graphics g, Rectangle r)
		{
			if(this.ListView.View != View.Details)
				return false;

			this.Render(g, r);
			return true;
		}

		/// <summary>Draw our data into the given rectangle using the given graphics context.</summary>
		/// <remarks>
		/// <para>Subclasses should override this method if they never want
		/// to fall back on the default processing</para></remarks>
		/// <param name="g">The graphics context that should be used for drawing</param>
		/// <param name="r">The bounds of the subitem cell</param>
		public virtual void Render(Graphics g, Rectangle r)
			=> this.StandardRender(g, r);

		/// <summary>Do the actual work of hit testing. Subclasses should override this rather than HitTest()</summary>
		/// <param name="g"></param>
		/// <param name="hti"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		protected virtual void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, Int32 x, Int32 y)
		{
			Rectangle r = this.CalculateAlignedRectangle(g, this.ApplyCellPadding(this.Bounds));
			this.StandardHitTest(g, hti, r, x, y);
		}

		/// <summary>Handle a HitTest request after all state information has been initialized</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="item"></param>
		/// <param name="subItemIndex"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		protected virtual Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize)
		{
			// MAINTAINER NOTE: This type testing is wrong (design-wise). The base class should return cell bounds,
			// and a more specialized class should return StandardGetEditRectangle(). But BaseRenderer is used directly
			// to draw most normal cells, as well as being directly subclassed for user implemented renderers. And this
			// method needs to return different bounds in each of those cases. We should have a StandardRenderer and make
			// BaseRenderer into an ABC -- but that would break too much existing code. And so we have this hack :(

			// If we are a standard renderer, return the position of the text, otherwise, use the whole cell.
			if(this.GetType() == typeof(BaseRenderer))
				return this.StandardGetEditRectangle(g, cellBounds, preferredSize);

			// Center the editor vertically
			if(cellBounds.Height != preferredSize.Height)
				cellBounds.Y += (cellBounds.Height - preferredSize.Height) / 2;

			return cellBounds;
		}

		#endregion

		#region Standard IRenderer implementations

		/// <summary>Draw the standard "[checkbox] [image] [text]" cell after the state properties have been initialized.</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		protected void StandardRender(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);

			// Adjust the first columns rectangle to match the padding used by the native mode of the ListView
			if(this.ColumnIsPrimary && this.CellHorizontalAlignment == HorizontalAlignment.Left)
			{
				r.X += 3;
				r.Width -= 1;
			}
			r = this.ApplyCellPadding(r);
			this.DrawAlignedImageAndText(g, r);

			// Show where the bounds of the cell padding are (debugging)
			if(ObjectListView.ShowCellPaddingBounds)
				g.DrawRectangle(Pens.Purple, r);
		}

		/// <summary>Change the bounds of the given rectangle to take any cell padding into account</summary>
		/// <param name="r"></param>
		/// <returns></returns>
		public virtual Rectangle ApplyCellPadding(Rectangle r)
		{
			Rectangle? padding = this.EffectiveCellPadding;
			if(!padding.HasValue)
				return r;
			// The two subtractions below look wrong, but are correct!
			Rectangle paddingRectangle = padding.Value;
			r.Width -= paddingRectangle.Right;
			r.Height -= paddingRectangle.Bottom;
			r.Offset(paddingRectangle.Location);
			return r;
		}

		/// <summary>Perform normal hit testing relative to the given aligned content bounds</summary>
		/// <param name="g"></param>
		/// <param name="hti"></param>
		/// <param name="alignedContentRectangle"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		protected virtual void StandardHitTest(Graphics g, OlvListViewHitTestInfo hti, Rectangle alignedContentRectangle, Int32 x, Int32 y)
		{
			Rectangle r = alignedContentRectangle;

			// Match tweaking from renderer
			if(this.ColumnIsPrimary && this.CellHorizontalAlignment == HorizontalAlignment.Left && !(this is TreeListView.TreeRenderer))
			{
				r.X += 3;
				r.Width -= 1;
			}
			Int32 width = 0;

			// Did they hit a check box on the primary column?
			if(this.ColumnIsPrimary && this.ListView.CheckBoxes)
			{
				Size checkBoxSize = this.CalculateCheckBoxSize(g);
				Int32 checkBoxTop = this.AlignVertically(r, checkBoxSize.Height);
				Rectangle r3 = new Rectangle(r.X, checkBoxTop, checkBoxSize.Width, checkBoxSize.Height);
				width = r3.Width + 6;
				// g.DrawRectangle(Pens.DarkGreen, r3);
				if(r3.Contains(x, y))
				{
					hti.HitTestLocation = HitTestLocation.CheckBox;
					return;
				}
			}

			// Did they hit the image? If they hit the image of a 
			// non-primary column that has a checkbox, it counts as a 
			// checkbox hit
			r.X += width;
			r.Width -= width;
			width = this.CalculateImageWidth(g, this.GetImageSelector());
			Rectangle rTwo = r;
			rTwo.Width = width;
			//g.DrawRectangle(Pens.Red, rTwo);
			if(rTwo.Contains(x, y))
			{
				hti.HitTestLocation = this.Column != null && (this.Column.Index > 0 && this.Column.CheckBoxes)
					? HitTestLocation.CheckBox
					: HitTestLocation.Image;
				return;
			}

			// Did they hit the text?
			r.X += width;
			r.Width -= width;
			width = this.CalculateTextWidth(g, this.GetText(), r.Width);
			rTwo = r;
			rTwo.Width = width;
			// g.DrawRectangle(Pens.Blue, rTwo);
			if(rTwo.Contains(x, y))
			{
				hti.HitTestLocation = HitTestLocation.Text;
				return;
			}

			hti.HitTestLocation = HitTestLocation.InCell;
		}

		/// <summary>This method calculates the bounds of the text within a standard layout (i.e. optional checkbox, optional image, text)</summary>
		/// <remarks>This method only works correctly if the state of the renderer has been fully initialized (see BaseRenderer.GetEditRectangle)</remarks>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		protected virtual Rectangle StandardGetEditRectangle(Graphics g, Rectangle cellBounds, Size preferredSize)
		{
			Size contentSize = this.CalculateContentSize(g, cellBounds);
			Int32 contentWidth = this.Column.CellEditUseWholeCellEffective ? cellBounds.Width : contentSize.Width;
			Rectangle editControlBounds = this.CalculatePaddedAlignedBounds(g, cellBounds, new Size(contentWidth, preferredSize.Height));

			Size checkBoxSize = this.CalculatePrimaryCheckBoxSize(g);
			Int32 imageWidth = this.CalculateImageWidth(g, this.GetImageSelector());

			Int32 width = checkBoxSize.Width + imageWidth + 2;

			// Indent the primary column by the required amount
			if(this.ColumnIsPrimary && this.ListItem.IndentCount > 0)
			{
				Int32 indentWidth = this.ListView.SmallImageSize.Width * this.ListItem.IndentCount;
				editControlBounds.X += indentWidth;
			}

			editControlBounds.X += width;
			editControlBounds.Width -= width;

			if(editControlBounds.Width < 50)
				editControlBounds.Width = 50;
			if(editControlBounds.Right > cellBounds.Right)
				editControlBounds.Width = cellBounds.Right - editControlBounds.Left;

			return editControlBounds;
		}

		/// <summary>Apply any padding to the given bounds, and then align a rectangle of the given size within that padded area.</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="preferredSize"></param>
		/// <returns></returns>
		protected Rectangle CalculatePaddedAlignedBounds(Graphics g, Rectangle cellBounds, Size preferredSize)
		{
			Rectangle r = this.ApplyCellPadding(cellBounds);
			r = this.AlignRectangle(r, new Rectangle(Point.Empty, preferredSize));
			return r;
		}

		#endregion

		#region Drawing routines

		/// <summary>Draw the given image aligned horizontally within the column.</summary>
		/// <remarks>Over tall images are scaled to fit. Over-wide images are truncated. This is by design!</remarks>
		/// <param name="g">Graphics context to use for drawing</param>
		/// <param name="r">Bounds of the cell</param>
		/// <param name="image">The image to be drawn</param>
		protected virtual void DrawAlignedImage(Graphics g, Rectangle r, Image image)
		{
			if(image == null)
				return;

			// By default, the image goes in the top left of the rectangle
			Rectangle imageBounds = new Rectangle(r.Location, image.Size);

			// If the image is too tall to be drawn in the space provided, proportionally scale it down.
			// Too wide images are not scaled.
			if(image.Height > r.Height)
			{
				Single scaleRatio = (Single)r.Height / (Single)image.Height;
				imageBounds.Width = (Int32)((Single)image.Width * scaleRatio);
				imageBounds.Height = r.Height - 1;
			}

			// Align and draw our (possibly scaled) image
			Rectangle alignRectangle = this.AlignRectangle(r, imageBounds);
			if(this.ListItem.Enabled)
				g.DrawImage(image, alignRectangle);
			else
				ControlPaint.DrawImageDisabled(g, image, alignRectangle.X, alignRectangle.Y, this.GetBackgroundColor());
		}

		/// <summary>Draw our subitems image and text</summary>
		/// <param name="g">Graphics context to use for drawing</param>
		/// <param name="r">Pre-padded bounds of the cell</param>
		protected virtual void DrawAlignedImageAndText(Graphics g, Rectangle r)
			=> this.DrawImageAndText(g, this.CalculateAlignedRectangle(g, r));

		/// <summary>Fill in the background of this cell</summary>
		/// <param name="g">Graphics context to use for drawing</param>
		/// <param name="r">Bounds of the cell</param>
		protected virtual void DrawBackground(Graphics g, Rectangle r)
		{
			if(!this.IsDrawBackground)
				return;

			Color backgroundColor = this.GetBackgroundColor();

			using(Brush brush = new SolidBrush(backgroundColor))
				g.FillRectangle(brush, r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2);
		}

		/// <summary>Draw the primary check box of this row (checkboxes in other sub items use a different method)</summary>
		/// <param name="g">Graphics context to use for drawing</param>
		/// <param name="r">The pre-aligned and padded target rectangle</param>
		protected virtual Int32 DrawCheckBox(Graphics g, Rectangle r)
		{
			// The odd constants are to match checkbox placement in native mode (on XP at least)
			// TODO: Unify this with CheckStateRenderer

			// The rectangle r is already horizontally aligned. We still need to align it vertically.
			Size checkBoxSize = this.CalculateCheckBoxSize(g);
			Point checkBoxLocation = new Point(r.X, this.AlignVertically(r, checkBoxSize.Height));

			if(this.IsPrinting || this.UseCustomCheckboxImages)
			{
				Int32 imageIndex = this.ListItem.StateImageIndex;
				if(this.ListView.StateImageList == null || imageIndex < 0 || imageIndex >= this.ListView.StateImageList.Images.Count)
					return 0;

				return this.DrawImage(g, new Rectangle(checkBoxLocation, checkBoxSize), this.ListView.StateImageList.Images[imageIndex]) + 4;
			}

			CheckBoxState boxState = this.GetCheckBoxState(this.ListItem.CheckState);
			CheckBoxRenderer.DrawCheckBox(g, checkBoxLocation, boxState);
			return checkBoxSize.Width;
		}

		/// <summary>Calculate the CheckBoxState we need to correctly draw the given state</summary>
		/// <param name="checkState"></param>
		/// <returns></returns>
		protected virtual CheckBoxState GetCheckBoxState(CheckState checkState)
		{
			// Should the checkbox be drawn as disabled?
			if(this.IsCheckBoxDisabled)
			{
				switch(checkState)
				{
				case CheckState.Checked:
					return CheckBoxState.CheckedDisabled;
				case CheckState.Unchecked:
					return CheckBoxState.UncheckedDisabled;
				default:
					return CheckBoxState.MixedDisabled;
				}
			}

			// Is the cursor currently over this checkbox?
			if(this.IsCheckboxHot)
			{
				switch(checkState)
				{
				case CheckState.Checked:
					return CheckBoxState.CheckedHot;
				case CheckState.Unchecked:
					return CheckBoxState.UncheckedHot;
				default:
					return CheckBoxState.MixedHot;
				}
			}

			// Not hot and not disabled -- just draw it normally
			switch(checkState)
			{
			case CheckState.Checked:
				return CheckBoxState.CheckedNormal;
			case CheckState.Unchecked:
				return CheckBoxState.UncheckedNormal;
			default:
				return CheckBoxState.MixedNormal;
			}
		}

		/// <summary>Should this checkbox be drawn as disabled?</summary>
		protected virtual Boolean IsCheckBoxDisabled
		{
			get
			{
				if(this.ListItem != null && !this.ListItem.Enabled)
					return true;

				if(!this.ListView.RenderNonEditableCheckboxesAsDisabled)
					return false;

				return (this.ListView.CellEditActivation == ObjectListView.CellEditActivateMode.None ||
						(this.Column != null && !this.Column.IsEditable));
			}
		}

		/// <summary>Is the current item hot (i.e. under the mouse)?</summary>
		protected Boolean IsCellHot
			=> this.ListView != null &&
			this.ListView.HotRowIndex == this.ListItem.Index &&
			this.ListView.HotColumnIndex == (this.Column == null ? 0 : this.Column.Index);

		/// <summary>Is the mouse over a checkbox in this cell?</summary>
		protected Boolean IsCheckboxHot => this.IsCellHot && this.ListView.HotCellHitLocation == HitTestLocation.CheckBox;

		/// <summary>Draw the given text and optional image in the "normal" fashion</summary>
		/// <param name="g">Graphics context to use for drawing</param>
		/// <param name="r">Bounds of the cell</param>
		/// <param name="imageSelector">The optional image to be drawn</param>
		protected virtual Int32 DrawImage(Graphics g, Rectangle r, Object imageSelector)
		{
			if(imageSelector == null || imageSelector == DBNull.Value)
				return 0;

			// Draw from the image list (most common case)
			ImageList il = this.ImageListOrDefault;
			if(il != null)
			{

				// Try to translate our imageSelector into a valid ImageList index
				Int32 selectorAsInt = -1;
				if(imageSelector is Int32 i)
					selectorAsInt = i >= il.Images.Count ? -1 : i;
				else if(imageSelector is String selectorAsString)
					selectorAsInt = il.Images.IndexOfKey(selectorAsString);

				// If we found a valid index into the ImageList, draw it.
				// We want to draw using the native DrawImageList calls, since that let's us do some nice effects
				// But the native call does not work on PrinterDCs, so if we're printing we have to skip this bit.
				if(selectorAsInt >= 0)
				{
					if(!this.IsPrinting)
					{
						if(il.ImageSize.Height < r.Height)
							r.Y = this.AlignVertically(r, new Rectangle(Point.Empty, il.ImageSize));

						// If we are not printing, it's probable that the given Graphics Object is double buffered using a BufferedGraphics Object.
						// But the ImageList.Draw method doesn't honor the Translation matrix that's probably in effect on the buffered
						// graphics. So we have to calculate our drawing rectangle, relative to the cells natural boundaries.
						// This effectively simulates the Translation matrix.

						Rectangle r2 = new Rectangle(r.X - this.Bounds.X, r.Y - this.Bounds.Y, r.Width, r.Height);
						NativeMethods.DrawImageList(g, il, selectorAsInt, r2.X, r2.Y, this.IsItemSelected, !this.ListItem.Enabled);
						return il.ImageSize.Width;
					}

					// For some reason, printing from an image list doesn't work onto a printer context
					// So get the image from the list and FALL THROUGH to the "print an image" case
					imageSelector = il.Images[selectorAsInt];
				}
			}

			// Is the selector actually an image?
			if(!(imageSelector is Image image))
				return 0; // no, give up

			if(image.Size.Height < r.Height)
				r.Y = this.AlignVertically(r, new Rectangle(Point.Empty, image.Size));

			if(this.ListItem.Enabled)
				g.DrawImageUnscaled(image, r.X, r.Y);
			else
				ControlPaint.DrawImageDisabled(g, image, r.X, r.Y, this.GetBackgroundColor());

			return image.Width;
		}

		/// <summary>Draw our subitems image and text</summary>
		/// <param name="g">Graphics context to use for drawing</param>
		/// <param name="r">Bounds of the cell</param>
		protected virtual void DrawImageAndText(Graphics g, Rectangle r)
		{
			Int32 offset;
			if(this.ListView.CheckBoxes && this.ColumnIsPrimary)
			{
				offset = this.DrawCheckBox(g, r) + 6;
				r.X += offset;
				r.Width -= offset;
			}

			offset = this.DrawImage(g, r, this.GetImageSelector());
			r.X += offset;
			r.Width -= offset;

			this.DrawText(g, r, this.GetText());
		}

		/// <summary>Draw the given collection of image selectors</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="imageSelectors"></param>
		protected virtual Int32 DrawImages(Graphics g, Rectangle r, ICollection imageSelectors)
		{
			// Collect the non-null images
			List<Image> images = new List<Image>();
			foreach(Object selector in imageSelectors)
			{
				Image image = this.GetImage(selector);
				if(image != null)
					images.Add(image);
			}

			// Figure out how much space they will occupy
			Int32 width = 0;
			Int32 height = 0;
			foreach(Image image in images)
			{
				width += (image.Width + this.Spacing);
				height = Math.Max(height, image.Height);
			}

			// Align the collection of images within the cell
			Rectangle r2 = this.AlignRectangle(r, new Rectangle(0, 0, width, height));

			// Finally, draw all the images in their correct location
			Color backgroundColor = this.GetBackgroundColor();
			Point pt = r2.Location;
			foreach(Image image in images)
			{
				if(this.ListItem.Enabled)
					g.DrawImage(image, pt);
				else
					ControlPaint.DrawImageDisabled(g, image, pt.X, pt.Y, backgroundColor);
				pt.X += (image.Width + this.Spacing);
			}

			// Return the width that the images occupy
			return width;
		}

		/// <summary>Draw the given text and optional image in the "normal" fashion</summary>
		/// <param name="g">Graphics context to use for drawing</param>
		/// <param name="r">Bounds of the cell</param>
		/// <param name="txt">The String to be drawn</param>
		public virtual void DrawText(Graphics g, Rectangle r, String txt)
		{
			if(String.IsNullOrEmpty(txt))
				return;

			if(this.UseGdiTextRendering)
				this.DrawTextGdi(g, r, txt);
			else
				this.DrawTextGdiPlus(g, r, txt);
		}

		/// <summary>Print the given text in the given rectangle using only GDI routines</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="txt"></param>
		/// <remarks>
		/// The native list control uses GDI routines to do its drawing, so using them
		/// here makes the owner drawn mode looks more natural.
		/// <para>This method doesn't honour the CanWrap setting on the renderer. All
		/// text is single line</para>
		/// </remarks>
		protected virtual void DrawTextGdi(Graphics g, Rectangle r, String txt)
		{
			Color backColor = Color.Transparent;
			if(this.IsDrawBackground && this.IsItemSelected && this.ColumnIsPrimary && !this.ListView.FullRowSelect)
				backColor = this.GetSelectedBackgroundColor();

			TextFormatFlags flags = NormalTextFormatFlags | this.CellVerticalAlignmentAsTextFormatFlag;

			// I think there is a bug in the TextRenderer. Setting or not setting SingleLine doesn't make 
			// any difference -- it is always single line.
			if(!this.CanWrapOrDefault)
				flags |= TextFormatFlags.SingleLine;
			TextRenderer.DrawText(g, txt, this.Font, r, this.GetForegroundColor(), backColor, flags);
		}

		private Boolean ColumnIsPrimary => this.Column != null && this.Column.Index == 0;

		/// <summary>Gets the cell's vertical alignment as a TextFormatFlag</summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		protected TextFormatFlags CellVerticalAlignmentAsTextFormatFlag
		{
			get
			{
				switch(this.EffectiveCellVerticalAlignment)
				{
				case StringAlignment.Near:
					return TextFormatFlags.Top;
				case StringAlignment.Center:
					return TextFormatFlags.VerticalCenter;
				case StringAlignment.Far:
					return TextFormatFlags.Bottom;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>Gets the StringFormat needed when drawing text using GDI+</summary>
		protected virtual StringFormat StringFormatForGdiPlus
		{
			get
			{
				StringFormat fmt = new StringFormat
				{
					LineAlignment = this.EffectiveCellVerticalAlignment,
					Trimming = StringTrimming.EllipsisCharacter,
					Alignment = this.Column == null ? StringAlignment.Near : this.Column.TextStringAlign
				};
				if(!this.CanWrapOrDefault)
					fmt.FormatFlags = StringFormatFlags.NoWrap;
				return fmt;
			}
		}

		/// <summary>Print the given text in the given rectangle using normal GDI+ .NET methods</summary>
		/// <remarks>Printing to a printer dc has to be done using this method.</remarks>
		protected virtual void DrawTextGdiPlus(Graphics g, Rectangle r, String txt)
		{
			using(StringFormat fmt = this.StringFormatForGdiPlus)
			{
				// Draw the background of the text as selected, if it's the primary column
				// and it's selected and it's not in FullRowSelect mode.
				Font f = this.Font;
				if(this.IsDrawBackground && this.IsItemSelected && this.ColumnIsPrimary && !this.ListView.FullRowSelect)
				{
					SizeF size = g.MeasureString(txt, f, r.Width, fmt);
					Rectangle r2 = r;
					r2.Width = (Int32)size.Width + 1;
					using(Brush brush = new SolidBrush(this.GetSelectedBackgroundColor()))
						g.FillRectangle(brush, r2);
				}
				RectangleF rf = r;
				g.DrawString(txt, f, this.TextBrush, rf, fmt);
			}

			// We should put a focus rectangle around the column 0 text if it's selected --
			// but we don't because:
			// - I really dislike this UI convention
			// - we are using buffered graphics, so the DrawFocusRecatangle method of the event doesn't work

			//if (this.ColumnIsPrimary) {
			//    Size size = TextRenderer.MeasureText(this.SubItem.Text, this.ListView.ListFont);
			//    if (r.Width > size.Width)
			//        r.Width = size.Width;
			//    this.Event.DrawFocusRectangle(r);
			//}
		}

		#endregion
	}

	/// <summary>This renderer highlights substrings that match a given text filter.</summary>
	/// <remarks>
	/// Implementation note:
	/// This renderer uses the functionality of BaseRenderer to draw the text, and
	/// then draws a translucent frame over the top of the already rendered text glyphs.
	/// There's no way to draw the matching text in a different font or color in this
	/// implementation.
	/// </remarks>
	public class HighlightTextRenderer : BaseRenderer, IFilterAwareRenderer
	{
		#region Life and death

		/// <summary>Create a HighlightTextRenderer</summary>
		public HighlightTextRenderer()
		{
			this.FramePen = Pens.DarkGreen;
			this.FillBrush = Brushes.Yellow;
		}

		/// <summary>Create a HighlightTextRenderer</summary>
		/// <param name="filter"></param>
		public HighlightTextRenderer(ITextMatchFilter filter)
			: this()
			=> this.Filter = filter;
		#endregion

		#region Configuration properties

		/// <summary>Gets or set how rounded will be the corners of the text match frame</summary>
		[Category("Appearance")]
		[DefaultValue(3.0f)]
		[Description("How rounded will be the corners of the text match frame?")]
		public Single CornerRoundness { get; set; } = 3.0f;

		/// <summary>Gets or set the brush will be used to paint behind the matched substrings.</summary>
		/// <remarks>Set this to null to not fill the frame.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Brush FillBrush { get; set; }

		/// <summary>Gets or sets the filter that is filtering the ObjectListView and for which this renderer should highlight text</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ITextMatchFilter Filter { get; set; }

		/// <summary>When a filter changes, keep track of the text matching filters</summary>
		IModelFilter IFilterAwareRenderer.Filter
		{
			get => this.Filter;
			set => this.RegisterNewFilter(value);
		}

		internal void RegisterNewFilter(IModelFilter newFilter)
		{
			if(newFilter is TextMatchFilter textFilter)
			{
				this.Filter = textFilter;
				return;
			}

			if(newFilter is CompositeFilter composite)
				foreach(TextMatchFilter textSubFilter in composite.TextFilters)
				{
					this.Filter = textSubFilter;
					return;
				}
			this.Filter = null;
		}

		/// <summary>Gets or set the pen will be used to frame the matched substrings.</summary>
		/// <remarks>Set this to null to not draw a frame.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Pen FramePen { get; set; }

		/// <summary>Gets or sets whether the frame around a text match will have rounded corners</summary>
		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("Will the frame around a text match will have rounded corners?")]
		public Boolean UseRoundedRectangle { get; set; } = true;

		#endregion

		#region IRenderer interface overrides
		/// <summary>Handle a HitTest request after all state information has been initialized</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="item"></param>
		/// <param name="subItemIndex"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize)
			=> this.StandardGetEditRectangle(g, cellBounds, preferredSize);

		#endregion

		#region Rendering

		// This class has two implement two highlighting schemes: one for GDI, another for GDI+.
		// Naturally, GDI+ makes the task easier, but we have to provide something for GDI
		// since that it is what is normally used.

		/// <summary>Draw text using GDI</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="txt"></param>
		protected override void DrawTextGdi(Graphics g, Rectangle r, String txt)
		{
			if(this.ShouldDrawHighlighting)
				this.DrawGdiTextHighlighting(g, r, txt);

			base.DrawTextGdi(g, r, txt);
		}

		/// <summary>Draw the highlighted text using GDI</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="txt"></param>
		protected virtual void DrawGdiTextHighlighting(Graphics g, Rectangle r, String txt)
		{
			// TextRenderer puts horizontal padding around the strings, so we need to take
			// that into account when measuring strings
			const Int32 paddingAdjustment = 6;

			// Cache the font
			Font f = this.Font;

			foreach(CharacterRange range in this.Filter.FindAllMatchedRanges(txt))
			{
				// Measure the text that comes before our substring
				Size precedingTextSize = Size.Empty;
				if(range.First > 0)
				{
					String precedingText = txt.Substring(0, range.First);
					precedingTextSize = TextRenderer.MeasureText(g, precedingText, f, r.Size, NormalTextFormatFlags);
					precedingTextSize.Width -= paddingAdjustment;
				}

				// Measure the length of our substring (may be different each time due to case differences)
				String highlightText = txt.Substring(range.First, range.Length);
				Size textToHighlightSize = TextRenderer.MeasureText(g, highlightText, f, r.Size, NormalTextFormatFlags);
				textToHighlightSize.Width -= paddingAdjustment;

				Single textToHighlightLeft = r.X + precedingTextSize.Width + 1;
				Single textToHighlightTop = this.AlignVertically(r, textToHighlightSize.Height);

				// Draw a filled frame around our substring
				this.DrawSubstringFrame(g, textToHighlightLeft, textToHighlightTop, textToHighlightSize.Width, textToHighlightSize.Height);
			}
		}

		/// <summary>Draw an indication around the given frame that shows a text match</summary>
		/// <param name="g"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		protected virtual void DrawSubstringFrame(Graphics g, Single x, Single y, Single width, Single height)
		{
			if(this.UseRoundedRectangle)
			{
				using(GraphicsPath path = this.GetRoundedRect(x, y, width, height, 3.0f))
				{
					if(this.FillBrush != null)
						g.FillPath(this.FillBrush, path);
					if(this.FramePen != null)
						g.DrawPath(this.FramePen, path);
				}
			} else if(this.FillBrush != null)
				g.FillRectangle(this.FillBrush, x, y, width, height);
			else if(this.FramePen != null)
				g.DrawRectangle(this.FramePen, x, y, width, height);
		}

		/// <summary>Draw the text using GDI+</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="txt"></param>
		protected override void DrawTextGdiPlus(Graphics g, Rectangle r, String txt)
		{
			if(this.ShouldDrawHighlighting)
				this.DrawGdiPlusTextHighlighting(g, r, txt);

			base.DrawTextGdiPlus(g, r, txt);
		}

		/// <summary>Draw the highlighted text using GDI+</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="txt"></param>
		protected virtual void DrawGdiPlusTextHighlighting(Graphics g, Rectangle r, String txt)
		{
			// Find the substrings we want to highlight
			List<CharacterRange> ranges = new List<CharacterRange>(this.Filter.FindAllMatchedRanges(txt));

			if(ranges.Count == 0)
				return;

			using(StringFormat fmt = this.StringFormatForGdiPlus)
			{
				RectangleF rf = r;
				fmt.SetMeasurableCharacterRanges(ranges.ToArray());
				Region[] stringRegions = g.MeasureCharacterRanges(txt, this.Font, rf, fmt);

				foreach(Region region in stringRegions)
				{
					RectangleF bounds = region.GetBounds(g);
					this.DrawSubstringFrame(g, bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height);
				}
			}
		}

		#endregion

		#region Utilities

		/// <summary>Gets whether the renderer should actually draw highlighting</summary>
		protected Boolean ShouldDrawHighlighting => this.Column == null || (this.Column.Searchable && this.Filter != null);

		/// <summary>Return a GraphicPath that is a round cornered rectangle</summary>
		/// <remarks>If I could rely on people using C# 3.0+, this should be an extension method of GraphicsPath.</remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="diameter"></param>
		/// <returns>A round cornered rectangle path</returns>
		protected GraphicsPath GetRoundedRect(Single x, Single y, Single width, Single height, Single diameter)
			=> this.GetRoundedRect(new RectangleF(x, y, width, height), diameter);

		/// <summary>Return a GraphicPath that is a round cornered rectangle</summary>
		/// <param name="rect">The rectangle</param>
		/// <param name="diameter">The diameter of the corners</param>
		/// <returns>A round cornered rectangle path</returns>
		/// <remarks>If I could rely on people using C# 3.0+, this should be
		/// an extension method of GraphicsPath.</remarks>
		protected GraphicsPath GetRoundedRect(RectangleF rect, Single diameter)
		{
			GraphicsPath path = new GraphicsPath();

			if(diameter > 0)
			{
				RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
				path.AddArc(arc, 180, 90);
				arc.X = rect.Right - diameter;
				path.AddArc(arc, 270, 90);
				arc.Y = rect.Bottom - diameter;
				path.AddArc(arc, 0, 90);
				arc.X = rect.Left;
				path.AddArc(arc, 90, 90);
				path.CloseFigure();
			} else
				path.AddRectangle(rect);

			return path;
		}

		#endregion
	}

	/// <summary>This class maps a data value to an image that should be drawn for that value.</summary>
	/// <remarks><para>It is useful for drawing data that is represented as an enum or boolean.</para></remarks>
	public class MappedImageRenderer : BaseRenderer
	{
		/// <summary>Return a renderer that draw boolean values using the given images</summary>
		/// <param name="trueImage">Draw this when our data value is true</param>
		/// <param name="falseImage">Draw this when our data value is false</param>
		/// <returns>A Renderer</returns>
		public static MappedImageRenderer Boolean(Object trueImage, Object falseImage)
			=> new MappedImageRenderer(true, trueImage, false, falseImage);

		/// <summary>Return a renderer that draw tristate boolean values using the given images</summary>
		/// <param name="trueImage">Draw this when our data value is true</param>
		/// <param name="falseImage">Draw this when our data value is false</param>
		/// <param name="nullImage">Draw this when our data value is null</param>
		/// <returns>A Renderer</returns>
		public static MappedImageRenderer TriState(Object trueImage, Object falseImage, Object nullImage)
			=> new MappedImageRenderer(new Object[] { true, trueImage, false, falseImage, null, nullImage });

		/// <summary>Make a new empty renderer</summary>
		public MappedImageRenderer()
		{ }

		/// <summary>
		/// Make a new renderer that will show the given image when the given key is the aspect value
		/// </summary>
		/// <param name="key">The data value to be matched</param>
		/// <param name="image">The image to be shown when the key is matched</param>
		public MappedImageRenderer(Object key, Object image)
			: this()
			=> this.Add(key, image);

		/// <summary>Make a new renderer that will show the given images when it receives the given keys</summary>
		/// <param name="key1"></param>
		/// <param name="image1"></param>
		/// <param name="key2"></param>
		/// <param name="image2"></param>
		public MappedImageRenderer(Object key1, Object image1, Object key2, Object image2)
			: this()
		{
			this.Add(key1, image1);
			this.Add(key2, image2);
		}

		/// <summary>Build a renderer from the given array of keys and their matching images</summary>
		/// <param name="keysAndImages">An array of key/image pairs</param>
		public MappedImageRenderer(Object[] keysAndImages)
			: this()
		{
			if((keysAndImages.GetLength(0) % 2) != 0)
				throw new ArgumentException("Array must have key/image pairs");

			for(Int32 i = 0; i < keysAndImages.GetLength(0); i += 2)
				this.Add(keysAndImages[i], keysAndImages[i + 1]);
		}

		/// <summary>Register the image that should be drawn when our Aspect has the data value.</summary>
		/// <param name="value">Value that the Aspect must match</param>
		/// <param name="image">An ImageSelector -- an int, String or image</param>
		public void Add(Object value, Object image)
		{
			if(value == null)
				this._nullImage = image;
			else
				this._map[value] = image;
		}

		/// <summary>Render our value</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);
			r = this.ApplyCellPadding(r);

			if(this.Aspect is ICollection aspectAsCollection)
				this.RenderCollection(g, r, aspectAsCollection);
			else
				this.RenderOne(g, r, this.Aspect);
		}

		/// <summary>Draw a collection of images</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="imageSelectors"></param>
		protected void RenderCollection(Graphics g, Rectangle r, ICollection imageSelectors)
		{
			ArrayList images = new ArrayList();
			Image image;
			foreach(Object selector in imageSelectors)
			{
				if(selector == null)
					image = this.GetImage(this._nullImage);
				else if(this._map.ContainsKey(selector))
					image = this.GetImage(this._map[selector]);
				else
					image = null;

				if(image != null)
					images.Add(image);
			}

			this.DrawImages(g, r, images);
		}

		/// <summary>Draw one image</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="selector"></param>
		protected void RenderOne(Graphics g, Rectangle r, Object selector)
		{
			Image image = null;
			if(selector == null)
				image = this.GetImage(this._nullImage);
			else if(this._map.ContainsKey(selector))
				image = this.GetImage(this._map[selector]);

			if(image != null)
				this.DrawAlignedImage(g, r, image);
		}

		#region Private variables

		private Hashtable _map = new Hashtable(); // Track the association between values and images
		private Object _nullImage; // image to be drawn for null values (since null can't be a key)

		#endregion
	}

	/// <summary>This renderer draws just a checkbox to match the check state of our model Object.</summary>
	public class CheckStateRenderer : BaseRenderer
	{
		/// <summary>Draw our cell</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);
			if(this.Column == null)
				return;
			r = this.ApplyCellPadding(r);
			CheckState state = this.Column.GetCheckState(this.RowObject);
			if(this.IsPrinting)
			{
				// Renderers don't work onto printer DCs, so we have to draw the image ourselves
				String key = ObjectListView.CHECKED_KEY;
				if(state == CheckState.Unchecked)
					key = ObjectListView.UNCHECKED_KEY;
				if(state == CheckState.Indeterminate)
					key = ObjectListView.INDETERMINATE_KEY;
				this.DrawAlignedImage(g, r, this.ImageListOrDefault.Images[key]);
			} else
			{
				r = this.CalculateCheckBoxBounds(g, r);
				CheckBoxRenderer.DrawCheckBox(g, r.Location, this.GetCheckBoxState(state));
			}
		}

		/// <summary>Handle the GetEditRectangle request</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="item"></param>
		/// <param name="subItemIndex"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize)
			=> this.CalculatePaddedAlignedBounds(g, cellBounds, preferredSize);

		/// <summary>Handle the HitTest request</summary>
		/// <param name="g"></param>
		/// <param name="hti"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, Int32 x, Int32 y)
		{
			Rectangle r = this.CalculateCheckBoxBounds(g, this.Bounds);
			if(r.Contains(x, y))
				hti.HitTestLocation = HitTestLocation.CheckBox;
		}
	}

	/// <summary>Render an image that comes from our data source.</summary>
	/// <remarks>The image can be sourced from:
	/// <list type="bullet">
	/// <item><description>a Byte-array (normally when the image to be shown is
	/// stored as a value in a database)</description></item>
	/// <item><description>an int, which is treated as an index into the image list</description></item>
	/// <item><description>a String, which is treated first as a file name, and failing that as an index into the image list</description></item>
	/// <item><description>an ICollection of ints or strings, which will be drawn as consecutive images</description></item>
	/// </list>
	/// <para>If an image is an animated GIF, it's state is stored in the SubItem Object.</para>
	/// <para>By default, the image renderer does not render animations (it begins life with animations paused).
	/// To enable animations, you must call Unpause().</para>
	/// <para>In the current implementation (2009-09), each column showing animated gifs must have a 
	/// different instance of ImageRenderer assigned to it. You cannot share the same instance of
	/// an image renderer between two animated gif columns. If you do, only the last column will be
	/// animated.</para>
	/// </remarks>
	public class ImageRenderer : BaseRenderer
	{
		/// <summary>Make an empty image renderer</summary>
		public ImageRenderer()
			=> this._stopwatch = new Stopwatch();

		/// <summary>Make an empty image renderer that begins life ready for animations</summary>
		public ImageRenderer(Boolean startAnimations)
			: this()
			=> this.Paused = !startAnimations;

		/// <summary>Finalizer</summary>
		protected override void Dispose(Boolean disposing)
		{
			this.Paused = true;
			base.Dispose(disposing);
		}

		#region Properties

		/// <summary>Should the animations in this renderer be paused?</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Boolean Paused
		{
			get => this._isPaused;
			set
			{
				if(this._isPaused == value)
					return;

				this._isPaused = value;
				if(this._isPaused)
				{
					this.StopTickler();
					this._stopwatch.Stop();
				} else
				{
					this.Tickler.Change(1, Timeout.Infinite);
					this._stopwatch.Start();
				}
			}
		}

		private Boolean _isPaused = true;

		private void StopTickler()
		{
			if(this._tickler == null)
				return;

			this._tickler.Dispose();
			this._tickler = null;
		}

		/// <summary>Gets a timer that can be used to trigger redraws on animations</summary>
		protected Timer Tickler
			=> this._tickler ?? (this._tickler = new Timer(new TimerCallback(this.OnTimer), null, Timeout.Infinite, Timeout.Infinite));
		#endregion

		#region Commands
		/// <summary>Pause any animations</summary>
		public void Pause()
			=> this.Paused = true;

		/// <summary>Unpause any animations</summary>
		public void Unpause()
			=> this.Paused = false;
		#endregion

		#region Drawing
		/// <summary>Draw our image</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);

			if(this.Aspect == null || this.Aspect == DBNull.Value)
				return;
			r = this.ApplyCellPadding(r);

			if(this.Aspect is System.Byte[])
				this.DrawAlignedImage(g, r, this.GetImageFromAspect());
			else
			{
				if(this.Aspect is ICollection imageSelectors)
					this.DrawImages(g, r, imageSelectors);
				else
					this.DrawAlignedImage(g, r, this.GetImageFromAspect());
			}
		}

		/// <summary>Translate our Aspect into an image.</summary>
		/// <remarks>The strategy is:<list type="bullet">
		/// <item><description>If its a Byte array, we treat it as an in-memory image</description></item>
		/// <item><description>If it's an int, we use that as an index into our image list</description></item>
		/// <item><description>If it's a String, we try to load a file by that name. If we can't, 
		/// we use the String as an index into our image list.</description></item>
		///</list></remarks>
		/// <returns>An image</returns>
		protected Image GetImageFromAspect()
		{
			// If we've already figured out the image, don't do it again
			if(this.OLVSubItem?.ImageSelector is Image img)
				return this.OLVSubItem.AnimationState?.image ?? img;

			// Try to convert our Aspect into an Image
			// If its a Byte array, we treat it as an in-memory image
			// If it's an int, we use that as an index into our image list
			// If it's a String, we try to find a file by that name.
			//    If we can't, we use the String as an index into our image list.
			Image image = this.Aspect as Image;
			if(image != null)
			{
				// Don't do anything else
			} else if(this.Aspect is Byte[] b)
			{
				using(MemoryStream stream = new MemoryStream(b))
				{
					try
					{
						image = Image.FromStream(stream);
					} catch(ArgumentException)
					{
						// ignore
					}
				}
			} else if(this.Aspect is Int32)
				image = this.GetImage(this.Aspect);
			else
			{
				String str = this.Aspect as String;
				if(!String.IsNullOrEmpty(str))
				{
					try
					{
						image = Image.FromFile(str);
					} catch(FileNotFoundException)
					{
						image = this.GetImage(this.Aspect);
					} catch(OutOfMemoryException)
					{
						image = this.GetImage(this.Aspect);
					}
				}
			}

			// If this image is an animation, initialize the animation process
			if(this.OLVSubItem != null && AnimationState.IsAnimation(image))
				this.OLVSubItem.AnimationState = new AnimationState(image);

			// Cache the image so we don't repeat this dreary process
			if(this.OLVSubItem != null)
				this.OLVSubItem.ImageSelector = image;

			return image;
		}
		#endregion

		#region Events
		/// <summary>This is the method that is invoked by the timer. It basically switches control to the listview thread.</summary>
		/// <param name="state">not used</param>
		public void OnTimer(Object state)
		{
			if(this.IsListViewDead)
				return;

			if(this.Paused)
				return;

			if(this.ListView.InvokeRequired)
				this.ListView.Invoke((MethodInvoker)delegate { this.OnTimer(state); });
			else
				this.OnTimerInThread();
		}

		private Boolean IsListViewDead// Apply a whole heap of sanity checks, which basically ensure that the ListView is still alive
			=> this.ListView == null ||
				this.ListView.Disposing ||
				this.ListView.IsDisposed ||
				!this.ListView.IsHandleCreated;

		/// <summary>This is the OnTimer callback, but invoked in the same thread as the creator of the ListView.</summary>
		/// <remarks>This method can use all of ListViews methods without creating a CrossThread exception.</remarks>
		protected void OnTimerInThread()
		{
			// MAINTAINER NOTE: This method must renew the tickler. If it doesn't the animations will stop.

			// If this listview has been destroyed, we can't do anything, so we return without
			// renewing the tickler, effectively killing all animations on this renderer

			if(this.IsListViewDead)
				return;

			if(this.Paused)
				return;

			// If we're not in Detail view or our column has been removed from the list,
			// we can't do anything at the moment, but we still renew the tickler because the view may change later.
			if(this.ListView.View != View.Details || this.Column == null || this.Column.Index < 0)
			{
				this.Tickler.Change(1000, Timeout.Infinite);
				return;
			}

			Int64 elapsedMilliseconds = this._stopwatch.ElapsedMilliseconds;
			Int32 subItemIndex = this.Column.Index;
			Int64 nextCheckAt = elapsedMilliseconds + 1000; // wait at most one second before checking again
			Rectangle updateRect = new Rectangle(); // what part of the view must be updated to draw the changed gifs?

			// Run through all the subitems in the view for our column, and for each one that
			// has an animation attached to it, see if the frame needs updating.

			for(Int32 i = 0; i < this.ListView.GetItemCount(); i++)
			{
				OLVListItem lvi = this.ListView.GetItem(i);

				// Get the animation state from the subitem. If there isn't an animation state, skip this row.
				OLVListSubItem lvsi = lvi.GetSubItem(subItemIndex);
				AnimationState state = lvsi.AnimationState;
				if(state == null || !state.IsValid)
					continue;

				// Has this frame of the animation expired?
				if(elapsedMilliseconds >= state.currentFrameExpiresAt)
				{
					state.AdvanceFrame(elapsedMilliseconds);

					// Track the area of the view that needs to be redrawn to show the changed images
					updateRect = updateRect.IsEmpty
						? lvsi.Bounds
						: Rectangle.Union(updateRect, lvsi.Bounds);
				}

				// Remember the minimum time at which a frame is next due to change
				nextCheckAt = Math.Min(nextCheckAt, state.currentFrameExpiresAt);
			}

			// Update the part of the listview where frames have changed
			if(!updateRect.IsEmpty)
				this.ListView.Invalidate(updateRect);

			// Renew the tickler in time for the next frame change
			this.Tickler.Change(nextCheckAt - elapsedMilliseconds, Timeout.Infinite);
		}

		#endregion

		/// <summary>Instances of this class kept track of the animation state of a single image.</summary>
		internal class AnimationState
		{
			private const Int32 PropertyTagTypeShort = 3;
			private const Int32 PropertyTagTypeLong = 4;
			private const Int32 PropertyTagFrameDelay = 0x5100;
			private const Int32 PropertyTagLoopCount = 0x5101;

			/// <summary>Is the given image an animation</summary>
			/// <param name="image">The image to be tested</param>
			/// <returns>Is the image an animation?</returns>
			public static Boolean IsAnimation(Image image)
				=> image != null
					&& Array.Exists(image.FrameDimensionsList, g => g == FrameDimension.Time.Guid);

			/// <summary>Create an AnimationState in a quiet state</summary>
			public AnimationState()
				=> this.imageDuration = new List<Int32>();

			/// <summary>Create an animation state for the given image, which may or may not be an animation</summary>
			/// <param name="image">The image to be rendered</param>
			public AnimationState(Image image)
				: this()
			{
				if(!AnimationState.IsAnimation(image))
					return;

				// How many frames in the animation?
				this.image = image;
				this.frameCount = this.image.GetFrameCount(FrameDimension.Time);

				// Find the delay between each frame.
				// The delays are stored an array of 4-Byte ints. Each int is the
				// number of 1/100th of a second that should elapsed before the frame expires
				foreach(PropertyItem pi in this.image.PropertyItems)
				{
					if(pi.Id == PropertyTagFrameDelay)
					{
						for(Int32 i = 0; i < pi.Len; i += 4)
						{
							//TODO: There must be a better way to convert 4-bytes to an int
							Int32 delay = (pi.Value[i + 3] << 24) + (pi.Value[i + 2] << 16) + (pi.Value[i + 1] << 8) + pi.Value[i];
							this.imageDuration.Add(delay * 10); // store delays as milliseconds
						}
						break;
					}
				}

				// There should be as many frame durations as frames
				Debug.Assert(this.imageDuration.Count == this.frameCount, "There should be as many frame durations as there are frames.");
			}

			/// <summary>Does this state represent a valid animation</summary>
			public Boolean IsValid
			{
				get => this.image != null && this.frameCount > 0;
			}

			/// <summary>Advance our images current frame and calculate when it will expire</summary>
			public void AdvanceFrame(Int64 millisecondsNow)
			{
				this.currentFrame = (this.currentFrame + 1) % this.frameCount;
				this.currentFrameExpiresAt = millisecondsNow + this.imageDuration[this.currentFrame];
				this.image.SelectActiveFrame(FrameDimension.Time, this.currentFrame);
			}

			internal Int32 currentFrame;
			internal Int64 currentFrameExpiresAt;
			internal Image image;
			internal List<Int32> imageDuration;
			internal Int32 frameCount;
		}

		private Timer _tickler; // timer used to tickle the animations
		private Stopwatch _stopwatch; // clock used to time the animation frame changes
	}

	/// <summary>Render our Aspect as a progress bar</summary>
	public class BarRenderer : BaseRenderer
	{
		/// <summary>Make a BarRenderer</summary>
		public BarRenderer()
			: base() { }

		/// <summary>Make a BarRenderer for the given range of data values</summary>
		public BarRenderer(Int32 minimum, Int32 maximum)
			: this()
		{
			this.MinimumValue = minimum;
			this.MaximumValue = maximum;
		}

		/// <summary>Make a BarRenderer using a custom bar scheme</summary>
		public BarRenderer(Pen pen, Brush brush)
			: this()
		{
			this.Pen = pen;
			this.Brush = brush;
			this.UseStandardBar = false;
		}

		/// <summary>Make a BarRenderer using a custom bar scheme</summary>
		public BarRenderer(Int32 minimum, Int32 maximum, Pen pen, Brush brush)
			: this(minimum, maximum)
		{
			this.Pen = pen;
			this.Brush = brush;
			this.UseStandardBar = false;
		}

		/// <summary>Make a BarRenderer that uses a horizontal gradient</summary>
		public BarRenderer(Pen pen, Color start, Color end)
			: this()
		{
			this.Pen = pen;
			this.SetGradient(start, end);
		}

		/// <summary>Make a BarRenderer that uses a horizontal gradient</summary>
		public BarRenderer(Int32 minimum, Int32 maximum, Pen pen, Color start, Color end)
			: this(minimum, maximum)
		{
			this.Pen = pen;
			this.SetGradient(start, end);
		}

		#region Configuration Properties

		/// <summary>Should this bar be drawn in the system style?</summary>
		[Category(Constants.ObjectListView)]
		[Description("Should this bar be drawn in the system style?")]
		[DefaultValue(true)]
		public Boolean UseStandardBar { get; set; } = true;

		/// <summary>How many pixels in from our cell border will this bar be drawn</summary>
		[Category(Constants.ObjectListView)]
		[Description("How many pixels in from our cell border will this bar be drawn")]
		[DefaultValue(2)]
		public Int32 Padding { get; set; } = 2;

		/// <summary>What color will be used to fill the interior of the control before the progress bar is drawn?
		/// </summary>
		[Category(Constants.ObjectListView)]
		[Description("The color of the interior of the bar")]
		[DefaultValue(typeof(Color), "AliceBlue")]
		public Color BackgroundColor { get; set; } = Color.AliceBlue;

		/// <summary>What color should the frame of the progress bar be?</summary>
		[Category(Constants.ObjectListView)]
		[Description("What color should the frame of the progress bar be")]
		[DefaultValue(typeof(Color), "Black")]
		public Color FrameColor { get; set; } = Color.Black;

		/// <summary>How many pixels wide should the frame of the progress bar be?</summary>
		[Category(Constants.ObjectListView)]
		[Description("How many pixels wide should the frame of the progress bar be")]
		[DefaultValue(1.0f)]
		public Single FrameWidth { get; set; } = 1.0f;

		/// <summary>What color should the 'filled in' part of the progress bar be?</summary>
		/// <remarks>This is only used if GradientStartColor is Color.Empty</remarks>
		[Category(Constants.ObjectListView)]
		[Description("What color should the 'filled in' part of the progress bar be")]
		[DefaultValue(typeof(Color), "BlueViolet")]
		public Color FillColor { get; set; } = Color.BlueViolet;

		/// <summary>Use a gradient to fill the progress bar starting with this color</summary>
		[Category(Constants.ObjectListView)]
		[Description("Use a gradient to fill the progress bar starting with this color")]
		[DefaultValue(typeof(Color), "CornflowerBlue")]
		public Color GradientStartColor { get; set; } = Color.CornflowerBlue;

		/// <summary>Use a gradient to fill the progress bar ending with this color</summary>
		[Category(Constants.ObjectListView)]
		[Description("Use a gradient to fill the progress bar ending with this color")]
		[DefaultValue(typeof(Color), "DarkBlue")]
		public Color GradientEndColor { get; set; } = Color.DarkBlue;

		/// <summary>Regardless of how wide the column become the progress bar will never be wider than this</summary>
		[Category("Behavior")]
		[Description("The progress bar will never be wider than this")]
		[DefaultValue(100)]
		public Int32 MaximumWidth { get; set; } = 100;

		/// <summary>Regardless of how high the cell is  the progress bar will never be taller than this</summary>
		[Category("Behavior")]
		[Description("The progress bar will never be taller than this")]
		[DefaultValue(16)]
		public Int32 MaximumHeight { get; set; } = 16;

		/// <summary>The minimum data value expected. Values less than this will given an empty bar</summary>
		[Category("Behavior")]
		[Description("The minimum data value expected. Values less than this will given an empty bar")]
		[DefaultValue(0.0)]
		public Double MinimumValue { get; set; } = 0.0;

		/// <summary>The maximum value for the range. Values greater than this will give a full bar</summary>
		[Category("Behavior")]
		[Description("The maximum value for the range. Values greater than this will give a full bar")]
		[DefaultValue(100.0)]
		public Double MaximumValue { get; set; } = 100.0;
		#endregion

		#region Public Properties (non-IDE)
		/// <summary>The Pen that will draw the frame surrounding this bar</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Pen Pen
		{
			get => this._pen == null && !this.FrameColor.IsEmpty
				? new Pen(this.FrameColor, this.FrameWidth)
				: this._pen;
			set => this._pen = value;
		}

		private Pen _pen;

		/// <summary>The brush that will be used to fill the bar</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Brush Brush
		{
			get => this._brush == null && !this.FillColor.IsEmpty
				? new SolidBrush(this.FillColor)
				: this._brush;
			set => this._brush = value;
		}

		private Brush _brush;

		/// <summary>The brush that will be used to fill the background of the bar</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Brush BackgroundBrush
		{
			get => this._backgroundBrush == null && !this.BackgroundColor.IsEmpty
				? new SolidBrush(this.BackgroundColor)
				: this._backgroundBrush;
			set => this._backgroundBrush = value;
		}

		private Brush _backgroundBrush;

		#endregion

		/// <summary>Draw this progress bar using a gradient</summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public void SetGradient(Color start, Color end)
		{
			this.GradientStartColor = start;
			this.GradientEndColor = end;
		}

		/// <summary>Draw our aspect</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);

			r = this.ApplyCellPadding(r);

			Rectangle frameRect = Rectangle.Inflate(r, 0 - this.Padding, 0 - this.Padding);
			frameRect.Width = Math.Min(frameRect.Width, this.MaximumWidth);
			frameRect.Height = Math.Min(frameRect.Height, this.MaximumHeight);
			frameRect = this.AlignRectangle(r, frameRect);

			// Convert our aspect to a numeric value
			if(!(this.Aspect is IConvertible convertable))
				return;
			Double aspectValue = convertable.ToDouble(NumberFormatInfo.InvariantInfo);

			Rectangle fillRect = Rectangle.Inflate(frameRect, -1, -1);
			if(aspectValue <= this.MinimumValue)
				fillRect.Width = 0;
			else if(aspectValue < this.MaximumValue)
				fillRect.Width = (Int32)(fillRect.Width * (aspectValue - this.MinimumValue) / this.MaximumValue);

			// MS-themed progress bars don't work when printing
			if(this.UseStandardBar && ProgressBarRenderer.IsSupported && !this.IsPrinting)
			{
				ProgressBarRenderer.DrawHorizontalBar(g, frameRect);
				ProgressBarRenderer.DrawHorizontalChunks(g, fillRect);
			} else
			{
				g.FillRectangle(this.BackgroundBrush, frameRect);
				if(fillRect.Width > 0)
				{
					// FillRectangle fills inside the given rectangle, so expand it a little
					fillRect.Width++;
					fillRect.Height++;
					if(this.GradientStartColor == Color.Empty)
						g.FillRectangle(this.Brush, fillRect);
					else
					{
						using(LinearGradientBrush gradient = new LinearGradientBrush(frameRect, this.GradientStartColor, this.GradientEndColor, LinearGradientMode.Horizontal))
							g.FillRectangle(gradient, fillRect);
					}
				}
				g.DrawRectangle(this.Pen, frameRect);
			}
		}

		/// <summary>Handle the GetEditRectangle request</summary>
		/// <param name="g"></param>
		/// <param name="cellBounds"></param>
		/// <param name="item"></param>
		/// <param name="subItemIndex"></param>
		/// <param name="preferredSize"> </param>
		/// <returns></returns>
		protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize)
			=> this.CalculatePaddedAlignedBounds(g, cellBounds, preferredSize);
	}

	/// <summary>An ImagesRenderer draws zero or more images depending on the data returned by its Aspect.</summary>
	/// <remarks><para>This renderer's Aspect must return a ICollection of ints, strings or Images,
	/// each of which will be drawn horizontally one after the other.</para>
	/// <para>As of v2.1, this functionality has been absorbed into ImageRenderer and this is now an empty shell, solely for backwards compatibility.</para>
	/// </remarks>
	[ToolboxItem(false)]
	public class ImagesRenderer : ImageRenderer { }

	/// <summary>A MultiImageRenderer draws the same image a number of times based on our data value</summary>
	/// <remarks><para>The stars in the Rating column of iTunes is a good example of this type of renderer.</para></remarks>
	public class MultiImageRenderer : BaseRenderer
	{
		/// <summary>Make a quiet renderer</summary>
		public MultiImageRenderer()
			: base() { }

		/// <summary>Make an image renderer that will draw the indicated image, at most maxImages times.</summary>
		/// <param name="imageSelector"></param>
		/// <param name="maxImages"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public MultiImageRenderer(Object imageSelector, Int32 maxImages, Int32 minValue, Int32 maxValue)
			: this()
		{
			this.ImageSelector = imageSelector;
			this.MaxNumberImages = maxImages;
			this.MinimumValue = minValue;
			this.MaximumValue = maxValue;
		}

		#region Configuration Properties

		/// <summary>The index of the image that should be drawn</summary>
		[Category("Behavior")]
		[Description("The index of the image that should be drawn")]
		[DefaultValue(-1)]
		public Int32 ImageIndex
		{
			get => this.ImageSelector is Int32 s ? s : -1;
			set => this.ImageSelector = value;
		}

		/// <summary>The name of the image that should be drawn</summary>
		[Category("Behavior")]
		[Description("The index of the image that should be drawn")]
		[DefaultValue(null)]
		public String ImageName
		{
			get => this.ImageSelector as String;
			set => this.ImageSelector = value;
		}

		/// <summary>The image selector that will give the image to be drawn</summary>
		/// <remarks>Like all image selectors, this can be an int, String or Image</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Object ImageSelector { get; set; }

		/// <summary>What is the maximum number of images that this renderer should draw?</summary>
		[Category("Behavior")]
		[Description("The maximum number of images that this renderer should draw")]
		[DefaultValue(10)]
		public Int32 MaxNumberImages { get; set; } = 10;

		/// <summary>Values less than or equal to this will have 0 images drawn</summary>
		[Category("Behavior")]
		[Description("Values less than or equal to this will have 0 images drawn")]
		[DefaultValue(0)]
		public Int32 MinimumValue { get; set; } = 0;

		/// <summary>Values greater than or equal to this will have MaxNumberImages images drawn</summary>
		[Category("Behavior")]
		[Description("Values greater than or equal to this will have MaxNumberImages images drawn")]
		[DefaultValue(100)]
		public Int32 MaximumValue { get; set; } = 100;

		#endregion

		/// <summary>Draw our data value</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);
			r = this.ApplyCellPadding(r);

			Image image = this.GetImage(this.ImageSelector);
			if(image == null)
				return;

			// Convert our aspect to a numeric value
			if(!(this.Aspect is IConvertible convertable))
				return;
			Double aspectValue = convertable.ToDouble(NumberFormatInfo.InvariantInfo);

			// Calculate how many images we need to draw to represent our aspect value
			Int32 numberOfImages;
			if(aspectValue <= this.MinimumValue)
				numberOfImages = 0;
			else if(aspectValue < this.MaximumValue)
				numberOfImages = 1 + (Int32)(this.MaxNumberImages * (aspectValue - this.MinimumValue) / this.MaximumValue);
			else
				numberOfImages = this.MaxNumberImages;

			// If we need to shrink the image, what will its on-screen dimensions be?
			Int32 imageScaledWidth = image.Width;
			Int32 imageScaledHeight = image.Height;
			if(r.Height < image.Height)
			{
				imageScaledWidth = (Int32)((Single)image.Width * (Single)r.Height / (Single)image.Height);
				imageScaledHeight = r.Height;
			}
			// Calculate where the images should be drawn
			Rectangle imageBounds = r;
			imageBounds.Width = (this.MaxNumberImages * (imageScaledWidth + this.Spacing)) - this.Spacing;
			imageBounds.Height = imageScaledHeight;
			imageBounds = this.AlignRectangle(r, imageBounds);

			// Finally, draw the images
			Rectangle singleImageRect = new Rectangle(imageBounds.X, imageBounds.Y, imageScaledWidth, imageScaledHeight);
			Color backgroundColor = this.GetBackgroundColor();
			for(Int32 i = 0; i < numberOfImages; i++)
			{
				if(this.ListItem.Enabled)
					this.DrawImage(g, singleImageRect, this.ImageSelector);
				else
					ControlPaint.DrawImageDisabled(g, image, singleImageRect.X, singleImageRect.Y, backgroundColor);
				singleImageRect.X += (imageScaledWidth + this.Spacing);
			}
		}
	}

	/// <summary>A class to render a value that contains a bitwise-OR'ed collection of values.</summary>
	public class FlagRenderer : BaseRenderer
	{
		/// <summary>Register the given image to the given value</summary>
		/// <param name="key">When this flag is present...</param>
		/// <param name="imageSelector">...draw this image</param>
		public void Add(Object key, Object imageSelector)
		{
			Int32 k2 = ((IConvertible)key).ToInt32(NumberFormatInfo.InvariantInfo);

			this._imageMap[k2] = imageSelector;
			this._keysInOrder.Remove(k2);
			this._keysInOrder.Add(k2);
		}

		/// <summary>Draw the flags</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);

			if(!(this.Aspect is IConvertible convertable))
				return;

			r = this.ApplyCellPadding(r);

			Int32 v2 = convertable.ToInt32(NumberFormatInfo.InvariantInfo);
			ArrayList images = new ArrayList();
			foreach(Int32 key in this._keysInOrder)
				if((v2 & key) == key)
				{
					Image image = this.GetImage(this._imageMap[key]);
					if(image != null)
						images.Add(image);
				}

			if(images.Count > 0)
				this.DrawImages(g, r, images);
		}

		/// <summary>Do the actual work of hit testing. Subclasses should override this rather than HitTest()</summary>
		/// <param name="g"></param>
		/// <param name="hti"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, Int32 x, Int32 y)
		{
			if(!(this.Aspect is IConvertible convertable))
				return;

			Int32 v2 = convertable.ToInt32(NumberFormatInfo.InvariantInfo);

			Point pt = this.Bounds.Location;
			foreach(Int32 key in this._keysInOrder)
			{
				if((v2 & key) == key)
				{
					Image image = this.GetImage(this._imageMap[key]);
					if(image != null)
					{
						Rectangle imageRect = new Rectangle(pt, image.Size);
						if(imageRect.Contains(x, y))
						{
							hti.UserData = key;
							return;
						}
						pt.X += (image.Width + this.Spacing);
					}
				}
			}
		}

		private List<Int32> _keysInOrder = new List<Int32>();
		private Dictionary<Int32, Object> _imageMap = new Dictionary<Int32, Object>();
	}

	/// <summary>This renderer draws an image, a single line title, and then multi-line description under the title.</summary>
	/// <remarks>
	/// <para>This class works best with FullRowSelect = true.</para>
	/// <para>It's not designed to work with cell editing -- it will work but will look odd.</para>
	/// <para>It's not RightToLeft friendly.</para>
	/// </remarks>
	public class DescribedTaskRenderer : BaseRenderer, IFilterAwareRenderer
	{
		private readonly StringFormat _noWrapStringFormat;
		private readonly HighlightTextRenderer _highlightTextRenderer = new HighlightTextRenderer();

		/// <summary>Create a DescribedTaskRenderer</summary>
		public DescribedTaskRenderer()
		{
			this._noWrapStringFormat = new StringFormat(StringFormatFlags.NoWrap)
			{
				Trimming = StringTrimming.EllipsisCharacter,
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Near
			};
			this._highlightTextRenderer.CellVerticalAlignment = StringAlignment.Near;
		}

		#region Configuration properties

		/// <summary>Should text be rendered using GDI routines? This makes the text look more like a native List view control.</summary>
		public override Boolean UseGdiTextRendering
		{
			get => base.UseGdiTextRendering;
			set
			{
				base.UseGdiTextRendering = value;
				this._highlightTextRenderer.UseGdiTextRendering = value;
			}
		}

		/// <summary>Gets or set the font that will be used to draw the title of the task</summary>
		/// <remarks>If this is null, the ListView's font will be used</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The font that will be used to draw the title of the task")]
		[DefaultValue(null)]
		public Font TitleFont { get; set; }

		/// <summary>Return a font that has been set for the title or a reasonable default</summary>
		[Browsable(false)]
		public Font TitleFontOrDefault => this.TitleFont ?? this.ListView.Font;

		/// <summary>Gets or set the color of the title of the task</summary>
		/// <remarks>This color is used when the task is not selected or when the listview
		/// has a translucent selection mechanism.</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The color of the title")]
		[DefaultValue(typeof(Color), "")]
		public Color TitleColor { get; set; }

		/// <summary>Return the color of the title of the task or a reasonable default</summary>
		[Browsable(false)]
		public Color TitleColorOrDefault
		{
			get
			{
				if(!this.ListItem.Enabled)
					return this.SubItem.ForeColor;
				if(this.IsItemSelected || this.TitleColor.IsEmpty)
					return this.GetForegroundColor();

				return this.TitleColor;
			}
		}

		/// <summary>Gets or set the font that will be used to draw the description of the task</summary>
		/// <remarks>If this is null, the ListView's font will be used</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The font that will be used to draw the description of the task")]
		[DefaultValue(null)]
		public Font DescriptionFont { get; set; }

		/// <summary>Return a font that has been set for the title or a reasonable default</summary>
		[Browsable(false)]
		public Font DescriptionFontOrDefault => this.DescriptionFont ?? this.ListView.Font;

		/// <summary>Gets or set the color of the description of the task</summary>
		/// <remarks>This color is used when the task is not selected or when the listview
		/// has a translucent selection mechanism.</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The color of the description")]
		[DefaultValue(typeof(Color), "")]
		public Color DescriptionColor { get; set; } = Color.Empty;

		/// <summary>Return the color of the description of the task or a reasonable default</summary>
		[Browsable(false)]
		public Color DescriptionColorOrDefault
		{
			get
			{
				if(!this.ListItem.Enabled)
					return this.SubItem.ForeColor;
				if(this.IsItemSelected && !this.ListView.UseTranslucentSelection)
					return this.GetForegroundColor();
				return this.DescriptionColor.IsEmpty ? _defaultDescriptionColor : this.DescriptionColor;
			}
		}
		private static Color _defaultDescriptionColor = Color.FromArgb(45, 46, 49);

		/// <summary>Gets or sets the number of pixels that will be left between the image and the text</summary>
		[Category(Constants.ObjectListView)]
		[Description("The number of pixels that will be left between the image and the text")]
		[DefaultValue(4)]
		public Int32 ImageTextSpace { get; set; } = 4;

		/// <summary>Gets or sets the number of pixels that will be left between the title and the description</summary>
		[Category(Constants.ObjectListView)]
		[Description("The number of pixels that that will be left between the title and the description")]
		[DefaultValue(2)]
		public Int32 TitleDescriptionSpace { get; set; } = 2;

		/// <summary>Gets or sets the name of the aspect of the model Object that contains the task description</summary>
		[Category(Constants.ObjectListView)]
		[Description("The name of the aspect of the model Object that contains the task description")]
		[DefaultValue(null)]
		public String DescriptionAspectName { get; set; }

		#endregion

		#region Text highlighting

		/// <summary>Gets or sets the filter that is filtering the ObjectListView and for which this renderer should highlight text</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ITextMatchFilter Filter
		{
			get => this._highlightTextRenderer.Filter;
			set => this._highlightTextRenderer.Filter = value;
		}

		/// <summary>When a filter changes, keep track of the text matching filters</summary>
		IModelFilter IFilterAwareRenderer.Filter
		{
			get => this.Filter;
			set => this._highlightTextRenderer.RegisterNewFilter(value);
		}

		#endregion

		#region Calculating

		/// <summary>Fetch the description from the model class</summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public virtual String GetDescription(Object model)
		{
			if(String.IsNullOrEmpty(this.DescriptionAspectName))
				return String.Empty;

			if(this._descriptionGetter == null)
				this._descriptionGetter = new Munger(this.DescriptionAspectName);

			return this._descriptionGetter.GetValue(model) as String;
		}
		private Munger _descriptionGetter;

		#endregion

		#region Rendering

		/// <summary></summary>
		/// <param name="e"></param>
		/// <param name="cellBounds"></param>
		/// <param name="model"></param>
		public override void ConfigureSubItem(DrawListViewSubItemEventArgs e, Rectangle cellBounds, Object model)
		{
			base.ConfigureSubItem(e, cellBounds, model);
			this._highlightTextRenderer.ConfigureSubItem(e, cellBounds, model);
		}

		/// <summary>Draw our item</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);
			r = this.ApplyCellPadding(r);
			this.DrawDescribedTask(g, r, this.GetText(), this.GetDescription(this.RowObject), this.GetImageSelector());
		}

		/// <summary>Draw the task</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="title"></param>
		/// <param name="description"></param>
		/// <param name="imageSelector"></param>
		protected virtual void DrawDescribedTask(Graphics g, Rectangle r, String title, String description, Object imageSelector)
		{

			//Debug.WriteLine(String.Format("DrawDescribedTask({0}, {1}, {2}, {3})", r, title, description, imageSelector));

			// Draw the image if one's been given
			Rectangle textBounds = r;
			if(imageSelector != null)
			{
				Int32 imageWidth = this.DrawImage(g, r, imageSelector);
				Int32 gapToText = imageWidth + this.ImageTextSpace;
				textBounds.X += gapToText;
				textBounds.Width -= gapToText;
			}

			// Draw the title
			if(!String.IsNullOrEmpty(title))
			{
				using(SolidBrush b = new SolidBrush(this.TitleColorOrDefault))
				{
					this._highlightTextRenderer.CanWrap = false;
					this._highlightTextRenderer.Font = this.TitleFontOrDefault;
					this._highlightTextRenderer.TextBrush = b;
					this._highlightTextRenderer.DrawText(g, textBounds, title);
				}

				// How tall was the title?
				SizeF size = g.MeasureString(title, this.TitleFontOrDefault, textBounds.Width, this._noWrapStringFormat);
				Int32 pixelsToDescription = this.TitleDescriptionSpace + (Int32)size.Height;
				textBounds.Y += pixelsToDescription;
				textBounds.Height -= pixelsToDescription;
			}

			// Draw the description
			if(!String.IsNullOrEmpty(description))
			{
				using(SolidBrush b = new SolidBrush(this.DescriptionColorOrDefault))
				{
					this._highlightTextRenderer.CanWrap = true;
					this._highlightTextRenderer.Font = this.DescriptionFontOrDefault;
					this._highlightTextRenderer.TextBrush = b;
					this._highlightTextRenderer.DrawText(g, textBounds, description);
				}
			}

			//g.DrawRectangle(Pens.OrangeRed, r);
		}

		#endregion

		#region Hit Testing

		/// <summary>Handle the HitTest request</summary>
		/// <param name="g"></param>
		/// <param name="hti"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, Int32 x, Int32 y)
		{
			if(this.Bounds.Contains(x, y))
				hti.HitTestLocation = HitTestLocation.Text;
		}

		#endregion
	}

	/// <summary>This renderer draws a functioning button in its cell</summary>
	public class ColumnButtonRenderer : BaseRenderer
	{

		#region Properties
		/// <summary>Gets or sets how each button will be sized</summary>
		[Category(Constants.ObjectListView)]
		[Description("How each button will be sized")]
		[DefaultValue(OLVColumn.ButtonSizingMode.TextBounds)]
		public OLVColumn.ButtonSizingMode SizingMode { get; set; } = OLVColumn.ButtonSizingMode.TextBounds;

		/// <summary>Gets or sets the size of the button when the SizingMode is FixedBounds</summary>
		/// <remarks>If this is not set, the bounds of the cell will be used</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The size of the button when the SizingMode is FixedBounds")]
		[DefaultValue(null)]
		public Size? ButtonSize { get; set; }

		/// <summary>Gets or sets the extra space that surrounds the cell when the SizingMode is TextBounds</summary>
		[Category(Constants.ObjectListView)]
		[Description("The extra space that surrounds the cell when the SizingMode is TextBounds")]
		public Size? ButtonPadding { get; set; } = new Size(10, 10);

		private Size ButtonPaddingOrDefault => this.ButtonPadding ?? new Size(10, 10);

		/// <summary>
		/// Gets or sets the maximum width that a button can occupy.
		/// -1 means there is no maximum width.
		/// </summary>
		/// <remarks>This is only considered when the SizingMode is TextBounds</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The maximum width that a button can occupy when the SizingMode is TextBounds")]
		[DefaultValue(-1)]
		public Int32 MaxButtonWidth { get; set; } = -1;

		/// <summary>
		/// Gets or sets the minimum width that a button can occupy.
		/// -1 means there is no minimum width.
		/// </summary>
		/// <remarks>This is only considered when the SizingMode is TextBounds</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The minimum width that a button can be when the SizingMode is TextBounds")]
		[DefaultValue(-1)]
		public Int32 MinButtonWidth { get; set; } = -1;

		#endregion

		#region Rendering

		/// <summary>Calculate the size of the contents</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		protected override Size CalculateContentSize(Graphics g, Rectangle r)
		{
			if(this.SizingMode == OLVColumn.ButtonSizingMode.CellBounds)
				return r.Size;

			if(this.SizingMode == OLVColumn.ButtonSizingMode.FixedBounds)
				return this.ButtonSize ?? r.Size;

			// Ok, SizingMode must be TextBounds. So figure out the size of the text
			Size textSize = this.CalculateTextSize(g, this.GetText(), r.Width);

			// Allow for padding and max width
			textSize.Height += this.ButtonPaddingOrDefault.Height * 2;
			textSize.Width += this.ButtonPaddingOrDefault.Width * 2;
			if(this.MaxButtonWidth != -1 && textSize.Width > this.MaxButtonWidth)
				textSize.Width = this.MaxButtonWidth;
			if(textSize.Width < this.MinButtonWidth)
				textSize.Width = this.MinButtonWidth;

			return textSize;
		}

		/// <summary>Draw the button</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		protected override void DrawImageAndText(Graphics g, Rectangle r)
		{
			TextFormatFlags textFormatFlags = TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPadding |
				TextFormatFlags.SingleLine |
				TextFormatFlags.PreserveGraphicsTranslateTransform;
			if(this.ListView.RightToLeftLayout)
				textFormatFlags |= TextFormatFlags.RightToLeft;

			String buttonText = this.GetText();
			if(!String.IsNullOrEmpty(buttonText))
				ButtonRenderer.DrawButton(g, r, buttonText, this.Font, textFormatFlags, false, this.CalculatePushButtonState());
		}

		/// <summary>What part of the control is under the given point?</summary>
		/// <param name="g"></param>
		/// <param name="hti"></param>
		/// <param name="bounds"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		protected override void StandardHitTest(Graphics g, OlvListViewHitTestInfo hti, Rectangle bounds, Int32 x, Int32 y)
		{
			Rectangle r = this.ApplyCellPadding(bounds);
			if(r.Contains(x, y))
				hti.HitTestLocation = HitTestLocation.Button;
		}

		/// <summary>What is the state of the button?</summary>
		/// <returns></returns>
		protected PushButtonState CalculatePushButtonState()
		{
			if(!this.ListItem.Enabled && !this.Column.EnableButtonWhenItemIsDisabled)
				return PushButtonState.Disabled;

			if(this.IsButtonHot)
				return ObjectListView.IsLeftMouseDown ? PushButtonState.Pressed : PushButtonState.Hot;

			return PushButtonState.Normal;
		}

		/// <summary>Is the mouse over the button?</summary>
		protected Boolean IsButtonHot => this.IsCellHot && this.ListView.HotCellHitLocation == HitTestLocation.Button;

		#endregion
	}
}