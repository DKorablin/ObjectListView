﻿/*
 * Adornments - Adornments are the basis for overlays and decorations -- things that can be rendered over the top of a ListView
 *
 * Author: Phillip Piper
 * Date: 16/08/2009 1:02 AM
 *
 * Change log:
 * v2.6
 * 2012-08-18   JPP  - Correctly dispose of brush and pen resources
 * v2.3
 * 2009-09-22   JPP  - Added Wrap property to TextAdornment, to allow text wrapping to be disabled
 *                   - Added ShrinkToWidth property to ImageAdornment
 * 2009-08-17   JPP  - Initial version
 *
 * To do:
 * - Use IPointLocator rather than Corners
 * - Add RotationCenter property rather than always using middle center
 * 
 * Copyright (C) 2009-2014 Phillip Piper
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BrightIdeasSoftware
{
	/// <summary>An adornment is the common base for overlays and decorations.</summary>
	public class GraphicAdornment
	{
		#region Public properties

		/// <summary>Gets or sets the corner of the adornment that will be positioned at the reference corner</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ContentAlignment AdornmentCorner { get; set; } = ContentAlignment.MiddleCenter;

		/// <summary>Gets or sets location within the reference rectangle where the adornment will be drawn</summary>
		/// <remarks>This is a simplified interface to ReferenceCorner and AdornmentCorner </remarks>
		[Category(Constants.ObjectListView)]
		[Description("How will the adornment be aligned")]
		[DefaultValue(ContentAlignment.BottomRight)]
		[NotifyParentProperty(true)]
		public ContentAlignment Alignment
		{
			get => this._alignment;
			set
			{
				this._alignment = value;
				this.ReferenceCorner = value;
				this.AdornmentCorner = value;
			}
		}
		private ContentAlignment _alignment = ContentAlignment.BottomRight;

		/// <summary>Gets or sets the offset by which the position of the adornment will be adjusted</summary>
		[Category(Constants.ObjectListView)]
		[Description("The offset by which the position of the adornment will be adjusted")]
		[DefaultValue(typeof(Size), "0,0")]
		public Size Offset { get; set; } = new Size();

		/// <summary>Gets or sets the point of the reference rectangle to which the adornment will be aligned.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ContentAlignment ReferenceCorner { get; set; } = ContentAlignment.MiddleCenter;

		/// <summary>
		/// Gets or sets the degree of rotation by which the adornment will be transformed.
		/// The centre of rotation will be the center point of the adornment.
		/// </summary>
		[Category(Constants.ObjectListView)]
		[Description("The degree of rotation that will be applied to the adornment.")]
		[DefaultValue(0)]
		[NotifyParentProperty(true)]
		public Int32 Rotation { get; set; }

		/// <summary>Gets or sets the transparency of the overlay.</summary>
		/// <remarks>0 is completely transparent, 255 is completely opaque.</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The transparency of this adornment. 0 is completely transparent, 255 is completely opaque.")]
		[DefaultValue(128)]
		public Int32 Transparency
		{
			get => this._transparency;
			set => this._transparency = Math.Min(255, Math.Max(0, value));
		}
		private Int32 _transparency = 128;

		#endregion

		#region Calculations

		/// <summary>Calculate the location of rectangle of the given size, so that it's indicated corner would be at the given point.</summary>
		/// <param name="pt">The point</param>
		/// <param name="size"></param>
		/// <param name="corner">Which corner will be positioned at the reference point</param>
		/// <returns></returns>
		/// <example>CalculateAlignedPosition(new Point(50, 100), new Size(10, 20), System.Drawing.ContentAlignment.TopLeft) -> Point(50, 100)</example>
		/// <example>CalculateAlignedPosition(new Point(50, 100), new Size(10, 20), System.Drawing.ContentAlignment.MiddleCenter) -> Point(45, 90)</example>
		/// <example>CalculateAlignedPosition(new Point(50, 100), new Size(10, 20), System.Drawing.ContentAlignment.BottomRight) -> Point(40, 80)</example>
		public virtual Point CalculateAlignedPosition(Point pt, Size size, ContentAlignment corner)
		{
			switch(corner)
			{
			case ContentAlignment.TopLeft:
				return pt;
			case ContentAlignment.TopCenter:
				return new Point(pt.X - (size.Width / 2), pt.Y);
			case ContentAlignment.TopRight:
				return new Point(pt.X - size.Width, pt.Y);
			case ContentAlignment.MiddleLeft:
				return new Point(pt.X, pt.Y - (size.Height / 2));
			case ContentAlignment.MiddleCenter:
				return new Point(pt.X - (size.Width / 2), pt.Y - (size.Height / 2));
			case ContentAlignment.MiddleRight:
				return new Point(pt.X - size.Width, pt.Y - (size.Height / 2));
			case ContentAlignment.BottomLeft:
				return new Point(pt.X, pt.Y - size.Height);
			case ContentAlignment.BottomCenter:
				return new Point(pt.X - (size.Width / 2), pt.Y - size.Height);
			case ContentAlignment.BottomRight:
				return new Point(pt.X - size.Width, pt.Y - size.Height);
			}

			// Should never reach here
			return pt;
		}

		/// <summary>Calculate a rectangle that has the given size which is positioned so that its alignment point is at the reference location of the given rect.</summary>
		/// <param name="r"></param>
		/// <param name="sz"></param>
		/// <returns></returns>
		public virtual Rectangle CreateAlignedRectangle(Rectangle r, Size sz)
			=> this.CreateAlignedRectangle(r, sz, this.ReferenceCorner, this.AdornmentCorner, this.Offset);

		/// <summary>Create a rectangle of the given size which is positioned so that its indicated corner is at the indicated corner of the reference rect.</summary>
		/// <param name="r"></param>
		/// <param name="sz"></param>
		/// <param name="corner"></param>
		/// <param name="referenceCorner"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <remarks>
		/// <para>Creates a rectangle so that its bottom left is at the centre of the reference:
		/// corner=BottomLeft, referenceCorner=MiddleCenter</para>
		/// <para>This is a powerful concept that takes some getting used to, but is
		/// very neat once you understand it.</para>
		/// </remarks>
		public virtual Rectangle CreateAlignedRectangle(Rectangle r, Size sz,
			ContentAlignment corner, ContentAlignment referenceCorner, Size offset)
		{
			Point referencePt = this.CalculateCorner(r, referenceCorner);
			Point topLeft = this.CalculateAlignedPosition(referencePt, sz, corner);
			return new Rectangle(topLeft + offset, sz);
		}

		/// <summary>Return the point at the indicated corner of the given rectangle (it doesn't have to be a corner, but a named location)</summary>
		/// <param name="r">The reference rectangle</param>
		/// <param name="corner">Which point of the rectangle should be returned?</param>
		/// <returns>A point</returns>
		/// <example>CalculateReferenceLocation(new Rectangle(0, 0, 50, 100), System.Drawing.ContentAlignment.TopLeft) -> Point(0, 0)</example>
		/// <example>CalculateReferenceLocation(new Rectangle(0, 0, 50, 100), System.Drawing.ContentAlignment.MiddleCenter) -> Point(25, 50)</example>
		/// <example>CalculateReferenceLocation(new Rectangle(0, 0, 50, 100), System.Drawing.ContentAlignment.BottomRight) -> Point(50, 100)</example>
		public virtual Point CalculateCorner(Rectangle r, ContentAlignment corner)
		{
			switch(corner)
			{
			case ContentAlignment.TopLeft:
				return new Point(r.Left, r.Top);
			case ContentAlignment.TopCenter:
				return new Point(r.X + (r.Width / 2), r.Top);
			case ContentAlignment.TopRight:
				return new Point(r.Right, r.Top);
			case ContentAlignment.MiddleLeft:
				return new Point(r.Left, r.Top + (r.Height / 2));
			case ContentAlignment.MiddleCenter:
				return new Point(r.X + (r.Width / 2), r.Top + (r.Height / 2));
			case ContentAlignment.MiddleRight:
				return new Point(r.Right, r.Top + (r.Height / 2));
			case ContentAlignment.BottomLeft:
				return new Point(r.Left, r.Bottom);
			case ContentAlignment.BottomCenter:
				return new Point(r.X + (r.Width / 2), r.Bottom);
			case ContentAlignment.BottomRight:
				return new Point(r.Right, r.Bottom);
			}

			// Should never reach here
			return r.Location;
		}

		/// <summary>Given the item and the subitem, calculate its bounds.</summary>
		/// <param name="item"></param>
		/// <param name="subItem"></param>
		/// <returns></returns>
		public virtual Rectangle CalculateItemBounds(OLVListItem item, OLVListSubItem subItem)
		{
			if(item == null)
				return Rectangle.Empty;

			return subItem == null
				? item.Bounds
				: item.GetSubItemBounds(item.SubItems.IndexOf(subItem));
		}

		#endregion

		#region Commands

		/// <summary>Apply any specified rotation to the Graphic content.</summary>
		/// <param name="g">The Graphics to be transformed</param>
		/// <param name="r">The rotation will be around the centre of this rect</param>
		protected virtual void ApplyRotation(Graphics g, Rectangle r)
		{
			if(this.Rotation == 0)
				return;

			// THINK: Do we want to reset the transform? I think we want to push a new transform
			g.ResetTransform();
			Matrix m = new Matrix();
			m.RotateAt(this.Rotation, new Point(r.Left + r.Width / 2, r.Top + r.Height / 2));
			g.Transform = m;
		}

		/// <summary>Reverse the rotation created by ApplyRotation()</summary>
		/// <param name="g"></param>
		protected virtual void UnapplyRotation(Graphics g)
		{
			if(this.Rotation != 0)
				g.ResetTransform();
		}

		#endregion
	}

	/// <summary>An overlay that will draw an image over the top of the ObjectListView</summary>
	public class ImageAdornment : GraphicAdornment
	{
		#region Public properties

		/// <summary>Gets or sets the image that will be drawn</summary>
		[Category(Constants.ObjectListView)]
		[Description("The image that will be drawn")]
		[DefaultValue(null)]
		[NotifyParentProperty(true)]
		public Image Image { get; set; }

		/// <summary>Gets or sets if the image will be shrunk to fit with its horizontal bounds</summary>
		[Category(Constants.ObjectListView)]
		[Description("Will the image be shrunk to fit within its width?")]
		[DefaultValue(false)]
		public Boolean ShrinkToWidth { get; set; }

		#endregion

		#region Commands

		/// <summary>Draw the image in its specified location</summary>
		/// <param name="g">The Graphics used for drawing</param>
		/// <param name="r">The bounds of the rendering</param>
		public virtual void DrawImage(Graphics g, Rectangle r)
		{
			if(this.ShrinkToWidth)
				this.DrawScaledImage(g, r, this.Image, this.Transparency);
			else
				this.DrawImage(g, r, this.Image, this.Transparency);
		}

		/// <summary>Draw the image in its specified location</summary>
		/// <param name="image">The image to be drawn</param>
		/// <param name="g">The Graphics used for drawing</param>
		/// <param name="r">The bounds of the rendering</param>
		/// <param name="transparency">How transparent should the image be (0 is completely transparent, 255 is opaque)</param>
		public virtual void DrawImage(Graphics g, Rectangle r, Image image, Int32 transparency)
		{
			if(image != null)
				this.DrawImage(g, r, image, image.Size, transparency);
		}

		/// <summary>Draw the image in its specified location</summary>
		/// <param name="image">The image to be drawn</param>
		/// <param name="g">The Graphics used for drawing</param>
		/// <param name="r">The bounds of the rendering</param>
		/// <param name="sz">How big should the image be?</param>
		/// <param name="transparency">How transparent should the image be (0 is completely transparent, 255 is opaque)</param>
		public virtual void DrawImage(Graphics g, Rectangle r, Image image, Size sz, Int32 transparency)
		{
			if(image == null)
				return;

			Rectangle adornmentBounds = this.CreateAlignedRectangle(r, sz);
			try
			{
				this.ApplyRotation(g, adornmentBounds);
				this.DrawTransparentBitmap(g, adornmentBounds, image, transparency);
			} finally
			{
				this.UnapplyRotation(g);
			}
		}

		/// <summary>Draw the image in its specified location, scaled so that it is not wider than the given rectangle.</summary>
		/// <remarks>Height is scaled proportional to the width.</remarks>
		/// <param name="image">The image to be drawn</param>
		/// <param name="g">The Graphics used for drawing</param>
		/// <param name="r">The bounds of the rendering</param>
		/// <param name="transparency">How transparent should the image be (0 is completely transparent, 255 is opaque)</param>
		public virtual void DrawScaledImage(Graphics g, Rectangle r, Image image, Int32 transparency)
		{
			if(image == null)
				return;

			// If the image is too wide to be drawn in the space provided, proportionally scale it down.
			// Too tall images are not scaled.
			Size size = image.Size;
			if(image.Width > r.Width)
			{
				Single scaleRatio = (Single)r.Width / (Single)image.Width;
				size.Height = (Int32)((Single)image.Height * scaleRatio);
				size.Width = r.Width - 1;
			}

			this.DrawImage(g, r, image, size, transparency);
		}

		/// <summary>Utility to draw a bitmap transparently.</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="image"></param>
		/// <param name="transparency"></param>
		protected virtual void DrawTransparentBitmap(Graphics g, Rectangle r, Image image, Int32 transparency)
		{
			ImageAttributes imageAttributes = null;
			if(transparency != 255)
			{
				imageAttributes = new ImageAttributes();
				Single a = (Single)transparency / 255.0f;
				Single[][] colorMatrixElements = {
					new Single[] {1,  0,  0,  0, 0},
					new Single[] {0,  1,  0,  0, 0},
					new Single[] {0,  0,  1,  0, 0},
					new Single[] {0,  0,  0,  a, 0},
					new Single[] {0,  0,  0,  0, 1}};

				imageAttributes.SetColorMatrix(new ColorMatrix(colorMatrixElements));
			}

			g.DrawImage(image,
				r,                                          // destination rectangle
				0, 0, image.Size.Width, image.Size.Height,  // source rectangle
				GraphicsUnit.Pixel,
				imageAttributes);
		}

		#endregion
	}

	/// <summary>An adornment that will draw text</summary>
	public class TextAdornment : GraphicAdornment
	{
		private Int32 _workingTransparency;
		#region Public properties

		/// <summary>Gets or sets the background color of the text</summary>
		/// <remarks>Set this to Color.Empty to not draw a background</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The background color of the text")]
		[DefaultValue(typeof(Color), "")]
		public Color BackColor { get; set; } = Color.Empty;

		/// <summary>Gets the brush that will be used to paint the text</summary>
		[Browsable(false)]
		public Brush BackgroundBrush => new SolidBrush(Color.FromArgb(this._workingTransparency, this.BackColor));

		/// <summary>Gets or sets the color of the border around the billboard.</summary>
		/// <remarks>Set this to Color.Empty to remove the border</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The color of the border around the text")]
		[DefaultValue(typeof(Color), "")]
		public Color BorderColor { get; set; } = Color.Empty;

		/// <summary>Gets the brush that will be used to paint the text</summary>
		[Browsable(false)]
		public Pen BorderPen => new Pen(Color.FromArgb(this._workingTransparency, this.BorderColor), this.BorderWidth);

		/// <summary>Gets or sets the width of the border around the text</summary>
		[Category(Constants.ObjectListView)]
		[Description("The width of the border around the text")]
		[DefaultValue(0.0f)]
		public Single BorderWidth { get; set; }

		/// <summary>How rounded should the corners of the border be? 0 means no rounding.</summary>
		/// <remarks>If this value is too large, the edges of the border will appear odd.</remarks>
		[Category(Constants.ObjectListView)]
		[Description("How rounded should the corners of the border be? 0 means no rounding.")]
		[DefaultValue(16.0f)]
		[NotifyParentProperty(true)]
		public Single CornerRounding { get; set; } = 16.0f;

		/// <summary>Gets or sets the font that will be used to draw the text</summary>
		[Category(Constants.ObjectListView)]
		[Description("The font that will be used to draw the text")]
		[DefaultValue(null)]
		[NotifyParentProperty(true)]
		public Font Font { get; set; }

		/// <summary>Gets the font that will be used to draw the text or a reasonable default</summary>
		[Browsable(false)]
		public Font FontOrDefault => this.Font ?? new Font("Tahoma", 16);

		/// <summary>Does this text have a background?</summary>
		[Browsable(false)]
		public Boolean HasBackground => this.BackColor != Color.Empty;

		/// <summary>Does this overlay have a border?</summary>
		[Browsable(false)]
		public Boolean HasBorder => this.BorderColor != Color.Empty && this.BorderWidth > 0;

		/// <summary>Gets or sets the maximum width of the text. Text longer than this will wrap.</summary>
		/// <remarks>0 means no maximum.</remarks>
		[Category(Constants.ObjectListView)]
		[Description("The maximum width the text (0 means no maximum). Text longer than this will wrap")]
		[DefaultValue(0)]
		public Int32 MaximumTextWidth { get; set; }

		/// <summary>Gets or sets the formatting that should be used on the text</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual StringFormat StringFormat
		{
			get
			{
				if(this._stringFormat == null)
				{
					this._stringFormat = new StringFormat
					{
						Alignment = StringAlignment.Center,
						LineAlignment = StringAlignment.Center,
						Trimming = StringTrimming.EllipsisCharacter
					};
					if(!this.Wrap)
						this._stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				}
				return this._stringFormat;
			}
			set { this._stringFormat = value; }
		}
		private StringFormat _stringFormat;

		/// <summary>Gets or sets the text that will be drawn</summary>
		[Category(Constants.ObjectListView)]
		[Description("The text that will be drawn over the top of the ListView")]
		[DefaultValue(null)]
		[NotifyParentProperty(true)]
		[Localizable(true)]
		public String Text { get; set; }

		/// <summary>Gets the brush that will be used to paint the text</summary>
		[Browsable(false)]
		public Brush TextBrush => new SolidBrush(Color.FromArgb(this._workingTransparency, this.TextColor));

		/// <summary>Gets or sets the color of the text</summary>
		[Category(Constants.ObjectListView)]
		[Description("The color of the text")]
		[DefaultValue(typeof(Color), "DarkBlue")]
		[NotifyParentProperty(true)]
		public Color TextColor { get; set; } = Color.DarkBlue;

		/// <summary>Gets or sets whether the text will wrap when it exceeds its bounds</summary>
		[Category(Constants.ObjectListView)]
		[Description("Will the text wrap?")]
		[DefaultValue(true)]
		public Boolean Wrap { get; set; } = true;

		#endregion

		#region Implementation

		/// <summary>Draw our text with our stored configuration in relation to the given reference rectangle</summary>
		/// <param name="g">The Graphics used for drawing</param>
		/// <param name="r">The reference rectangle in relation to which the text will be drawn</param>
		public virtual void DrawText(Graphics g, Rectangle r)
			=> this.DrawText(g, r, this.Text, this.Transparency);

		/// <summary>Draw the given text with our stored configuration</summary>
		/// <param name="g">The Graphics used for drawing</param>
		/// <param name="r">The reference rectangle in relation to which the text will be drawn</param>
		/// <param name="s">The text to draw</param>
		/// <param name="transparency">How opaque should be text be</param>
		public virtual void DrawText(Graphics g, Rectangle r, String s, Int32 transparency)
		{
			if(String.IsNullOrEmpty(s))
				return;

			Rectangle textRect = this.CalculateTextBounds(g, r, s);
			this.DrawBorderedText(g, textRect, s, transparency);
		}

		/// <summary>Draw the text with a border</summary>
		/// <param name="g">The Graphics used for drawing</param>
		/// <param name="textRect">The bounds within which the text should be drawn</param>
		/// <param name="text">The text to draw</param>
		/// <param name="transparency">How opaque should be text be</param>
		protected virtual void DrawBorderedText(Graphics g, Rectangle textRect, String text, Int32 transparency)
		{
			Rectangle borderRect = textRect;
			borderRect.Inflate((Int32)this.BorderWidth / 2, (Int32)this.BorderWidth / 2);
			borderRect.Y -= 1; // Looker better a little higher

			try
			{
				this.ApplyRotation(g, textRect);
				using(GraphicsPath path = this.GetRoundedRect(borderRect, this.CornerRounding))
				{
					this._workingTransparency = transparency;
					if(this.HasBackground)
					{
						using(Brush b = this.BackgroundBrush)
							g.FillPath(b, path);
					}

					using(Brush b = this.TextBrush)
						g.DrawString(text, this.FontOrDefault, b, textRect, this.StringFormat);

					if(this.HasBorder)
					{
						using(Pen p = this.BorderPen)
							g.DrawPath(p, path);
					}
				}
			} finally
			{
				this.UnapplyRotation(g);
			}
		}

		/// <summary>Return the rectangle that will be the precise bounds of the displayed text</summary>
		/// <param name="g"></param>
		/// <param name="r"></param>
		/// <param name="s"></param>
		/// <returns>The bounds of the text</returns>
		protected virtual Rectangle CalculateTextBounds(Graphics g, Rectangle r, String s)
		{
			Int32 maxWidth = this.MaximumTextWidth <= 0 ? r.Width : this.MaximumTextWidth;
			SizeF sizeF = g.MeasureString(s, this.FontOrDefault, maxWidth, this.StringFormat);
			Size size = new Size(1 + (Int32)sizeF.Width, 1 + (Int32)sizeF.Height);
			return this.CreateAlignedRectangle(r, size);
		}

		/// <summary>Return a GraphicPath that is a round cornered rectangle</summary>
		/// <param name="rect">The rectangle</param>
		/// <param name="diameter">The diameter of the corners</param>
		/// <returns>A round cornered rectangle path</returns>
		/// <remarks>If I could rely on people using C# 3.0+, this should be
		/// an extension method of GraphicsPath.</remarks>
		protected virtual GraphicsPath GetRoundedRect(Rectangle rect, Single diameter)
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
			{
				path.AddRectangle(rect);
			}

			return path;
		}

		#endregion
	}
}