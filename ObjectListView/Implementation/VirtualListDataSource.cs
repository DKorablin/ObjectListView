/*
 * VirtualListDataSource - Encapsulate how data is provided to a virtual list
 *
 * Author: Phillip Piper
 * Date: 28/08/2009 11:10am
 *
 * Change log:
 * v2.4
 * 2010-04-01   JPP  - Added IFilterableDataSource
 * v2.3
 * 2009-08-28   JPP  - Initial version (Separated from VirtualObjectListView.cs)
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
using System.Collections;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
	/// <summary>
	/// A VirtualListDataSource is a complete manner to provide functionality to a virtual list.
	/// An Object that implements this interface provides a VirtualObjectListView with all the
	/// information it needs to be fully functional.
	/// </summary>
	/// <remarks>Implementors must provide functioning implementations of at least GetObjectCount()
	/// and GetNthObject(), otherwise nothing will appear in the list.</remarks>
	public interface IVirtualListDataSource
	{
		/// <summary>Return the Object that should be displayed at the n'th row.</summary>
		/// <param name="n">The index of the row whose object is to be returned.</param>
		/// <returns>The model Object at the n'th row, or null if the fetching was unsuccessful.</returns>
		Object GetNthObject(Int32 n);

		/// <summary>Return the number of rows that should be visible in the virtual list</summary>
		/// <returns>The number of rows the list view should have.</returns>
		Int32 GetObjectCount();

		/// <summary>Get the index of the row that is showing the given model object</summary>
		/// <param name="model">The model Object sought</param>
		/// <returns>The index of the row showing the model, or -1 if the Object could not be found.</returns>
		Int32 GetObjectIndex(Object model);

		/// <summary>The ListView is about to request the given range of items. Do whatever caching seems appropriate.</summary>
		/// <param name="first"></param>
		/// <param name="last"></param>
		void PrepareCache(Int32 first, Int32 last);

		/// <summary>Find the first row that "matches" the given text in the given range.</summary>
		/// <param name="value">The text typed by the user</param>
		/// <param name="first">Start searching from this index. This may be greater than the 'to' parameter, 
		/// in which case the search should descend</param>
		/// <param name="last">Do not search beyond this index. This may be less than the 'from' parameter.</param>
		/// <param name="column">The column that should be considered when looking for a match.</param>
		/// <returns>Return the index of row that was matched, or -1 if no match was found</returns>
		Int32 SearchText(String value, Int32 first, Int32 last, OLVColumn column);

		/// <summary>Sort the model objects in the data source.</summary>
		/// <param name="column"></param>
		/// <param name="order"></param>
		void Sort(OLVColumn column, SortOrder order);

		//-----------------------------------------------------------------------------------
		// Modification commands
		// THINK: Should we split these four into a separate interface?

		/// <summary>Add the given collection of model objects to this control.</summary>
		/// <param name="modelObjects">A collection of model objects</param>
		void AddObjects(ICollection modelObjects);

		/// <summary>Insert the given collection of model objects to this control at the position</summary>
		/// <param name="index">Index where the collection will be added</param>
		/// <param name="modelObjects">A collection of model objects</param>
		void InsertObjects(Int32 index, ICollection modelObjects);

		/// <summary>Remove all of the given objects from the control</summary>
		/// <param name="modelObjects">Collection of objects to be removed</param>
		void RemoveObjects(ICollection modelObjects);

		/// <summary>Set the collection of objects that this control will show.</summary>
		/// <param name="collection"></param>
		void SetObjects(IEnumerable collection);

		/// <summary>Update/replace the nth Object with the given object</summary>
		/// <param name="index"></param>
		/// <param name="modelObject"></param>
		void UpdateObject(Int32 index, Object modelObject);
	}

	/// <summary>This extension allow virtual lists to filter their contents</summary>
	public interface IFilterableDataSource
	{
		/// <summary>All subsequent retrievals on this data source should be filtered through the given filters. null means no filtering of that kind.</summary>
		/// <param name="modelFilter"></param>
		/// <param name="listFilter"></param>
		void ApplyFilters(IModelFilter modelFilter, IListFilter listFilter);
	}

	/// <summary>A do-nothing implementation of the VirtualListDataSource interface.</summary>
	public class AbstractVirtualListDataSource : IVirtualListDataSource, IFilterableDataSource
	{
		/// <summary>Creates an AbstractVirtualListDataSource</summary>
		/// <param name="listView"></param>
		public AbstractVirtualListDataSource(VirtualObjectListView listView)
			=> this._listView = listView;

		/// <summary>The list view that this data source is giving information to.</summary>
		protected VirtualObjectListView _listView;

		/// <summary></summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public virtual Object GetNthObject(Int32 n)
			=> null;

		/// <summary></summary>
		/// <returns></returns>
		public virtual Int32 GetObjectCount()
			=> -1;

		/// <summary></summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public virtual Int32 GetObjectIndex(Object model)
			=> -1;

		/// <summary></summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public virtual void PrepareCache(Int32 from, Int32 to)
		{
		}

		/// <summary></summary>
		/// <param name="value"></param>
		/// <param name="first"></param>
		/// <param name="last"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public virtual Int32 SearchText(String value, Int32 first, Int32 last, OLVColumn column)
			=> -1;

		/// <summary></summary>
		/// <param name="column"></param>
		/// <param name="order"></param>
		public virtual void Sort(OLVColumn column, SortOrder order)
		{
		}

		/// <summary></summary>
		/// <param name="modelObjects"></param>
		public virtual void AddObjects(ICollection modelObjects)
		{
		}

		/// <summary></summary>
		/// <param name="index"></param>
		/// <param name="modelObjects"></param>
		public virtual void InsertObjects(Int32 index, ICollection modelObjects)
		{
		}

		/// <summary></summary>
		/// <param name="modelObjects"></param>
		public virtual void RemoveObjects(ICollection modelObjects)
		{
		}

		/// <summary></summary>
		/// <param name="collection"></param>
		public virtual void SetObjects(IEnumerable collection)
		{
		}

		/// <summary>Update/replace the nth Object with the given Object</summary>
		/// <param name="index"></param>
		/// <param name="modelObject"></param>
		public virtual void UpdateObject(Int32 index, Object modelObject)
		{
		}

		/// <summary>
		/// This is a useful default implementation of SearchText method, intended to be called
		/// by implementors of IVirtualListDataSource.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="first"></param>
		/// <param name="last"></param>
		/// <param name="column"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		static public Int32 DefaultSearchText(String value, Int32 first, Int32 last, OLVColumn column, IVirtualListDataSource source)
		{
			if(first <= last)
				for(Int32 i = first; i <= last; i++)
				{
					String data = column.GetStringValue(source.GetNthObject(i));
					if(data.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
						return i;
				}
			else
				for(Int32 i = first; i >= last; i--)
				{
					String data = column.GetStringValue(source.GetNthObject(i));
					if(data.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
						return i;
				}

			return -1;
		}

		#region IFilterableDataSource Members

		/// <summary></summary>
		/// <param name="modelFilter"></param>
		/// <param name="listFilter"></param>
		virtual public void ApplyFilters(IModelFilter modelFilter, IListFilter listFilter)
		{
		}

		#endregion
	}

	/// <summary>This class mimics the behavior of VirtualObjectListView v1.x.</summary>
	public class VirtualListVersion1DataSource : AbstractVirtualListDataSource
	{
		/// <summary>Creates a VirtualListVersion1DataSource</summary>
		/// <param name="listView"></param>
		public VirtualListVersion1DataSource(VirtualObjectListView listView)
			: base(listView)
		{
		}

		#region Public properties

		/// <summary>How will the n'th Object of the data source be fetched?</summary>
		public RowGetterDelegate RowGetter { get; set; }

		#endregion

		#region IVirtualListDataSource implementation

		/// <summary></summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public override Object GetNthObject(Int32 n)
			=> this.RowGetter == null
				? null
				: this.RowGetter(n);

		/// <inheritdoc/>
		public override Int32 SearchText(String value, Int32 first, Int32 last, OLVColumn column)
			=> DefaultSearchText(value, first, last, column, this);

		#endregion
	}
}