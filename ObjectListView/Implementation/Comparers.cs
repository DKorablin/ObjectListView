/*
 * Comparers - Various Comparer classes used within ObjectListView
 *
 * Author: Phillip Piper
 * Date: 25/11/2008 17:15 
 *
 * Change log:
 * v2.8.1
 * 2014-12-03  JPP  - Added StringComparer
 * v2.3
 * 2009-08-24  JPP  - Added OLVGroupComparer
 * 2009-06-01  JPP  - ModelObjectComparer would crash if secondary sort column was null.
 * 2008-12-20  JPP  - Fixed bug with group comparisons when a group key was null (SF#2445761)
 * 2008-11-25  JPP  Initial version
 *
 * TO DO:
 *
 * Copyright (C) 2006-2014 Phillip Piper
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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
	/// <summary>
	/// ColumnComparer is the workhorse for all comparison between two values of a particular column.
	/// If the column has a specific comparer, use that to compare the values. Otherwise, do
	/// a case insensitive String compare of the String representations of the values.
	/// </summary>
	/// <remarks><para>This class inherits from both IComparer and its generic counterpart
	/// so that it can be used on untyped and typed collections.</para>
	/// <para>This is used by normal (non-virtual) ObjectListViews. Virtual lists use
	/// ModelObjectComparer</para>
	/// </remarks>
	public class ColumnComparer : IComparer, IComparer<OLVListItem>
	{
		/// <summary>Gets or sets the method that will be used to compare two strings.</summary>
		/// <remarks>The default is to compare on the current culture, case-insensitive</remarks>
		public static StringCompareDelegate StringComparer { get; set; }

		/// <summary>Create a ColumnComparer that will order the rows in a list view according to the values in a given column</summary>
		/// <param name="col">The column whose values will be compared</param>
		/// <param name="order">The ordering for column values</param>
		public ColumnComparer(OLVColumn col, SortOrder order)
		{
			this._column = col;
			this._sortOrder = order;
		}

		/// <summary>
		/// Create a ColumnComparer that will order the rows in a list view according
		/// to the values in a given column, and by a secondary column if the primary
		/// column is equal.
		/// </summary>
		/// <param name="col">The column whose values will be compared</param>
		/// <param name="order">The ordering for column values</param>
		/// <param name="col2">The column whose values will be compared for secondary sorting</param>
		/// <param name="order2">The ordering for secondary column values</param>
		public ColumnComparer(OLVColumn col, SortOrder order, OLVColumn col2, SortOrder order2)
			: this(col, order)
		{
			// There is no point in secondary sorting on the same column
			if(col != col2)
				this._secondComparer = new ColumnComparer(col2, order2);
		}

		/// <summary>Compare two rows</summary>
		/// <param name="x">row1</param>
		/// <param name="y">row2</param>
		/// <returns>An ordering indication: -1, 0, 1</returns>
		public Int32 Compare(Object x, Object y)
			=> this.Compare((OLVListItem)x, (OLVListItem)y);

		/// <summary>Compare two rows</summary>
		/// <param name="x">row1</param>
		/// <param name="y">row2</param>
		/// <returns>An ordering indication: -1, 0, 1</returns>
		public Int32 Compare(OLVListItem x, OLVListItem y)
		{
			if(this._sortOrder == SortOrder.None)
				return 0;

			Int32 result;
			Object x1 = this._column.GetValue(x.RowObject);
			Object y1 = this._column.GetValue(y.RowObject);

			// Handle nulls. Null values come last
			Boolean xIsNull = (x1 == null || x1 == System.DBNull.Value);
			Boolean yIsNull = (y1 == null || y1 == System.DBNull.Value);
			if(xIsNull || yIsNull)
			{
				result = xIsNull && yIsNull
					? 0
					: (xIsNull ? -1 : 1);
			} else
				result = this.CompareValues(x1, y1);

			if(this._sortOrder == SortOrder.Descending)
				result = 0 - result;

			// If the result was equality, use the secondary comparer to resolve it
			if(result == 0 && this._secondComparer != null)
				result = this._secondComparer.Compare(x, y);

			return result;
		}

		/// <summary>Compare the actual values to be used for sorting</summary>
		/// <param name="x">The aspect extracted from the first row</param>
		/// <param name="y">The aspect extracted from the second row</param>
		/// <returns>An ordering indication: -1, 0, 1</returns>
		public Int32 CompareValues(Object x, Object y)
			=> x is String xAsString
				? CompareStrings(xAsString, y as String)// Force case insensitive compares on strings
				: x is IComparable comparable
				? comparable.CompareTo(y)
				: 0;

		private static Int32 CompareStrings(String x, String y)
			=> StringComparer == null
				? String.Compare(x, y, StringComparison.CurrentCultureIgnoreCase)
				: StringComparer(x, y);

		private OLVColumn _column;
		private SortOrder _sortOrder;
		private ColumnComparer _secondComparer;
	}

	/// <summary>
	/// This comparer sort list view groups. OLVGroups have a "SortValue" property,
	/// which is used if present. Otherwise, the titles of the groups will be compared.
	/// </summary>
	public class OLVGroupComparer : IComparer<OLVGroup>
	{
		/// <summary>Create a group comparer</summary>
		/// <param name="order">The ordering for column values</param>
		public OLVGroupComparer(SortOrder order)
		{
			this._sortOrder = order;
		}

		/// <summary>
		/// Compare the two groups. OLVGroups have a "SortValue" property, which is used if present.
		/// Otherwise, the titles of the groups will be compared.
		/// </summary>
		/// <param name="x">group1</param>
		/// <param name="y">group2</param>
		/// <returns>An ordering indication: -1, 0, 1</returns>
		public Int32 Compare(OLVGroup x, OLVGroup y)
		{
			// If we can compare the sort values, do that.
			// Otherwise do a case insensitive compare on the group header.
			Int32 result = x.SortValue != null && y.SortValue != null
				? x.SortValue.CompareTo(y.SortValue)
				: String.Compare(x.Header, y.Header, StringComparison.CurrentCultureIgnoreCase);

			if(this._sortOrder == SortOrder.Descending)
				result = 0 - result;

			return result;
		}

		private SortOrder _sortOrder;
	}

	/// <summary>This comparer can be used to sort a collection of model objects by a given column</summary>
	/// <remarks>
	/// <para>This is used by virtual ObjectListViews. Non-virtual lists use ColumnComparer</para>
	/// </remarks>
	public class ModelObjectComparer : IComparer, IComparer<Object>
	{
		/// <summary>Gets or sets the method that will be used to compare two strings.</summary>
		/// <remarks>The default is to compare on the current culture, case-insensitive</remarks>
		public static StringCompareDelegate StringComparer { get; set; }

		/// <summary>Create a model Object comparer</summary>
		/// <param name="col"></param>
		/// <param name="order"></param>
		public ModelObjectComparer(OLVColumn col, SortOrder order)
		{
			this._column = col;
			this._sortOrder = order;
		}

		/// <summary>Create a model Object comparer with a secondary sorting column</summary>
		/// <param name="col"></param>
		/// <param name="order"></param>
		/// <param name="col2"></param>
		/// <param name="order2"></param>
		public ModelObjectComparer(OLVColumn col, SortOrder order, OLVColumn col2, SortOrder order2)
			: this(col, order)
		{
			// There is no point in secondary sorting on the same column
			if(col != col2 && col2 != null && order2 != SortOrder.None)
				this._secondComparer = new ModelObjectComparer(col2, order2);
		}

		/// <summary>Compare the two model objects</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Int32 Compare(Object x, Object y)
		{
			Int32 result;
			Object x1 = this._column.GetValue(x);
			Object y1 = this._column.GetValue(y);

			if(this._sortOrder == SortOrder.None)
				return 0;

			// Handle nulls. Null values come last
			Boolean xIsNull = (x1 == null || x1 == DBNull.Value);
			Boolean yIsNull = (y1 == null || y1 == DBNull.Value);
			if(xIsNull || yIsNull)
				result = xIsNull && yIsNull
					? 0
					: (xIsNull ? -1 : 1);
			else
				result = this.CompareValues(x1, y1);

			if(this._sortOrder == SortOrder.Descending)
				result = 0 - result;

			// If the result was equality, use the secondary comparer to resolve it
			if(result == 0 && this._secondComparer != null)
				result = this._secondComparer.Compare(x, y);

			return result;
		}

		/// <summary>Compare the actual values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Int32 CompareValues(Object x, Object y)
			=> x is String xStr
				? CompareStrings(xStr, y as String)// Force case insensitive compares on strings
				: x is IComparable comparable
				? comparable.CompareTo(y)
				: 0;

		private static Int32 CompareStrings(String x, String y)
			=> StringComparer == null
				? String.Compare(x, y, StringComparison.CurrentCultureIgnoreCase)
				: StringComparer(x, y);

		private OLVColumn _column;
		private SortOrder _sortOrder;
		private ModelObjectComparer _secondComparer;
	}
}