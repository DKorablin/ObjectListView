using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ObjectListViewDemo.Models;
using ObjectListViewDemo.Properties;

namespace ObjectListViewDemo
{
	public partial class TabFastList : OlvDemoTab
	{
		public TabFastList()
		{
			this.InitializeComponent();
			this.ListView = this.olvFast;
		}

		protected override void InitializeTab()
		{
			this.SetupControls();
			this.SetupColumns();

			this.olvFast.SetObjects(Coordinator.PersonList);
		}

		private void SetupControls()
		{
			comboBoxFilterType.SelectedIndex = 2;
			comboBoxEditable.SelectedIndex = 0;
			comboBoxView.SelectedIndex = 4;
		}

		private void SetupColumns()
		{

			// Setup all the columns for the control.
			// Using a FastObjectListView is almost identical to using a normal ObjectListView.
			// So almost all the setup here is the same as the setup for the Complex tab.

			// One difference is that I've written AspectGetters for each column.
			// AspectGetters are always much faster than using AspectNames, and the whole point
			// of using a FastObjectListView is to be, well, faster. 

			this.olvColumn18.AspectGetter = (x) => ((Person)x).Name;

			this.olvColumn18.ImageGetter = (row) =>
			{
				// People whose names start with a vowel get a star,
				// otherwise the first half of the alphabet gets hearts
				// and the second half gets music
				Person person = (Person)row;
				if("AEIOU".Contains(person.Name.Substring(0, 1)))
					return 0; // star
				if(person.Name.CompareTo("N") < 0)
					return 1; // heart
				return 2; // music
			};

			this.olvColumn19.AspectGetter = (x) => ((Person)x).Occupation;
			this.olvColumn26.AspectGetter = (x) => ((Person)x).CulinaryRating;
			this.olvColumn26.Renderer = new MultiImageRenderer(Resource.star16, 5, 0, 40);
			this.olvColumn26.MakeGroupies(
				new Object[] { 10, 20, 30, 40 },
				new String[] { "Pay to eat out", "Suggest take-away", "Passable", "Seek dinner invitation", "Hire as chef" },
				new String[] { "not", "hamburger", "toast", "beef", "chef" },
				new String[] {
					"Pay good money -- or flee the house -- rather than eat their homecooked food",
					"Offer to buy takeaway rather than risk what may appear on your plate",
					"Neither spectacular nor dangerous",
					"Try to visit at dinner time to wrangle an invitation to dinner",
					"Do whatever is necessary to procure their services"
				},
				new String[] { "Call 911", "Phone PizzaHut", "", "Open calendar", "Check bank balance" }
				);

			this.olvColumn27.AspectGetter = (x) => ((Person)x).YearOfBirth;

			this.olvColumn28.AspectGetter = (x) => ((Person)x).BirthDate;
			this.olvColumn28.ImageGetter = (row) =>
			{
				Person p = (Person)row;
				if((p.BirthDate.Year % 10) == 4)
					return 3;
				return -1; // no image
			};

			this.olvColumn29.AspectGetter = (x) => ((Person)x).GetRate();
			this.olvColumn29.AspectPutter = (x, newValue) => ((Person)x).SetRate((Double)newValue);

			this.olvColumn31.AspectGetter = (row) =>
			{
				if(((Person)row).GetRate() < 100)
					return "Little";
				if(((Person)row).GetRate() > 1000)
					return "Lots";
				return "Medium";
			};
			this.olvColumn31.Renderer = new MappedImageRenderer(new Object[] { "Little", Resource.down16, "Medium", Resource.tick16, "Lots", Resource.star16 });

			this.olvColumn32.AspectGetter = (row) => DateTime.Now - ((Person)row).BirthDate;
			this.olvColumn32.AspectToStringConverter = (aspect) => ((TimeSpan)aspect).Days.ToString("#,##0");

			this.olvColumn33.AspectGetter = (row) => ((Person)row).CanTellJokes;
		}

		#region UI event handlers

		private void textBoxFilterFast_TextChanged(Object sender, EventArgs e)
			=> this.Coordinator.TimedFilter(this.ListView, ((TextBox)sender).Text, this.comboBoxFilterType.SelectedIndex);

