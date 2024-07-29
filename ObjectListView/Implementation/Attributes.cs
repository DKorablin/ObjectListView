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

		/// <summary>This format String will be used to convert an aspect to its String representation.</summary>
		public String AspectToStringFormat { get; set; }

		/// <summary></summary>
		public Int32 DisplayIndex { get; set; } = -1;

		/// <summary>Should this column resize to fill the free space in the listview?</summary>
		public Boolean FillsFreeSpace
		{
			get => this._fillsFreeSpace;
			set
			{
				this._fillsFreeSpace = value;
				this.FillsFreeSpaceSet = true;
			}
		}
		private Boolean _fillsFreeSpace;
		internal Boolean FillsFreeSpaceSet = false;

		/// <summary>What proportion of the unoccupied horizontal space in the control should be given to this column?</summary>
		public Int32 FreeSpaceProportion
		{
			get => this._freeSpaceProportion;
			set
			{
				this._freeSpaceProportion = value;
				this.FreeSpaceProportionSet = true;
			}
		}
		private Int32 _freeSpaceProportion;
		internal Boolean FreeSpaceProportionSet = false;

		/// <summary>An array of IComparables that mark the cutoff points for values when grouping on this column.</summary>
		public Object[] GroupCutoffs { get; set; }

		/// <summary></summary>
		public String[] GroupDescriptions { get; set; }

		/// <summary>When the listview is grouped by this column and group title has an item count, how should the label be formatted?</summary>
		public String GroupWithItemCountFormat { get; set; }

		/// <summary>
		/// When the listview is grouped by this column and a group title has an item count,
		/// how should the label be formatted if there is only one item in the group?
		/// </summary>
		public String GroupWithItemCountSingularFormat { get; set; }

		/// <summary>Gets or sets whether the text values in this column will act like hyperlinks</summary>
		public Boolean Hyperlink { get; set; }

		/// <summary>
		/// This is the name of property that will be invoked to get the image selector of the image that should be shown in this column.
		/// It can return an int, String, Image or null.
		/// </summary>
		public String ImageAspectName { get; set; }

		/// <summary>Can this column be seen by the user?</summary>
		public Boolean IsVisible { get; set; } = true;

		/// <summary>Get/set whether this column should be used when the view is switched to tile view.</summary>
		public Boolean IsTileViewColumn { get; set; }

		/// <summary>What is the maximum width that the user can give to this column?</summary>
		public Int32 MaximumWidth { get; set; } = -1;

		/// <summary>What is the minimum width that the user can give to this column?</summary>
		public Int32 MinimumWidth { get; set; } = -1;

		/// <summary></summary>
		public String Name { get; set; }

		/// <summary></summary>
		public String Tag { get; set; }

		/// <summary>The title of the column</summary>
		public String Title { get; set; }

		/// <summary>What string should be displayed when the mouse is hovered over the header of this column?</summary>
		public String ToolTipText { get; set; }

		/// <summary>Group objects by the initial letter of the aspect of the column</summary>
		public Boolean UseInitialLetterForGroup
		{
			get => this._useInitialLetterForGroup;
			set
			{
				this._useInitialLetterForGroup = value;
				this.UseInitialLetterForGroupSet = true;
			}
		}
		private Boolean _useInitialLetterForGroup;
		internal Boolean UseInitialLetterForGroupSet = false;

		/// <summary>What is the width of this column?</summary>
		public Int32 Width
		{
			get => this._width;
			set
			{
				this._width = value;
				this.WidthSet = true;
			}
		}
		private Int32 _width;
		internal Boolean WidthSet = false;

		/// <summary>Gets or sets whether or not this column should be user filterable</summary>
		public Boolean UseFiltering
		{
			get => this._useFiltering;
			set
			{
				this._useFiltering = value;
				this.UseFilteringSet = true;
			}
		}
		private Boolean _useFiltering;
		internal Boolean UseFilteringSet = false;

		/// <summary>Gets or sets whether the cell editor should use AutoComplete</summary>
		public Boolean AutoCompleteEditor
		{
			get => this._autoCompleteEditor;
			set
			{
				this._autoCompleteEditor = value;
				this.AutoCompleteEditorSet = true;
			}
		}
		private Boolean _autoCompleteEditor;
		internal Boolean AutoCompleteEditorSet = false;

		/// <summary>Gets or set whether the contents of this column's cells should be word wrapped</summary>
		public Boolean WordWrap
		{
			get => this._wordWrap;
			set
			{
				this._wordWrap = value;
				this.WordWrapSet = true;
			}
		}
		private Boolean _wordWrap;
		internal Boolean WordWrapSet = false;

		/// <summary>Gets or sets whether this column will show a checkbox.</summary>
		public Boolean CheckBoxes
		{
			get => this._checkBoxes;
			set
			{
				this._checkBoxes = value;
				this.CheckBoxesSet = true;
			}
		}

		private Boolean _checkBoxes;
		internal Boolean CheckBoxesSet = false;

		/// <summary>Can the values shown in this column be edited?</summary>
		public Boolean IsEditable
		{
			get => this._isEditable;
			set
			{
				this._isEditable = value;
				this.IsEditableSet = true;
			}
		}

		private Boolean _isEditable;
		internal Boolean IsEditableSet = false;

		/// <summary>Gets or sets the horizontal alignment of the contents of the column.</summary>
		public HorizontalAlignment TextAlign
		{
			get => this._textAlign;
			set
			{
				this._textAlign = value;
				this.TextAlignSet = true;
			}
		}

		private HorizontalAlignment _textAlign;
		internal Boolean TextAlignSet = false;

		/// <summary>Should this column have a tri-state checkbox?</summary>
		public Boolean TriStateCheckBoxes
		{
			get => this._triStateCheckBoxes;
			set
			{
				this._triStateCheckBoxes = value;
				this.TriStateCheckBoxesSet = true;
			}
		}

		private Boolean _triStateCheckBoxes;
		internal Boolean TriStateCheckBoxesSet = false;

		/// <summary>Gets or sets whether the cell editor should use AutoComplete</summary>
		public AutoCompleteMode AutoCompleteEditorMode
		{
			get => this._autoCompleteEditorMode;
			set
			{
				this._autoCompleteEditorMode = value;
				this.AutoCompleteEditorModeSet = true;
			}
		}

		private AutoCompleteMode _autoCompleteEditorMode;
		internal Boolean AutoCompleteEditorModeSet = false;

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