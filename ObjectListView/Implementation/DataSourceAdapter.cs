﻿/*
 * DataSourceAdapter - A helper class that translates DataSource events for an ObjectListView
 *
 * Author: Phillip Piper
 * Date: 20/09/2010 7:42 AM
 *
 * Change log:
 * 2018-04-30  JPP  - Sanity check upper limit against CurrencyManager rather than ListView just in
 *                    case the CurrencyManager has gotten ahead of the ListView's contents.
 * v2.9
 * 2015-10-31  JPP  - Put back sanity check on upper limit of source items
 * 2015-02-02  JPP  - Made CreateColumnsFromSource() only rebuild columns when new ones were added
 * v2.8.1
 * 2014-11-23  JPP  - Honour initial CurrencyManager.Position when setting DataSource.
 * 2014-10-27  JPP  - Fix issue where SelectedObject was not sync'ed with CurrencyManager.Position (SF #129)
 * v2.6
 * 2012-08-16  JPP  - Unify common column creation functionality with Generator when possible
 * 
 * 2010-09-20  JPP  - Initial version
 * 
 * Copyright (C) 2010-2014 Phillip Piper
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
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

namespace BrightIdeasSoftware
{
	/// <summary>A helper class that translates DataSource events for an ObjectListView</summary>
	public class DataSourceAdapter : IDisposable
	{
		#region Life and death

		/// <summary>Make a DataSourceAdapter</summary>
		public DataSourceAdapter(ObjectListView olv)
		{
			this.ListView = olv ?? throw new ArgumentNullException(nameof(olv));
			// ReSharper disable once DoNotCallOverridableMethodsInConstructor
			this.BindListView(this.ListView);
		}

		/// <summary>Finalize this Object</summary>
		~DataSourceAdapter()
			=> this.Dispose(false);

		/// <summary>Release all the resources used by this instance</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Release all the resources used by this instance</summary>
		public virtual void Dispose(Boolean fromUser)
		{
			this.UnbindListView(this.ListView);
			this.UnbindDataSource();
		}

		#endregion

		#region Public Properties

		/// <summary>Gets or sets whether or not columns will be automatically generated to show the columns when the DataSource is set.</summary>
		/// <remarks>This must be set before the DataSource is set. It has no effect afterwards.</remarks>
		public Boolean AutoGenerateColumns { get; set; } = true;

		/// <summary>Get or set the DataSource that will be displayed in this list view.</summary>
		public virtual Object DataSource
		{
			get => this._dataSource;
			set
			{
				this._dataSource = value;
				this.RebindDataSource(true);
			}
		}
		private Object _dataSource;

		/// <summary>Gets or sets the name of the list or table in the data source for which the DataListView is displaying data.</summary>
		/// <remarks>If the data source is not a DataSet or DataViewManager, this property has no effect</remarks>
		public virtual String DataMember
		{
			get => this._dataMember;
			set
			{
				if(this._dataMember != value)
				{
					this._dataMember = value;
					this.RebindDataSource();
				}
			}
		}
		private String _dataMember = String.Empty;

		/// <summary>Gets the ObjectListView upon which this adaptor will operate</summary>
		public ObjectListView ListView { get; internal set; }

		#endregion

		#region Implementation properties

		/// <summary>Gets or sets the currency manager which is handling our binding context</summary>
		protected CurrencyManager CurrencyManager { get; set; }

		#endregion

		#region Binding and unbinding

		/// <summary></summary>
		/// <param name="olv"></param>
		protected virtual void BindListView(ObjectListView olv)
		{
			if(olv == null)
				return;

			olv.Freezing += new EventHandler<FreezeEventArgs>(HandleListViewFreezing);
			olv.SelectionChanged += new EventHandler(HandleListViewSelectionChanged);
			olv.BindingContextChanged += new EventHandler(HandleListViewBindingContextChanged);
		}

		/// <summary></summary>
		/// <param name="olv"></param>
		protected virtual void UnbindListView(ObjectListView olv)
		{
			if(olv == null)
				return;

			olv.Freezing -= new EventHandler<FreezeEventArgs>(HandleListViewFreezing);
			olv.SelectionChanged -= new EventHandler(HandleListViewSelectionChanged);
			olv.BindingContextChanged -= new EventHandler(HandleListViewBindingContextChanged);
		}

		/// <summary></summary>
		protected virtual void BindDataSource()
		{
			if(this.CurrencyManager == null)
				return;

			this.CurrencyManager.MetaDataChanged += new EventHandler(HandleCurrencyManagerMetaDataChanged);
			this.CurrencyManager.PositionChanged += new EventHandler(HandleCurrencyManagerPositionChanged);
			this.CurrencyManager.ListChanged += new ListChangedEventHandler(CurrencyManagerListChanged);
		}

		/// <summary></summary>
		protected virtual void UnbindDataSource()
		{
			if(this.CurrencyManager == null)
				return;

			this.CurrencyManager.MetaDataChanged -= new EventHandler(HandleCurrencyManagerMetaDataChanged);
			this.CurrencyManager.PositionChanged -= new EventHandler(HandleCurrencyManagerPositionChanged);
			this.CurrencyManager.ListChanged -= new ListChangedEventHandler(CurrencyManagerListChanged);
		}

		#endregion

		#region Initialization

		/// <summary>Our data source has changed. Figure out how to handle the new source</summary>
		protected virtual void RebindDataSource()
			=> RebindDataSource(false);

		/// <summary>Our data source has changed. Figure out how to handle the new source</summary>
		protected virtual void RebindDataSource(Boolean forceDataInitialization)
		{

			CurrencyManager tempCurrencyManager = null;
			if(this.ListView != null && this.ListView.BindingContext != null && this.DataSource != null)
				tempCurrencyManager = this.ListView.BindingContext[this.DataSource, this.DataMember] as CurrencyManager;

			// Has our currency manager changed?
			if(this.CurrencyManager != tempCurrencyManager)
			{
				this.UnbindDataSource();
				this.CurrencyManager = tempCurrencyManager;
				this.BindDataSource();

				// Our currency manager has changed so we have to initialize a new data source
				forceDataInitialization = true;
			}

			if(forceDataInitialization)
				this.InitializeDataSource();
		}

		/// <summary>The data source for this control has changed. Reconfigure the control for the new source</summary>
		protected virtual void InitializeDataSource()
		{
			if(this.ListView.Frozen || this.CurrencyManager == null)
				return;

			this.CreateColumnsFromSource();
			this.CreateMissingAspectGettersAndPutters();
			this.SetListContents();
			this.ListView.AutoSizeColumns();

			// Fake a position change event so that the control matches any initial Position
			this.HandleCurrencyManagerPositionChanged(null, null);
		}

		/// <summary>Take the contents of the currently bound list and put them into the control</summary>
		protected virtual void SetListContents()
			=> this.ListView.Objects = this.CurrencyManager.List;

		/// <summary>Create columns for the listview based on what properties are available in the data source</summary>
		/// <remarks>
		/// <para>This method will create columns if there is not already a column displaying that property.</para>
		/// </remarks>
		protected virtual void CreateColumnsFromSource()
		{
			if(this.CurrencyManager == null)
				return;

			// Don't generate any columns in design mode. If we do, the user will see them,
			// but the Designer won't know about them and won't persist them, which is very confusing
			if(this.ListView.IsDesignMode)
				return;

			// Don't create columns if we've been told not to
			if(!this.AutoGenerateColumns)
				return;

			// Use a Generator to create columns
			Generator generator = Generator.Instance as Generator ?? new Generator();

			PropertyDescriptorCollection properties = this.CurrencyManager.GetItemProperties();
			if(properties.Count == 0)
				return;

			Boolean wereColumnsAdded = false;
			foreach(PropertyDescriptor property in properties)
			{

				if(!this.ShouldCreateColumn(property))
					continue;

				// Create a column
				OLVColumn column = generator.MakeColumnFromPropertyDescriptor(property);
				this.ConfigureColumn(column, property);

				// Add it to our list
				this.ListView.AllColumns.Add(column);
				wereColumnsAdded = true;
			}

			if(wereColumnsAdded)
				generator.PostCreateColumns(this.ListView);
		}

		/// <summary>Decide if a new column should be added to the control to display the given property</summary>
		/// <param name="property"></param>
		/// <returns></returns>
		protected virtual Boolean ShouldCreateColumn(PropertyDescriptor property)
		{

			// Is there a column that already shows this property? If so, we don't show it again
			if(this.ListView.AllColumns.Exists(delegate (OLVColumn x) { return x.AspectName == property.Name; }))
				return false;

			// Relationships to other tables turn up as IBindibleLists. Don't make columns to show them.
			// CHECK: Is this always true? What other things could be here? Constraints? Triggers?
			if(property.PropertyType == typeof(IBindingList))
				return false;

			// Ignore anything marked with [OLVIgnore]
			return property.Attributes[typeof(OLVIgnoreAttribute)] == null;
		}

		/// <summary>Configure the given column to show the given property.</summary>
		/// <remarks>The title and aspect name of the column are already filled in.</remarks>
		/// <param name="column"></param>
		/// <param name="property"></param>
		protected virtual void ConfigureColumn(OLVColumn column, PropertyDescriptor property)
		{

			column.LastDisplayIndex = this.ListView.AllColumns.Count;

			// If our column is a BLOB, it could be an image, so assign a renderer to draw it.
			// CONSIDER: Is this a common enough case to warrant this code?
			if(property.PropertyType == typeof(System.Byte[]))
				column.Renderer = new ImageRenderer();
		}

		/// <summary>
		/// Generate aspect getters and putters for any columns that are missing them (and for which we have
		/// enough information to actually generate a getter)
		/// </summary>
		protected virtual void CreateMissingAspectGettersAndPutters()
		{
			foreach(OLVColumn x in this.ListView.AllColumns)
			{
				OLVColumn column = x; // stack based variable accessible from closures
				if(column.AspectGetter == null && !String.IsNullOrEmpty(column.AspectName))
				{
					column.AspectGetter = row =>
					{
						// In most cases, rows will be DataRowView objects
						return row is DataRowView drv
							? (drv.Row.RowState == DataRowState.Detached) ? null : drv[column.AspectName]
							: column.GetAspectByName(row);
					};
				}
				if(column.IsEditable && column.AspectPutter == null && !String.IsNullOrEmpty(column.AspectName))
				{
					column.AspectPutter = (row, newValue) =>
					{
						// In most cases, rows will be DataRowView objects
						if(row is DataRowView drv)
						{
							if(drv.Row.RowState != DataRowState.Detached)
								drv[column.AspectName] = newValue;
						} else
							column.PutAspectByName(row, newValue);
					};
				}
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// CurrencyManager ListChanged event handler.
		/// Deals with fine-grained changes to list items.
		/// </summary>
		/// <remarks>
		/// It's actually difficult to deal with these changes in a fine-grained manner.
		/// If our listview is grouped, then any change may make a new group appear or
		/// an old group disappear. It is rarely enough to simply update the affected row.
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void CurrencyManagerListChanged(Object sender, ListChangedEventArgs e)
		{
			Debug.Assert(sender == this.CurrencyManager);

			// Ignore changes make while frozen, since we will do a complete rebuild when we unfreeze
			if(this.ListView.Frozen)
				return;

			//System.Diagnostics.Debug.WriteLine(e.ListChangedType);
			Stopwatch sw = Stopwatch.StartNew();
			switch(e.ListChangedType)
			{
			case ListChangedType.Reset:
				this.HandleListChangedReset(e);
				break;

			case ListChangedType.ItemChanged:
				this.HandleListChangedItemChanged(e);
				break;

			case ListChangedType.ItemAdded:
				this.HandleListChangedItemAdded(e);
				break;

			// An item has gone away.
			case ListChangedType.ItemDeleted:
				this.HandleListChangedItemDeleted(e);
				break;

			// An item has changed its index.
			case ListChangedType.ItemMoved:
				this.HandleListChangedItemMoved(e);
				break;

			// Something has changed in the metadata.
			// CHECK: When are these events actually fired?
			case ListChangedType.PropertyDescriptorAdded:
			case ListChangedType.PropertyDescriptorChanged:
			case ListChangedType.PropertyDescriptorDeleted:
				this.HandleListChangedMetadataChanged(e);
				break;
			}
			sw.Stop();
			Debug.WriteLine(String.Format("PERF - Processing {0} event on {1} rows took {2}ms", e.ListChangedType, this.ListView.GetItemCount(), sw.ElapsedMilliseconds));

		}

		/// <summary>Handle PropertyDescriptor* events</summary>
		/// <param name="e"></param>
		protected virtual void HandleListChangedMetadataChanged(ListChangedEventArgs e)
			=> this.InitializeDataSource();

		/// <summary>Handle ItemMoved event</summary>
		/// <param name="e"></param>
		protected virtual void HandleListChangedItemMoved(ListChangedEventArgs e)
			=> this.InitializeDataSource();// When is this actually triggered?

		/// <summary>Handle the ItemDeleted event</summary>
		/// <param name="e"></param>
		protected virtual void HandleListChangedItemDeleted(ListChangedEventArgs e)
			=> this.InitializeDataSource();

		/// <summary>Handle an ItemAdded event.</summary>
		/// <param name="e"></param>
		protected virtual void HandleListChangedItemAdded(ListChangedEventArgs e)
		{
			// We get this event twice if certain grid controls are used to add a new row to a
			// datatable: once when the editing of a new row begins, and once again when that
			// editing commits. (If the user cancels the creation of the new row, we never see
			// the second creation.) We detect this by seeing if this is a view on a row in a
			// DataTable, and if it is, testing to see if it's a new row under creation.

			Object newRow = this.CurrencyManager.List[e.NewIndex];
			DataRowView drv = newRow as DataRowView;
			if(drv == null || !drv.IsNew)
			{
				// Either we're not dealing with a view on a data table, or this is the commit
				// notification. Either way, this is the final notification, so we want to
				// handle the new row now!
				this.InitializeDataSource();
			}
		}

		/// <summary>Handle the Reset event</summary>
		/// <param name="e"></param>
		protected virtual void HandleListChangedReset(ListChangedEventArgs e)
			=> this.InitializeDataSource();// The whole list has changed utterly, so reload it.

		/// <summary>
		/// Handle ItemChanged event.
		/// This is triggered when a single item has changed, so just refresh that one item.
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>Even in this simple case, we should probably rebuild the list.
		/// For example, the change could put the item into its own new group.</remarks>
		protected virtual void HandleListChangedItemChanged(ListChangedEventArgs e)
		{
			// A single item has changed, so just refresh that.
			//System.Diagnostics.Debug.WriteLine(String.Format("HandleListChangedItemChanged: {0}, {1}", e.NewIndex, e.PropertyDescriptor.Name));

			Object changedRow = this.CurrencyManager.List[e.NewIndex];
			this.ListView.RefreshObject(changedRow);
		}

		/// <summary>The CurrencyManager calls this if the data source looks different. We just reload everything.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>
		/// CHECK: Do we need this if we are handle ListChanged metadata events?
		/// </remarks>
		protected virtual void HandleCurrencyManagerMetaDataChanged(Object sender, EventArgs e)
			=> this.InitializeDataSource();

		/// <summary>
		/// Called by the CurrencyManager when the currently selected item
		/// changes. We update the ListView selection so that we stay in sync
		/// with any other controls bound to the same source.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void HandleCurrencyManagerPositionChanged(Object sender, EventArgs e)
		{
			Int32 index = this.CurrencyManager.Position;

			// Make sure the index is sane (-1 pops up from time to time)
			if(index < 0 || index >= this.CurrencyManager.Count)
				return;

			// Avoid recursion. If we are currently changing the index, don't
			// start the process again.
			if(this._isChangingIndex)
				return;

			try
			{
				this._isChangingIndex = true;
				this.ChangePosition(index);
			} finally
			{
				this._isChangingIndex = false;
			}
		}
		private Boolean _isChangingIndex = false;

		/// <summary>Change the control's position (which is it's currently selected row) to the nth row in the dataset</summary>
		/// <param name="index">The index of the row to be selected</param>
		protected virtual void ChangePosition(Int32 index)
		{
			// We can't use the index directly, since our listview may be sorted
			this.ListView.SelectedObject = this.CurrencyManager.List[index];

			// THINK: Do we always want to bring it into view?
			if(this.ListView.SelectedIndices.Count > 0)
				this.ListView.EnsureVisible(this.ListView.SelectedIndices[0]);
		}

		#endregion

		#region ObjectListView event handlers

		/// <summary>Handle the selection changing in our ListView.</summary>
		/// <remarks>We need to tell our currency manager about the new position.</remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void HandleListViewSelectionChanged(Object sender, EventArgs e)
		{
			// Prevent recursion
			if(this._isChangingIndex)
				return;

			// Sanity 
			if(this.CurrencyManager == null)
				return;

			// If only one item is selected, tell the currency manager which item is selected.
			// CurrencyManager can't handle multiple selection so there's nothing we can do 
			// if more than one row is selected.
			if(this.ListView.SelectedIndices.Count != 1)
				return;

			try
			{
				this._isChangingIndex = true;

				// We can't use the selectedIndex directly, since our listview may be sorted and/or filtered
				// So we have to find the index of the selected Object within the original list.
				this.CurrencyManager.Position = this.CurrencyManager.List.IndexOf(this.ListView.SelectedObject);
			} finally
			{
				this._isChangingIndex = false;
			}
		}

		/// <summary>Handle the frozenness of our ListView changing.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void HandleListViewFreezing(Object sender, FreezeEventArgs e)
		{
			if(!_alreadyFreezing && e.FreezeLevel == 0)
			{
				try
				{
					_alreadyFreezing = true;
					this.RebindDataSource(true);
				} finally
				{
					_alreadyFreezing = false;
				}
			}
		}
		private Boolean _alreadyFreezing = false;

		/// <summary>Handle a change to the BindingContext of our ListView.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void HandleListViewBindingContextChanged(Object sender, EventArgs e)
			=> this.RebindDataSource(false);

		#endregion
	}
}