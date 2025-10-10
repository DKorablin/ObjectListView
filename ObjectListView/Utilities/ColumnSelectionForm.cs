/*
 * ColumnSelectionForm - A utility form that allows columns to be rearranged and/or hidden
 *
 * Author: Phillip Piper
 * Date: 1/04/2011 11:15 AM
 *
 * Change log:
 * 2013-04-21  JPP  - Fixed obscure bug in column re-ordered. Thanks to Edwin Chen.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
	/// <summary>
	/// This form is an example of how an application could allows the user to select which columns 
	/// an ObjectListView will display, as well as select which order the columns are displayed in.
	/// </summary>
	/// <remarks>
	/// <para>In Tile view, ColumnHeader.DisplayIndex does nothing. To reorder the columns you have
	/// to change the order of objects in the Columns property.</para>
	/// <para>Remember that the first column is special!
	/// It has to remain the first column.</para>
	/// </remarks>
	public partial class ColumnSelectionForm : Form
	{
		/// <summary>Make a new ColumnSelectionForm</summary>
		public ColumnSelectionForm()
			=> this.InitializeComponent();

		/// <summary>Open this form so it will edit the columns that are available in the ListView's current view</summary>
		/// <param name="olv">The ObjectListView whose columns are to be altered</param>
		public void OpenOn(ObjectListView olv)
			=> this.OpenOn(olv, olv.View);

		/// <summary>Open this form so it will edit the columns that are available in the given ListView when the ListView is showing the given type of view.</summary>
		/// <param name="olv">The ObjectListView whose columns are to be altered</param>
		/// <param name="view">The view that is to be altered. Must be View.Details or View.Tile</param>
		public void OpenOn(ObjectListView olv, View view)
		{
			if(view != View.Details && view != View.Tile)
				return;

			this.InitializeForm(olv, view);
			if(this.ShowDialog() == DialogResult.OK)
				this.Apply(olv, view);
		}

		/// <summary>Initialize the form to show the columns of the given view</summary>
		/// <param name="olv"></param>
		/// <param name="view"></param>
		protected void InitializeForm(ObjectListView olv, View view)
		{
			this.AllColumns = olv.AllColumns;
			this.RearrangeableColumns = new List<OLVColumn>(this.AllColumns);
			foreach(OLVColumn col in this.RearrangeableColumns)
				this.MapColumnToVisible[col] = view == View.Details
					? col.IsVisible
					: col.IsTileViewColumn;

			this.RearrangeableColumns.Sort(new SortByDisplayOrder(this));

			this.objectListView1.BooleanCheckStateGetter = (Object rowObject)
				=> this.MapColumnToVisible[(OLVColumn)rowObject];

			this.objectListView1.BooleanCheckStatePutter = delegate (Object rowObject, Boolean newValue)
			{
				// Some columns should always be shown, so ignore attempts to hide them
				OLVColumn column = (OLVColumn)rowObject;
				if(!column.CanBeHidden)
					return true;

				this.MapColumnToVisible[column] = newValue;
				this.EnableControls();
				return newValue;
			};

			this.objectListView1.SetObjects(this.RearrangeableColumns);
			this.EnableControls();
		}
		private List<OLVColumn> AllColumns = null;
		private List<OLVColumn> RearrangeableColumns = new List<OLVColumn>();
		private readonly Dictionary<OLVColumn, Boolean> MapColumnToVisible = new Dictionary<OLVColumn, Boolean>();

		/// <summary>The user has pressed OK. Do what's required.</summary>
		/// <param name="olv"></param>
		/// <param name="view"></param>
		protected void Apply(ObjectListView olv, View view)
		{
			olv.Freeze();

			// Update the column definitions to reflect whether they have been hidden
			if(view == View.Details)
				foreach(OLVColumn col in olv.AllColumns)
					col.IsVisible = this.MapColumnToVisible[col];
			else
				foreach(OLVColumn col in olv.AllColumns)
					col.IsTileViewColumn = this.MapColumnToVisible[col];

			// Collect the columns are still visible
			List<OLVColumn> visibleColumns = this.RearrangeableColumns.FindAll(x => this.MapColumnToVisible[x]);

			// Detail view and Tile view have to be handled in different ways.
			if(view == View.Details)
			{
				// Of the still visible columns, change DisplayIndex to reflect their position in the rearranged list
				olv.ChangeToFilteredColumns(view);
				foreach(OLVColumn col in visibleColumns)
				{
					col.DisplayIndex = visibleColumns.IndexOf(col);
					col.LastDisplayIndex = col.DisplayIndex;
				}
			} else
			{
				// In Tile view, DisplayOrder does nothing. So to change the display order, we have to change the 
				// order of the columns in the Columns property.
				// Remember, the primary column is special and has to remain first!
				OLVColumn primaryColumn = this.AllColumns[0];
				visibleColumns.Remove(primaryColumn);

				olv.Columns.Clear();
				olv.Columns.Add(primaryColumn);
				olv.Columns.AddRange(visibleColumns.ToArray());
				olv.CalculateReasonableTileSize();
			}

			olv.Unfreeze();
		}

		#region Event handlers

		private void buttonMoveUp_Click(Object sender, EventArgs e)
		{
			Int32 selectedIndex = this.objectListView1.SelectedIndices[0];
			OLVColumn col = this.RearrangeableColumns[selectedIndex];
			this.RearrangeableColumns.RemoveAt(selectedIndex);
			this.RearrangeableColumns.Insert(selectedIndex - 1, col);

			this.objectListView1.BuildList();

			this.EnableControls();
		}

		private void buttonMoveDown_Click(Object sender, EventArgs e)
		{
			Int32 selectedIndex = this.objectListView1.SelectedIndices[0];
			OLVColumn col = this.RearrangeableColumns[selectedIndex];
			this.RearrangeableColumns.RemoveAt(selectedIndex);
			this.RearrangeableColumns.Insert(selectedIndex + 1, col);

			this.objectListView1.BuildList();

			this.EnableControls();
		}

		private void buttonShow_Click(Object sender, EventArgs e)
			=> this.objectListView1.SelectedItem.Checked = true;

		private void buttonHide_Click(Object sender, EventArgs e)
			=> this.objectListView1.SelectedItem.Checked = false;

		private void buttonOK_Click(Object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void buttonCancel_Click(Object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void objectListView1_SelectionChanged(Object sender, EventArgs e)
			=> this.EnableControls();

		#endregion

		#region Control enabling

		/// <summary>Enable the controls on the dialog to match the current state</summary>
		protected void EnableControls()
		{
			if(this.objectListView1.SelectedIndices.Count == 0)
			{
				this.buttonMoveUp.Enabled = false;
				this.buttonMoveDown.Enabled = false;
				this.buttonShow.Enabled = false;
				this.buttonHide.Enabled = false;
			} else
			{
				// Can't move the first row up or the last row down
				this.buttonMoveUp.Enabled = (this.objectListView1.SelectedIndices[0] != 0);
				this.buttonMoveDown.Enabled = (this.objectListView1.SelectedIndices[0] < (this.objectListView1.GetItemCount() - 1));

				OLVColumn selectedColumn = (OLVColumn)this.objectListView1.SelectedObject;

				// Some columns cannot be hidden (and hence cannot be Shown)
				this.buttonShow.Enabled = !this.MapColumnToVisible[selectedColumn] && selectedColumn.CanBeHidden;
				this.buttonHide.Enabled = this.MapColumnToVisible[selectedColumn] && selectedColumn.CanBeHidden;
			}
		}
		#endregion

		/// <summary>A Comparer that will sort a list of columns so that visible ones come before hidden ones, and that are ordered by their display order.</summary>
		private sealed class SortByDisplayOrder : IComparer<OLVColumn>
		{
			public SortByDisplayOrder(ColumnSelectionForm form)
				=> this._form = form;

			private readonly ColumnSelectionForm _form;

			Int32 IComparer<OLVColumn>.Compare(OLVColumn x, OLVColumn y)
			{
				if(this._form.MapColumnToVisible[x] && !this._form.MapColumnToVisible[y])
					return -1;

				if(!this._form.MapColumnToVisible[x] && this._form.MapColumnToVisible[y])
					return 1;

				return x.DisplayIndex == y.DisplayIndex
					? x.Text.CompareTo(y.Text)
					: x.DisplayIndex - y.DisplayIndex;
			}
		}
	}
}