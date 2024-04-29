using System;
using System.Collections;
using System.ComponentModel;

namespace BrightIdeasSoftware
{
	/// <summary>A TreeDataSourceAdapter knows how to build a tree structure from a binding list.</summary>
	/// <remarks>To build a tree</remarks>
	public class TreeDataSourceAdapter : DataSourceAdapter
	{
		#region Life and death

		/// <summary>Create a data source adaptor that knows how to build a tree structure</summary>
		/// <param name="tlv"></param>
		public TreeDataSourceAdapter(DataTreeListView tlv)
			: base(tlv)
		{
			this.TreeListView = tlv;
			this.TreeListView.CanExpandGetter = (model) => this.CalculateHasChildren(model);
			this.TreeListView.ChildrenGetter = (model) => this.CalculateChildren(model);
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets the name of the property/column that uniquely identifies each row.</summary>
		/// <remarks>
		/// <para>
		/// The value contained by this column must be unique across all rows in the data source.
		/// Odd and unpredictable things will happen if two rows have the same id.
		/// </para>
		/// <para>Null cannot be a valid key value.</para>
		/// </remarks>
		public virtual String KeyAspectName
		{
			get => this._keyAspectName;
			set
			{
				if(this._keyAspectName == value)
					return;
				this._keyAspectName = value;
				this._keyMunger = new Munger(this.KeyAspectName);
				this.InitializeDataSource();
			}
		}
		private String _keyAspectName;

		/// <summary>Gets or sets the name of the property/column that contains the key of the parent of a row.</summary>
		/// <remarks>
		/// <para>
		/// The test condition for deciding if one row is the parent of another is functionally equivalent to this:
		/// <code>Object.Equals(candidateParentRow[this.KeyAspectName], row[this.ParentKeyAspectName])</code>
		/// </para>
		/// <para>Unlike key value, parent keys can be null but a null parent key can only be used to identify root objects.</para>
		/// </remarks>
		public virtual String ParentKeyAspectName
		{
			get => this._parentKeyAspectName;
			set
			{
				if(this._parentKeyAspectName == value)
					return;
				this._parentKeyAspectName = value;
				this._parentKeyMunger = new Munger(this.ParentKeyAspectName);
				this.InitializeDataSource();
			}
		}
		private String _parentKeyAspectName;

		/// <summary>
		/// Gets or sets the value that identifies a row as a root Object.
		/// When the ParentKey of a row equals the RootKeyValue, that row will be treated as root of the TreeListView.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The test condition for deciding a root Object is functionally equivalent to this:
		/// <code>Object.Equals(candidateRow[this.ParentKeyAspectName], this.RootKeyValue)</code>
		/// </para>
		/// <para>The RootKeyValue can be null.</para>
		/// </remarks>
		public virtual Object RootKeyValue
		{
			get => this._rootKeyValue;
			set
			{
				if(Equals(this._rootKeyValue, value))
					return;
				this._rootKeyValue = value;
				this.InitializeDataSource();
			}
		}
		private Object _rootKeyValue;

		/// <summary>Gets or sets whether or not the key columns (id and parent id) should be shown to the user.</summary>
		/// <remarks>This must be set before the DataSource is set. It has no effect afterwards.</remarks>
		public virtual Boolean ShowKeyColumns { get; set; } = true;

		#endregion

		#region Implementation properties

		/// <summary>Gets the DataTreeListView that is being managed</summary>
		protected DataTreeListView TreeListView { get; }

		#endregion

		#region Implementation

		/// <inheritdoc/>
		protected override void InitializeDataSource()
		{
			base.InitializeDataSource();
			this.TreeListView.RebuildAll(true);
		}

		/// <inheritdoc/>
		protected override void SetListContents()
		{
			this.TreeListView.Roots = this.CalculateRoots();
		}

		/// <inheritdoc/>
		protected override Boolean ShouldCreateColumn(PropertyDescriptor property)
			=> (this.ShowKeyColumns || (property.Name != this.KeyAspectName && property.Name != this.ParentKeyAspectName))// If the property is a key column, and we aren't supposed to show keys, don't show it
				&& base.ShouldCreateColumn(property);

		/// <inheritdoc/>
		protected override void HandleListChangedItemChanged(System.ComponentModel.ListChangedEventArgs e)
		{
			// If the id or the parent id of a row changes, we just rebuild everything.
			// We can't do anything more specific. We don't know what the previous values, so we can't 
			// tell the previous parent to refresh itself. If the id itself has changed, things that used
			// to be children will no longer be children. Just rebuild everything.
			// It seems PropertyDescriptor is only filled in .NET 4 :(
			if(e.PropertyDescriptor != null &&
				(e.PropertyDescriptor.Name == this.KeyAspectName ||
				 e.PropertyDescriptor.Name == this.ParentKeyAspectName))
				this.InitializeDataSource();
			else
				base.HandleListChangedItemChanged(e);
		}

		/// <inheritdoc/>
		protected override void ChangePosition(Int32 index)
		{
			// We can't use our base method directly, since the normal position management
			// doesn't know about our tree structure. They treat our dataset as a flat list
			// but we have a collapsible structure. This means that the 5'th row to them
			// may not even be visible to us

			// To display the n'th row, we have to make sure that all its ancestors
			// are expanded. Then we will be able to select it.
			Object model = this.CurrencyManager.List[index];
			Object parent = this.CalculateParent(model);
			while(parent != null && !this.TreeListView.IsExpanded(parent))
			{
				this.TreeListView.Expand(parent);
				parent = this.CalculateParent(parent);
			}

			base.ChangePosition(index);
		}

		private IEnumerable CalculateRoots()
		{
			foreach(Object x in this.CurrencyManager.List)
			{
				Object parentKey = this.GetParentValue(x);
				if(Object.Equals(this.RootKeyValue, parentKey))
					yield return x;
			}
		}

		private Boolean CalculateHasChildren(Object model)
		{
			Object keyValue = this.GetKeyValue(model);
			if(keyValue == null)
				return false;

			foreach(Object x in this.CurrencyManager.List)
			{
				Object parentKey = this.GetParentValue(x);
				if(Object.Equals(keyValue, parentKey))
					return true;
			}
			return false;
		}

		private IEnumerable CalculateChildren(Object model)
		{
			Object keyValue = this.GetKeyValue(model);
			if(keyValue != null)
			{
				foreach(Object x in this.CurrencyManager.List)
				{
					Object parentKey = this.GetParentValue(x);
					if(Object.Equals(keyValue, parentKey))
						yield return x;
				}
			}
		}

		private Object CalculateParent(Object model)
		{
			Object parentValue = this.GetParentValue(model);
			if(parentValue == null)
				return null;

			foreach(Object x in this.CurrencyManager.List)
			{
				Object key = this.GetKeyValue(x);
				if(Object.Equals(parentValue, key))
					return x;
			}
			return null;
		}

		private Object GetKeyValue(Object model)
			=> this._keyMunger == null ? null : this._keyMunger.GetValue(model);

		private Object GetParentValue(Object model)
			=> this._parentKeyMunger == null ? null : this._parentKeyMunger.GetValue(model);

		#endregion

		private Munger _keyMunger;
		private Munger _parentKeyMunger;
	}
}