		private void comboBoxFilterType_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.TimedFilter(this.ListView, this.textBoxFilterFast.Text, this.comboBoxFilterType.SelectedIndex);

		private void checkBoxGroups_CheckedChanged(Object sender, EventArgs e)
			=> this.Coordinator.ShowGroupsChecked(this.ListView, (CheckBox)sender);

		private void checkBoxCheckboxes_CheckedChanged(Object sender, EventArgs e)
			=> this.ListView.CheckBoxes = ((CheckBox)sender).Checked;

		private void comboBoxEditable_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeEditable(this.ListView, (ComboBox)sender);

		private void comboBoxView_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeView(this.ListView, (ComboBox)sender);

		private void buttonCopy_Click(Object sender, EventArgs e)
			=> this.ListView.CopyObjectsToClipboard(this.ListView.CheckedObjects);

		private void buttonDisable_Click(Object sender, EventArgs e)
		{
			Stopwatch ts = Stopwatch.StartNew();

			Boolean isControlKeyDown = ((Control.ModifierKeys & Keys.Control) == Keys.Control);
			if(isControlKeyDown)
				this.ListView.EnableObjects(this.ListView.DisabledObjects);
			else
				this.ListView.DisableObjects(this.ListView.SelectedObjects);

			Debug.WriteLine(String.Format("Disable UI action took {0} ms", ts.Elapsed.TotalMilliseconds));
		}

		private void buttonRemove_Click(Object sender, EventArgs e)
		{
			Boolean isControlKeyDown = ((Control.ModifierKeys & Keys.Control) == Keys.Control);
			if(isControlKeyDown)
				this.ListView.ClearObjects();
			else
				this.ListView.RemoveObjects(this.ListView.SelectedObjects);
		}

		private void buttonAdd_Click(Object sender, EventArgs e)
		{
			ArrayList l = new ArrayList();
			while(l.Count < 1000)
			{
				Person x = this.Coordinator.PersonList[l.Count % Coordinator.PersonList.Count];
				l.Add(new Person(x));
			}

			Stopwatch stopWatch = new Stopwatch();
			try
			{
				this.Cursor = Cursors.WaitCursor;
				stopWatch.Start();
				this.olvFast.AddObjects(l);
			} finally
			{
				stopWatch.Stop();
				this.Cursor = Cursors.Default;
			}

			this.Coordinator.ToolStripStatus1 =
				String.Format("Build time: {0} items in {1}ms, average per item: {2:F}ms",
					this.olvFast.Items.Count,
					stopWatch.ElapsedMilliseconds,
					(Single)stopWatch.ElapsedMilliseconds / this.olvFast.Items.Count);
		}

		#endregion

		/// <summary>Add this decoration as a cell decoration to your ListView to give a grid line effect.</summary>
		/// <remarks>
		/// Setting GridLines = true works fine, EXCEPT when the ListView
		/// is grouped -- in which cause there are no grid lines.
		/// This decoration will work in either mode.
		/// </remarks>
		/// <example>
		/// this.olv.UseCellFormatEvents = true;
		/// var gridLineCellDecoration = new GridLineCellDecoration();
		/// this.olv.FormatCell += delegate(Object sender, FormatCellEventArgs args) {
		///     args.SubItem.Decoration = gridLineCellDecoration;
		/// };
		/// </example>
		public class GridLineCellDecoration : CellBorderDecoration
		{
			public GridLineCellDecoration()
				=> this.BorderPen = new Pen(Color.FromArgb(255, 0xE0, 0xEC, 0xEF), 1);

			protected override Rectangle CalculateBounds()
			{
				Rectangle bounds = this.CellBounds;
				if(bounds.IsEmpty)
					return bounds;

				// It seems cell 0 is off by 1 on the x-axis
				if(this.ListItem.SubItems[0] == this.SubItem)
					bounds.X -= 1;

				// We want the grid of one cell to overlap with the bottom of the previous cell,
				// so we move the top up by one but don't move the bottom
				bounds.Y -= 1;
				bounds.Height += 1;

				return bounds;
			}

			public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
			{
				Rectangle bounds = this.CalculateBounds();
				if(!bounds.IsEmpty && this.BorderPen != null)
					g.DrawRectangle(this.BorderPen, bounds);
			}
		}
	}
}