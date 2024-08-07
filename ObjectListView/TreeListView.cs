/*
 * TreeListView - A listview that can show a tree of objects in a column
 *
 * Author: Phillip Piper
 * Date: 23/09/2008 11:15 AM
 * 
 * Change log:
 * 2018-05-03  JPP  - Added ITreeModel to allow models to provide the required information to TreeListView.
 * 2018-04-30  JPP  - Fix small visual glitch where connecting lines were not correctly drawn when filters changed
 * v2.9.2
 * 2016-06-02  JPP  - Added bounds check to GetNthObject().
 * v2.9
 * 2015-08-02  JPP  - Fixed buy with hierarchical checkboxes where setting the checkedness of a deeply
 *                    nested Object would sometimes not correctly calculate the changes in the hierarchy. SF #150.
 * 2015-06-27  JPP  - Corrected small UI glitch when focus was lost and HideSelection was false. SF #135.
 * v2.8.1
 * 2014-11-28  JPP  - Fixed issue in RefreshObject() where a model with less children than previous that could not
 *                    longer be expanded would cause an exception.
 * 2014-11-23  JPP  - Fixed an issue where collapsing a branch could leave the internal Object->index map out of date.
 * v2.8
 * 2014-10-08  JPP  - Fixed an issue where pre-expanded branches would not initially expand properly
 * 2014-09-29  JPP  - Fixed issue where RefreshObject() on a root Object could cause exceptions
 *                  - Fixed issue where CollapseAll() while filtering could cause exception
 * 2014-03-09  JPP  - Fixed issue where removing a branches only child and then calling RefreshObject()
 *                    could throw an exception.
 * v2.7
 * 2014-02-23  JPP  - Added Reveal() method to show a deeply nested models.
 * 2014-02-05  JPP  - Fix issue where refreshing a non-root item would collapse all expanded children of that item
 * 2014-02-01  JPP  - ClearObjects() now actually, you know, clears objects :)
 *                  - Corrected issue where Expanded event was being raised twice.
 *                  - RebuildChildren() no longer checks if CanExpand is true before rebuilding.
 * 2014-01-16  JPP  - Corrected an off-by-1 error in hit detection, which meant that clicking in the last 16 pixels
 *                    of an items label was being ignored.
 * 2013-11-20  JPP  - Moved event triggers into Collapse() and Expand() so that the events are always triggered.
 *                  - CheckedObjects now includes objects that are in a branch that is currently collapsed
 *                  - CollapseAll() and ExpandAll() now trigger cancellable events
 * 2013-09-29  JPP  - Added TreeFactory to allow the underlying Tree to be replaced by another implementation.
 * 2013-09-23  JPP  - Fixed long standing issue where RefreshObject() would not work on root objects
 *                    which overrode Equals()/GetHashCode().
 * 2013-02-23  JPP  - Added HierarchicalCheckboxes. When this is true, the checkedness of a parent
 *                    is an synopsis of the checkedness of its children. When all children are checked,
 *                    the parent is checked. When all children are unchecked, the parent is unchecked.
 *                    If some children are checked and some are not, the parent is indeterminate.
 * v2.6
 * 2012-10-25  JPP  - Circumvent annoying issue in ListView control where changing
 *                    selection would leave artefacts on the control.
 * 2012-08-10  JPP  - Don't trigger selection changed events during expands
 * 
 * v2.5.1
 * 2012-04-30  JPP  - Fixed issue where CheckedObjects would return model objects that had been filtered out.
 *                  - Allow any column to render the tree, not just column 0 (still not sure about this one)
 * v2.5.0
 * 2011-04-20  JPP  - Added ExpandedObjects property and RebuildAll() method.
 * 2011-04-09  JPP  - Added Expanding, Collapsing, Expanded and Collapsed events.
 *                    The ..ing events are cancellable. These are only fired in response
 *                    to user actions.
 * v2.4.1
 * 2010-06-15  JPP  - Fixed issue in Tree.RemoveObjects() which resulted in removed objects
 *                    being reported as still existing.
 * v2.3
 * 2009-09-01  JPP  - Fixed off-by-one error that was messing up hit detection
 * 2009-08-27  JPP  - Fixed issue when dragging a node from one place to another in the tree
 * v2.2.1
 * 2009-07-14  JPP  - Clicks to the left of the expander in tree cells are now ignored.
 * v2.2
 * 2009-05-12  JPP  - Added tree traverse operations: GetParent and GetChildren.
 *                  - Added DiscardAllState() to completely reset the TreeListView.
 * 2009-05-10  JPP  - Removed all unsafe code
 * 2009-05-09  JPP  - Fixed issue where any command (Expand/Collapse/Refresh) on a model
 *                    Object that was once visible but that is currently in a collapsed branch
 *                    would cause the control to crash.
 * 2009-05-07  JPP  - Fixed issue where RefreshObjects() would fail when none of the given
 *                    objects were present/visible.
 * 2009-04-20  JPP  - Fixed issue where calling Expand() on an already expanded branch confused
 *                    the display of the children (SF#2499313)
 * 2009-03-06  JPP  - Calculate edit rectangle on column 0 more accurately
 * v2.1
 * 2009-02-24  JPP  - All commands now work when the list is empty (SF #2631054)
 *                  - TreeListViews can now be printed with ListViewPrinter
 * 2009-01-27  JPP  - Changed to use new Renderer and HitTest scheme
 * 2009-01-22  JPP  - Added RevealAfterExpand property. If this is true (the default),
 *                    after expanding a branch, the control scrolls to reveal as much of the
 *                    expanded branch as possible.
 * 2009-01-13  JPP  - Changed TreeRenderer to work with visual styles are disabled
 * v2.0.1
 * 2009-01-07  JPP  - Made all public and protected methods virtual 
 *                  - Changed some classes from 'internal' to 'protected' so that they
 *                    can be accessed by subclasses of TreeListView.
 * 2008-12-22  JPP  - Added UseWaitCursorWhenExpanding property
 *                  - Made TreeRenderer public so that it can be subclassed
 *                  - Added LinePen property to TreeRenderer to allow the connection drawing 
 *                    pen to be changed 
 *                  - Fixed some rendering issues where the text highlight rect was miscalculated
 *                  - Fixed connection line problem when there is only a single root
 * v2.0
 * 2008-12-10  JPP  - Expand/collapse with mouse now works when there is no SmallImageList.
 * 2008-12-01  JPP  - Search-by-typing now works.
 * 2008-11-26  JPP  - Corrected calculation of expand/collapse icon (SF#2338819)
 *                  - Fixed ugliness with dotted lines in renderer (SF#2332889)
 *                  - Fixed problem with custom selection colors (SF#2338805)
 * 2008-11-19  JPP  - Expand/collapse now preserve the selection -- more or less :)
 *                  - Overrode RefreshObjects() to rebuild the given objects and their children
 * 2008-11-05  JPP  - Added ExpandAll() and CollapseAll() commands
 *                  - CanExpand is no longer cached
 *                  - Renamed InitialBranches to RootModels since it deals with model objects
 * 2008-09-23  JPP  Initial version
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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
	/// <summary>A TreeListView combines an expandable tree structure with list view columns.</summary>
	/// <remarks>
	/// <para>To support tree operations, two delegates must be provided:</para>
	/// <list type="table">
	/// <item>
	/// <term>CanExpandGetter</term> 
	/// <description>
	/// This delegate must accept a model Object and return a boolean indicating
	/// if that model should be expandable. 
	/// </description>
	/// </item>
	/// <item>
	/// <term>ChildrenGetter</term>
	/// <description>
	/// This delegate must accept a model Object and return an IEnumerable of model
	/// objects that will be displayed as children of the parent model. This delegate will only be called
	/// for a model Object if the CanExpandGetter has already returned true for that model.
	/// </description>
	/// </item>
	/// <item>
	/// <term>ParentGetter</term> 
	/// <description>
	/// This delegate must accept a model Object and return the parent model. 
	/// This delegate will only be called when HierarchicalCheckboxes is true OR when Reveal() is called. 
	/// </description>
	/// </item>
	/// </list>
	/// <para>
	/// The top level branches of the tree are set via the Roots property. SetObjects(), AddObjects() 
	/// and RemoveObjects() are interpreted as operations on this collection of roots.
	/// </para>
	/// <para>
	/// To add new children to an existing branch, make changes to your model objects and then
	/// call RefreshObject() on the parent.
	/// </para>
	/// <para>The tree must be a directed acyclic graph -- no cycles are allowed. Put more mundanely, 
	/// each model Object must appear only once in the tree. If the same model Object appears in two
	/// places in the tree, the control will become confused.</para>
	/// </remarks>
	public partial class TreeListView : VirtualObjectListView
	{
		/// <summary>Make a default TreeListView</summary>
		public TreeListView()
		{
			this.OwnerDraw = true;
			this.View = View.Details;
			this.CheckedObjectsMustStillExistInList = false;

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			this.RegenerateTree();
			this.TreeColumnRenderer = new TreeRenderer();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor

			// This improves hit detection even if we don't have any state image
			this.SmallImageList = new ImageList();
			// this.StateImageList.ImageSize = new Size(6, 6);
		}

		//------------------------------------------------------------------------------------------
		// Properties

		/// <summary>This is the delegate that will be used to decide if a model Object can be expanded.</summary>
		/// <remarks>
		/// <para>
		/// This is called *often* -- on every mouse move when required. It must be fast.
		/// Don't do database lookups, linear searches, or pi calculations. Just return the
		/// value of a property.
		/// </para>
		/// <para>
		/// When this delegate is called, the TreeListView is not in a stable state. Don't make
		/// calls back into the control.
		/// </para>
		/// </remarks>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual CanExpandGetterDelegate CanExpandGetter
		{
			get => this.TreeModel.CanExpandGetter;
			set => this.TreeModel.CanExpandGetter = value;
		}

		/// <summary>Gets whether or not this listview is capable of showing groups</summary>
		[Browsable(false)]
		public override Boolean CanShowGroups
		{
			get => false;
		}

		/// <summary>This is the delegate that will be used to fetch the children of a model Object</summary>
		/// <remarks>
		/// <para>
		/// This delegate will only be called if the CanExpand delegate has 
		/// returned true for the model Object.
		/// </para>
		/// <para>
		/// When this delegate is called, the TreeListView is not in a stable state. Don't do anything
		/// that will result in calls being made back into the control.
		/// </para>
		/// </remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual ChildrenGetterDelegate ChildrenGetter
		{
			get => this.TreeModel.ChildrenGetter;
			set => this.TreeModel.ChildrenGetter = value;
		}

		/// <summary>This is the delegate that will be used to fetch the parent of a model Object</summary>
		/// <returns>The parent of the given model, or null if the model doesn't exist or 
		/// if the model is a root</returns>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ParentGetterDelegate ParentGetter
		{
			get => this._parentGetter ?? Tree.DefaultParentGetter;
			set => this._parentGetter = value;
		}
		private ParentGetterDelegate _parentGetter;

		/// <summary>
		/// Get or set the collection of model objects that are checked.
		/// When setting this property, any row whose model Object isn't
		/// in the given collection will be unchecked. Setting to null is
		/// equivalent to unchecking all.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property returns a simple collection. Changes made to the returned
		/// collection do NOT affect the list. This is different to the behaviour of
		/// CheckedIndicies collection.
		/// </para>
		/// <para>
		/// When getting CheckedObjects, the performance of this method is O(n) where n is the number of checked objects.
		/// When setting CheckedObjects, the performance of this method is O(n) where n is the number of checked objects plus
		/// the number of objects to be checked.
		/// </para>
		/// <para>
		/// If the ListView is not currently showing CheckBoxes, this property does nothing. It does
		/// not remember any check box settings made.
		/// </para>
		/// </remarks>
		public override IList CheckedObjects
		{
			get => base.CheckedObjects;
			set
			{
				ArrayList objectsToRecalculate = new ArrayList(this.CheckedObjects);
				if(value != null)
					objectsToRecalculate.AddRange(value);

				base.CheckedObjects = value;

				if(this.HierarchicalCheckboxes)
					this.RecalculateHierarchicalCheckBoxGraph(objectsToRecalculate);
			}
		}

		/// <summary>Gets or sets the model objects that are expanded.</summary>
		/// <remarks>
		/// <para>This can be used to expand model objects before they are seen.</para>
		/// <para>
		/// Setting this does *not* force the control to rebuild
		/// its display. You need to call RebuildAll(true).
		/// </para>
		/// </remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IEnumerable ExpandedObjects
		{
			get => this.TreeModel.mapObjectToExpanded.Keys;
			set
			{
				this.TreeModel.mapObjectToExpanded.Clear();
				if(value != null)
					foreach(Object x in value)
						this.TreeModel.SetModelExpanded(x, true);
			}
		}

		/// <summary>Gets or sets the filter that is applied to our whole list of objects.</summary>
		/// <remarks>TreeListViews do not currently support whole list filters.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override IListFilter ListFilter
		{
			get => null;
			set => Debug.Assert(value == null, "TreeListView do not support ListFilters");
		}

		/// <summary>
		/// Gets or sets whether this tree list view will display hierarchical checkboxes.
		/// Hierarchical checkboxes is when a parent's "checkedness" is calculated from
		/// the "checkedness" of its children. If all children are checked, the parent
		/// will be checked. If all children are unchecked, the parent will also be unchecked.
		/// If some children are checked and others are not, the parent will be indeterminate.
		/// </summary>
		/// <remarks>
		/// Hierarchical checkboxes don't work with either CheckStateGetters or CheckedAspectName
		/// (which is basically the same thing). This is because it is too expensive to build the 
		/// initial state of the control if these are installed, since the control would have to walk
		/// *every* branch recursively since a single bottom level leaf could change the checkedness
		/// of the top root.
		/// </remarks>
		[Category(Constants.ObjectListView)]
		[Description("Show hierarchical checkboxes be enabled?")]
		[DefaultValue(false)]
		public virtual Boolean HierarchicalCheckboxes
		{
			get => this._hierarchicalCheckboxes;
			set
			{
				if(this._hierarchicalCheckboxes == value)
					return;

				this._hierarchicalCheckboxes = value;
				this.CheckBoxes = value;
				if(value)
					this.TriStateCheckBoxes = false;
			}
		}
		private Boolean _hierarchicalCheckboxes;

		/// <summary>Gets or sets the collection of root objects of the tree</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override IEnumerable Objects
		{
			get => this.Roots;
			set => this.Roots = value;
		}

		/// <summary>
		/// Gets the collection of objects that will be considered when creating clusters
		/// (which are used to generate Excel-like column filters)
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override IEnumerable ObjectsForClustering
		{
			get
			{
				for(Int32 i = 0; i < this.TreeModel.GetObjectCount(); i++)
					yield return this.TreeModel.GetNthObject(i);
			}
		}

		/// <summary>After expanding a branch, should the TreeListView attempts to show as much of the revealed descendants as possible.</summary>
		[Category(Constants.ObjectListView)]
		[Description("Should the parent of an expand subtree be scrolled to the top revealing the children?")]
		[DefaultValue(true)]
		public Boolean RevealAfterExpand { get; set; } = true;

		/// <summary>The model objects that form the top level branches of the tree.</summary>
		/// <remarks>Setting this does <b>NOT</b> reset the state of the control.
		/// In particular, it does not collapse branches.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual IEnumerable Roots
		{
			get => this.TreeModel.RootObjects;
			set
			{
				this.TreeColumnRenderer = this.TreeColumnRenderer;
				this.TreeModel.RootObjects = value ?? new ArrayList();
				this.UpdateVirtualListSize();
			}
		}

		/// <summary>
		/// Make sure that at least one column is displaying a tree. 
		/// If no columns is showing the tree, make column 0 do it.
		/// </summary>
		protected virtual void EnsureTreeRendererPresent(TreeRenderer renderer)
		{
			if(this.Columns.Count == 0)
				return;

			foreach(OLVColumn col in this.Columns)
				if(col.Renderer is TreeRenderer)
				{
					col.Renderer = renderer;
					return;
				}

			// No column held a tree renderer, so give column 0 one
			OLVColumn columnZero = this.GetColumn(0);
			columnZero.Renderer = renderer;
			columnZero.WordWrap = columnZero.WordWrap;
		}

		/// <summary>
		/// Gets or sets the renderer that will be used to draw the tree structure.
		/// Setting this to null resets the renderer to default.
		/// </summary>
		/// <remarks>If a column is currently rendering the tree, the renderer
		/// for that column will be replaced. If no column is rendering the tree,
		/// column 0 will be given this renderer.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual TreeRenderer TreeColumnRenderer
		{
			get => this._treeRenderer;
			set
			{
				this._treeRenderer = value ?? new TreeRenderer();
				this.EnsureTreeRendererPresent(this._treeRenderer);
			}
		}
		private TreeRenderer _treeRenderer = new TreeRenderer();

		/// <summary>
		/// This is the delegate that will be used to create the underlying Tree structure
		/// that the TreeListView uses to manage the information about the tree.
		/// </summary>
		/// <remarks>
		/// <para>The factory must not return null. </para>
		/// <para>Most users of TreeListView will never have to use this delegate.</para>
		/// </remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TreeFactoryDelegate TreeFactory { get; set; }

		/// <summary>Should a wait cursor be shown when a branch is being expanded?</summary>
		/// <remarks>When this is true, the wait cursor will be shown whilst the children of the 
		/// branch are being fetched. If the children of the branch have already been cached, 
		/// the cursor will not change.</remarks>
		[Category(Constants.ObjectListView)]
		[Description("Should a wait cursor be shown when a branch is being expanded?")]
		[DefaultValue(true)]
		public virtual Boolean UseWaitCursorWhenExpanding { get; set; } = true;

		/// <summary>Gets the model that is used to manage the tree structure</summary>
		/// <remarks>
		/// Don't mess with this property unless you really know what you are doing.
		/// If you don't already know what it's for, you don't need it.</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Tree TreeModel { get; protected set; }

		//------------------------------------------------------------------------------------------
		// Accessing

		/// <summary>Return true if the branch at the given model is expanded</summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public virtual Boolean IsExpanded(Object model)
		{
			Branch br = this.TreeModel.GetBranch(model);
			return (br != null && br.IsExpanded);
		}

		//------------------------------------------------------------------------------------------
		// Commands

		/// <summary>Collapse the subtree underneath the given model</summary>
		/// <param name="model"></param>
		public virtual void Collapse(Object model)
		{
			if(this.GetItemCount() == 0)
				return;

			OLVListItem item = this.ModelToItem(model);
			TreeBranchCollapsingEventArgs args = new TreeBranchCollapsingEventArgs(model, item);
			this.OnCollapsing(args);
			if(args.Canceled)
				return;

			IList selection = this.SelectedObjects;
			Int32 index = this.TreeModel.Collapse(model);
			if(index >= 0)
			{
				this.UpdateVirtualListSize();
				this.SelectedObjects = selection;
				if(index < this.GetItemCount())
					this.RedrawItems(index, this.GetItemCount() - 1, true);
				this.OnCollapsed(new TreeBranchCollapsedEventArgs(model, item));
			}
		}

		/// <summary>Collapse all subtrees within this control</summary>
		public virtual void CollapseAll()
		{
			if(this.GetItemCount() == 0)
				return;

			TreeBranchCollapsingEventArgs args = new TreeBranchCollapsingEventArgs(null, null);
			this.OnCollapsing(args);
			if(args.Canceled)
				return;

			IList selection = this.SelectedObjects;
			Int32 index = this.TreeModel.CollapseAll();
			if(index >= 0)
			{
				this.UpdateVirtualListSize();
				this.SelectedObjects = selection;
				if(index < this.GetItemCount())
					this.RedrawItems(index, this.GetItemCount() - 1, true);
				this.OnCollapsed(new TreeBranchCollapsedEventArgs(null, null));
			}
		}

		/// <summary>Remove all items from this list</summary>
		/// <remark>This method can safely be called from background threads.</remark>
		public override void ClearObjects()
		{
			if(this.InvokeRequired)
				this.Invoke(new MethodInvoker(this.ClearObjects));
			else
			{
				this.Roots = null;
				this.DiscardAllState();
			}
		}

		/// <summary>Collapse all roots and forget everything we know about all models</summary>
		public virtual void DiscardAllState()
		{
			this.CheckStateMap.Clear();
			this.RebuildAll(false);
		}

		/// <summary>Expand the subtree underneath the given model Object</summary>
		/// <param name="model"></param>
		public virtual void Expand(Object model)
		{
			if(this.GetItemCount() == 0)
				return;

			// Give the world a chance to cancel the expansion
			OLVListItem item = this.ModelToItem(model);
			TreeBranchExpandingEventArgs args = new TreeBranchExpandingEventArgs(model, item);
			this.OnExpanding(args);
			if(args.Canceled)
				return;

			// Remember the selection so we can put it back later
			IList selection = this.SelectedObjects;

			// Expand the model first
			Int32 index = this.TreeModel.Expand(model);
			if(index < 0)
				return;

			// Update the size of the list and restore the selection
			this.UpdateVirtualListSize();
			using(this.SuspendSelectionEventsDuring())
				this.SelectedObjects = selection;

			// Redraw the items that were changed by the expand operation
			this.RedrawItems(index, this.GetItemCount() - 1, true);

			this.OnExpanded(new TreeBranchExpandedEventArgs(model, item));

			if(this.RevealAfterExpand && index > 0)
			{
				// TODO: This should be a separate method
				this.BeginUpdate();
				try
				{
					Int32 countPerPage = NativeMethods.GetCountPerPage(this);
					Int32 descedentCount = this.TreeModel.GetVisibleDescendentCount(model);
					// If all of the descendants can be shown in the window, make sure that last one is visible.
					// If all the descendants can't fit into the window, move the model to the top of the window
					// (which will show as many of the descendants as possible)
					if(descedentCount < countPerPage)
						this.EnsureVisible(index + descedentCount);
					else
						this.TopItemIndex = index;
				} finally
				{
					this.EndUpdate();
				}
			}
		}

		/// <summary>Expand all the branches within this tree recursively.</summary>
		/// <remarks>Be careful: this method could take a long time for large trees.</remarks>
		public virtual void ExpandAll()
		{
			if(this.GetItemCount() == 0)
				return;

			// Give the world a chance to cancel the expansion
			TreeBranchExpandingEventArgs args = new TreeBranchExpandingEventArgs(null, null);
			this.OnExpanding(args);
			if(args.Canceled)
				return;

			IList selection = this.SelectedObjects;
			Int32 index = this.TreeModel.ExpandAll();
			if(index < 0)
				return;

			this.UpdateVirtualListSize();
			using(this.SuspendSelectionEventsDuring())
				this.SelectedObjects = selection;
			this.RedrawItems(index, this.GetItemCount() - 1, true);
			this.OnExpanded(new TreeBranchExpandedEventArgs(null, null));
		}

		/// <summary>Completely rebuild the tree structure</summary>
		/// <param name="preserveState">If true, the control will try to preserve selection and expansion</param>
		public virtual void RebuildAll(Boolean preserveState)
		{
			Int32 previousTopItemIndex = preserveState ? this.TopItemIndex : -1;

			this.RebuildAll(
				preserveState ? this.SelectedObjects : null,
				preserveState ? this.ExpandedObjects : null,
				preserveState ? this.CheckedObjects : null);

			if(preserveState)
				this.TopItemIndex = previousTopItemIndex;
		}

		/// <summary>Completely rebuild the tree structure</summary>
		/// <param name="selected">If not null, this list of objects will be selected after the tree is rebuilt</param>
		/// <param name="expanded">If not null, this collection of objects will be expanded after the tree is rebuilt</param>
		/// <param name="checkedObjects">If not null, this collection of objects will be checked after the tree is rebuilt</param>
		protected virtual void RebuildAll(IList selected, IEnumerable expanded, IList checkedObjects)
		{
			// Remember the bits of info we don't want to forget (anyone ever see Memento?)
			IEnumerable roots = this.Roots;
			CanExpandGetterDelegate canExpand = this.CanExpandGetter;
			ChildrenGetterDelegate childrenGetter = this.ChildrenGetter;

			try
			{
				this.BeginUpdate();

				// Give ourselves a new data structure
				this.RegenerateTree();

				// Put back the bits we didn't want to forget
				this.CanExpandGetter = canExpand;
				this.ChildrenGetter = childrenGetter;
				if(expanded != null)
					this.ExpandedObjects = expanded;
				this.Roots = roots;
				if(selected != null)
					this.SelectedObjects = selected;
				if(checkedObjects != null)
					this.CheckedObjects = checkedObjects;
			} finally
			{
				this.EndUpdate();
			}
		}

		/// <summary>Unroll all the ancestors of the given model and make sure it is then visible.</summary>
		/// <remarks>This works best when a ParentGetter is installed.</remarks>
		/// <param name="modelToReveal">The Object to be revealed</param>
		/// <param name="selectAfterReveal">If true, the model will be selected and focused after being revealed</param>
		/// <returns>True if the Object was found and revealed. False if it was not found.</returns>
		public virtual void Reveal(Object modelToReveal, Boolean selectAfterReveal)
		{
			// Collect all the ancestors of the model
			ArrayList ancestors = new ArrayList();
			foreach(Object ancestor in this.GetAncestors(modelToReveal))
				ancestors.Add(ancestor);

			// Arrange them from root down to the model's immediate parent
			ancestors.Reverse();
			try
			{
				this.BeginUpdate();
				foreach(Object ancestor in ancestors)
					this.Expand(ancestor);
				this.EnsureModelVisible(modelToReveal);
				if(selectAfterReveal)
					this.SelectObject(modelToReveal, true);
			} finally
			{
				this.EndUpdate();
			}
		}

		/// <summary>Update the rows that are showing the given objects</summary>
		public override void RefreshObjects(IList modelObjects)
		{
			if(this.InvokeRequired)
			{
				this.Invoke((MethodInvoker)delegate { this.RefreshObjects(modelObjects); });
				return;
			}

			// There is no point in refreshing anything if the list is empty
			if(this.GetItemCount() == 0)
				return;

			// Remember the selection so we can put it back later
			IList selection = this.SelectedObjects;

			// We actually need to refresh the parents.
			// Refreshes on root objects have to be handled differently
			ArrayList updatedRoots = new ArrayList();
			Hashtable modelsAndParents = new Hashtable();
			foreach(Object model in modelObjects)
			{
				if(model == null)
					continue;
				modelsAndParents[model] = true;
				Object parent = GetParent(model);
				if(parent == null)
					updatedRoots.Add(model);
				else
					modelsAndParents[parent] = true;
			}

			// Update any changed roots
			if(updatedRoots.Count > 0)
			{
				ArrayList newRoots = ObjectListView.EnumerableToArray(this.Roots, false);
				Boolean changed = false;
				foreach(Object model in updatedRoots)
				{
					Int32 index = newRoots.IndexOf(model);
					if(index >= 0 && !ReferenceEquals(newRoots[index], model))
					{
						newRoots[index] = model;
						changed = true;
					}
				}
				if(changed)
					this.Roots = newRoots;
			}

			// Refresh each Object, remembering where the first update occurred
			Int32 firstChange = Int32.MaxValue;
			foreach(Object model in modelsAndParents.Keys)
				if(model != null)
				{
					Int32 index = this.TreeModel.RebuildChildren(model);
					if(index >= 0)
						firstChange = Math.Min(firstChange, index);
				}

			// If we didn't refresh any objects, don't do anything else
			if(firstChange >= this.GetItemCount())
				return;

			this.ClearCachedInfo();
			this.UpdateVirtualListSize();
			this.SelectedObjects = selection;

			// Redraw everything from the first update to the end of the list
			this.RedrawItems(firstChange, this.GetItemCount() - 1, true);
		}

		/// <summary>Change the check state of the given Object to be the given state.</summary>
		/// <remarks>
		/// If the given model Object isn't in the list, we still try to remember
		/// its state, in case it is referenced in the future.</remarks>
		/// <param name="modelObject"></param>
		/// <param name="state"></param>
		/// <returns>True if the checkedness of the model changed</returns>
		protected override Boolean SetObjectCheckedness(Object modelObject, CheckState state)
		{
			// If the checkedness of the given model changes AND this tree has 
			// hierarchical checkboxes, then we need to update the checkedness of 
			// its children, and recalculate the checkedness of the parent (recursively)
			if(!base.SetObjectCheckedness(modelObject, state))
				return false;

			if(!this.HierarchicalCheckboxes)
				return true;

			// Give each child the same checkedness as the model

			CheckState? checkedness = this.GetCheckState(modelObject);
			if(!checkedness.HasValue || checkedness.Value == CheckState.Indeterminate)
				return true;

			foreach(Object child in this.GetChildrenWithoutExpanding(modelObject))
				this.SetObjectCheckedness(child, checkedness.Value);

			ArrayList args = new ArrayList
			{
				modelObject
			};
			this.RecalculateHierarchicalCheckBoxGraph(args);

			return true;
		}

		private IEnumerable GetChildrenWithoutExpanding(Object model)
		{
			Branch br = this.TreeModel.GetBranch(model);
			return br == null || !br.CanExpand
				? new ArrayList()
				: br.Children;
		}

		/// <summary>Toggle the expanded state of the branch at the given model Object</summary>
		/// <param name="model"></param>
		public virtual void ToggleExpansion(Object model)
		{
			if(this.IsExpanded(model))
				this.Collapse(model);
			else
				this.Expand(model);
		}

		//------------------------------------------------------------------------------------------
		// Commands - Tree traversal

		/// <summary>Return whether or not the given model can expand.</summary>
		/// <param name="model"></param>
		/// <remarks>The given model must have already been seen in the tree</remarks>
		public virtual Boolean CanExpand(Object model)
		{
			Branch br = this.TreeModel.GetBranch(model);
			return br != null && br.CanExpand;
		}

		/// <summary>Return the model Object that is the parent of the given model Object.</summary>
		/// <param name="model"></param>
		/// <returns></returns>
		/// <remarks>The given model must have already been seen in the tree.</remarks>
		public virtual Object GetParent(Object model)
		{
			Branch br = this.TreeModel.GetBranch(model);
			return br == null || br.ParentBranch == null
				? null
				: br.ParentBranch.Model;
		}

		/// <summary>Return the collection of model objects that are the children of the given model as they exist in the tree at the moment.</summary>
		/// <param name="model"></param>
		/// <remarks>
		/// <para>
		/// This method returns the collection of children as the tree knows them. If the given
		/// model has never been presented to the user (e.g. it belongs to a parent that has
		/// never been expanded), then this method will return an empty collection.</para>
		/// <para>
		/// Because of this, if you want to traverse the whole tree, this is not the method to use.
		/// It's better to traverse the your data model directly.
		/// </para>
		/// <para>
		/// If the given model has not already been seen in the tree or
		/// if it is not expandable, an empty collection will be returned.
		/// </para>
		/// </remarks>
		public virtual IEnumerable GetChildren(Object model)
		{
			Branch br = this.TreeModel.GetBranch(model);
			if(br == null || !br.CanExpand)
				return new ArrayList();

			br.FetchChildren();

			return br.Children;
		}

		//------------------------------------------------------------------------------------------
		// Delegates

		/// <summary>Delegates of this type are use to decide if the given model Object can be expanded</summary>
		/// <param name="model">The model under consideration</param>
		/// <returns>Can the given model be expanded?</returns>
		public delegate Boolean CanExpandGetterDelegate(Object model);

		/// <summary>Delegates of this type are used to fetch the children of the given model Object</summary>
		/// <param name="model">The parent whose children should be fetched</param>
		/// <returns>An enumerable over the children</returns>
		public delegate IEnumerable ChildrenGetterDelegate(Object model);

		/// <summary>Delegates of this type are used to fetch the parent of the given model Object.</summary>
		/// <param name="model">The child whose parent should be fetched</param>
		/// <returns>The parent of the child or null if the child is a root</returns>
		public delegate Object ParentGetterDelegate(Object model);

		/// <summary>Delegates of this type are used to create a new underlying Tree structure.</summary>
		/// <param name="view">The view for which the Tree is being created</param>
		/// <returns>A subclass of Tree</returns>
		public delegate Tree TreeFactoryDelegate(TreeListView view);

		//------------------------------------------------------------------------------------------
		#region Implementation

		/// <summary>Handle a left button down event</summary>
		/// <param name="hti"></param>
		/// <returns></returns>
		protected override Boolean ProcessLButtonDown(OlvListViewHitTestInfo hti)
		{
			// Did they click in the expander?
			if(hti.HitTestLocation == HitTestLocation.ExpandButton)
			{
				this.PossibleFinishCellEditing();
				this.ToggleExpansion(hti.RowObject);
				return true;
			}

			return base.ProcessLButtonDown(hti);
		}

		/// <summary>Create a OLVListItem for given row index</summary>
		/// <param name="itemIndex">The index of the row that is needed</param>
		/// <returns>An OLVListItem</returns>
		/// <remarks>This differs from the base method by also setting up the IndentCount property.</remarks>
		public override OLVListItem MakeListViewItem(Int32 itemIndex)
		{
			OLVListItem olvItem = base.MakeListViewItem(itemIndex);
			Branch br = this.TreeModel.GetBranch(olvItem.RowObject);
			if(br != null)
				olvItem.IndentCount = br.Level;
			return olvItem;
		}

		/// <summary>Reinitialise the Tree structure</summary>
		protected virtual void RegenerateTree()
		{
			this.TreeModel = this.TreeFactory == null ? new Tree(this) : this.TreeFactory(this);
			Trace.Assert(this.TreeModel != null);
			this.VirtualListDataSource = this.TreeModel;
		}

		/// <summary>Recalculate the state of the checkboxes of all the items in the given list and their ancestors.</summary>
		/// <remarks>This only makes sense when HierarchicalCheckboxes is true.</remarks>
		/// <param name="toCheck"></param>
		protected virtual void RecalculateHierarchicalCheckBoxGraph(IList toCheck)
		{
			if(toCheck == null || toCheck.Count == 0)
				return;

			// Avoid recursive calculations
			if(this._isRecalculatingHierarchicalCheckBox)
				return;

			try
			{
				this._isRecalculatingHierarchicalCheckBox = true;
				foreach(Object ancestor in this.CalculateDistinctAncestors(toCheck))
					this.RecalculateSingleHierarchicalCheckBox(ancestor);
			} finally
			{
				this._isRecalculatingHierarchicalCheckBox = false;
			}
		}
		private Boolean _isRecalculatingHierarchicalCheckBox;

		/// <summary>Recalculate the hierarchy state of the given item and its ancestors</summary>
		/// <remarks>This only makes sense when HierarchicalCheckboxes is true.</remarks>
		/// <param name="modelObject"></param>
		protected virtual void RecalculateSingleHierarchicalCheckBox(Object modelObject)
		{
			if(modelObject == null)
				return;

			// Only branches have calculated check states. Leaf node checkedness is not calculated
			if(!this.CanExpandUncached(modelObject))
				return;

			// Set the checkedness of the given model based on the state of its children.
			CheckState? aggregate = null;
			foreach(Object child in this.GetChildrenUncached(modelObject))
			{
				CheckState? checkedness = this.GetCheckState(child);
				if(!checkedness.HasValue)
					continue;

				if(aggregate.HasValue)
				{
					if(aggregate.Value != checkedness.Value)
					{
						aggregate = CheckState.Indeterminate;
						break;
					}
				} else
					aggregate = checkedness;
			}

			base.SetObjectCheckedness(modelObject, aggregate ?? CheckState.Indeterminate);
		}

		private Boolean CanExpandUncached(Object model)
			=> this.CanExpandGetter != null && model != null && this.CanExpandGetter(model);

		private IEnumerable GetChildrenUncached(Object model)
			=> this.ChildrenGetter != null && model != null ? this.ChildrenGetter(model) : new ArrayList();

		/// <summary>
		/// Yield the unique ancestors of the given collection of objects.
		/// The order of the ancestors is guaranteed to be deeper objects first.
		/// Roots will always be last.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns>Unique ancestors of the given objects</returns>
		protected virtual IEnumerable CalculateDistinctAncestors(IList toCheck)
		{

			if(toCheck.Count == 1)
				foreach(Object ancestor in this.GetAncestors(toCheck[0]))
					yield return ancestor;
			else
			{
				// WARNING - Clever code

				// Example:  Root --> GP +--> P +--> A
				//                       |      +--> B
				//                       |
				//                       +--> Q +--> X
				//                              +--> Y
				//
				// Calculate ancestors of A, B, X and Y

				// Build a list of all ancestors of all objects we need to check
				ArrayList allAncestors = new ArrayList();
				foreach(Object child in toCheck)
					foreach(Object ancestor in this.GetAncestors(child))
						allAncestors.Add(ancestor);

				// allAncestors = { P, GP, Root, P, GP, Root, Q, GP, Root, Q, GP, Root }

				// Reverse them so "higher" ancestors come first
				allAncestors.Reverse();

				// allAncestors = { Root, GP, Q, Root, GP, Q, Root, GP, P, Root, GP, P  }

				ArrayList uniqueAncestors = new ArrayList();
				Dictionary<Object, Boolean> alreadySeen = new Dictionary<Object, Boolean>();
				foreach(Object ancestor in allAncestors)
					if(!alreadySeen.ContainsKey(ancestor))
					{
						alreadySeen[ancestor] = true;
						uniqueAncestors.Add(ancestor);
					}

				// uniqueAncestors = { Root, GP, Q, P }

				uniqueAncestors.Reverse();
				foreach(Object x in uniqueAncestors)
					yield return x;
			}
		}

		/// <summary>Return all the ancestors of the given model</summary>
		/// <remarks>
		/// <para>This uses ParentGetter if possible.</para>
		/// <para>If the given model is a root OR if the model doesn't exist, the collection will be empty</para>
		/// </remarks>
		/// <param name="model">The model whose ancestors should be calculated</param>
		/// <returns>Return a collection of ancestors of the given model.</returns>
		protected virtual IEnumerable GetAncestors(Object model)
		{
			ParentGetterDelegate parentGetterDelegate = this.ParentGetter ?? this.GetParent;

			Object parent = parentGetterDelegate(model);
			while(parent != null)
			{
				yield return parent;
				parent = parentGetterDelegate(parent);
			}
		}

		#endregion

		//------------------------------------------------------------------------------------------
		#region Event handlers

		/// <summary>The application is idle and a SelectionChanged event has been scheduled</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void HandleApplicationIdle(Object sender, EventArgs e)
		{
			base.HandleApplicationIdle(sender, e);

			// There is an annoying redraw issue on ListViews that use indentation and
			// that have full row select enabled. When the selection reduces to a subset
			// of previously selected rows, or when the selection is extended using
			// shift-pageup/down, then the space occupied by the indentation is not
			// invalidated, and hence remains highlighted.
			// Ideally we'd want to know exactly which rows were selected or deselected
			// and then invalidate just the indentation region of those rows,
			// but that's too much work. So just redraw the control.
			// Actually... the selection issues show just slightly for non-full row select
			// controls as well. So, always redraw the control after the selection
			// changes.
			this.Invalidate();
		}

		/// <summary>Decide if the given key event should be handled as a normal key input to the control?</summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override Boolean IsInputKey(Keys keyData)
		{
			// We want to handle Left and Right keys within the control
			Keys key = keyData & Keys.KeyCode;
			return key == Keys.Left
				|| key == Keys.Right
				|| base.IsInputKey(keyData);
		}

		/// <summary>Handle focus being lost, including making sure that the whole control is redrawn.</summary>
		/// <param name="e"></param>
		protected override void OnLostFocus(EventArgs e)
		{
			// When this focus is lost, the normal invalidation logic doesn't invalid the region
			// of the control created by the IndentLevel on each row. This makes the control 
			// look wrong when HideSelection is false, since part of the selected row's background
			// correctly changes colour to the "inactive" colour, but the left part of the row
			// created by IndentLevel doesn't change colour.
			// SF #135.

			this.Invalidate();
		}

		/// <summary>Handle the keyboard input to mimic a TreeView.</summary>
		/// <param name="e"></param>
		/// <returns>Was the key press handled?</returns>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(!(this.FocusedItem is OLVListItem focused))
			{
				base.OnKeyDown(e);
				return;
			}

			Object modelObject = focused.RowObject;
			Branch br = this.TreeModel.GetBranch(modelObject);

			switch(e.KeyCode)
			{
			case Keys.Left:
				// If the branch is expanded, collapse it. If it's collapsed,
				// select the parent of the branch.
				if(br.IsExpanded)
					this.Collapse(modelObject);
				else if(br.ParentBranch != null && br.ParentBranch.Model != null)
					this.SelectObject(br.ParentBranch.Model, true);

				e.Handled = true;
				break;

			case Keys.Right:
				// If the branch is expanded, select the first child.
				// If it isn't expanded and can be, expand it.
				if(br.IsExpanded)
				{
					List<Branch> filtered = br.FilteredChildBranches;
					if(filtered.Count > 0)
						this.SelectObject(filtered[0].Model, true);
				} else if(br.CanExpand)
					this.Expand(modelObject);

				e.Handled = true;
				break;
			}

			base.OnKeyDown(e);
		}

		#endregion

		//------------------------------------------------------------------------------------------
		// Support classes

		/// <summary>
		/// A Tree Object represents a tree structure data model that supports both 
		/// tree and flat list operations as well as fast access to branches.
		/// </summary>
		/// <remarks>If you create a subclass of Tree, you must install it in the TreeListView
		/// via the TreeFactory delegate.</remarks>
		public class Tree : IVirtualListDataSource, IFilterableDataSource
		{
			/// <summary>Create a Tree</summary>
			/// <param name="treeView"></param>
			public Tree(TreeListView treeView)
			{
				this._treeView = treeView;
				this.trunk = new Branch(null, this, null)
				{
					IsExpanded = true,
				};
			}

			//------------------------------------------------------------------------------------------
			// Properties

			/// <summary>This is the delegate that will be used to decide if a model Object can be expanded.</summary>
			public CanExpandGetterDelegate CanExpandGetter
			{
				get => this._canExpandGetter ?? DefaultCanExpandGetter;
				set => this._canExpandGetter = value;
			}
			private CanExpandGetterDelegate _canExpandGetter;

			/// <summary>This is the delegate that will be used to fetch the children of a model Object</summary>
			/// <remarks>This delegate will only be called if the CanExpand delegate has 
			/// returned true for the model Object.</remarks>
			public ChildrenGetterDelegate ChildrenGetter
			{
				get => this._childrenGetter ?? DefaultChildrenGetter;
				set => this._childrenGetter = value;
			}
			private ChildrenGetterDelegate _childrenGetter;

			/// <summary>Get or return the top level model objects in the tree</summary>
			public IEnumerable RootObjects
			{
				get => this.trunk.Children;
				set
				{
					this.trunk.Children = value;
					foreach(Branch br in this.trunk.ChildBranches)
						br.RefreshChildren();
					this.RebuildList();
				}
			}

			/// <summary>What tree view is this Tree the model for?</summary>
			public TreeListView TreeView { get => this._treeView; }

			//------------------------------------------------------------------------------------------
			// Commands

			/// <summary>Collapse the subtree underneath the given model</summary>
			/// <param name="model">The model to be collapsed. If the model isn't in the tree,
			/// or if it is already collapsed, the command does nothing.</param>
			/// <returns>The index of the model in flat list version of the tree</returns>
			public virtual Int32 Collapse(Object model)
			{
				Branch br = this.GetBranch(model);
				if(br == null || !br.IsExpanded)
					return -1;

				// Remember that the branch is collapsed, even if it's currently not visible
				if(!br.Visible)
				{
					br.Collapse();
					return -1;
				}

				Int32 count = br.NumberVisibleDescendents;
				br.Collapse();

				// Remove the visible descendants from after the branch itself
				Int32 index = this.GetObjectIndex(model);
				this.objectList.RemoveRange(index + 1, count);
				this.RebuildObjectMap(0);
				return index;
			}

			/// <summary>Collapse all branches in this tree</summary>
			/// <returns>Nothing useful</returns>
			public virtual Int32 CollapseAll()
			{
				this.trunk.CollapseAll();
				this.RebuildList();
				return 0;
			}

			/// <summary>Expand the subtree underneath the given model Object</summary>
			/// <param name="model">The model to be expanded.</param> 
			/// <returns>The index of the model in flat list version of the tree</returns>
			/// <remarks>
			/// If the model isn't in the tree,
			/// if it cannot be expanded or if it is already expanded, the command does nothing.
			/// </remarks>
			public virtual Int32 Expand(Object model)
			{
				Branch br = this.GetBranch(model);
				if(br == null || !br.CanExpand || br.IsExpanded)
					return -1;

				// Remember that the branch is expanded, even if it's currently not visible
				br.Expand();
				if(!br.Visible)
					return -1;

				Int32 index = this.GetObjectIndex(model);
				this.InsertChildren(br, index + 1);
				return index;
			}

			/// <summary>Expand all branches in this tree</summary>
			/// <returns>Return the index of the first branch that was expanded</returns>
			public virtual Int32 ExpandAll()
			{
				this.trunk.ExpandAll();
				this.Sort(this.lastSortColumn, this.lastSortOrder);
				return 0;
			}

			/// <summary>Return the Branch Object that represents the given model in the tree</summary>
			/// <param name="model">The model whose branches is to be returned</param>
			/// <returns>The branch that represents the given model, or null if the model
			/// isn't in the tree.</returns>
			public virtual Branch GetBranch(Object model)
			{
				if(model == null)
					return null;

				this.mapObjectToBranch.TryGetValue(model, out Branch br);
				return br;
			}

			/// <summary>Return the number of visible descendants that are below the given model.</summary>
			/// <param name="model">The model whose descendent count is to be returned</param>
			/// <returns>The number of visible descendants. 0 if the model doesn't exist or is collapsed</returns>
			public virtual Int32 GetVisibleDescendentCount(Object model)
			{
				Branch br = this.GetBranch(model);
				return br == null || !br.IsExpanded ? 0 : br.NumberVisibleDescendents;
			}

			/// <summary>Rebuild the children of the given model, refreshing any cached information held about the given Object</summary>
			/// <param name="model"></param>
			/// <returns>The index of the model in flat list version of the tree</returns>
			public virtual Int32 RebuildChildren(Object model)
			{
				Branch br = this.GetBranch(model);
				if(br == null || !br.Visible)
					return -1;

				Int32 count = br.NumberVisibleDescendents;

				// Remove the visible descendants from after the branch itself
				Int32 index = this.GetObjectIndex(model);
				if(count > 0)
					this.objectList.RemoveRange(index + 1, count);

				// Refresh our knowledge of our children (do this even if CanExpand is false, because
				// the branch have already collected some children and that information could be stale)
				br.RefreshChildren();

				// Insert the refreshed children if the branch can expand and is expanded
				if(br.CanExpand && br.IsExpanded)
					this.InsertChildren(br, index + 1);
				else
					this.RebuildObjectMap(index);

				return index;
			}

			//------------------------------------------------------------------------------------------
			// Implementation

			private static Boolean DefaultCanExpandGetter(Object model)
				=> model is ITreeModelWithChildren treeModel && treeModel.TreeCanExpand;

			private static IEnumerable DefaultChildrenGetter(Object model)
				=> model is ITreeModelWithChildren treeModel ? treeModel.TreeChildren : new ArrayList();

			internal static Object DefaultParentGetter(Object model)
				=> model is ITreeModelWithParent treeModel ? treeModel.TreeParent : null;

			/// <summary>Is the given model expanded?</summary>
			/// <param name="model"></param>
			/// <returns></returns>
			internal Boolean IsModelExpanded(Object model)
			{
				// Special case: model == null is the container for the roots. This is always expanded
				if(model == null)
					return true;
				this.mapObjectToExpanded.TryGetValue(model, out Boolean isExpanded);
				return isExpanded;
			}

			/// <summary>Remember whether or not the given model was expanded</summary>
			/// <param name="model"></param>
			/// <param name="isExpanded"></param>
			internal void SetModelExpanded(Object model, Boolean isExpanded)
			{
				if(model == null) return;

				if(isExpanded)
					this.mapObjectToExpanded[model] = true;
				else
					this.mapObjectToExpanded.Remove(model);
			}

			/// <summary>Insert the children of the given branch into the given position</summary>
			/// <param name="br">The branch whose children should be inserted</param>
			/// <param name="index">The index where the children should be inserted</param>
			protected virtual void InsertChildren(Branch br, Int32 index)
			{
				// Expand the branch
				br.Expand();
				br.Sort(this.GetBranchComparer());

				// Insert the branch's visible descendants after the branch itself
				this.objectList.InsertRange(index, br.Flatten());
				this.RebuildObjectMap(index);
			}

			/// <summary>Rebuild our flat internal list of objects.</summary>
			protected virtual void RebuildList()
			{
				this.objectList = ArrayList.Adapter(this.trunk.Flatten());
				List<Branch> filtered = this.trunk.FilteredChildBranches;
				if(filtered.Count > 0)
				{
					filtered[0].IsFirstBranch = true;
					filtered[0].IsOnlyBranch = (filtered.Count == 1);
				}
				this.RebuildObjectMap(0);
			}

			/// <summary>Rebuild our reverse index that maps an Object to its location in the filteredObjectList array.</summary>
			/// <param name="startIndex"></param>
			protected virtual void RebuildObjectMap(Int32 startIndex)
			{
				if(startIndex == 0)
					this.mapObjectToIndex.Clear();
				for(Int32 i = startIndex; i < this.objectList.Count; i++)
					this.mapObjectToIndex[this.objectList[i]] = i;
			}

			/// <summary>Create a new branch within this tree</summary>
			/// <param name="parent"></param>
			/// <param name="model"></param>
			/// <returns></returns>
			internal Branch MakeBranch(Branch parent, Object model)
			{
				Branch br = new Branch(parent, this, model);

				// Remember that the given branch is part of this tree.
				this.mapObjectToBranch[model] = br;
				return br;
			}

			//------------------------------------------------------------------------------------------

			#region IVirtualListDataSource Members

			/// <summary></summary>
			/// <param name="n"></param>
			/// <returns></returns>
			public virtual Object GetNthObject(Int32 n)
				=> n >= 0 && n < this.objectList.Count
					? this.objectList[n]
					: null;

			/// <summary></summary>
			/// <returns></returns>
			public virtual Int32 GetObjectCount()
				=> this.trunk.NumberVisibleDescendents;

			/// <summary></summary>
			/// <param name="model"></param>
			/// <returns></returns>
			public virtual Int32 GetObjectIndex(Object model)
				=> model != null && this.mapObjectToIndex.TryGetValue(model, out Int32 index)
					? index
					: -1;

			/// <summary></summary>
			/// <param name="first"></param>
			/// <param name="last"></param>
			public virtual void PrepareCache(Int32 first, Int32 last)
			{
			}

			/// <summary></summary>
			/// <param name="value"></param>
			/// <param name="first"></param>
			/// <param name="last"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public virtual Int32 SearchText(String value, Int32 first, Int32 last, OLVColumn column)
				=> AbstractVirtualListDataSource.DefaultSearchText(value, first, last, column, this);

			/// <summary>Sort the tree on the given column and in the given order</summary>
			/// <param name="column"></param>
			/// <param name="order"></param>
			public virtual void Sort(OLVColumn column, SortOrder order)
			{
				this.lastSortColumn = column;
				this.lastSortOrder = order;

				// TODO: Need to raise an AboutToSortEvent here

				// Sorting is going to change the order of the branches so clear
				// the "first branch" flag
				foreach(Branch b in this.trunk.ChildBranches)
					b.IsFirstBranch = false;

				this.trunk.Sort(this.GetBranchComparer());
				this.RebuildList();
			}

			/// <summary></summary>
			/// <returns></returns>
			protected virtual BranchComparer GetBranchComparer()
			{
				if(this.lastSortColumn == null)
					return null;

				return new BranchComparer(new ModelObjectComparer(
					this.lastSortColumn,
					this.lastSortOrder,
					this._treeView.SecondarySortColumn ?? this._treeView.GetColumn(0),
					this._treeView.SecondarySortColumn == null ? this.lastSortOrder : this._treeView.SecondarySortOrder));
			}

			/// <summary>Add the given collection of objects to the roots of this tree</summary>
			/// <param name="modelObjects"></param>
			public virtual void AddObjects(ICollection modelObjects)
			{
				ArrayList newRoots = ObjectListView.EnumerableToArray(this._treeView.Roots, true);
				foreach(Object x in modelObjects)
					newRoots.Add(x);
				this.SetObjects(newRoots);
			}

			/// <summary></summary>
			/// <param name="index"></param>
			/// <param name="modelObjects"></param>
			public void InsertObjects(Int32 index, ICollection modelObjects)
				=> throw new NotImplementedException();

			/// <summary>
			/// Remove all of the given objects from the roots of the tree.
			/// Any objects that is not already in the roots collection is ignored.
			/// </summary>
			/// <param name="modelObjects"></param>
			public virtual void RemoveObjects(ICollection modelObjects)
			{
				ArrayList newRoots = new ArrayList();
				foreach(Object x in this._treeView.Roots)
					newRoots.Add(x);
				foreach(Object x in modelObjects)
				{
					newRoots.Remove(x);
					this.mapObjectToIndex.Remove(x);
				}
				this.SetObjects(newRoots);
			}

			/// <summary>Set the roots of this tree to be the given collection</summary>
			/// <param name="collection"></param>
			public virtual void SetObjects(IEnumerable collection)
				// We interpret a SetObjects() call as setting the roots of the tree
				=> this._treeView.Roots = collection;

			/// <summary>Update/replace the nth Object with the given Object</summary>
			/// <param name="index"></param>
			/// <param name="modelObject"></param>
			public void UpdateObject(Int32 index, Object modelObject)
			{
				ArrayList newRoots = ObjectListView.EnumerableToArray(this._treeView.Roots, false);
				if(index < newRoots.Count)
					newRoots[index] = modelObject;
				this.SetObjects(newRoots);
			}

			#endregion

			#region IFilterableDataSource Members

			/// <summary></summary>
			/// <param name="mFilter"></param>
			/// <param name="lFilter"></param>
			public void ApplyFilters(IModelFilter mFilter, IListFilter lFilter)
			{
				this.modelFilter = mFilter;
				this.listFilter = lFilter;
				this.RebuildList();
			}

			/// <summary>Is this list currently being filtered?</summary>
			internal Boolean IsFiltering
			{
				get => this._treeView.UseFiltering && (this.modelFilter != null || this.listFilter != null);
			}

			/// <summary>Should the given model be included in this control?</summary>
			/// <param name="model">The model to consider</param>
			/// <returns>True if it will be included</returns>
			internal Boolean IncludeModel(Object model)
				=> this._treeView.UseFiltering == false
					|| this.modelFilter == null
					|| this.modelFilter.Filter(model);

			#endregion

			//------------------------------------------------------------------------------------------
			// Private instance variables

			private OLVColumn lastSortColumn;
			private SortOrder lastSortOrder;
			private readonly Dictionary<Object, Branch> mapObjectToBranch = new Dictionary<Object, Branch>();
			// ReSharper disable once InconsistentNaming
			internal Dictionary<Object, Boolean> mapObjectToExpanded = new Dictionary<Object, Boolean>();
			private readonly Dictionary<Object, Int32> mapObjectToIndex = new Dictionary<Object, Int32>();
			private ArrayList objectList = new ArrayList();
			private readonly TreeListView _treeView;
			private readonly Branch trunk;

			/// <summary></summary>
			// ReSharper disable once InconsistentNaming
			protected IModelFilter modelFilter;
			/// <summary></summary>
			// ReSharper disable once InconsistentNaming
			protected IListFilter listFilter;
		}

		/// <summary>A Branch represents a sub-tree within a tree</summary>
		public class Branch
		{
			/// <summary>Indicators for branches</summary>
			[Flags]
			public enum BranchFlags
			{
				/// <summary>FirstBranch of tree</summary>
				FirstBranch = 1,

				/// <summary>LastChild of parent</summary>
				LastChild = 2,

				/// <summary>OnlyBranch of tree</summary>
				OnlyBranch = 4
			}

			#region Life and death

			/// <summary>Create a Branch</summary>
			/// <param name="parent"></param>
			/// <param name="tree"></param>
			/// <param name="model"></param>
			public Branch(Branch parent, Tree tree, Object model)
			{
				this.ParentBranch = parent;
				this.Tree = tree;
				this.Model = model;
			}

			#endregion

			#region Public properties

			//------------------------------------------------------------------------------------------
			// Properties

			/// <summary>Get the ancestor branches of this branch, with the 'oldest' ancestor first.</summary>
			public virtual IList<Branch> Ancestors
			{
				get
				{
					List<Branch> ancestors = new List<Branch>();
					this.ParentBranch?.PushAncestors(ancestors);
					return ancestors;
				}
			}

			private void PushAncestors(IList<Branch> list)
			{
				// This is designed to ignore the trunk (which has no parent)
				if(this.ParentBranch != null)
				{
					this.ParentBranch.PushAncestors(list);
					list.Add(this);
				}
			}

			/// <summary>Can this branch be expanded?</summary>
			public virtual Boolean CanExpand
			{
				get => this.Tree.CanExpandGetter != null
					&& this.Model != null
					&& this.Tree.CanExpandGetter(this.Model);
			}

			/// <summary>Gets or sets our children</summary>
			public List<Branch> ChildBranches { get; set; } = new List<Branch>();

			/// <summary>Get/set the model objects that are beneath this branch</summary>
			public virtual IEnumerable Children
			{
				get
				{
					ArrayList children = new ArrayList();
					foreach(Branch x in this.ChildBranches)
						children.Add(x.Model);
					return children;
				}
				set
				{
					this.ChildBranches.Clear();

					TreeListView treeListView = this.Tree.TreeView;
					CheckState? checkedness = null;
					if(treeListView?.HierarchicalCheckboxes == true)
						checkedness = treeListView.GetCheckState(this.Model);
					foreach(Object x in value)
					{
						this.AddChild(x);

						// If the tree view is showing hierarchical checkboxes, then
						// when a child Object is first added, it has the same checkedness as this branch
						if(checkedness.HasValue && checkedness.Value == CheckState.Checked)
							treeListView.SetObjectCheckedness(x, checkedness.Value);
					}
				}
			}

			private void AddChild(Object childModel)
			{
				Branch br = this.Tree.GetBranch(childModel);
				if(br == null)
					br = this.Tree.MakeBranch(this, childModel);
				else
				{
					br.ParentBranch = this;
					br.Model = childModel;
					br.ClearCachedInfo();
				}
				this.ChildBranches.Add(br);
			}

			/// <summary>Gets a list of all the branches that survive filtering</summary>
			public List<Branch> FilteredChildBranches
			{
				get
				{
					if(!this.IsExpanded)
						return new List<Branch>();

					if(!this.Tree.IsFiltering)
						return this.ChildBranches;

					List<Branch> filtered = new List<Branch>();
					foreach(Branch b in this.ChildBranches)
					{
						if(this.Tree.IncludeModel(b.Model))
							filtered.Add(b);
						else
						{
							// Also include this branch if it has any filtered branches (yes, its recursive)
							if(b.FilteredChildBranches.Count > 0)
								filtered.Add(b);
						}
					}
					return filtered;
				}
			}

			/// <summary>Gets or set whether this branch is expanded</summary>
			public Boolean IsExpanded
			{
				get => this.Tree.IsModelExpanded(this.Model);
				set => this.Tree.SetModelExpanded(this.Model, value);
			}

			/// <summary>Return true if this branch is the first branch of the entire tree</summary>
			public virtual Boolean IsFirstBranch
			{
				get => ((this._flags & Branch.BranchFlags.FirstBranch) != 0);
				set
				{
					if(value)
						this._flags |= Branch.BranchFlags.FirstBranch;
					else
						this._flags &= ~Branch.BranchFlags.FirstBranch;
				}
			}

			/// <summary>Return true if this branch is the last child of its parent</summary>
			public virtual Boolean IsLastChild
			{
				get => ((this._flags & Branch.BranchFlags.LastChild) != 0);
				set
				{
					if(value)
						this._flags |= Branch.BranchFlags.LastChild;
					else
						this._flags &= ~Branch.BranchFlags.LastChild;
				}
			}

			/// <summary>Return true if this branch is the only top level branch</summary>
			public virtual Boolean IsOnlyBranch
			{
				get => ((this._flags & Branch.BranchFlags.OnlyBranch) != 0);
				set
				{
					if(value)
						this._flags |= Branch.BranchFlags.OnlyBranch;
					else
						this._flags &= ~Branch.BranchFlags.OnlyBranch;
				}
			}

			/// <summary>Gets the depth level of this branch</summary>
			public Int32 Level
			{
				get => this.ParentBranch == null
					? 0
					: this.ParentBranch.Level + 1;
			}

			/// <summary>Gets or sets which model is represented by this branch</summary>
			public Object Model { get; set; }

			/// <summary>Return the number of descendants of this branch that are currently visible</summary>
			/// <returns></returns>
			public virtual Int32 NumberVisibleDescendents
			{
				get
				{
					if(!this.IsExpanded)
						return 0;

					List<Branch> filtered = this.FilteredChildBranches;
					Int32 count = filtered.Count;
					foreach(Branch br in filtered)
						count += br.NumberVisibleDescendents;
					return count;
				}
			}

			/// <summary>Gets or sets our parent branch</summary>
			public Branch ParentBranch { get; set; }

			/// <summary>Gets or sets our overall tree</summary>
			public Tree Tree { get; set; }

			/// <summary>
			/// Is this branch currently visible? A branch is visible
			/// if it has no parent (i.e. it's a root), or its parent
			/// is visible and expanded.
			/// </summary>
			public virtual Boolean Visible
			{
				get => this.ParentBranch == null
					|| (this.ParentBranch.IsExpanded && this.ParentBranch.Visible);
			}

			#endregion

			#region Commands

			//------------------------------------------------------------------------------------------
			// Commands

			/// <summary>Clear any cached information that this branch is holding</summary>
			public virtual void ClearCachedInfo()
			{
				this.Children = new ArrayList();
				this._alreadyHasChildren = false;
			}

			/// <summary>Collapse this branch</summary>
			public virtual void Collapse()
				=> this.IsExpanded = false;

			/// <summary>Expand this branch</summary>
			public virtual void Expand()
			{
				if(this.CanExpand)
				{
					this.IsExpanded = true;
					this.FetchChildren();
				}
			}

			/// <summary>Expand this branch recursively</summary>
			public virtual void ExpandAll()
			{
				this.Expand();
				foreach(Branch br in this.ChildBranches)
				{
					if(br.CanExpand)
						br.ExpandAll();
				}
			}

			/// <summary>Collapse all branches in this tree</summary>
			/// <returns>Nothing useful</returns>
			public virtual void CollapseAll()
			{
				this.Collapse();
				foreach(Branch br in this.ChildBranches)
					if(br.IsExpanded)
						br.CollapseAll();
			}

			/// <summary>Fetch the children of this branch.</summary>
			/// <remarks>This should only be called when CanExpand is true.</remarks>
			public virtual void FetchChildren()
			{
				if(this._alreadyHasChildren)
					return;

				this._alreadyHasChildren = true;

				if(this.Tree.ChildrenGetter == null)
					return;

				Cursor previous = Cursor.Current;
				try
				{
					if(this.Tree.TreeView.UseWaitCursorWhenExpanding)
						Cursor.Current = Cursors.WaitCursor;
					this.Children = this.Tree.ChildrenGetter(this.Model);
				} finally
				{
					Cursor.Current = previous;
				}
			}

			/// <summary>Collapse the visible descendants of this branch into list of model objects</summary>
			/// <returns></returns>
			public virtual IList Flatten()
			{
				ArrayList flatList = new ArrayList();
				if(this.IsExpanded)
					this.FlattenOnto(flatList);
				return flatList;
			}

			/// <summary>Flatten this branch's visible descendants onto the given list.</summary>
			/// <param name="flatList"></param>
			/// <remarks>The branch itself is <b>not</b> included in the list.</remarks>
			public virtual void FlattenOnto(IList flatList)
			{
				Branch lastBranch = null;
				foreach(Branch br in this.FilteredChildBranches)
				{
					lastBranch = br;
					br.IsFirstBranch = br.IsOnlyBranch = br.IsLastChild = false;
					flatList.Add(br.Model);
					if(br.IsExpanded)
					{
						br.FetchChildren(); // make sure we have the branches children
						br.FlattenOnto(flatList);
					}
				}
				if(lastBranch != null)
					lastBranch.IsLastChild = true;
			}

			/// <summary>Force a refresh of all children recursively</summary>
			public virtual void RefreshChildren()
			{
				// Forget any previous children. We always do this so that if
				// IsExpanded or CanExpand have changed, we aren't left with stale information.
				this.ClearCachedInfo();

				if(!this.IsExpanded || !this.CanExpand)
					return;

				this.FetchChildren();
				foreach(Branch br in this.ChildBranches)
					br.RefreshChildren();
			}

			/// <summary>Sort the sub-branches and their descendants so they are ordered according to the given comparer.</summary>
			/// <param name="comparer">The comparer that orders the branches</param>
			public virtual void Sort(BranchComparer comparer)
			{
				if(this.ChildBranches.Count == 0)
					return;

				if(comparer != null)
					this.ChildBranches.Sort(comparer);

				foreach(Branch br in this.ChildBranches)
					br.Sort(comparer);
			}

			#endregion

			//------------------------------------------------------------------------------------------
			// Private instance variables

			private Boolean _alreadyHasChildren;
			private BranchFlags _flags;
		}

		/// <summary>This class sorts branches according to how their respective model objects are sorted</summary>
		public class BranchComparer : IComparer<Branch>
		{
			private readonly IComparer _actualComparer;

			/// <summary>Create a BranchComparer</summary>
			/// <param name="actualComparer"></param>
			public BranchComparer(IComparer actualComparer)
				=> this._actualComparer = actualComparer;

			/// <summary>Order the two branches</summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public Int32 Compare(Branch x, Branch y)
				=> this._actualComparer.Compare(x.Model, y.Model);
		}
	}

	/// <summary>
	/// This interface should be implemented by model objects that can provide children,
	/// but that don't have a parent. This is either because the model objects are always
	/// root level, or because they are used in TreeListView that never uses parent
	/// calculations. Parent calculations are only used when HierarchicalCheckBoxes is true.
	/// </summary>
	public interface ITreeModelWithChildren
	{
		/// <summary>Get whether this this model can be expanded? If true, an expand glyph will be drawn next to it.</summary>
		/// <remarks>This is called often! It must be fast. Don�t do a database lookup, calculate pi, or do linear searches � just return a property value.</remarks>
		Boolean TreeCanExpand { get; }

		/// <summary>Get the models that will be shown under this model when it is expanded.</summary>
		/// <remarks>This is only called when CanExpand returns true.</remarks>
		IEnumerable TreeChildren { get; }
	}

	/// <summary>
	/// This interface should be implemented by model objects that can never have children,
	/// but that are used in a TreeListView that uses parent calculations.
	/// Parent calculations are only used when HierarchicalCheckBoxes is true.
	/// </summary>
	public interface ITreeModelWithParent
	{

		/// <summary>Get the hierarchical parent of this model.</summary>
		Object TreeParent { get; }
	}

	/// <summary>
	/// ITreeModel allows model objects to provide the required information to TreeListView
	/// without using the normal Getter delegates.
	/// </summary>
	public interface ITreeModel : ITreeModelWithChildren, ITreeModelWithParent
	{

	}
}