/*
 * OLVListItem - A row in an ObjectListView
 *
 * Author: Phillip Piper
 * Date: 31-March-2011 5:53 pm
 *
 * Change log:
 * 2018-09-01  JPP  - Handle rare case of getting subitems when there are no columns
 * v2.9
 * 2015-08-22  JPP  - Added OLVListItem.SelectedBackColor and SelectedForeColor
 * 2015-06-09  JPP  - Added HasAnyHyperlinks property
 * v2.8
 * 2014-09-27  JPP  - Remove faulty caching of CheckState
 * 2014-05-06  JPP  - Added OLVListItem.Enabled flag
 * vOld
 * 2011-03-31  JPP  - Split into its own file
 * 
 * Copyright (C) 2011-2018 Phillip Piper
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{

	/// <summary>
	/// OLVListItems are specialized ListViewItems that know which row Object they came from,
	/// and the row index at which they are displayed, even when in group view mode.
	/// They also know the image they should draw against themselves
	/// </summary>
	public class OLVListItem : ListViewItem
	{
		#region Constructors

		/// <summary>Create a OLVListItem for the given row Object</summary>
		public OLVListItem(Object rowObject)
			=> this.RowObject = rowObject;

		/// <summary>Create a OLVListItem for the given row Object, represented by the given String and image</summary>
		public OLVListItem(Object rowObject, String text, Object image)
			: base(text, -1)
		{
			this.RowObject = rowObject;
			this._imageSelector = image;
		}

		#endregion.

		#region Properties

		/// <summary>Gets the bounding rectangle of the item, including all subitems</summary>
		new public Rectangle Bounds
		{
			get
			{
				try
				{
					return base.Bounds;
				} catch(System.ArgumentException)
				{
					// If the item is part of a collapsed group, Bounds will throw an exception
					return Rectangle.Empty;
				}
			}
		}

		/// <summary>Gets or sets how many pixels will be left blank around each cell of this item</summary>
		/// <remarks>This setting only takes effect when the control is owner drawn.</remarks>
		public Rectangle? CellPadding { get; set; }

		/// <summary>Gets or sets how the cells of this item will be vertically aligned</summary>
		/// <remarks>This setting only takes effect when the control is owner drawn.</remarks>
		public StringAlignment? CellVerticalAlignment { get; set; }

		/// <summary>Gets or sets the checkedness of this item.</summary>
		/// <remarks>
		/// Virtual lists don't handle checkboxes well, so we have to intercept attempts to change them
		/// through the items, and change them into something that will work.
		/// Unfortunately, this won't work if this property is set through the base class, since
		/// the property is not declared as virtual.
		/// </remarks>
		new public Boolean Checked
		{
			get => base.Checked;
			set
			{
				if(this.Checked != value)
				{
					if(value)
						((ObjectListView)this.ListView).CheckObject(this.RowObject);
					else
						((ObjectListView)this.ListView).UncheckObject(this.RowObject);
				}
			}
		}

		/// <summary>Enable tri-state checkbox.</summary>
		/// <remarks>.NET's Checked property was not built to handle tri-state checkboxes,
		/// and will return True for both Checked and Indeterminate states.</remarks>
		public CheckState CheckState
		{
			get
			{
				switch(this.StateImageIndex)
				{
				case 0:
					return CheckState.Unchecked;
				case 1:
					return CheckState.Checked;
				case 2:
					return CheckState.Indeterminate;
				default:
					return CheckState.Unchecked;
				}
			}
			set
			{
				switch(value)
				{
				case CheckState.Unchecked:
					this.StateImageIndex = 0;
					break;
				case CheckState.Checked:
					this.StateImageIndex = 1;
					break;
				case CheckState.Indeterminate:
					this.StateImageIndex = 2;
					break;
				}
			}
		}

		/// <summary>Gets if this item has any decorations set for it.</summary>
		public Boolean HasDecoration
		{
			get => this._decorations != null && this._decorations.Count > 0;
		}

		/// <summary>Gets or sets the decoration that will be drawn over this item</summary>
		/// <remarks>Setting this replaces all other decorations</remarks>
		public IDecoration Decoration
		{
			get => this.HasDecoration
					? this.Decorations[0]
					: null;
			set
			{
				this.Decorations.Clear();
				if(value != null)
					this.Decorations.Add(value);
			}
		}

		/// <summary>Gets the collection of decorations that will be drawn over this item</summary>
		public IList<IDecoration> Decorations
		{
			get => this._decorations ?? (this._decorations = new List<IDecoration>());
		}
		private IList<IDecoration> _decorations;

		/// <summary>Gets whether or not this row can be selected and activated</summary>
		public Boolean Enabled { get; internal set; }

		/// <summary>Gets whether any cell on this item is showing a hyperlink</summary>
		public Boolean HasAnyHyperlinks
		{
			get
			{
				foreach(OLVListSubItem subItem in this.SubItems)
				{
					if(!String.IsNullOrEmpty(subItem.Url))
						return true;
				}
				return false;
			}
		}

		/// <summary>Get or set the image that should be shown against this item</summary>
		/// <remarks><para>This can be an Image, a String or an int. A String or an int will
		/// be used as an index into the small image list.</para></remarks>
		public Object ImageSelector
		{
			get => this._imageSelector;
			set
			{
				this._imageSelector = value;
				if(value is Int32 i)
					this.ImageIndex = i;
				else if(value is String s)
					this.ImageKey = s;
				else
					this.ImageIndex = -1;
			}
		}
		private Object _imageSelector;

		/// <summary>Gets or sets the model Object that is source of the data for this list item.</summary>
		public Object RowObject { get; set; }

		/// <summary>Gets or sets the color that will be used for this row's background when it is selected and the control is focused.
		/// </summary>
		/// <remarks>
		/// <para>To work reliably, this property must be set during a FormatRow event.</para>
		/// <para>If this is not set, the normal selection BackColor will be used.</para>
		/// </remarks>
		public Color? SelectedBackColor { get; set; }

		/// <summary>Gets or sets the color that will be used for this row's foreground when it is selected and the control is focused.</summary>
		/// <remarks>
		/// <para>To work reliably, this property must be set during a FormatRow event.</para>
		/// <para>If this is not set, the normal selection ForeColor will be used.</para>
		/// </remarks>
		public Color? SelectedForeColor { get; set; }

		#endregion

		#region Accessing

		/// <summary>Return the sub item at the given index</summary>
		/// <param name="index">Index of the subitem to be returned</param>
		/// <returns>An OLVListSubItem</returns>
		public virtual OLVListSubItem GetSubItem(Int32 index)
		{
			if(index >= 0 && index < this.SubItems.Count)
				// If the control has 0 columns, ListViewItem.SubItems will auto create a
				// SubItem of the wrong type. Casting using 'as' handles this rare case. 
				return this.SubItems[index] as OLVListSubItem;

			return null;
		}

		/// <summary>Return bounds of the given subitem</summary>
		/// <remarks>This correctly calculates the bounds even for column 0.</remarks>
		public virtual Rectangle GetSubItemBounds(Int32 subItemIndex)
		{
			if(subItemIndex == 0)
			{
				Rectangle r = this.Bounds;
				Point sides = NativeMethods.GetScrolledColumnSides(this.ListView, subItemIndex);
				r.X = sides.X + 1;
				r.Width = sides.Y - sides.X;
				return r;
			}

			OLVListSubItem subItem = this.GetSubItem(subItemIndex);
			return subItem == null ? new Rectangle() : subItem.Bounds;
		}

		#endregion
	}
}