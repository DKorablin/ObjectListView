﻿/*
 * OlvListViewHitTestInfo - All information gathered during a OlvHitTest() operation
 *
 * Author: Phillip Piper
 * Date: 31-March-2011 5:53 pm
 *
 * Change log:
 * 2011-03-31  JPP  - Split into its own file
 * 
 * Copyright (C) 2011-2014 Phillip Piper
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
using System.Text;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{

	/// <summary>An indication of where a hit was within ObjectListView cell</summary>
	public enum HitTestLocation
	{
		/// <summary>Nowhere</summary>
		Nothing,

		/// <summary>On the text</summary>
		Text,

		/// <summary>On the image</summary>
		Image,

		/// <summary>On the checkbox</summary>
		CheckBox,

		/// <summary>On the expand button (TreeListView)</summary>
		ExpandButton,

		/// <summary>In a button (cell must have ButtonRenderer)</summary>
		Button,

		/// <summary>In the cell but not in any more specific location</summary>
		InCell,

		/// <summary>UserDefined location1 (used for custom renderers)</summary>
		UserDefined,

		/// <summary>On the expand/collapse widget of the group</summary>
		GroupExpander,

		/// <summary>Somewhere on a group</summary>
		Group,

		/// <summary>Somewhere in a column header</summary>
		Header,

		/// <summary>Somewhere in a column header checkbox</summary>
		HeaderCheckBox,

		/// <summary>Somewhere in a header divider</summary>
		HeaderDivider,
	}

	/// <summary>A collection of ListViewHitTest constants</summary>
	[Flags]
	public enum HitTestLocationEx
	{
		/// <summary>The position is inside the list-view control's client window, but it is not over a list item</summary>
		LVHT_NOWHERE = 0x00000001,
		/// <summary>The position is over a list-view item's icon</summary>
		LVHT_ONITEMICON = 0x00000002,
		/// <summary>The position is over a list-view item's text</summary>
		LVHT_ONITEMLABEL = 0x00000004,
		/// <summary>The position is over the state image of a list-view item</summary>
		LVHT_ONITEMSTATEICON = 0x00000008,
		/// <summary>The position is over a list-view item</summary>
		LVHT_ONITEM = (LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON),

		/// <summary>The position is above the control's client area</summary>
		LVHT_ABOVE = 0x00000008,
		/// <summary>The position is below the control's client area</summary>
		LVHT_BELOW = 0x00000010,
		/// <summary>The position is to the right of the list-view control's client area</summary>
		LVHT_TORIGHT = 0x00000020,
		/// <summary>The position is to the left of the list-view control's client area</summary>
		LVHT_TOLEFT = 0x00000040,

		/// <summary>The point is within the group header</summary>
		LVHT_EX_GROUP_HEADER = 0x10000000,
		/// <summary>The point is within the group footer</summary>
		LVHT_EX_GROUP_FOOTER = 0x20000000,
		/// <summary>The point is within the collapse/expand button of the group</summary>
		LVHT_EX_GROUP_COLLAPSE = 0x40000000,
		/// <summary>The point is within the area of the group where items are displayed</summary>
		LVHT_EX_GROUP_BACKGROUND = -2147483648, // 0x80000000
		/// <summary> The point is within the state icon of the group</summary>
		LVHT_EX_GROUP_STATEICON = 0x01000000,
		/// <summary>The point is within the subset link of the group</summary>
		LVHT_EX_GROUP_SUBSETLINK = 0x02000000,
		/// <summary></summary>
		LVHT_EX_GROUP = (LVHT_EX_GROUP_BACKGROUND | LVHT_EX_GROUP_COLLAPSE | LVHT_EX_GROUP_FOOTER | LVHT_EX_GROUP_HEADER | LVHT_EX_GROUP_STATEICON | LVHT_EX_GROUP_SUBSETLINK),
		/// <summary></summary>
		LVHT_EX_GROUP_MINUS_FOOTER_AND_BKGRD = (LVHT_EX_GROUP_COLLAPSE | LVHT_EX_GROUP_HEADER | LVHT_EX_GROUP_STATEICON | LVHT_EX_GROUP_SUBSETLINK),
		/// <summary>The point is within the icon or text content of the item and not on the background</summary>
		LVHT_EX_ONCONTENTS = 0x04000000, // On item AND not on the background
		/// <summary>The point is within the footer of the list-view control</summary>
		LVHT_EX_FOOTER = 0x08000000,
	}

	/// <summary>Instances of this class encapsulate the information gathered during a OlvHitTest() operation.
	/// </summary>
	/// <remarks>Custom renderers can use HitTestLocation.UserDefined and the UserData
	/// Object to store more specific locations for use during event handlers.</remarks>
	public class OlvListViewHitTestInfo
	{

		/// <summary>Create a OlvListViewHitTestInfo</summary>
		public OlvListViewHitTestInfo(OLVListItem olvListItem, OLVListSubItem subItem, Int32 flags, OLVGroup group, Int32 iColumn)
		{
			this.Item = olvListItem;
			this.SubItem = subItem;
			this.Location = ConvertNativeFlagsToDotNetLocation(olvListItem, flags);
			this.HitTestLocationEx = (HitTestLocationEx)flags;
			this.Group = group;
			this.ColumnIndex = iColumn;
			this.ListView = olvListItem == null ? null : (ObjectListView)olvListItem.ListView;

			switch(Location)
			{
			case ListViewHitTestLocations.StateImage:
				this.HitTestLocation = HitTestLocation.CheckBox;
				break;
			case ListViewHitTestLocations.Image:
				this.HitTestLocation = HitTestLocation.Image;
				break;
			case ListViewHitTestLocations.Label:
				this.HitTestLocation = HitTestLocation.Text;
				break;
			default:
				if((this.HitTestLocationEx & HitTestLocationEx.LVHT_EX_GROUP_COLLAPSE) == HitTestLocationEx.LVHT_EX_GROUP_COLLAPSE)
					this.HitTestLocation = HitTestLocation.GroupExpander;
				else if((this.HitTestLocationEx & HitTestLocationEx.LVHT_EX_GROUP_MINUS_FOOTER_AND_BKGRD) != 0)
					this.HitTestLocation = HitTestLocation.Group;
				else
					this.HitTestLocation = HitTestLocation.Nothing;
				break;
			}
		}

		/// <summary>Create a OlvListViewHitTestInfo when the header was hit</summary>
		public OlvListViewHitTestInfo(ObjectListView olv, Int32 iColumn, Boolean isOverCheckBox, Int32 iDivider)
		{
			this.ListView = olv;
			this.ColumnIndex = iColumn;
			this.HeaderDividerIndex = iDivider;
			this.HitTestLocation = isOverCheckBox ? HitTestLocation.HeaderCheckBox : (iDivider < 0 ? HitTestLocation.Header : HitTestLocation.HeaderDivider);
		}

		private static ListViewHitTestLocations ConvertNativeFlagsToDotNetLocation(OLVListItem hitItem, Int32 flags)
		{
			// Untangle base .NET behaviour.

			// In Windows SDK, the value 8 can have two meanings here: LVHT_ONITEMSTATEICON or LVHT_ABOVE.
			// .NET changes these to be:
			// - LVHT_ABOVE becomes ListViewHitTestLocations.AboveClientArea (which is 0x100).
			// - LVHT_ONITEMSTATEICON becomes ListViewHitTestLocations.StateImage (which is 0x200).
			// So, if we see the 8 bit set in flags, we change that to either a state image hit
			// (if we hit an item) or to AboveClientAream if nothing was hit.

			if((8 & flags) == 8)
				return (ListViewHitTestLocations)(0xf7 & flags | (hitItem == null ? 0x100 : 0x200));

			// Mask off the LVHT_EX_XXXX values since ListViewHitTestLocations doesn't have them
			return (ListViewHitTestLocations)(flags & 0xffff);
		}

		#region Public fields

		/// <summary>Where is the hit location?</summary>
		public HitTestLocation HitTestLocation;

		/// <summary>Where is the hit location?</summary>
		public HitTestLocationEx HitTestLocationEx;

		/// <summary>Which group was hit?</summary>
		public OLVGroup Group;

		/// <summary>Custom renderers can use this information to supply more details about the hit location</summary>
		public Object UserData;

		#endregion

		#region Public read-only properties

		/// <summary>Gets the item that was hit</summary>
		public OLVListItem Item { get; internal set; }

		/// <summary>Gets the subitem that was hit</summary>
		public OLVListSubItem SubItem { get; internal set; }

		/// <summary>Gets the part of the subitem that was hit</summary>
		public ListViewHitTestLocations Location { get; internal set; }

		/// <summary>Gets the ObjectListView that was tested</summary>
		public ObjectListView ListView { get; internal set; }

		/// <summary>Gets the model Object that was hit</summary>
		public Object RowObject
		{
			get => this.Item?.RowObject;
		}

		/// <summary>Gets the index of the row under the hit point or -1</summary>
		public Int32 RowIndex
		{
			get => this.Item == null ? -1 : this.Item.Index;
		}

		/// <summary>Gets the index of the column under the hit point</summary>
		public Int32 ColumnIndex { get; internal set; }

		/// <summary>
		/// Gets the index of the header divider
		/// </summary>
		public Int32 HeaderDividerIndex { get; internal set; } = -1;

		/// <summary>Gets the column that was hit</summary>
		public OLVColumn Column
		{
			get
			{
				Int32 index = this.ColumnIndex;
				return index < 0 || this.ListView == null ? null : this.ListView.GetColumn(index);
			}
		}

		#endregion

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		/// <filterpriority>2</filterpriority>
		public override String ToString()
			=> $"HitTestLocation: {this.HitTestLocation}, HitTestLocationEx: {this.HitTestLocationEx}, Item: {this.Item}, SubItem: {this.SubItem}, Location: {this.Location}, Group: {this.Group}, ColumnIndex: {this.ColumnIndex}";

		internal class HeaderHitTestInfo
		{
			public Int32 ColumnIndex;
			public Boolean IsOverCheckBox;
			public Int32 OverDividerIndex;
		}
	}
}