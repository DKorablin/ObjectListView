using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;

namespace BrightIdeasSoftware
{
	/// <summary>PenData represents the data required to create a pen.</summary>
	/// <remarks>Pens cannot be edited directly within the IDE (is this VCS EE only?)
	/// These objects allow pen characters to be edited within the IDE and then real
	/// Pen objects created.</remarks>
	[Editor(typeof(PenDataEditor), typeof(UITypeEditor))]
	[TypeConverter(typeof(PenDataConverter))]
	public class PenData
	{
		public PenData() : this(new SolidBrushData())
		{
		}

		public PenData(IBrushData brush)
			=> this.Brush = brush;

		public Pen GetPen()
		{
			Pen p = new Pen(this.Brush.GetBrush(), this.Width);
			p.SetLineCap(this.StartCap, this.EndCap, this.DashCap);
			p.DashStyle = this.DashStyle;
			p.LineJoin = this.LineJoin;
			return p;
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public IBrushData Brush { get; set; }

		[DefaultValue(typeof(DashCap), "Round")]
		public DashCap DashCap { get; set; } = DashCap.Round;

		[DefaultValue(typeof(DashStyle), "Solid")]
		public DashStyle DashStyle { get; set; } = DashStyle.Solid;

		[DefaultValue(typeof(LineCap), "NoAnchor")]
		public LineCap EndCap { get; set; } = LineCap.NoAnchor;

		[DefaultValue(typeof(LineJoin), "Round")]
		public LineJoin LineJoin { get; set; } = LineJoin.Round;

		[DefaultValue(typeof(LineCap), "NoAnchor")]
		public LineCap StartCap { get; set; } = LineCap.NoAnchor;

		[DefaultValue(1.0f)]
		public Single Width { get; set; } = 1.0f;
	}

	[Editor(typeof(BrushDataEditor), typeof(UITypeEditor))]
	[TypeConverter(typeof(BrushDataConverter))]
	public interface IBrushData
	{
		Brush GetBrush();
	}

	public class SolidBrushData : IBrushData
	{
		public Brush GetBrush()
			=> this.Alpha < 255
				? new SolidBrush(Color.FromArgb(this.Alpha, this.Color))
				: (Brush)new SolidBrush(this.Color);

		[DefaultValue(typeof(Color), "")]
		public Color Color { get; set; } = Color.Empty;

		[DefaultValue(255)]
		public Int32 Alpha { get; set; } = 255;
	}

	public class LinearGradientBrushData : IBrushData
	{
		public Brush GetBrush()
			=> new LinearGradientBrush(new Rectangle(0, 0, 100, 100), this.FromColor, this.ToColor, this.GradientMode);

		public Color FromColor { get; set; } = Color.Aqua;

		public Color ToColor { get; set; } = Color.Pink;

		public LinearGradientMode GradientMode { get; set; } = LinearGradientMode.Horizontal;
	}

	public class HatchBrushData : IBrushData
	{
		public Brush GetBrush()
			=> new HatchBrush(this.HatchStyle, this.ForegroundColor, this.BackgroundColor);

		public Color BackgroundColor { get; set; } = Color.AliceBlue;

		public Color ForegroundColor { get; set; } = Color.Aqua;

		public HatchStyle HatchStyle { get; set; } = HatchStyle.Cross;
	}

	public class TextureBrushData : IBrushData
	{
		public Brush GetBrush()
			=> this.Image == null
				? null
				: new TextureBrush(this.Image, this.WrapMode);

		public Image Image { get; set; }

		public WrapMode WrapMode { get; set; } = WrapMode.Tile;
	}
}