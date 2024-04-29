/*
 * Virtual groups - Classes and interfaces needed to implement virtual groups
 *
 * Author: Phillip Piper
 * Date: 28/08/2009 11:10am
 *
 * Change log:
 * 2011-02-21   JPP  - Correctly honor group comparer and collapsible groups settings
 * v2.3
 * 2009-08-28   JPP  - Initial version
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
using System.Runtime.InteropServices;

namespace BrightIdeasSoftware
{
	/// <summary>A IVirtualGroups is the interface that a virtual list must implement to support virtual groups</summary>
	public interface IVirtualGroups
	{
		/// <summary>Return the list of groups that should be shown according to the given parameters</summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		IList<OLVGroup> GetGroups(GroupingParameters parameters);

		/// <summary>Return the index of the item that appears at the given position within the given group.</summary>
		/// <param name="group"></param>
		/// <param name="indexWithinGroup"></param>
		/// <returns></returns>
		Int32 GetGroupMember(OLVGroup group, Int32 indexWithinGroup);

		/// <summary>Return the index of the group to which the given item belongs</summary>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		Int32 GetGroup(Int32 itemIndex);

		/// <summary>Return the index at which the given item is shown in the given group</summary>
		/// <param name="group"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		Int32 GetIndexWithinGroup(OLVGroup group, Int32 itemIndex);

		/// <summary>A hint that the given range of items are going to be required</summary>
		/// <param name="fromGroupIndex"></param>
		/// <param name="fromIndex"></param>
		/// <param name="toGroupIndex"></param>
		/// <param name="toIndex"></param>
		void CacheHint(Int32 fromGroupIndex, Int32 fromIndex, Int32 toGroupIndex, Int32 toIndex);
	}

	/// <summary>This is a safe, do nothing implementation of a grouping strategy</summary>
	public class AbstractVirtualGroups : IVirtualGroups
	{
		/// <summary>Return the list of groups that should be shown according to the given parameters</summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public virtual IList<OLVGroup> GetGroups(GroupingParameters parameters)
			=> new List<OLVGroup>();

		/// <summary>Return the index of the item that appears at the given position within the given group.</summary>
		/// <param name="group"></param>
		/// <param name="indexWithinGroup"></param>
		/// <returns></returns>
		public virtual Int32 GetGroupMember(OLVGroup group, Int32 indexWithinGroup)
			=> -1;

		/// <summary>Return the index of the group to which the given item belongs</summary>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public virtual Int32 GetGroup(Int32 itemIndex)
			=> -1;

		/// <summary>Return the index at which the given item is shown in the given group</summary>
		/// <param name="group"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public virtual Int32 GetIndexWithinGroup(OLVGroup group, Int32 itemIndex)
			=> -1;

		/// <summary>A hint that the given range of items are going to be required</summary>
		/// <param name="fromGroupIndex"></param>
		/// <param name="fromIndex"></param>
		/// <param name="toGroupIndex"></param>
		/// <param name="toIndex"></param>
		public virtual void CacheHint(Int32 fromGroupIndex, Int32 fromIndex, Int32 toGroupIndex, Int32 toIndex)
		{
		}
	}

	/// <summary>Provides grouping functionality to a FastObjectListView</summary>
	public class FastListGroupingStrategy : AbstractVirtualGroups
	{
		/// <summary>Create groups for FastListView</summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public override IList<OLVGroup> GetGroups(GroupingParameters parameters)
		{

			// There is a lot of overlap between this method and ObjectListView.MakeGroups()
			// Any changes made here may need to be reflected there

			// This strategy can only be used on FastObjectListViews
			FastObjectListView folv = (FastObjectListView)parameters.ListView;

			// Separate the list view items into groups, using the group key as the descrimanent
			Int32 objectCount = 0;
			NullableDictionary<Object, List<Object>> map = new NullableDictionary<Object, List<Object>>();
			foreach(Object model in folv.FilteredObjects)
			{
				Object key = parameters.GroupByColumn.GetGroupKey(model);
				if(!map.ContainsKey(key))
					map[key] = new List<Object>();
				map[key].Add(model);
				objectCount++;
			}

			// Sort the items within each group
			OLVColumn primarySortColumn = parameters.SortItemsByPrimaryColumn ? parameters.ListView.GetColumn(0) : parameters.PrimarySort;
			ModelObjectComparer sorter = new ModelObjectComparer(primarySortColumn, parameters.PrimarySortOrder,
				parameters.SecondarySort, parameters.SecondarySortOrder);
			foreach(Object key in map.Keys)
				map[key].Sort(sorter);

			// Make a list of the required groups
			List<OLVGroup> groups = new List<OLVGroup>();
			foreach(Object key in map.Keys)
			{
				OLVGroup lvg = parameters.CreateGroup(key, map[key].Count, folv.HasCollapsibleGroups);
				lvg.Contents = map[key].ConvertAll<Int32>(x => folv.IndexOf(x));
				lvg.VirtualItemCount = map[key].Count;
				parameters.GroupByColumn.GroupFormatter?.Invoke(lvg, parameters);
				groups.Add(lvg);
			}

			// Sort the groups
			if(parameters.GroupByOrder != SortOrder.None)
				groups.Sort(parameters.GroupComparer ?? new OLVGroupComparer(parameters.GroupByOrder));

			// Build an array that remembers which group each item belongs to.
			this.indexToGroupMap = new List<Int32>(objectCount);
			this.indexToGroupMap.AddRange(new Int32[objectCount]);

			for(Int32 i = 0; i < groups.Count; i++)
			{
				OLVGroup group = groups[i];
				List<Int32> members = (List<Int32>)group.Contents;
				foreach(Int32 j in members)
					this.indexToGroupMap[j] = i;
			}

			return groups;
		}
		private List<Int32> indexToGroupMap;

		/// <inheritdoc/>
		public override Int32 GetGroupMember(OLVGroup group, Int32 indexWithinGroup)
			=> (Int32)group.Contents[indexWithinGroup];

		/// <inheritdoc/>
		public override Int32 GetGroup(Int32 itemIndex)
			=> this.indexToGroupMap[itemIndex];

		/// <inheritdoc/>
		public override Int32 GetIndexWithinGroup(OLVGroup group, Int32 itemIndex)
			=> group.Contents.IndexOf(itemIndex);
	}

	/// <summary>This is the COM interface that a ListView must be given in order for groups in virtual lists to work.</summary>
	/// <remarks>
	/// This interface is NOT documented by MS. It was found on Greg Chapell's site. This means that there is
	/// no guarantee that it will work on future versions of Windows, nor continue to work on current ones.
	/// </remarks>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("44C09D56-8D3B-419D-A462-7B956B105B47")]
	internal interface IOwnerDataCallback
	{
		/// <summary>Not sure what this does</summary>
		/// <param name="i"></param>
		/// <param name="pt"></param>
		void GetItemPosition(Int32 i, out NativeMethods.POINT pt);

		/// <summary>Not sure what this does</summary>
		/// <param name="t"></param>
		/// <param name="pt"></param>
		void SetItemPosition(Int32 t, NativeMethods.POINT pt);

		/// <summary>Get the index of the item that occurs at the n'th position of the indicated group.</summary>
		/// <param name="groupIndex">Index of the group</param>
		/// <param name="n">Index within the group</param>
		/// <param name="itemIndex">Index of the item within the whole list</param>
		void GetItemInGroup(Int32 groupIndex, Int32 n, out Int32 itemIndex);

		/// <summary>Get the index of the group to which the given item belongs</summary>
		/// <param name="itemIndex">Index of the item within the whole list</param>
		/// <param name="occurrenceCount">Which occurrences of the item is wanted</param>
		/// <param name="groupIndex">Index of the group</param>
		void GetItemGroup(Int32 itemIndex, Int32 occurrenceCount, out Int32 groupIndex);

		/// <summary>Get the number of groups that contain the given item</summary>
		/// <param name="itemIndex">Index of the item within the whole list</param>
		/// <param name="occurrenceCount">How many groups does it occur within</param>
		void GetItemGroupCount(Int32 itemIndex, out Int32 occurrenceCount);

		/// <summary>A hint to prepare any cache for the given range of requests</summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		void OnCacheHint(NativeMethods.LVITEMINDEX from, NativeMethods.LVITEMINDEX to);
	}

	/// <summary>A default implementation of the IOwnerDataCallback interface</summary>
	[Guid("6FC61F50-80E8-49b4-B200-3F38D3865ABD")]
	internal class OwnerDataCallbackImpl : IOwnerDataCallback
	{
		public OwnerDataCallbackImpl(VirtualObjectListView olv)
			=> this._olv = olv;

		VirtualObjectListView _olv;

		#region IOwnerDataCallback Members

		public void GetItemPosition(Int32 i, out NativeMethods.POINT pt)
			=> throw new NotSupportedException();//System.Diagnostics.Debug.WriteLine("GetItemPosition");

		public void SetItemPosition(Int32 t, NativeMethods.POINT pt)
			=> throw new NotSupportedException();//System.Diagnostics.Debug.WriteLine("SetItemPosition");

		public void GetItemInGroup(Int32 groupIndex, Int32 n, out Int32 itemIndex)
		{
			//System.Diagnostics.Debug.WriteLine(String.Format("-> GetItemInGroup({0}, {1})", groupIndex, n));
			itemIndex = this._olv.GroupingStrategy.GetGroupMember(this._olv.OLVGroups[groupIndex], n);
			//System.Diagnostics.Debug.WriteLine(String.Format("<- {0}", itemIndex));
		}

		public void GetItemGroup(Int32 itemIndex, Int32 occurrenceCount, out Int32 groupIndex)
		{
			//System.Diagnostics.Debug.WriteLine(String.Format("GetItemGroup({0}, {1})", itemIndex, occurrenceCount));
			groupIndex = this._olv.GroupingStrategy.GetGroup(itemIndex);
			//System.Diagnostics.Debug.WriteLine(String.Format("<- {0}", groupIndex));
		}

		public void GetItemGroupCount(Int32 itemIndex, out Int32 occurrenceCount)
			=> occurrenceCount = 1;//System.Diagnostics.Debug.WriteLine(String.Format("GetItemGroupCount({0})", itemIndex));

		public void OnCacheHint(NativeMethods.LVITEMINDEX from, NativeMethods.LVITEMINDEX to)

			=> this._olv.GroupingStrategy.CacheHint(from.iGroup, from.iItem, to.iGroup, to.iItem);//System.Diagnostics.Debug.WriteLine(String.Format("OnCacheHint({0}, {1}, {2}, {3})", from.iGroup, from.iItem, to.iGroup, to.iItem));

		#endregion
	}
}