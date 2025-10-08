using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace ObjectListViewDemo
{
	public partial class TabComplexExample : OlvDemoTab
	{
		public TabComplexExample()
		{
			this.InitializeComponent();
			this.ListView = this.olvComplex;
		}

		protected override void InitializeTab()
		{
			this.SetupFasterAspectGetters();
			this.SetupDragAndDrop();
			this.SetupTooltips();
			this.SetupColumns();
			this.SetupDecorations();
			this.SetupListView();

			this.SetupControls();

			// Uncomment this block to see a darker theme
			//SetupDarkerStyle();

			this.olvComplex.SetObjects(Coordinator.PersonList);
		}

		private void SetupListView()
		{
			// Setup some configuration of the ObjectListView itself
			// There is also the standard configuration in OlvDemoTab.SetupGeneralListViewEvents()

			this.olvComplex.TileSize = new Size(250, 120);

			// Do some fancy editing
			this.columnCookingSkill.CellEditUseWholeCell = true;
			this.olvComplex.CellEditStarting += listViewComplex_CellEditStarting;
			this.olvComplex.CellEditFinishing += listViewComplex_CellEditFinishing;
			this.olvComplex.CellEditValidating += listViewComplex_CellEditValidating;

			// Put love heart decorations on one of the items
			this.olvComplex.UseCellFormatEvents = true;
			this.olvComplex.FormatRow += olvComplex_FormatRow;
			this.olvComplex.FormatCell += olvComplex_FormatCell;

			// Just to be silly and show how it is done, when the user holds down the Ctrl key
			// while sorting by the Married column, the groups and items within groups will be
			// sorted by the second letter of their names.
			this.olvComplex.BeforeCreatingGroups += (sender, e) =>
			{
				if(e.Parameters.PrimarySort == olvMarriedColumn && Control.ModifierKeys == Keys.Control)
				{
					e.Parameters.GroupComparer = new StrangeGroupComparer(e.Parameters.PrimarySortOrder);
					e.Parameters.ItemComparer = new StrangeItemComparer(this.olvComplex.GetColumn(0), e.Parameters.PrimarySortOrder);
				}
			};

			// The Occupation column is configured to be a URL. This event decides which URL should be opened
			// when the link is clicked. Just to show how it works, any occupation that starts with "s" 
			// will not be a hyperlink
			this.olvComplex.IsHyperlink += (sender, e) =>
				e.Url = e.Text.ToLowerInvariant().StartsWith("s") ? null : "http://objectlistview.sourceforge.net";

			// Setup a right click handler that will show a menu that allows the user to jump to any group
			this.olvComplex.CellRightClick += (sender, cellRightClickEventArgs) =>
			{
				ContextMenuStrip ms = new ContextMenuStrip();
				ObjectListView olv = cellRightClickEventArgs.ListView;
				if(olv.ShowGroups)
					foreach(ListViewGroup lvg in olv.Groups)
					{
						ToolStripMenuItem mi = new ToolStripMenuItem(String.Format("Jump to group '{0}'", lvg.Header))
						{
							Tag = lvg
						};
						ms.Items.Add(mi);
					}
				else
				{
					ToolStripMenuItem mi = new ToolStripMenuItem("Turn on 'Show Groups' to see this context menu in action")
					{
						Enabled = false
					};
					ms.Items.Add(mi);
				}

				ms.ItemClicked += (_, e) =>
				{
					ToolStripMenuItem mi = (ToolStripMenuItem)e.ClickedItem;
					olv.EnsureGroupVisible((ListViewGroup)mi.Tag);
				};

				// Set the MenuStrip property to have the OLV show the context menu at the mouse position.
				cellRightClickEventArgs.MenuStrip = ms;
			};
		}

		private void SetupControls()
		{
			// Setup initial state of controls on tab
			if(ObjectListView.IsVistaOrLater)
				this.comboBoxHotItemStyle.Items.Add("Vista");
			this.comboBoxHotItemStyle.SelectedIndex = 3; // Translucent
			this.comboBoxView.SelectedIndex = 4; // Details
			this.comboBoxEditable.SelectedIndex = 0; // Not editable
		}

		private void SetupDecorations()
		{
			// Add a more interesting focus for editing operations
			this.olvComplex.AddDecoration(new EditingCellBorderDecoration(true));

			// ItemRenderer is used to draw the entire item when the OLV is in
			// non-details view. This code installs a custom renderer that draws
			// something that looks like a BusinessCard when the OLV is in Tile view
			this.olvComplex.ItemRenderer = new BusinessCardRenderer();
			this.olvComplex.TileSize = new Size(230, 120);
		}

		private void SetupColumns()
		{
			// Setup an AspectToString converter to show how it is done
			this.personColumn.AspectToStringConverter = (cellValue) => ((String)cellValue).ToUpperInvariant();

			// Change the icon used based on the data in the model being displayed.
			// When the OLV is not owner drawn, the ImageGetter must return either 
			// the name or the index of the image in the SmallImageList it wants to use.
			// If the OLV is owner drawn, the image getter can also just return an Image directly
			this.personColumn.ImageGetter = (row) =>
			{
				// People whose names start with a vowel get a star,
				// the last few letters get music and everyone else gets a person
				String name = ((Person)row).Name.ToUpperInvariant();
				if(name.Length > 0 && "AEIOU".Contains(name.Substring(0, 1)))
					return "star";

				if(name.CompareTo("T") < 0)
					return 2; // person

				return "music";
			};

			// Setup some fancy grouping formatting on the Cooking skill columns
			this.columnCookingSkill.MakeGroupies(
				new Object[] { 10, 20, 30, 40 },
				new String[] { "Pay to eat out", "Suggest take-away", "Passable", "Seek dinner invitation", "Hire as chef" },
				new String[] { "not", "hamburger", "toast", "beef", "chef" },
				new String[] {
					"Pay good money -- or flee the house -- rather than eat their home-cooked food",
					"Offer to buy takeaway rather than risk what may appear on your plate",
					"Neither spectacular nor dangerous",
					"Try to visit at dinner time to wrangle an invitation to dinner",
					"Do whatever is necessary to procure their services"
				},
				new String[] { "Call 911", "Phone PizzaHut", "", "Open calendar", "Check bank balance" }
				);

			// Setup grouping on the Hourly rate column
			// This uses the simpler form of MakeGroupies, that just produces a title for each group
			this.hourlyRateColumn.MakeGroupies(
				new Double[] { 100, 1000 },
				new String[] { "Less than $100", "$100-$1000", "Megabucks" });
			this.hourlyRateColumn.AspectPutter = (x, newValue) => ((Person)x).SetRate((Double)newValue);

			// The salary indicator column shows an image based on the person's salary.
			// This has two parts:
			// - the AspectGetter reduces the persons salary to a key value
			// - that key value is then used by a MappedImageRenderer to decide which image to display
			this.moneyImageColumn.AspectGetter = (row) =>
			{
				if(((Person)row).GetRate() < 100)
					return "Little";
				if(((Person)row).GetRate() > 1000)
					return "Lots";
				return "Medium";
			};
			this.moneyImageColumn.Renderer = new MappedImageRenderer(new Object[] {
				"Little", Resource.down16,
				"Medium", Resource.tick16,
				"Lots", Resource.star16
			});

			// Birthday column

			// When grouping on the birthday column, we want people to be grouped by the month 
			// of their birthday. To do this, we install a GroupKeyGetter -- which fetches the
			// number of the month -- and a GroupKeyToTitleConverter which takes a month number
			// and make it into a month name.
			// We could have just returned the month name from the GroupKeyGetter, but then the order
			// of the groups would have been alphabetical (April, August, December, February...), 
			// rather than chronological (January, February, March...).
			this.birthdayColumn.GroupKeyGetter = (row) => ((Person)row).BirthDate.Month;
			this.birthdayColumn.GroupKeyToTitleConverter = (key) => (new DateTime(1, (Int32)key, 1)).ToString("MMMM");
			this.birthdayColumn.ImageGetter = (row) =>
			{
				Person p = (Person)row;
				// People born in leap years get an asterisk (yes, the leap year calculation is wrong).
				if((p.BirthDate.Year % 4) == 0)
					return "hidden";

				return -1; // no image
			};

			// When using the filter on this column, we want to filter by birth month
			this.birthdayColumn.ClusteringStrategy = new DateTimeClusteringStrategy(DateTimePortion.Month, "MMMM");

			// Use this column to test sorting and group on TimeSpan objects
			this.daysSinceBirthColumn.AspectGetter = (row) => DateTime.Now - ((Person)row).BirthDate;
			this.daysSinceBirthColumn.AspectToStringConverter = (aspect) => ((TimeSpan)aspect).Days.ToString();

			// Give the Tells Jokes? column better grouping labels
			this.olvJokeColumn.GroupKeyToTitleConverter = (groupKey) =>
			{
				Boolean? canTellJokes = (Boolean?)groupKey;
				if(!canTellJokes.HasValue)
					return "Undecided";

				return canTellJokes.Value ? "Tells excellent jokes" : "Tells terrible Dad jokes";
			};
		}

		private void SetupDragAndDrop()
		{
			// Drag and drop support

			// Indicate that the OLV can be used to drag items out.
			// You can write this code, or you can set IsSimpleDropSource to true (in the Designer).
			this.olvComplex.DragSource = new SimpleDragSource();

			// Indicate that the OLV can be accept items being dropped on it.
			// These items could come from an ObjectListView or from some other control (or even
			// from another application).
			// If the drag source is an ObjectListView, you can listen for ModelCanDrop and
			// ModelDropped events, which have some extra properties to make your life easier.
			// If the drag source is not an ObjectListView, you have to listen for CanDrop and
			// Dropped events, and figure out everything for yourself.

			SimpleDropSink dropSink = new SimpleDropSink
			{
				CanDropOnItem = true,
				//dropSink.CanDropOnSubItem = true;
				FeedbackColor = Color.IndianRed // just to be different
			};
			this.olvComplex.DropSink = dropSink;

			// For our purpose here, we will make it that if you drop one or more person
			// onto someone, they all become married. 
			dropSink.ModelCanDrop += (sender, e) =>
			{
				if(e.TargetModel is Person person)
				{
					if(person.MaritalStatus == MaritalStatus.Married)
					{
						e.Effect = DragDropEffects.None;
						e.InfoMessage = "Can't drop on someone who is already married";
					} else
						e.Effect = DragDropEffects.Move;
				} else
					e.Effect = DragDropEffects.None;
			};

			dropSink.ModelDropped += (sender, e) =>
			{
				if(e.TargetModel == null)
					return;

				// Change the dropped people plus the target person to be married
				((Person)e.TargetModel).MaritalStatus = MaritalStatus.Married;
				foreach(Person p in e.SourceModels)
					p.MaritalStatus = MaritalStatus.Married;

				// Force them to refresh
				e.RefreshObjects();
			};
		}

		private void SetupFasterAspectGetters()
		{
			// The following line makes getting aspect about 10x faster. Since getting the aspect is
			// the slowest part of building the ListView, it is worthwhile BUT NOT NECESSARY to do.

			TypedObjectListView<Person> tlist = new TypedObjectListView<Person>(this.olvComplex);
			tlist.GenerateAspectGetters();

			/* 
			 * The line above the equivalent to typing the following:
			 */

			//            tlist.GetColumn(0).AspectGetter = delegate(Person x) { return x.Name; };
			//            tlist.GetColumn(1).AspectGetter = delegate(Person x) { return x.Occupation; };
			//            tlist.GetColumn(2).AspectGetter = delegate(Person x) { return x.CulinaryRating; };
			//            tlist.GetColumn(3).AspectGetter = delegate(Person x) { return x.YearOfBirth; };
			//            tlist.GetColumn(4).AspectGetter = delegate(Person x) { return x.BirthDate; };
			//            tlist.GetColumn(5).AspectGetter = delegate(Person x) { return x.GetRate(); };
			//            tlist.GetColumn(6).AspectGetter = delegate(Person x) { return x.Comments; };
		}

		private void SetupDarkerStyle()
		{
			// This shows how to change the styling to something darker. 
			// The other parts of this demo are not setup to work well with this 
			// darker theme (e.g. other hot item styles)

			this.olvComplex.UseAlternatingBackColors = true;
			this.olvComplex.BackColor = Color.FromArgb(30, 30, 40);
			this.olvComplex.AlternateRowBackColor = Color.FromArgb(35, 35, 42);
			this.olvComplex.ForeColor = Color.WhiteSmoke;
			this.olvComplex.DefaultRenderer = new HighlightTextRenderer { FillBrush = Brushes.Transparent, FramePen = new Pen(Color.PaleGreen, 3) };

			this.olvComplex.HyperlinkStyle = new HyperlinkStyle
			{
				Normal = new CellStyle
				{
					FontStyle = FontStyle.Underline,
				},
			};

			this.olvComplex.DisabledItemStyle = new SimpleItemStyle
			{
				ForeColor = Color.Gray,
				BackColor = Color.FromArgb(30, 30, 35),
				Font = new Font("Stencil", 10)
			};
		}

		private void SetupTooltips()
		{
			// Setup some more complex tooltips
			// This is normally done via the ToolTipShowing event, though
			// many styling properties can be setup beforehand.

			// Don't use a normal ToolTip control with an ObjectListView.
			// A normal ToolTip treats the ListView as being a single control
			// with a single tooltip for the whole control. ObjectListView
			// has a built-in tooltip mechanism which is much more powerful.

			// Make the tooltips look somewhat different
			// Unfortunately, styling on tooltip controls only works on XP!
			// Even balloon style doesn't look good on systems after XP.
			//            this.olvComplex.CellToolTip.BackColor = Color.Red;
			//            this.olvComplex.CellToolTip.ForeColor = Color.Green;
			//            this.olvComplex.CellToolTip.IsBalloon = true;
			//            this.olvComplex.HeaderToolTip.BackColor = Color.Blue;
			//            this.olvComplex.HeaderToolTip.ForeColor = Color.Red;
			//            this.olvComplex.HeaderToolTip.IsBalloon = true;

			this.olvComplex.HeaderToolTipShowing += (sender, e) =>
			{
				if(Control.ModifierKeys != Keys.Control)
					return;

				e.Title = "Information";
				e.StandardIcon = ToolTipControl.StandardIcons.Info;
				e.AutoPopDelay = 10000;
				e.Text = String.Format("More details about the '{0}' column\r\n\r\nThis only shows when the control key is down.",
					e.Column.Text);
			};

			this.olvComplex.CellToolTipShowing += (sender, e) =>
			{
				// Show a long tooltip over cells when the control key is down
				if(Control.ModifierKeys != Keys.Control)
					return;

				OLVColumn col = e.Column ?? e.ListView.GetColumn(0);
				String stringValue = col.GetStringValue(e.Model);
				if(stringValue.StartsWith("m", StringComparison.InvariantCultureIgnoreCase))
				{
					e.ToolTipControl.SetMaxWidth(400);
					e.Title = "WARNING";
					e.StandardIcon = ToolTipControl.StandardIcons.InfoLarge;

					// Changing colour doesn't work in systems other than XP
					e.BackColor = Color.AliceBlue;
					e.ForeColor = Color.IndianRed;

					e.AutoPopDelay = 15000;
					e.Font = new Font("Tahoma", 12.0f);
					e.Text = "THIS VALUE BEGINS WITH A DANGEROUS LETTER!\r\n\r\n" +
							 "On no account should members of the public attempt to pronounce this word without " +
							 "the assistance of trained vocalization specialists.";
				} else
				{
					e.Text = String.Format("Tool tip for '{0}', column '{1}'\r\nValue shown: '{2}'",
						((Person)e.Model).Name, col.Text, stringValue);
				}
			};

		}

		#region ObjectListView event handlers

		private void olvComplex_FormatRow(Object sender, FormatRowEventArgs e)
		{
			// We only want to do this when we AREN'T in Detail view.
			// The heart decoration is added in the FormatCell event when in Details view
			if(olvComplex.View == View.Details)
				return;

			e.Item.Decoration = e.Item.Text.ToLowerInvariant().StartsWith("nicola")
				? new ImageDecoration(Resource.loveheart, 64)
				: null;
		}

		private void olvComplex_FormatCell(Object sender, FormatCellEventArgs e)
		{
			_ = (Person)e.Model;

			// Put a love heart next to Nicola's name :)
			if(e.ColumnIndex == 0)
			{
				if(e.SubItem.Text.ToLowerInvariant().StartsWith("nicola"))
					e.SubItem.Decoration = new ImageDecoration(Resource.loveheart, 64);
				else
					e.SubItem.Decoration = null;
			}

			// If the occupation is missing a value, put a composite decoration over it
			// to draw attention to.
			if(e.ColumnIndex == 1 && e.SubItem.Text == String.Empty)
			{
				e.SubItem.Decoration = new TextDecoration("Missing!", 255)
				{
					Alignment = ContentAlignment.MiddleCenter,
					Font = new Font(this.Font.Name, this.Font.SizeInPoints + 2),
					TextColor = Color.Firebrick,
					Rotation = -20
				};
				e.SubItem.Decorations.Add(new CellBorderDecoration
				{
					BorderPen = new Pen(Color.FromArgb(128, Color.Firebrick)),
					FillBrush = null,
					CornerRounding = 4.0f
				});
			}
		}

		private void listViewComplex_CellEditStarting(Object sender, CellEditEventArgs e)
		{
			// We only want to mess with the Cooking Skill column
			if(e.Column.Text != "Cooking skill")
				return;

			Person personBeingEdited = (Person)e.RowObject;
			ComboBox cb = new ComboBox
			{
				Bounds = e.CellBounds,
				Font = ((ObjectListView)sender).Font,
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			cb.Items.AddRange(new Object[] { "Pay to eat out", "Suggest take-away", "Passable", "Seek dinner invitation", "Hire as chef" });
			cb.SelectedIndex = Math.Max(0, Math.Min(cb.Items.Count - 1, ((Int32)e.Value) / 10));
			cb.SelectedIndexChanged += (o, args) =>
				personBeingEdited.CulinaryRating = cb.SelectedIndex * 10;
			ControlUtilities.AutoResizeDropDown(cb);

			e.Control = cb;
		}

		private void listViewComplex_CellEditValidating(Object sender, CellEditEventArgs e)
		{
			// Disallow professions from starting with "a" or "z" -- just to be arbitrary
			if(e.Column.Text == "Occupation")
			{
				String newValue = ((TextBox)e.Control).Text.ToLowerInvariant();
				if(newValue.StartsWith("a") || newValue.StartsWith("z"))
				{
					e.Cancel = true;
					MessageBox.Show(this, "Occupations cannot begin with 'a' or 'z' (just to show cell edit validation at work).", "ObjectListViewDemo",
						MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}

			// Disallow birthdays from being on the 29th -- just to be arbitrary
			if(e.Column.Text == "Birthday")
			{
				DateTime newValue = ((DateTimePicker)e.Control).Value;
				if(newValue.Day == 29)
				{
					e.Cancel = true;
					MessageBox.Show(this, "Sorry. Birthdays cannot be on 29th of any month (just to show cell edit validation at work).", "ObjectListViewDemo",
						MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private void listViewComplex_CellEditFinishing(Object sender, CellEditEventArgs e)
		{
			// We only want to mess with the Cooking Skill column
			if(e.Column.Text != "Cooking skill")
				return;

			// ObjectListView will automatically dispose of the control we created.
			// If we didn't want that (e.g. if the control was expensive to create and we
			// wanted to cache and reuse it), we would set e.AutoDispose = false

			// Any updating will have been down in the SelectedIndexChanged event handler
			// Here we simply make the list redraw the involved ListViewItem
			((ObjectListView)sender).RefreshItem(e.ListViewItem);

			// We have updated the model Object, so we cancel the auto update
			e.Cancel = true;
		}

		#endregion

		#region UI event handlers 

		private void textBoxFilterComplex_TextChanged(Object sender, EventArgs e)
			=> this.Coordinator.TimedFilter(this.ListView, ((TextBox)sender).Text);

		private void buttonAdd_Click(Object sender, EventArgs e)
		{
			List<Person> list = new List<Person>
			{
				new Person("A New Person"),
				new Person("Brave New Person"),
				new Person("someone like e e cummings"),
				new Person("Luis Nova Pessoa")
			};

			// Give him a birthday that will display an image to make sure the image appears.
			list[list.Count - 1].BirthDate = new DateTime(1984, 12, 25);

			this.olvComplex.AddObjects(list);
		}

		private void comboBoxHotItemStyle_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeHotItemStyle(this.ListView, (ComboBox)sender);

		private void comboBoxView_SelectedIndexChanged(Object sender, EventArgs e)
		{
			this.Coordinator.ChangeView(this.ListView, (ComboBox)sender);

			// Make the hot item show an overlay when it changes
			if(this.olvComplex.UseTranslucentHotItem)
			{
				this.olvComplex.HotItemStyle.Overlay = new BusinessCardOverlay();
				this.olvComplex.HotItemStyle = this.olvComplex.HotItemStyle;
			}

			this.olvComplex.UseTranslucentSelection = this.olvComplex.UseTranslucentHotItem;

			this.olvComplex.Invalidate();
		}

		private void buttonRebuild_Click(Object sender, EventArgs e)
			=> this.Coordinator.TimedRebuildList(this.ListView);

		private void buttonRemove_Click(Object sender, EventArgs e)
			=> this.ListView.RemoveObjects(this.ListView.SelectedObjects);

		private void comboBoxEditable_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeEditable(this.ListView, (ComboBox)sender);

		private void checkBoxGroups_CheckedChanged(Object sender, EventArgs e)
			=> this.Coordinator.ShowGroupsChecked(this.ListView, (CheckBox)sender);

		private void buttonDisable_Click(Object sender, EventArgs e)
		{
			// If the user Ctrl-Clicks on the button, enable all rows
			Boolean isControlKeyDown = ((Control.ModifierKeys & Keys.Control) == Keys.Control);
			if(isControlKeyDown)
				this.ListView.EnableObjects(this.ListView.DisabledObjects);
			else
				this.ListView.DisableObjects(this.ListView.SelectedObjects);
		}

		#endregion
	}

	/// <summary>
	/// This comparer sort groups alphabetically by their header, BUT ignoring the first letter.
	/// Not at all useful by itself, but it's a good example of how to implement a group orderer.
	/// </summary>
	public class StrangeGroupComparer : IComparer<OLVGroup>
	{
		public StrangeGroupComparer(SortOrder order)
			=> this._sortOrder = order;

		public Int32 Compare(OLVGroup x, OLVGroup y)
		{
			String xValue = x.Header;
			String yValue = y.Header;

			if(xValue.Length >= 2)
				xValue = xValue.Substring(1);
			if(yValue.Length >= 2)
				yValue = yValue.Substring(1);

			Int32 result = String.Compare(xValue, yValue, StringComparison.CurrentCultureIgnoreCase);

			if(this._sortOrder == SortOrder.Descending)
				result = 0 - result;

			return result;
		}

		private SortOrder _sortOrder;
	}

	/// <summary>
	/// StrangeItemComparer is an example of how to customize the ordering of items
	/// within groups. It orders items by their text representation but ignoring
	/// the first letter. This admittedly a pointless way to order items, but it
	/// is simply an example.
	/// </summary>
	public class StrangeItemComparer : IComparer<OLVListItem>
	{
		public StrangeItemComparer(OLVColumn col, SortOrder order)
		{
			this._column = col;
			this._sortOrder = order;
		}

		public Int32 Compare(OLVListItem x, OLVListItem y)
		{
			String xValue = this._column.GetStringValue(x.RowObject);
			String yValue = this._column.GetStringValue(y.RowObject);

			if(xValue.Length >= 2)
				xValue = xValue.Substring(1);
			if(yValue.Length >= 2)
				yValue = yValue.Substring(1);

			Int32 result = String.Compare(xValue, yValue, StringComparison.CurrentCultureIgnoreCase);

			if(this._sortOrder == SortOrder.Descending)
				result = 0 - result;

			return result;
		}

		private OLVColumn _column;
		private SortOrder _sortOrder;
	}
}