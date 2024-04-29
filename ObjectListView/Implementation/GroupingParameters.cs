/*
 * GroupingParameters - All the data that is used to create groups in an ObjectListView
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
using System.Windows.Forms;

namespace BrightIdeasSoftware
{

	/// <summary>This class contains all the settings used when groups are created</summary>
	public class GroupingParameters
	{
		/// <summary>Create a GroupingParameters</summary>
		/// <param name="olv"></param>
		/// <param name="groupByColumn"></param>
		/// <param name="groupByOrder"></param>
		/// <param name="column"></param>
		/// <param name="order"></param>
		/// <param name="secondaryColumn"></param>
		/// <param name="secondaryOrder"></param>
		/// <param name="titleFormat"></param>
		/// <param name="titleSingularFormat"></param>
		/// <param name="sortItemsByPrimaryColumn"></param>
		public GroupingParameters(ObjectListView olv, OLVColumn groupByColumn, SortOrder groupByOrder,
			OLVColumn column, SortOrder order, OLVColumn secondaryColumn, SortOrder secondaryOrder,
			String titleFormat, String titleSingularFormat, Boolean sortItemsByPrimaryColumn)
		{
			this.ListView = olv;
			this.GroupByColumn = groupByColumn;
			this.GroupByOrder = groupByOrder;
			this.PrimarySort = column;
			this.PrimarySortOrder = order;
			this.SecondarySort = secondaryColumn;
			this.SecondarySortOrder = secondaryOrder;
			this.SortItemsByPrimaryColumn = sortItemsByPrimaryColumn;
			this.TitleFormat = titleFormat;
			this.TitleSingularFormat = titleSingularFormat;
		}

		/// <summary>Gets or sets the ObjectListView being grouped</summary>
		public ObjectListView ListView { get; set; }

		/// <summary>Gets or sets the column used to create groups</summary>
		public OLVColumn GroupByColumn { get; set; }

		/// <summary>In what order will the groups themselves be sorted?</summary>
		public SortOrder GroupByOrder { get; set; }

		/// <summary>If this is set, this comparer will be used to order the groups</summary>
		public IComparer<OLVGroup> GroupComparer { get; set; }

		/// <summary>If this is set, this comparer will be used to order items within each group</summary>
		public IComparer<OLVListItem> ItemComparer { get; set; }

		/// <summary>Gets or sets the column that will be the primary sort</summary>
		public OLVColumn PrimarySort { get; set; }

		/// <summary>Gets or sets the ordering for the primary sort</summary>
		public SortOrder PrimarySortOrder { get; set; }

		/// <summary>Gets or sets the column used for secondary sorting</summary>
		public OLVColumn SecondarySort { get; set; }

		/// <summary>Gets or sets the ordering for the secondary sort</summary>
		public SortOrder SecondarySortOrder { get; set; }

		/// <summary>Gets or sets the title format used for groups with zero or more than one element</summary>
		public String TitleFormat { get; set; }

		/// <summary>Gets or sets the title format used for groups with only one element</summary>
		public String TitleSingularFormat { get; set; }

		/// <summary>Gets or sets whether the items should be sorted by the primary column</summary>
		public Boolean SortItemsByPrimaryColumn { get; set; }

		/// <summary>Create an OLVGroup for the given information</summary>
		/// <param name="key"></param>
		/// <param name="count"></param>
		/// <param name="hasCollapsibleGroups"></param>
		/// <returns></returns>
		public OLVGroup CreateGroup(Object key, Int32 count, Boolean hasCollapsibleGroups)
		{
			String title = GroupByColumn.ConvertGroupKeyToTitle(key);
			if(!String.IsNullOrEmpty(TitleFormat))
			{
				String format = (count == 1 ? TitleSingularFormat : TitleFormat);
				try
				{
					title = String.Format(format, title, count);
				} catch(FormatException)
				{
					title = "Invalid group format: " + format;
				}
			}
			return new OLVGroup(title)
			{
				Column = GroupByColumn,
				Collapsible = hasCollapsibleGroups,
				Key = key,
				SortValue = key as IComparable
			};
		}
	}
}