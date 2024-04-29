using System;
using System.ComponentModel;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace ObjectListViewDemo
{
	public class OlvDemoTab : UserControl
	{
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public OLVDemoCoordinator Coordinator
		{
			get => this._coordinator;
			set
			{
				this._coordinator = value;
				if(value != null)
				{
					this.InitializeTab();
					this.SetupGeneralListViewEvents();
				}
			}
		}
		private OLVDemoCoordinator _coordinator;

		protected virtual void InitializeTab() { }

		public ObjectListView ListView
		{
			get;
			protected set;
		}

		private void SetupGeneralListViewEvents()
		{
			if(this.ListView == null || this.Coordinator == null)
				return;

			this.ListView.SelectionChanged += (sender, args)
				=> this.Coordinator.HandleSelectionChanged(this.ListView);

			this.ListView.HotItemChanged += (sender, args)
				=> this.Coordinator.HandleHotItemChanged(sender, args);

			this.ListView.GroupTaskClicked += (sender, args)
				=> this.Coordinator.ShowMessage("Clicked on group task: " + args.Group.Name);

			this.ListView.GroupStateChanged += (sender, e)
				=> System.Diagnostics.Debug.WriteLine(String.Format("Group '{0}' was {1}{2}{3}{4}{5}{6}",
					e.Group.Header,
					e.Selected ? "Selected" : String.Empty,
					e.Focused ? "Focused" : String.Empty,
					e.Collapsed ? "Collapsed" : String.Empty,
					e.Unselected ? "Unselected" : String.Empty,
					e.Unfocused ? "Unfocused" : String.Empty,
					e.Uncollapsed ? "Uncollapsed" : String.Empty));
		}
	}
}