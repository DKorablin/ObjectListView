using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ObjectListViewDemo.Models;

namespace ObjectListViewDemo
{
	/// <summary>Hackish renderer that draw a fancy version of a person for a Tile view.</summary>
	/// <remarks>This is not the way to write a professional level renderer.
	/// It is hideously inefficient (we should at least cache the images), but it is obvious
	/// </remarks>
	public class BusinessCardRenderer : AbstractRenderer
	{
		public override Boolean RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, Object rowObject)
		{
			// If we're in any other view than Tile, return false to say that we haven't done
			// the rendering and the default process should do it's stuff
			ObjectListView olv = e.Item.ListView as ObjectListView;
			if(olv == null || olv.View != View.Tile)
				return false;

			// Use buffered graphics to kill flickers
			BufferedGraphics buffered = BufferedGraphicsManager.Current.Allocate(g, itemBounds);
			g = buffered.Graphics;
			g.Clear(olv.BackColor);
			g.SmoothingMode = ObjectListView.SmoothingMode;
			g.TextRenderingHint = ObjectListView.TextRenderingHint;

			if(e.Item.Selected)
			{
				this.BorderPen = Pens.Blue;
				this.HeaderBackBrush = new SolidBrush(olv.SelectedBackColorOrDefault);
			} else
			{
				this.BorderPen = new Pen(Color.FromArgb(0x33, 0x33, 0x33));
				this.HeaderBackBrush = new SolidBrush(Color.FromArgb(0x33, 0x33, 0x33));
			}
			this.DrawBusinessCard(g, itemBounds, rowObject, olv, (OLVListItem)e.Item);

			// Finally render the buffered graphics
			buffered.Render();
			buffered.Dispose();

			// Return true to say that we've handled the drawing
			return true;
		}

		internal Pen BorderPen = new Pen(Color.FromArgb(0x33, 0x33, 0x33));
		internal Brush TextBrush = new SolidBrush(Color.FromArgb(0x22, 0x22, 0x22));
		internal Brush HeaderTextBrush = Brushes.AliceBlue;
		internal Brush HeaderBackBrush = new SolidBrush(Color.FromArgb(0x33, 0x33, 0x33));
		internal Brush BackBrush = Brushes.LemonChiffon;

		public void DrawBusinessCard(Graphics g, Rectangle itemBounds, Object rowObject, ObjectListView olv, OLVListItem item)
		{
			const Int32 spacing = 8;

			// Allow a border around the card
			itemBounds.Inflate(-2, -2);

			// Draw card background
			const Int32 rounding = 20;
			GraphicsPath path = this.GetRoundedRect(itemBounds, rounding);
			g.FillPath(this.BackBrush, path);
			g.DrawPath(this.BorderPen, path);

			g.Clip = new Region(itemBounds);

			// Draw the photo
			Rectangle photoRect = itemBounds;
			photoRect.Inflate(-spacing, -spacing);
			if(rowObject is Person person)
			{
				photoRect.Width = 80;
				String photoFile = String.Format(@".\Photos\{0}.png", person.Photo);
				if(File.Exists(photoFile))
				{
					Image photo = Image.FromFile(photoFile);
					if(photo.Width > photoRect.Width)
						photoRect.Height = (Int32)(photo.Height * ((Single)photoRect.Width / photo.Width));
					else
						photoRect.Height = photo.Height;
					g.DrawImage(photo, photoRect);
				} else
					g.DrawRectangle(Pens.DarkGray, photoRect);
			}

			// Now draw the text portion
			RectangleF textBoxRect = photoRect;
			textBoxRect.X += (photoRect.Width + spacing);
			textBoxRect.Width = itemBounds.Right - textBoxRect.X - spacing;

			StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap)
			{
				Trimming = StringTrimming.EllipsisCharacter,
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Near
			};

			String txt = item.Text;
			using(Font font = new Font("Tahoma", 11))
			{
				// Measure the height of the title
				SizeF size = g.MeasureString(txt, font, (Int32)textBoxRect.Width, fmt);
				// Draw the title
				RectangleF r3 = textBoxRect;
				r3.Height = size.Height;
				path = this.GetRoundedRect(r3, 15);
				g.FillPath(this.HeaderBackBrush, path);
				g.DrawString(txt, font, this.HeaderTextBrush, textBoxRect, fmt);
				textBoxRect.Y += size.Height + spacing;
			}

			// Draw the other bits of information
			using(Font font = new Font("Tahoma", 8))
			{
				SizeF size = g.MeasureString("Wj", font, itemBounds.Width, fmt);
				textBoxRect.Height = size.Height;
				fmt.Alignment = StringAlignment.Near;
				for(Int32 i = 0; i < olv.Columns.Count; i++)
				{
					OLVColumn column = olv.GetColumn(i);
					if(column.IsTileViewColumn)
					{
						txt = column.GetStringValue(rowObject);
						g.DrawString(txt, font, this.TextBrush, textBoxRect, fmt);
						textBoxRect.Y += size.Height;
					}
				}
			}
		}

		private GraphicsPath GetRoundedRect(RectangleF rect, Single diameter)
		{
			GraphicsPath path = new GraphicsPath();

			RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
			path.AddArc(arc, 180, 90);
			arc.X = rect.Right - diameter;
			path.AddArc(arc, 270, 90);
			arc.Y = rect.Bottom - diameter;
			path.AddArc(arc, 0, 90);
			arc.X = rect.Left;
			path.AddArc(arc, 90, 90);
			path.CloseFigure();

			return path;
		}
	}
}