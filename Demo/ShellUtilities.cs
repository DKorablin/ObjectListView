/*
 * [File purpose]
 * Author: Phillip Piper
 * Date: 1 May 2007 7:44 PM
 * 
 * CHANGE LOG:
 * 2009-07-08  JPP  Don't cache the image collections
 * 1 May 2007  JPP  Initial Version
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace ObjectListViewDemo
{
	/// <summary>
	/// This helper class allows listviews and tree views to use image from the system image list.
	/// </summary>
	/// <remarks>Instances of this helper class know how to retrieve icon from the Windows shell for
	/// a given file path. These icons are then added to the imagelist on the given control. ListViews need 
	/// special handling since they have two image lists which need to be kept in sync.</remarks>
	public class SysImageListHelper
	{
		private SysImageListHelper()
		{
		}

		protected ImageList.ImageCollection SmallImageCollection
		{
			get => this.listView != null
				? this.listView.SmallImageList.Images
				: this.treeView != null
				? this.treeView.ImageList.Images
				: null;
		}

		protected ImageList.ImageCollection LargeImageCollection
		{
			get => this.listView?.LargeImageList.Images;
		}

		protected ImageList SmallImageList
		{
			get
			{
				return this.listView != null
					? this.listView.SmallImageList
					: this.treeView != null
					? this.treeView.ImageList
					: null;
			}
		}

		protected ImageList LargeImageList
		{
			get => this.listView?.LargeImageList;
		}

		/// <summary>Create a SysImageListHelper that will fetch images for the given tree control</summary>
		/// <param name="treeView">The tree view that will use the images</param>
		public SysImageListHelper(TreeView treeView)
		{
			if(treeView.ImageList == null)
				treeView.ImageList = new ImageList
				{
					ImageSize = new Size(16, 16)
				};
			this.treeView = treeView;
		}
		protected TreeView treeView;

		/// <summary>Create a SysImageListHelper that will fetch images for the given listview control.</summary>
		/// <param name="listView">The listview that will use the images</param>
		/// <remarks>Listviews manage two image lists, but each item can only have one image index.
		/// This means that the image for an item must occur at the same index in the two lists. 
		/// SysImageListHelper instances handle this requirement. However, if the listview already
		/// has image lists installed, they <b>must</b> be of the same length.</remarks>
		public SysImageListHelper(ObjectListView listView)
		{
			if(listView.SmallImageList == null)
				listView.SmallImageList = new ImageList
				{
					ColorDepth = ColorDepth.Depth32Bit,
					ImageSize = new Size(16, 16)
				};

			if(listView.LargeImageList == null)
				listView.LargeImageList = new ImageList
				{
					ColorDepth = ColorDepth.Depth32Bit,
					ImageSize = new Size(32, 32)
				};

			//if (listView.SmallImageList.Images.Count != listView.LargeImageList.Images.Count)
			//    throw new ArgumentException("Small and large image lists must have the same number of items.");

			this.listView = listView;
		}
		protected ObjectListView listView;

		/// <summary>Return the index of the image that has the Shell Icon for the given file/directory.</summary>
		/// <param name="path">The full path to the file/directory</param>
		/// <returns>The index of the image or -1 if something goes wrong.</returns>
		public Int32 GetImageIndex(String path)
		{
			if(System.IO.Directory.Exists(path))
				path = Environment.SystemDirectory; // optimization! give all directories the same image
			else if(System.IO.Path.HasExtension(path))
				path = System.IO.Path.GetExtension(path);

			if(this.SmallImageCollection.ContainsKey(path))
				return this.SmallImageCollection.IndexOfKey(path);

			try
			{
				this.AddImageToCollection(path, this.SmallImageList, ShellUtilities.GetFileIcon(path, true, true));
				this.AddImageToCollection(path, this.LargeImageList, ShellUtilities.GetFileIcon(path, false, true));
			} catch(ArgumentNullException)
			{
				return -1;
			}

			return this.SmallImageCollection.IndexOfKey(path);
		}

		private void AddImageToCollection(String key, ImageList imageList, Icon image)
		{
			if(imageList == null)
				return;

			if(imageList.ImageSize == image.Size)
			{
				imageList.Images.Add(key, image);
				return;
			}

			using(Bitmap imageAsBitmap = image.ToBitmap())
			{
				Bitmap bm = new Bitmap(imageList.ImageSize.Width, imageList.ImageSize.Height);
				Graphics g = Graphics.FromImage(bm);
				g.Clear(imageList.TransparentColor);
				Size size = imageAsBitmap.Size;
				Int32 x = Math.Max(0, (bm.Size.Width - size.Width) / 2);
				Int32 y = Math.Max(0, (bm.Size.Height - size.Height) / 2);
				g.DrawImage(imageAsBitmap, x, y, size.Width, size.Height);
				imageList.Images.Add(key, bm);
			}
		}
	}

	/// <summary>ShellUtilities contains routines to interact with the Windows Shell.</summary>
	public static class ShellUtilities
	{
		/// <summary>
		/// Execute the default verb on the file or directory identified by the given path.
		/// For documents, this will open them with their normal application. For executables,
		/// this will cause them to run.
		/// </summary>
		/// <param name="path">The file or directory to be executed</param>
		/// <returns>Values &lt; 31 indicate some sort of error. See ShellExecute() documentation for specifics.</returns>
		/// <remarks>The same effect can be achieved by <code>System.Diagnostics.Process.Start(path)</code>.</remarks>
		public static Int32 Execute(String path)
			=> ShellUtilities.Execute(path, String.Empty);

		/// <summary>
		/// Execute the given operation on the file or directory identified by the given path.
		/// Example operations are "edit", "print", "explore".
		/// </summary>
		/// <param name="path">The file or directory to be operated on</param>
		/// <param name="operation">What operation should be performed</param>
		/// <returns>Values &lt; 31 indicate some sort of error. See ShellExecute() documentation for specifics.</returns>
		public static Int32 Execute(String path, String operation)
		{
			IntPtr result = ShellUtilities.ShellExecute(0, operation, path, String.Empty, String.Empty, SW_SHOWNORMAL);
			return result.ToInt32();
		}

		/// <summary>Get the String that describes the file's type.</summary>
		/// <param name="path">The file or directory whose type is to be fetched</param>
		/// <returns>A String describing the type of the file, or an empty String if something goes wrong.</returns>
		public static String GetFileType(String path)
		{
			SHFILEINFO shfi = new SHFILEINFO();
			Int32 flags = SHGFI_TYPENAME;
			IntPtr result = ShellUtilities.SHGetFileInfo(path, 0, out shfi, Marshal.SizeOf(shfi), flags);
			return result.ToInt32() == 0
				? String.Empty
				: shfi.szTypeName;
		}

		/// <summary>Return the icon for the given file/directory.</summary>
		/// <param name="path">The full path to the file whose icon is to be returned</param>
		/// <param name="isSmallImage">True if the small (16x16) icon is required, otherwise the 32x32 icon will be returned</param>
		/// <param name="useFileType">If this is true, only the file extension will be considered</param>
		/// <returns>The icon of the given file, or null if something goes wrong</returns>
		public static Icon GetFileIcon(String path, Boolean isSmallImage, Boolean useFileType)
		{
			Int32 flags = SHGFI_ICON;
			if(isSmallImage)
				flags |= SHGFI_SMALLICON;

			Int32 fileAttributes = 0;
			if(useFileType)
			{
				flags |= SHGFI_USEFILEATTRIBUTES;
				if(System.IO.Directory.Exists(path))
					fileAttributes = FILE_ATTRIBUTE_DIRECTORY;
				else
					fileAttributes = FILE_ATTRIBUTE_NORMAL;
			}

			SHFILEINFO shfi = new SHFILEINFO();
			IntPtr result = ShellUtilities.SHGetFileInfo(path, fileAttributes, out shfi, Marshal.SizeOf(shfi), flags);
			return result.ToInt32() == 0
				? null
				: Icon.FromHandle(shfi.hIcon);
		}

		/// <summary>Return the index into the system image list of the image that represents the given file.</summary>
		/// <param name="path">The full path to the file or directory whose icon is required</param>
		/// <returns>The index of the icon, or -1 if something goes wrong</returns>
		/// <remarks>This is only useful if you are using the system image lists directly. Since there is
		/// no way to do that in .NET, it isn't a very useful.</remarks>
		public static Int32 GetSysImageIndex(String path)
		{
			SHFILEINFO shfi = new SHFILEINFO();
			Int32 flags = SHGFI_ICON | SHGFI_SYSICONINDEX;
			IntPtr result = ShellUtilities.SHGetFileInfo(path, 0, out shfi, Marshal.SizeOf(shfi), flags);
			return result.ToInt32() == 0
				? -1
				: shfi.iIcon;
		}

		#region Native methods

		private const Int32 SHGFI_ICON = 0x00100;     // get icon
		private const Int32 SHGFI_DISPLAYNAME = 0x00200;     // get display name
		private const Int32 SHGFI_TYPENAME = 0x00400;     // get type name
		private const Int32 SHGFI_ATTRIBUTES = 0x00800;     // get attributes
		private const Int32 SHGFI_ICONLOCATION = 0x01000;     // get icon location
		private const Int32 SHGFI_EXETYPE = 0x02000;     // return exe type
		private const Int32 SHGFI_SYSICONINDEX = 0x04000;     // get system icon index
		private const Int32 SHGFI_LINKOVERLAY = 0x08000;     // put a link overlay on icon
		private const Int32 SHGFI_SELECTED = 0x10000;     // show icon in selected state
		private const Int32 SHGFI_ATTR_SPECIFIED = 0x20000;     // get only specified attributes
		private const Int32 SHGFI_LARGEICON = 0x00000;     // get large icon
		private const Int32 SHGFI_SMALLICON = 0x00001;     // get small icon
		private const Int32 SHGFI_OPENICON = 0x00002;     // get open icon
		private const Int32 SHGFI_SHELLICONSIZE = 0x00004;     // get shell size icon
		private const Int32 SHGFI_PIDL = 0x00008;     // pszPath is a pidl
		private const Int32 SHGFI_USEFILEATTRIBUTES = 0x00010;     // use passed dwFileAttribute
		//if (_WIN32_IE >= 0x0500)
		private const Int32 SHGFI_ADDOVERLAYS = 0x00020;     // apply the appropriate overlays
		private const Int32 SHGFI_OVERLAYINDEX = 0x00040;     // Get the index of the overlay

		private const Int32 FILE_ATTRIBUTE_NORMAL = 0x00080;     // Normal file
		private const Int32 FILE_ATTRIBUTE_DIRECTORY = 0x00010;     // Directory

		private const Int32 MAX_PATH = 260;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public Int32 iIcon;
			public Int32 dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public String szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public String szTypeName;
		}

		private const Int32 SW_SHOWNORMAL = 1;

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr ShellExecute(Int32 hwnd, String lpOperation, String lpFile,
			String lpParameters, String lpDirectory, Int32 nShowCmd);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SHGetFileInfo(String pszPath, Int32 dwFileAttributes,
			out SHFILEINFO psfi, Int32 cbFileInfo, Int32 uFlags);

		#endregion
	}
}