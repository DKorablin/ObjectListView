using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ObjectListViewDemo.Properties;

namespace ObjectListViewDemo
{
	public partial class TabDragAndDrop : OlvDemoTab
	{
		public TabDragAndDrop()
			=> this.InitializeComponent();

		protected override void InitializeTab()
		{
			this.SetupColumns();
			this.SetupDragAndDrop();

			this.comboBoxGeeksAndTweebsView.SelectedIndex = 4;
			this.comboBoxCoolFroodsView.SelectedIndex = 4;

			this.olvGeeks.SetObjects(Coordinator.PersonList);
		}

		private void SetupColumns()
		{
			this.olvGeeks.GetColumn(0).ImageGetter = (x) => "user";
			this.olvFroods.GetColumn(0).ImageGetter = (x) => "user";

			this.olvGeeks.GetColumn(2).Renderer = new MultiImageRenderer(Resource.star16, 5, 0, 40);
			this.olvFroods.GetColumn(2).Renderer = new MultiImageRenderer(Resource.star16, 5, 0, 40);
		}

		private void SetupDragAndDrop()
		{
			// Make each listview capable of dragging rows out
			this.olvGeeks.DragSource = new SimpleDragSource();
			this.olvFroods.DragSource = new SimpleDragSource();

			// Make each listview capable of accepting drops.
			// More than that, make it so it's items can be rearranged
			this.olvGeeks.DropSink = new RearrangingDropSink(true);
			this.olvFroods.DropSink = new RearrangingDropSink(true);

			// For a normal drag and drop situation, you will need to create a SimpleDropSink
			// and then listen for ModelCanDrop and ModelDropped events
		}

		#region UI event handlers

		private void comboBoxGeeksAndTweebsView_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeView(this.olvGeeks, (ComboBox)sender);

		private void comboBoxCoolFroodsView_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeView(this.olvFroods, (ComboBox)sender);
		#endregion
	}
}