using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace ObjectListViewDemo
{
	public partial class TabFileExplorer : OlvDemoTab
	{

		public TabFileExplorer()
		{
			this.InitializeComponent();
			this.ListView = this.olvFiles;
		}

		protected override void InitializeTab()
		{
			this.SetupControls();

			this.SetupList();
			this.SetupColumns();

			this.PopulateListFromPath(this.textBoxFolderPath.Text);
		}

		private void SetupControls()
		{
			// Setup initial state of controls on tab
			if(ObjectListView.IsVistaOrLater)
				this.comboBoxHotItemStyle.Items.Add("Vista");
			this.comboBoxView.SelectedIndex = 4; // Details
			this.comboBoxHotItemStyle.SelectedIndex = 0; // None
			this.comboBoxNagLevel.SelectedIndex = 0; // Slight
			this.textBoxFolderPath.Text = @"c:\";
		}

		private void SetupList()
		{
			// We want to draw the system icon against each file name. SysImageListHelper does this work for us.

			SysImageListHelper helper = new SysImageListHelper(this.olvFiles);
			this.olvColumnName.ImageGetter = x => helper.GetImageIndex(((FileSystemInfo)x).FullName);

			// Show tooltips when the appropriate checkbox is clicked
			this.olvFiles.ShowItemToolTips = true;
			this.olvFiles.CellToolTipShowing += delegate (Object sender, ToolTipShowingEventArgs e)
			{
				if(this.showToolTipsOnFiles)
					e.Text = String.Format("Tool tip for '{0}', column '{1}'\r\nValue shown: '{2}'", e.Model, e.Column.Text, e.SubItem.Text);
			};

			// Show a menu -- but only when the user right clicks on the first column
			this.olvFiles.CellRightClick += delegate (Object sender, CellRightClickEventArgs e)
			{
				Trace.WriteLine(String.Format("right clicked {0}, {1}). model {2}", e.RowIndex, e.ColumnIndex, e.Model));
				if(e.ColumnIndex == 0)
					e.MenuStrip = this.contextMenuStrip2;
			};
		}

		private void SetupColumns()
		{
			// Get the size of the file system entity. 
			// Folders and errors are represented as negative numbers
			this.olvColumnSize.AspectGetter = x =>
			{
				if(x is DirectoryInfo)
					return (Int64)(-1);

				try
				{
					return ((FileInfo)x).Length;
				} catch(FileNotFoundException)
				{
					// Mono 1.2.6 throws this for hidden files
					return (Int64)(-2);
				}
			};

			// Show the size of files as GB, MB and KBs. By returning the actual
			// size in the AspectGetter, and doing the conversion in the 
			// AspectToStringConverter, sorting on this column will work off the
			// actual sizes, rather than the formatted String.
			this.olvColumnSize.AspectToStringConverter = x=>
			{
				Int64 sizeInBytes = (Int64)x;
				return sizeInBytes < 0
					? String.Empty // folder or error
					: this.Coordinator.FormatFileSize(sizeInBytes);
			};
			this.olvColumnSize.MakeGroupies(new Int64[] { 0, 1024 * 1024, 512 * 1024 * 1024 },
				new String[] { "Folders", "Small", "Big", "Disk space chewer" });

			// Group by month-year, rather than date
			// This code is duplicated for FileCreated and FileModified, so we really should
			// create named methods rather than using anonymous delegates.
			this.olvColumnCreated.GroupKeyGetter = delegate (Object x)
			{
				DateTime dt = ((FileSystemInfo)x).CreationTime;
				return new DateTime(dt.Year, dt.Month, 1);
			};
			this.olvColumnCreated.GroupKeyToTitleConverter = x => ((DateTime)x).ToString("MMMM yyyy");

			// Group by month-year, rather than date
			this.olvColumnModified.GroupKeyGetter = delegate (Object x)
			{
				DateTime dt = ((FileSystemInfo)x).LastWriteTime;
				return new DateTime(dt.Year, dt.Month, 1);
			};
			this.olvColumnModified.GroupKeyToTitleConverter = x => ((DateTime)x).ToString("MMMM yyyy");

			// Show the system description for this Object
			this.olvColumnFileType.AspectGetter = x => ShellUtilities.GetFileType(((FileSystemInfo)x).FullName);

			// Show the file attributes for this Object
			// A FlagRenderer masks off various values and draws zero or more images based 
			// on the presence of individual bits.
			this.olvColumnAttributes.AspectGetter = x => ((FileSystemInfo)x).Attributes;

			FlagRenderer attributesRenderer = new FlagRenderer
			{
				ImageList = imageList1
			};
			attributesRenderer.Add(FileAttributes.Archive, "archive");
			attributesRenderer.Add(FileAttributes.ReadOnly, "readonly");
			attributesRenderer.Add(FileAttributes.System, "system");
			attributesRenderer.Add(FileAttributes.Hidden, "hidden");
			attributesRenderer.Add(FileAttributes.Temporary, "temporary");
			this.olvColumnAttributes.Renderer = attributesRenderer;

			// Tell the filtering subsystem that the attributes column is a collection of flags
			this.olvColumnAttributes.ClusteringStrategy = new FlagClusteringStrategy(typeof(FileAttributes));
		}

		private void PopulateListFromPath(String path)
		{
			if(String.IsNullOrEmpty(path))
				return;

			DirectoryInfo pathInfo = new DirectoryInfo(path);
			if(!pathInfo.Exists)
				return;

			Stopwatch sw = new Stopwatch();

			Cursor.Current = Cursors.WaitCursor;
			sw.Start();
			this.olvFiles.SetObjects(pathInfo.GetFileSystemInfos());
			sw.Stop();
			Cursor.Current = Cursors.Default;

			Single msPerItem = (olvFiles.Items.Count == 0 ? 0 : (Single)sw.ElapsedMilliseconds / olvFiles.Items.Count);
			this.Coordinator.ToolStripStatus1 = String.Format("Timed build: {0} items in {1}ms ({2:F}ms per item)",
				olvFiles.Items.Count, sw.ElapsedMilliseconds, msPerItem);
		}

		#region UI event handlers

		private void olvFiles_ItemActivate(Object sender, EventArgs e)
		{
			Object rowObject = this.olvFiles.SelectedObject;
			if(rowObject == null)
				return;

			if(rowObject is DirectoryInfo info)
			{
				this.textBoxFolderPath.Text = info.FullName;
				this.buttonGo.PerformClick();
			} else
				ShellUtilities.Execute(((FileInfo)rowObject).FullName);
		}

		private void textBoxFolderPath_TextChanged(Object sender, EventArgs e)
		{
			if(Directory.Exists(this.textBoxFolderPath.Text))
			{
				this.textBoxFolderPath.ForeColor = Color.Black;
				this.buttonGo.Enabled = true;
				this.buttonUp.Enabled = true;
			} else
			{
				this.textBoxFolderPath.ForeColor = Color.Red;
				this.buttonGo.Enabled = false;
				this.buttonUp.Enabled = false;
			}
		}

		private void textBoxFolderPath_KeyPress(Object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == (Char)13)
			{
				this.buttonGo.PerformClick();
				e.Handled = true;
			}
		}

		private void buttonGo_Click(Object sender, EventArgs e)
		{
			String path = this.textBoxFolderPath.Text;
			this.PopulateListFromPath(path);
		}

		private void buttonUp_Click(Object sender, EventArgs e)
		{
			DirectoryInfo di = Directory.GetParent(this.textBoxFolderPath.Text);
			if(di == null)
				System.Media.SystemSounds.Asterisk.Play();
			else
			{
				this.textBoxFolderPath.Text = di.FullName;
				this.buttonGo.PerformClick();
			}
		}

		private void comboBoxNagLevel_SelectedIndexChanged(Object sender, EventArgs e)
		{
			this.ListView.RemoveOverlay(this.nagOverlay);

			this.nagOverlay = new TextOverlay();
			switch(comboBoxNagLevel.SelectedIndex)
			{
			case 0:
				this.nagOverlay.Alignment = ContentAlignment.BottomRight;
				this.nagOverlay.Text = "Trial version";
				this.nagOverlay.BackColor = Color.White;
				this.nagOverlay.BorderWidth = 2.0f;
				this.nagOverlay.BorderColor = Color.RoyalBlue;
				this.nagOverlay.TextColor = Color.DarkBlue;
				this.ListView.OverlayTransparency = 255;
				break;
			case 1:
				this.nagOverlay.Alignment = ContentAlignment.TopRight;
				this.nagOverlay.Text = "TRIAL VERSION EXPIRED";
				this.nagOverlay.TextColor = Color.Red;
				this.nagOverlay.BackColor = Color.White;
				this.nagOverlay.BorderWidth = 2.0f;
				this.nagOverlay.BorderColor = Color.DarkGray;
				this.nagOverlay.Rotation = 20;
				this.nagOverlay.InsetX = 5;
				this.nagOverlay.InsetY = 50;
				this.ListView.OverlayTransparency = 192;
				break;
			case 2:
				this.nagOverlay.Alignment = ContentAlignment.MiddleCenter;
				this.nagOverlay.Text = "TRIAL EXPIRED! BUY NOW!";
				this.nagOverlay.TextColor = Color.Red;
				this.nagOverlay.BorderWidth = 4.0f;
				this.nagOverlay.BorderColor = Color.Red;
				this.nagOverlay.Rotation = -30;
				this.nagOverlay.Font = new Font("Stencil", 36);
				this.ListView.OverlayTransparency = 192;
				break;
			}
			this.ListView.AddOverlay(this.nagOverlay);
		}

		private TextOverlay nagOverlay;

		private void checkBoxGroups_CheckedChanged(Object sender, EventArgs e)
			=> this.Coordinator.ShowGroupsChecked(this.ListView, (CheckBox)sender);

		private void checkBoxTooltips_CheckedChanged(Object sender, EventArgs e)
			=> this.showToolTipsOnFiles = !this.showToolTipsOnFiles;

		private Boolean showToolTipsOnFiles = false;

		private void comboBoxHotItemStyle_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeHotItemStyle(this.ListView, (ComboBox)sender);

		private void comboBoxView_SelectedIndexChanged(Object sender, EventArgs e)
			=> this.Coordinator.ChangeView(this.ListView, (ComboBox)sender);

		private void buttonSaveState_Click(Object sender, EventArgs e)
		{
			// SaveState() returns a Byte array that holds the current state of the columns.
			// For this demo, we just hold onto that value in an instance variable. For your
			// application, you should persist it some more permanent fashion than this.
			this.fileListViewState = this.olvFiles.SaveState();
			this.buttonRestoreState.Enabled = true;
		}

		private Byte[] fileListViewState;

		private void buttonRestoreState_Click(Object sender, EventArgs e)
			=> this.olvFiles.RestoreState(this.fileListViewState);// Restore the state is just a single call

		private void buttonColumns_Click(Object sender, EventArgs e)
		{
			ColumnSelectionForm form = new ColumnSelectionForm();
			form.OpenOn(this.olvFiles);
		}

		#endregion
	}
}