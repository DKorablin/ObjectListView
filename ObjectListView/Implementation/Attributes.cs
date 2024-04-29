/*
 * Attributes - Attributes that can be attached to properties of models to allow columns to be
 *              built from them directly
 *
 * Author: Phillip Piper
 * Date: 15/08/2009 22:01
 *
 * Change log:
 * v2.6
 * 2012-08-16  JPP  - Added [OLVChildren] and [OLVIgnore]
 *                  - OLV attributes can now only be set on properties
 * v2.4
 * 2010-04-14  JPP  - Allow Name property to be set
 * 
 * v2.3
 * 2009-08-15  JPP  - Initial version
 *
 * To do:
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
using System.Collections.Generic;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
	/// <summary>This attribute is used to mark a property of a model class that should be noticed by Generator class.</summary>
	/// <remarks>All the attributes of this class match their equivalent properties on <see cref="OLVColumn"/>.</remarks>
	[AttributeUsage(AttributeTargets.Property)]
	public class OLVColumnAttribute : Attribute
	{
		#region Constructor

		// There are several property where we actually want nullable value (bool?, int?),
		// but it seems attribute properties can't be nullable types.
		// So we explicitly track if those properties have been set.

		/// <summary>Create a new <see cref="OLVColumnAttribute"/></summary>
		public OLVColumnAttribute()
		{
		}

		/// <summary>Create a new <see cref="OLVColumnAttribute"/> with the given title</summary>
		/// <param name="title">The title of the column</param>
		public OLVColumnAttribute(String title)
			=> this.Title = title;

		#endregion

		#region Public properties

		/// <summary></summary>
		public String AspectToStringFormat { get; set; }

		/// <summary></summary>
		public Boolean CheckBoxes
		{
			get => this._checkBoxes;
			set
			{
				this._checkBoxes = value;
				this.IsCheckBoxesSet = true;
			}
		}
		private Boolean _checkBoxes;
		internal Boolean IsCheckBoxesSet = false;

		/// <summary></summary>
		public Int32 DisplayIndex { get; set; } = -1;

		/// <summary></summary>
		public Boolean FillsFreeSpace { get; set; }

		/// <summary></summary>
		public Int32 FreeSpaceProportion
		{
			get => this._freeSpaceProportion;
			set
			{
				this._freeSpaceProportion = value;
				IsFreeSpaceProportionSet = true;
			}
		}
		private Int32 _freeSpaceProportion;
		internal Boolean IsFreeSpaceProportionSet = false;

		/// <summary>An array of IComparables that mark the cutoff points for values when grouping on this column.</summary>
		public Object[] GroupCutoffs { get; set; }

		/// <summary></summary>
		public String[] GroupDescriptions { get; set; }

		/// <summary></summary>
		public String GroupWithItemCountFormat { get; set; }

		/// <summary></summary>
		public String GroupWithItemCountSingularFormat { get; set; }

		/// <summary></summary>
		public Boolean Hyperlink { get; set; }

		/// <summary></summary>
		public String ImageAspectName { get; set; }

		/// <summary></summary>
		public Boolean IsEditable
		{
			get => this._isEditable;
			set
			{
				this._isEditable = value;
				this.IsEditableSet = true;
			}
		}
		private Boolean _isEditable = true;
		internal Boolean IsEditableSet = false;

		/// <summary></summary>
		public Boolean IsVisible { get; set; } = true;

		/// <summary></summary>
		public Boolean IsTileViewColumn { get; set; }

		/// <summary></summary>
		public Int32 MaximumWidth { get; set; } = -1;

		/// <summary></summary>
		public Int32 MinimumWidth { get; set; } = -1;

		/// <summary></summary>
		public String Name { get; set; }

		/// <summary></summary>
		public HorizontalAlignment TextAlign
		{
			get => this._textAlign;
			set
			{
				this._textAlign = value;
				IsTextAlignSet = true;
			}
		}
		private HorizontalAlignment _textAlign = HorizontalAlignment.Left;
		internal Boolean IsTextAlignSet = false;

		/// <summary></summary>
		public String Tag { get; set; }

		/// <summary></summary>
		public String Title { get; set; }

		/// <summary></summary>
		public String ToolTipText { get; set; }

		/// <summary></summary>
		public Boolean TriStateCheckBoxes
		{
			get => this._triStateCheckBoxes;
			set
			{
				this._triStateCheckBoxes = value;
				this.IsTriStateCheckBoxesSet = true;
			}
		}
		private Boolean _triStateCheckBoxes;
		internal Boolean IsTriStateCheckBoxesSet = false;

		/// <summary></summary>
		public Boolean UseInitialLetterForGroup { get; set; }

		/// <summary></summary>
		public Int32 Width { get; set; } = 150;

		#endregion
	}

	/// <summary>Properties marked with [OLVChildren] will be used as the children source in a TreeListView.</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class OLVChildrenAttribute : Attribute
	{

	}

	/// <summary>Properties marked with [OLVIgnore] will not have columns generated for them.</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class OLVIgnoreAttribute : Attribute
	{

	}
}