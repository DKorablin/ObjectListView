﻿/*
 * TreeRenderer - Draw the major column in a TreeListView
 *
 * Author: Phillip Piper
 * Date: 27/06/2015 
 *
 * Change log:
 * 2016-07-17  JPP  - Added TreeRenderer.UseTriangles and IsShowGlyphs
 * 2015-06-27  JPP  - Split out from TreeListView.cs
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace BrightIdeasSoftware
{

	public partial class TreeListView {
		/// <summary>This class handles drawing the tree structure of the primary column.</summary>
		public class TreeRenderer : HighlightTextRenderer {
			/// <summary>Create a TreeRenderer</summary>
			public TreeRenderer()
				=> this.LinePen = new Pen(Color.Blue, 1.0f)
				{
					DashStyle = DashStyle.Dot
				};

			#region Configuration properties

			/// <summary>Should the renderer draw glyphs at the expansion points?</summary>
			/// <remarks>The expansion points will still function to expand/collapse even if this is false.</remarks>
			public Boolean IsShowGlyphs { get; set; }

			/// <summary>Should the renderer draw lines connecting siblings?</summary>
			public Boolean IsShowLines { get; set; }

			/// <summary>Return the pen that will be used to draw the lines between branches</summary>
			public Pen LinePen { get; set; }

			/// <summary>Should the renderer draw triangles as the expansion glyphs?</summary>
			/// <remarks>This looks best with ShowLines = false</remarks>
			public Boolean UseTriangles { get; set; } = false;

			#endregion

			/// <summary>Return the branch that the renderer is currently drawing.</summary>
			private Branch Branch => this.TreeListView.TreeModel.GetBranch(this.RowObject);

			/// <summary>Return the TreeListView for which the renderer is being used.</summary>
			public TreeListView TreeListView => (TreeListView)this.ListView;

			/// <summary>How many pixels will be reserved for each level of indentation?</summary>
			public static Int32 PIXELS_PER_LEVEL = 16 + 1;

			/// <summary>The real work of drawing the tree is done in this method</summary>
			/// <param name="g"></param>
			/// <param name="r"></param>
			public override void Render(Graphics g, Rectangle r) {
				this.DrawBackground(g, r);

				Branch br = this.Branch;

				Rectangle paddedRectangle = this.ApplyCellPadding(r);

				Rectangle expandGlyphRectangle = paddedRectangle;
				expandGlyphRectangle.Offset((br.Level - 1) * PIXELS_PER_LEVEL, 0);
				expandGlyphRectangle.Width = PIXELS_PER_LEVEL;
				expandGlyphRectangle.Height = PIXELS_PER_LEVEL;
				expandGlyphRectangle.Y = this.AlignVertically(paddedRectangle, expandGlyphRectangle);
				Int32 expandGlyphRectangleMidVertical = expandGlyphRectangle.Y + (expandGlyphRectangle.Height/2);

				if (this.IsShowLines)
					this.DrawLines(g, r, this.LinePen, br, expandGlyphRectangleMidVertical);

				if (br.CanExpand && this.IsShowGlyphs) 
					this.DrawExpansionGlyph(g, expandGlyphRectangle, br.IsExpanded);

				Int32 indent = br.Level * PIXELS_PER_LEVEL;
				paddedRectangle.Offset(indent, 0);
				paddedRectangle.Width -= indent;

				this.DrawImageAndText(g, paddedRectangle);
			}

			/// <summary>Draw the expansion indicator</summary>
			/// <param name="g"></param>
			/// <param name="r"></param>
			/// <param name="isExpanded"></param>
			protected virtual void DrawExpansionGlyph(Graphics g, Rectangle r, Boolean isExpanded)
			{
				if(this.UseStyles)
					this.DrawExpansionGlyphStyled(g, r, isExpanded);
				else
					this.DrawExpansionGlyphManual(g, r, isExpanded);
			}

			/// <summary>Gets whether or not we should render using styles</summary>
			protected virtual Boolean UseStyles => !this.IsPrinting && Application.RenderWithVisualStyles;

			/// <summary>Draw the expansion indicator using styles</summary>
			/// <param name="g"></param>
			/// <param name="r"></param>
			/// <param name="isExpanded"></param>
			protected virtual void DrawExpansionGlyphStyled(Graphics g, Rectangle r, Boolean isExpanded) {
				if (this.UseTriangles && this.IsShowLines) {
					using (SolidBrush b = new SolidBrush(GetBackgroundColor())) {
						Rectangle r2 = r;
						r2.Inflate(-2, -2);
						g.FillRectangle(b, r2);
					}
				}

				VisualStyleRenderer renderer = new VisualStyleRenderer(DecideVisualElement(isExpanded));
				renderer.DrawBackground(g, r);
			}

			private VisualStyleElement DecideVisualElement(Boolean isExpanded) {
				String klass = this.UseTriangles ? "Explorer::TreeView" : "TREEVIEW";
				Int32 part = this.UseTriangles && this.IsExpansionHot ? 4 : 2;
				Int32 state = isExpanded ? 2 : 1;
				return VisualStyleElement.CreateElement(klass, part, state);
			}

			/// <summary>Is the mouse over a checkbox in this cell?</summary>
			protected Boolean IsExpansionHot => this.IsCellHot && this.ListView.HotCellHitLocation == HitTestLocation.ExpandButton;

			/// <summary>Draw the expansion indicator without using styles</summary>
			/// <param name="g"></param>
			/// <param name="r"></param>
			/// <param name="isExpanded"></param>
			protected virtual void DrawExpansionGlyphManual(Graphics g, Rectangle r, Boolean isExpanded)
			{
				Int32 h = 8;
				Int32 w = 8;
				Int32 x = r.X + 4;
				Int32 y = r.Y + (r.Height / 2) - 4;

				g.DrawRectangle(new Pen(SystemBrushes.ControlDark), x, y, w, h);
				g.FillRectangle(Brushes.White, x + 1, y + 1, w - 1, h - 1);
				g.DrawLine(Pens.Black, x + 2, y + 4, x + w - 2, y + 4);

				if(!isExpanded)
					g.DrawLine(Pens.Black, x + 4, y + 2, x + 4, y + h - 2);
			}

			/// <summary>Draw the lines of the tree</summary>
			/// <param name="g"></param>
			/// <param name="r"></param>
			/// <param name="p"></param>
			/// <param name="br"></param>
			/// <param name="glyphMidVertical"> </param>
			protected virtual void DrawLines(Graphics g, Rectangle r, Pen p, Branch br, Int32 glyphMidVertical) {
				Rectangle r2 = r;
				r2.Width = PIXELS_PER_LEVEL;

				// Vertical lines have to start on even points, otherwise the dotted line looks wrong.
				// This is only needed if pen is dotted.
				Int32 top = r2.Top;
				//if (p.DashStyle == DashStyle.Dot && (top & 1) == 0)
				//    top += 1;

				// Draw lines for ancestors
				Int32 midX;
				IList<Branch> ancestors = br.Ancestors;
				foreach (Branch ancestor in ancestors) {
					if (!ancestor.IsLastChild && !ancestor.IsOnlyBranch) {
						midX = r2.Left + r2.Width / 2;
						g.DrawLine(p, midX, top, midX, r2.Bottom);
					}
					r2.Offset(PIXELS_PER_LEVEL, 0);
				}

				// Draw lines for this branch
				midX = r2.Left + r2.Width / 2;

				// Horizontal line first
				g.DrawLine(p, midX, glyphMidVertical, r2.Right, glyphMidVertical);

				// Vertical line second
				if (br.IsFirstBranch) {
					if (!br.IsLastChild && !br.IsOnlyBranch)
						g.DrawLine(p, midX, glyphMidVertical, midX, r2.Bottom);
				} else {
					if (br.IsLastChild)
						g.DrawLine(p, midX, top, midX, glyphMidVertical);
					else
						g.DrawLine(p, midX, top, midX, r2.Bottom);
				}
			}

			/// <summary>Do the hit test</summary>
			/// <param name="g"></param>
			/// <param name="hti"></param>
			/// <param name="x"></param>
			/// <param name="y"></param>
			protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, Int32 x, Int32 y)
			{
				Branch br = this.Branch;

				Rectangle r = this.ApplyCellPadding(this.Bounds);
				if(br.CanExpand)
				{
					r.Offset((br.Level - 1) * PIXELS_PER_LEVEL, 0);
					r.Width = PIXELS_PER_LEVEL;
					if(r.Contains(x, y))
					{
						hti.HitTestLocation = HitTestLocation.ExpandButton;
						return;
					}
				}

				r = this.Bounds;
				Int32 indent = br.Level * PIXELS_PER_LEVEL;
				r.X += indent;
				r.Width -= indent;

				// Ignore events in the indent zone
				if(x < r.Left)
					hti.HitTestLocation = HitTestLocation.Nothing;
				else
					this.StandardHitTest(g, hti, r, x, y);
			}

			/// <summary>Calculate the edit rect</summary>
			/// <param name="g"></param>
			/// <param name="cellBounds"></param>
			/// <param name="item"></param>
			/// <param name="subItemIndex"></param>
			/// <param name="preferredSize"> </param>
			/// <returns></returns>
			protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, Int32 subItemIndex, Size preferredSize) {
				return this.StandardGetEditRectangle(g, cellBounds, preferredSize);
			}
		}
	}
}