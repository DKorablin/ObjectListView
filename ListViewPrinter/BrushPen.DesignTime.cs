using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace BrightIdeasSoftware
{
	/// <summary>Editor for use within the IDE when changing BrushFactory values</summary>
	internal class BrushDataEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
			=> UITypeEditorEditStyle.DropDown;

		/// <summary>The user has asked to edit a value. Do it.</summary>
		public override Object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			// For future reference. When we are running within the IDE, IDesignerHost will be non-null, e.g.
			//IDesignerHost host = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;

			// We need editor service otherwise we can't do anything
			IWindowsFormsEditorService wfes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
			if(wfes == null)
				return value;

			BrushForm form = new BrushForm();
			form.SetBrush(value as IBrushData);
			wfes.DropDownControl(form);
			return form.GetBrushData();
		}

		/// <summary>Can we paint a representation of our value?</summary>
		public override Boolean GetPaintValueSupported(ITypeDescriptorContext context)
			=> true;

		/// <summary>Draw a representation of our value</summary>
		public override void PaintValue(PaintValueEventArgs e)
		{
			if(e.Value is IBrushData bd)
				e.Graphics.FillRectangle(BlockFormat.PrepareBrushForDrawing(bd.GetBrush(), e.Bounds), e.Bounds);
			else
				base.PaintValue(e);
		}
	}

	/// <summary>Editor for use within the IDE when changing BrushFactory values</summary>
	internal class PenDataEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
			=> UITypeEditorEditStyle.DropDown;

		/// <summary>The user has asked to edit a value. Do it.</summary>
		public override Object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			// For future reference. When we are running within the IDE, IDesignerHost will be non-null, e.g.
			//IDesignerHost host = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;

			// We need editor service otherwise we can't do anything
			IWindowsFormsEditorService wfes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
			if(wfes == null)
				return value;

			PenForm form = new PenForm();
			form.SetPenData(value as PenData);
			wfes.DropDownControl(form);
			return form.GetPenData();
		}

		/// <summary>Can we paint a representation of our value?</summary>
		public override Boolean GetPaintValueSupported(ITypeDescriptorContext context)
			=> true;

		/// <summary>Draw a representation of our value</summary>
		public override void PaintValue(PaintValueEventArgs e)
		{
			if(e.Value is PenData p)
			{
				e.Graphics.SetClip(e.Bounds);
				e.Graphics.DrawLine(p.GetPen(), e.Bounds.Left, e.Bounds.Top, e.Bounds.Right, e.Bounds.Bottom);
				e.Graphics.ResetClip();

			} else
				base.PaintValue(e);
		}
	}

	/// <summary>
	/// Instances of this class handle converting BrushFactories to and from various other forms.
	/// This class can convert BrushFactories to and from string representations, and can convert
	/// to InstanceDescriptor (which are used to convert an Object into source code).
	/// </summary>
	internal class BrushDataConverter : ExpandableObjectConverter
	{
		/// <summary>
		/// Indicate that we can convert to a string or to an InstanceDescriptor (these
		/// are used to serial an Object to code).
		/// </summary>
		public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(String) || base.CanConvertTo(context, destinationType);

		public override Object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, Object value, Type destinationType)
		{
			if(destinationType == typeof(String))
			{
				return value is IBrushData b
					? b.GetType().Name
					: (Object)"(none)";
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(String) || base.CanConvertFrom(context, sourceType);

		public override Object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, Object value)
		{
			if(value != null && value.GetType() == typeof(String))
			{
				String s = value as String;
				if(s.Length == 0)
					return null;
				switch(s[0])
				{
				case 'S':
					return new SolidBrushData();
				case 'G':
					return new LinearGradientBrushData();
				case 'H':
					return new HatchBrushData();
				default:
					return null;
				}
			}
			return base.ConvertFrom(context, culture, value);
		}
	}

	/// <summary>
	/// Instances of this class handle converting BrushFactories to and from various other forms.
	/// This class can convert BrushFactories to and from string representations, and can convert
	/// to InstanceDescriptor (which are used to convert an Object into source code).
	/// </summary>
	internal class PenDataConverter : ExpandableObjectConverter
	{
		/// <summary>
		/// Indicate that we can convert to a string or to an InstanceDescriptor (these
		/// are used to serial an Object to code).
		/// </summary>
		public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(String) || base.CanConvertTo(context, destinationType);

		public override Object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, Object value, Type destinationType)
		{
			if(destinationType == typeof(String))
			{
				PenData p = value as PenData;
				if(p == null || p.Brush == null)
					return "(none)";
				else
				{
					String name = p.Brush.GetType().Name;
					return name.Substring(0, name.Length - "BrushData".Length) + "Pen";
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(String) || base.CanConvertFrom(context, sourceType);

		public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
		{
			if(value != null && value.GetType() == typeof(String))
			{
				String s = value as String;
				if(s.Length == 0)
					return null;
				switch(s[0])
				{
				case 'S':
					return new PenData(new SolidBrushData());
				case 'G':
					return new PenData(new LinearGradientBrushData());
				case 'H':
					return new PenData(new HatchBrushData());
				default:
					return null;
				}
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}