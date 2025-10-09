/*
 * NativeMethods - All the Windows SDK structures and imports
 *
 * Author: Phillip Piper
 * Date: 10/10/2006
 *
 * Change log:
 * v2.8.0
 * 2014-05-21   JPP  - Added DeselectOneItem
 *                   - Added new imagelist drawing
 * v2.3
 * 2006-10-10   JPP  - Initial version
 *
 * To do:
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
	/// <summary>Wrapper for all native method calls on ListView controls</summary>
	internal static class NativeMethods
	{
		#region Constants

		private const Int32 LVM_FIRST = 0x1000;
		private const Int32 LVM_GETCOLUMN = LVM_FIRST + 95;
		private const Int32 LVM_GETCOUNTPERPAGE = LVM_FIRST + 40;
		private const Int32 LVM_GETGROUPINFO = LVM_FIRST + 149;
		private const Int32 LVM_GETGROUPSTATE = LVM_FIRST + 92;
		private const Int32 LVM_GETHEADER = LVM_FIRST + 31;
		private const Int32 LVM_GETTOOLTIPS = LVM_FIRST + 78;
		private const Int32 LVM_GETTOPINDEX = LVM_FIRST + 39;
		private const Int32 LVM_HITTEST = LVM_FIRST + 18;
		private const Int32 LVM_INSERTGROUP = LVM_FIRST + 145;
		private const Int32 LVM_REMOVEALLGROUPS = LVM_FIRST + 160;
		private const Int32 LVM_SCROLL = LVM_FIRST + 20;
		private const Int32 LVM_SETBKIMAGE = LVM_FIRST + 0x8A;
		private const Int32 LVM_SETCOLUMN = LVM_FIRST + 96;
		private const Int32 LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
		private const Int32 LVM_SETGROUPINFO = LVM_FIRST + 147;
		private const Int32 LVM_SETGROUPMETRICS = LVM_FIRST + 155;
		private const Int32 LVM_SETIMAGELIST = LVM_FIRST + 3;
		private const Int32 LVM_SETITEM = LVM_FIRST + 76;
		private const Int32 LVM_SETITEMCOUNT = LVM_FIRST + 47;
		private const Int32 LVM_SETITEMSTATE = LVM_FIRST + 43;
		private const Int32 LVM_SETSELECTEDCOLUMN = LVM_FIRST + 140;
		private const Int32 LVM_SETTOOLTIPS = LVM_FIRST + 74;
		private const Int32 LVM_SUBITEMHITTEST = LVM_FIRST + 57;
		private const Int32 LVS_EX_SUBITEMIMAGES = 0x0002;

		private const Int32 LVIF_TEXT = 0x0001;
		private const Int32 LVIF_IMAGE = 0x0002;
		private const Int32 LVIF_PARAM = 0x0004;
		private const Int32 LVIF_STATE = 0x0008;
		private const Int32 LVIF_INDENT = 0x0010;
		private const Int32 LVIF_NORECOMPUTE = 0x0800;

		private const Int32 LVIS_SELECTED = 2;

		private const Int32 LVCF_FMT = 0x0001;
		private const Int32 LVCF_WIDTH = 0x0002;
		private const Int32 LVCF_TEXT = 0x0004;
		private const Int32 LVCF_SUBITEM = 0x0008;
		private const Int32 LVCF_IMAGE = 0x0010;
		private const Int32 LVCF_ORDER = 0x0020;
		private const Int32 LVCFMT_LEFT = 0x0000;
		private const Int32 LVCFMT_RIGHT = 0x0001;
		private const Int32 LVCFMT_CENTER = 0x0002;
		private const Int32 LVCFMT_JUSTIFYMASK = 0x0003;

		private const Int32 LVCFMT_IMAGE = 0x0800;
		private const Int32 LVCFMT_BITMAP_ON_RIGHT = 0x1000;
		private const Int32 LVCFMT_COL_HAS_IMAGES = 0x8000;

		private const Int32 LVBKIF_SOURCE_NONE = 0x0;
		private const Int32 LVBKIF_SOURCE_HBITMAP = 0x1;
		private const Int32 LVBKIF_SOURCE_URL = 0x2;
		private const Int32 LVBKIF_SOURCE_MASK = 0x3;
		private const Int32 LVBKIF_STYLE_NORMAL = 0x0;
		private const Int32 LVBKIF_STYLE_TILE = 0x10;
		private const Int32 LVBKIF_STYLE_MASK = 0x10;
		private const Int32 LVBKIF_FLAG_TILEOFFSET = 0x100;
		private const Int32 LVBKIF_TYPE_WATERMARK = 0x10000000;
		private const Int32 LVBKIF_FLAG_ALPHABLEND = 0x20000000;

		private const Int32 LVSICF_NOINVALIDATEALL = 1;
		private const Int32 LVSICF_NOSCROLL = 2;

		private const Int32 HDM_FIRST = 0x1200;
		private const Int32 HDM_HITTEST = HDM_FIRST + 6;
		private const Int32 HDM_GETITEMRECT = HDM_FIRST + 7;
		private const Int32 HDM_GETITEM = HDM_FIRST + 11;
		private const Int32 HDM_SETITEM = HDM_FIRST + 12;

		private const Int32 HDI_WIDTH = 0x0001;
		private const Int32 HDI_TEXT = 0x0002;
		private const Int32 HDI_FORMAT = 0x0004;
		private const Int32 HDI_BITMAP = 0x0010;
		private const Int32 HDI_IMAGE = 0x0020;

		private const Int32 HDF_LEFT = 0x0000;
		private const Int32 HDF_RIGHT = 0x0001;
		private const Int32 HDF_CENTER = 0x0002;
		private const Int32 HDF_JUSTIFYMASK = 0x0003;
		private const Int32 HDF_RTLREADING = 0x0004;
		private const Int32 HDF_STRING = 0x4000;
		private const Int32 HDF_BITMAP = 0x2000;
		private const Int32 HDF_BITMAP_ON_RIGHT = 0x1000;
		private const Int32 HDF_IMAGE = 0x0800;
		private const Int32 HDF_SORTUP = 0x0400;
		private const Int32 HDF_SORTDOWN = 0x0200;

		private const Int32 SB_HORZ = 0;
		private const Int32 SB_VERT = 1;
		private const Int32 SB_CTL = 2;
		private const Int32 SB_BOTH = 3;

		private const Int32 SIF_RANGE = 0x0001;
		private const Int32 SIF_PAGE = 0x0002;
		private const Int32 SIF_POS = 0x0004;
		private const Int32 SIF_DISABLENOSCROLL = 0x0008;
		private const Int32 SIF_TRACKPOS = 0x0010;
		private const Int32 SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS);

		private const Int32 ILD_NORMAL = 0x0;
		private const Int32 ILD_TRANSPARENT = 0x1;
		private const Int32 ILD_MASK = 0x10;
		private const Int32 ILD_IMAGE = 0x20;
		private const Int32 ILD_BLEND25 = 0x2;
		private const Int32 ILD_BLEND50 = 0x4;

		const Int32 SWP_NOSIZE = 1;
		const Int32 SWP_NOMOVE = 2;
		const Int32 SWP_NOZORDER = 4;
		const Int32 SWP_NOREDRAW = 8;
		const Int32 SWP_NOACTIVATE = 16;
		public const Int32 SWP_FRAMECHANGED = 32;

		const Int32 SWP_ZORDERONLY = SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOACTIVATE;
		const Int32 SWP_SIZEONLY = SWP_NOMOVE | SWP_NOREDRAW | SWP_NOZORDER | SWP_NOACTIVATE;
		const Int32 SWP_UPDATE_FRAME = SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER | SWP_FRAMECHANGED;

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential)]
		public struct HDITEM
		{
			public Int32 mask;
			public Int32 cxy;
			public IntPtr pszText;
			public IntPtr hbm;
			public Int32 cchTextMax;
			public Int32 fmt;
			public IntPtr lParam;
			public Int32 iImage;
			public Int32 iOrder;
			//if (_WIN32_IE >= 0x0500)
			public Int32 type;
			public IntPtr pvFilter;
		}

		[StructLayout(LayoutKind.Sequential)]
		public class HDHITTESTINFO
		{
			public Int32 pt_x;
			public Int32 pt_y;
			public Int32 flags;
			public Int32 iItem;
		}

		[StructLayout(LayoutKind.Sequential)]
		public class HDLAYOUT
		{
			public IntPtr prc;
			public IntPtr pwpos;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGELISTDRAWPARAMS
		{
			public Int32 cbSize;
			public IntPtr himl;
			public Int32 i;
			public IntPtr hdcDst;
			public Int32 x;
			public Int32 y;
			public Int32 cx;
			public Int32 cy;
			public Int32 xBitmap;
			public Int32 yBitmap;
			public UInt32 rgbBk;
			public UInt32 rgbFg;
			public UInt32 fStyle;
			public UInt32 dwRop;
			public UInt32 fState;
			public UInt32 Frame;
			public UInt32 crEffect;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct LVBKIMAGE
		{
			public Int32 ulFlags;
			public IntPtr hBmp;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszImage;
			public Int32 cchImageMax;
			public Int32 xOffset;
			public Int32 yOffset;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct LVCOLUMN
		{
			public Int32 mask;
			public Int32 fmt;
			public Int32 cx;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszText;
			public Int32 cchTextMax;
			public Int32 iSubItem;
			// These are available in Common Controls >= 0x0300
			public Int32 iImage;
			public Int32 iOrder;
		};

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct LVFINDINFO
		{
			public Int32 flags;
			public String psz;
			public IntPtr lParam;
			public Int32 ptX;
			public Int32 ptY;
			public Int32 vkDirection;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LVGROUP
		{
			public UInt32 cbSize;
			public UInt32 mask;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszHeader;
			public Int32 cchHeader;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszFooter;
			public Int32 cchFooter;
			public Int32 iGroupId;
			public UInt32 stateMask;
			public UInt32 state;
			public UInt32 uAlign;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LVGROUP2
		{
			public UInt32 cbSize;
			public UInt32 mask;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszHeader;
			public UInt32 cchHeader;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszFooter;
			public Int32 cchFooter;
			public Int32 iGroupId;
			public UInt32 stateMask;
			public UInt32 state;
			public UInt32 uAlign;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszSubtitle;
			public UInt32 cchSubtitle;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszTask;
			public UInt32 cchTask;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszDescriptionTop;
			public UInt32 cchDescriptionTop;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszDescriptionBottom;
			public UInt32 cchDescriptionBottom;
			public Int32 iTitleImage;
			public Int32 iExtendedImage;
			public Int32 iFirstItem;         // Read only
			public Int32 cItems;             // Read only
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszSubsetTitle;     // NULL if group is not subset
			public UInt32 cchSubsetTitle;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LVGROUPMETRICS
		{
			public UInt32 cbSize;
			public UInt32 mask;
			public UInt32 Left;
			public UInt32 Top;
			public UInt32 Right;
			public UInt32 Bottom;
			public Int32 crLeft;
			public Int32 crTop;
			public Int32 crRight;
			public Int32 crBottom;
			public Int32 crHeader;
			public Int32 crFooter;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct LVHITTESTINFO
		{
			public Int32 pt_x;
			public Int32 pt_y;
			public Int32 flags;
			public Int32 iItem;
			public Int32 iSubItem;
			public Int32 iGroup;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct LVITEM
		{
			public Int32 mask;
			public Int32 iItem;
			public Int32 iSubItem;
			public Int32 state;
			public Int32 stateMask;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String pszText;
			public Int32 cchTextMax;
			public Int32 iImage;
			public IntPtr lParam;
			// These are available in Common Controls >= 0x0300
			public Int32 iIndent;
			// These are available in Common Controls >= 0x056
			public Int32 iGroupId;
			public Int32 cColumns;
			public IntPtr puColumns;
		};

		[StructLayout(LayoutKind.Sequential)]
		public struct NMHDR
		{
			public IntPtr hwndFrom;
			public IntPtr idFrom;
			public Int32 code;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMCUSTOMDRAW
		{
			public NativeMethods.NMHDR nmcd;
			public Int32 dwDrawStage;
			public IntPtr hdc;
			public NativeMethods.RECT rc;
			public IntPtr dwItemSpec;
			public Int32 uItemState;
			public IntPtr lItemlParam;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMHEADER
		{
			public NMHDR nhdr;
			public Int32 iItem;
			public Int32 iButton;
			public IntPtr pHDITEM;
		}

		const Int32 MAX_LINKID_TEXT = 48;
		const Int32 L_MAX_URL_LENGTH = 2048 + 32 + 4;
		//#define L_MAX_URL_LENGTH    (2048 + 32 + sizeof("://"))

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct LITEM
		{
			public UInt32 mask;
			public Int32 iLink;
			public UInt32 state;
			public UInt32 stateMask;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_LINKID_TEXT)]
			public String szID;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = L_MAX_URL_LENGTH)]
			public String szUrl;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMLISTVIEW
		{
			public NativeMethods.NMHDR hdr;
			public Int32 iItem;
			public Int32 iSubItem;
			public Int32 uNewState;
			public Int32 uOldState;
			public Int32 uChanged;
			public IntPtr lParam;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMLVCUSTOMDRAW
		{
			public NativeMethods.NMCUSTOMDRAW nmcd;
			public Int32 clrText;
			public Int32 clrTextBk;
			public Int32 iSubItem;
			public Int32 dwItemType;
			public Int32 clrFace;
			public Int32 iIconEffect;
			public Int32 iIconPhase;
			public Int32 iPartId;
			public Int32 iStateId;
			public NativeMethods.RECT rcText;
			public UInt32 uAlign;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMLVFINDITEM
		{
			public NativeMethods.NMHDR hdr;
			public Int32 iStart;
			public NativeMethods.LVFINDINFO lvfi;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMLVGETINFOTIP
		{
			public NativeMethods.NMHDR hdr;
			public Int32 dwFlags;
			public String pszText;
			public Int32 cchTextMax;
			public Int32 iItem;
			public Int32 iSubItem;
			public IntPtr lParam;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMLVGROUP
		{
			public NMHDR hdr;
			public Int32 iGroupId; // which group is changing
			public UInt32 uNewState; // LVGS_xxx flags
			public UInt32 uOldState;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMLVLINK
		{
			public NMHDR hdr;
			public LITEM link;
			public Int32 iItem;
			public Int32 iSubItem;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMLVSCROLL
		{
			public NativeMethods.NMHDR hdr;
			public Int32 dx;
			public Int32 dy;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct NMTTDISPINFO
		{
			public NativeMethods.NMHDR hdr;
			[MarshalAs(UnmanagedType.LPTStr)]
			public String lpszText;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public String szText;
			public IntPtr hinst;
			public Int32 uFlags;
			public IntPtr lParam;
			//public int hbmp; This is documented but doesn't work
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public Int32 left;
			public Int32 top;
			public Int32 right;
			public Int32 bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public class SCROLLINFO
		{
			public Int32 cbSize = Marshal.SizeOf(typeof(NativeMethods.SCROLLINFO));
			public Int32 fMask;
			public Int32 nMin;
			public Int32 nMax;
			public Int32 nPage;
			public Int32 nPos;
			public Int32 nTrackPos;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class TOOLINFO
		{
			public Int32 cbSize = Marshal.SizeOf(typeof(NativeMethods.TOOLINFO));
			public Int32 uFlags;
			public IntPtr hwnd;
			public IntPtr uId;
			public NativeMethods.RECT rect;
			public IntPtr hinst = IntPtr.Zero;
			public IntPtr lpszText;
			public IntPtr lParam = IntPtr.Zero;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPOS
		{
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public Int32 x;
			public Int32 y;
			public Int32 cx;
			public Int32 cy;
			public Int32 flags;
		}

		#endregion

		#region Entry points

		// Various flavors of SendMessage: plain vanilla, and passing references to various structures
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, Int32 lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, IntPtr wParam, Int32 lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, IntPtr lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageLVItem(IntPtr hWnd, Int32 msg, Int32 wParam, ref LVITEM lvi);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, ref LVHITTESTINFO ht);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, ref LVGROUP lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, ref LVGROUP2 lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, ref LVGROUPMETRICS lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageRECT(IntPtr hWnd, Int32 msg, Int32 wParam, ref RECT r);

		//[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		//private static extern IntPtr SendMessageLVColumn(IntPtr hWnd, int m, int wParam, ref LVCOLUMN lvc);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessageHDItem(IntPtr hWnd, Int32 msg, Int32 wParam, ref HDITEM hdi);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageHDHITTESTINFO(IntPtr hWnd, Int32 Msg, IntPtr wParam, [In, Out] HDHITTESTINFO lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageTOOLINFO(IntPtr hWnd, Int32 Msg, Int32 wParam, NativeMethods.TOOLINFO lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageLVBKIMAGE(IntPtr hWnd, Int32 Msg, Int32 wParam, ref NativeMethods.LVBKIMAGE lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageString(IntPtr hWnd, Int32 Msg, Int32 wParam, String lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessageIUnknown(IntPtr hWnd, Int32 msg, [MarshalAs(UnmanagedType.IUnknown)] Object wParam, Int32 lParam);

		[DllImport("gdi32.dll")]
		public static extern Boolean DeleteObject(IntPtr objectHandle);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern Boolean GetClientRect(IntPtr hWnd, ref Rectangle r);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern Boolean GetScrollInfo(IntPtr hWnd, Int32 fnBar, SCROLLINFO scrollInfo);

		[DllImport("user32.dll", EntryPoint = "GetUpdateRect", CharSet = CharSet.Auto)]
		private static extern Boolean GetUpdateRectInternal(IntPtr hWnd, ref Rectangle r, Boolean eraseBackground);

		[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
		private static extern Boolean ImageList_Draw(IntPtr himl, Int32 i, IntPtr hdcDst, Int32 x, Int32 y, Int32 fStyle);

		[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
		private static extern Boolean ImageList_DrawIndirect(ref IMAGELISTDRAWPARAMS parms);

		[DllImport("user32.dll")]
		public static extern Boolean SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, Int32 X, Int32 Y, Int32 cx, Int32 cy, UInt32 uFlags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern Boolean GetWindowRect(IntPtr hWnd, ref Rectangle r);

		[DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLong32(IntPtr hWnd, Int32 nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, Int32 nIndex);

		[DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
		public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
		public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

		[DllImport("user32.dll")]
		public static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

		[DllImport("user32.dll", EntryPoint = "ValidateRect", CharSet = CharSet.Auto)]
		private static extern IntPtr ValidatedRectInternal(IntPtr hWnd, ref Rectangle r);

		#endregion

		//[DllImport("user32.dll", EntryPoint = "LockWindowUpdate", CharSet = CharSet.Auto)]
		//private static extern int LockWindowUpdateInternal(IntPtr hWnd);

		//public static void LockWindowUpdate(IWin32Window window) {
		//    if (window == null)
		//        NativeMethods.LockWindowUpdateInternal(IntPtr.Zero);
		//    else
		//        NativeMethods.LockWindowUpdateInternal(window.Handle);
		//}

		/// <summary>
		/// Put an image under the ListView.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The ListView must have its handle created before calling this.
		/// </para>
		/// <para>
		/// This doesn't work very well. Specifically, it doesn't play well with owner drawn, 
		/// and grid lines are drawn over it.
		/// </para>
		/// </remarks>
		/// <param name="lv"></param>
		/// <param name="image">The image to be used as the background. If this is null, any existing background image will be cleared.</param>
		/// <param name="isWatermark">If this is true, the image is pinned to the bottom right and does not scroll. The other parameters are ignored</param>
		/// <param name="isTiled">If this is true, the image will be tiled to fill the whole control background. The offset parameters will be ignored.</param>
		/// <param name="xOffset">If both watermark and tiled are false, this indicates the horizontal percentage where the image will be placed. 0 is absolute left, 100 is absolute right.</param>
		/// <param name="yOffset">If both watermark and tiled are false, this indicates the vertical percentage where the image will be placed.</param>
		/// <returns></returns>
		public static Boolean SetBackgroundImage(ListView lv, Image image, Boolean isWatermark, Boolean isTiled, Int32 xOffset, Int32 yOffset)
		{

			LVBKIMAGE lvbkimage = new LVBKIMAGE
			{
				// We have to clear any pre-existing background image, otherwise the attempt to set the image will fail.
				// We don't know which type may already have been set, so we just clear both the watermark and the image.
				ulFlags = LVBKIF_TYPE_WATERMARK
			};
			_ = NativeMethods.SendMessageLVBKIMAGE(lv.Handle, LVM_SETBKIMAGE, 0, ref lvbkimage);
			lvbkimage.ulFlags = LVBKIF_SOURCE_HBITMAP;
			IntPtr result = NativeMethods.SendMessageLVBKIMAGE(lv.Handle, LVM_SETBKIMAGE, 0, ref lvbkimage);

			if(image is Bitmap bm)
			{
				lvbkimage.hBmp = bm.GetHbitmap();
				lvbkimage.ulFlags = isWatermark ? LVBKIF_TYPE_WATERMARK : (isTiled ? LVBKIF_SOURCE_HBITMAP | LVBKIF_STYLE_TILE : LVBKIF_SOURCE_HBITMAP);
				lvbkimage.xOffset = xOffset;
				lvbkimage.yOffset = yOffset;
				result = NativeMethods.SendMessageLVBKIMAGE(lv.Handle, LVM_SETBKIMAGE, 0, ref lvbkimage);
			}

			return (result != IntPtr.Zero);
		}

		public static Boolean DrawImageList(Graphics g, ImageList il, Int32 index, Int32 x, Int32 y, Boolean isSelected, Boolean isDisabled)
		{
			ImageListDrawItemConstants flags = (isSelected ? ImageListDrawItemConstants.ILD_SELECTED : ImageListDrawItemConstants.ILD_NORMAL) | ImageListDrawItemConstants.ILD_TRANSPARENT;
			ImageListDrawStateConstants state = isDisabled ? ImageListDrawStateConstants.ILS_SATURATE : ImageListDrawStateConstants.ILS_NORMAL;
			try
			{
				IntPtr hdc = g.GetHdc();
				return DrawImage(il, hdc, index, x, y, flags, 0, 0, state);
			} finally
			{
				g.ReleaseHdc();
			}
		}

		/// <summary>Flags controlling how the Image List item is drawn</summary>
		[Flags]
		public enum ImageListDrawItemConstants
		{
			/// <summary>Draw item normally.</summary>
			ILD_NORMAL = 0x0,
			/// <summary>Draw item transparently.</summary>
			ILD_TRANSPARENT = 0x1,
			/// <summary>Draw item blended with 25% of the specified foreground colour or the Highlight colour if no foreground colour specified.</summary>
			ILD_BLEND25 = 0x2,
			/// <summary>Draw item blended with 50% of the specified foreground colour or the Highlight colour if no foreground colour specified.</summary>
			ILD_SELECTED = 0x4,
			/// <summary>Draw the icon's mask</summary>
			ILD_MASK = 0x10,
			/// <summary>Draw the icon image without using the mask</summary>
			ILD_IMAGE = 0x20,
			/// <summary>Draw the icon using the ROP specified.</summary>
			ILD_ROP = 0x40,
			/// <summary>Preserves the alpha channel in dest. XP only.</summary>
			ILD_PRESERVEALPHA = 0x1000,
			/// <summary>Scale the image to cx, cy instead of clipping it. XP only.</summary>
			ILD_SCALE = 0x2000,
			/// <summary>Scale the image to the current DPI of the display. XP only.</summary>
			ILD_DPISCALE = 0x4000
		}

		/// <summary>Enumeration containing XP ImageList Draw State options</summary>
		[Flags]
		public enum ImageListDrawStateConstants
		{
			/// <summary>The image state is not modified.</summary>
			ILS_NORMAL = (0x00000000),
			/// <summary>
			/// Adds a glow effect to the icon, which causes the icon to appear to glow 
			/// with a given color around the edges. (Note: does not appear to be implemented)
			/// </summary>
			ILS_GLOW = (0x00000001), //The color for the glow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
			/// <summary>Adds a drop shadow effect to the icon. (Note: does not appear to be implemented)</summary>
			ILS_SHADOW = (0x00000002), //The color for the drop shadow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
			/// <summary>
			/// Saturates the icon by increasing each color component 
			/// of the RGB triplet for each pixel in the icon. (Note: only ever appears to result in a completely unsaturated icon)
			/// </summary>
			ILS_SATURATE = (0x00000004), // The amount to increase is indicated by the frame member in the IMAGELISTDRAWPARAMS method. 
			/// <summary>
			/// Alpha blends the icon. Alpha blending controls the transparency 
			/// level of an icon, according to the value of its alpha channel. 
			/// (Note: does not appear to be implemented).
			/// </summary>
			ILS_ALPHA = (0x00000008) //The value of the alpha channel is indicated by the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel can be from 0 to 255, with 0 being completely transparent, and 255 being completely opaque. 
		}

		private const UInt32 CLR_DEFAULT = 0xFF000000;

		/// <summary>Draws an image using the specified flags and state on XP systems.</summary>
		/// <param name="il">The image list from which an item will be drawn</param>
		/// <param name="hdc">Device context to draw to</param>
		/// <param name="index">Index of image to draw</param>
		/// <param name="x">X Position to draw at</param>
		/// <param name="y">Y Position to draw at</param>
		/// <param name="flags">Drawing flags</param>
		/// <param name="cx">Width to draw</param>
		/// <param name="cy">Height to draw</param>
		/// <param name="stateFlags">State flags</param>
		public static Boolean DrawImage(ImageList il, IntPtr hdc, Int32 index, Int32 x, Int32 y, ImageListDrawItemConstants flags, Int32 cx, Int32 cy, ImageListDrawStateConstants stateFlags)
		{
			IMAGELISTDRAWPARAMS pimldp = new IMAGELISTDRAWPARAMS
			{
				hdcDst = hdc,
				i = index,
				x = x,
				y = y,
				cx = cx,
				cy = cy,
				rgbFg = CLR_DEFAULT,
				fStyle = (UInt32)flags,
				fState = (UInt32)stateFlags,
				himl = il.Handle
			};
			pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
			return ImageList_DrawIndirect(ref pimldp);
		}

		/// <summary>Make sure the ListView has the extended style that says to display subitem images.</summary>
		/// <remarks>This method must be called after any .NET call that update the extended styles since they seem to erase this setting.</remarks>
		/// <param name="list">The listview to send a m to</param>
		public static void ForceSubItemImagesExStyle(ListView list)
			=> SendMessage(list.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_SUBITEMIMAGES, LVS_EX_SUBITEMIMAGES);

		/// <summary>Change the virtual list size of the given ListView (which must be in virtual mode)</summary>
		/// <remarks>This will not change the scroll position</remarks>
		/// <param name="list">The listview to send a message to</param>
		/// <param name="count">How many rows should the list have?</param>
		public static void SetItemCount(ListView list, Int32 count)
			=> SendMessage(list.Handle, LVM_SETITEMCOUNT, count, LVSICF_NOSCROLL);

		/// <summary>Make sure the ListView has the extended style that says to display subitem images.</summary>
		/// <remarks>This method must be called after any .NET call that update the extended styles
		/// since they seem to erase this setting.</remarks>
		/// <param name="list">The listview to send a m to</param>
		/// <param name="style"></param>
		/// <param name="styleMask"></param>
		public static void SetExtendedStyle(ListView list, Int32 style, Int32 styleMask)
			=> SendMessage(list.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, styleMask, style);

		/// <summary>Calculates the number of items that can fit vertically in the visible area of a list-view (which must be in details or list view.</summary>
		/// <param name="list">The listView</param>
		/// <returns>Number of visible items per page</returns>
		public static Int32 GetCountPerPage(ListView list)
			=> (Int32)SendMessage(list.Handle, LVM_GETCOUNTPERPAGE, 0, 0);

		/// <summary>For the given item and subitem, make it display the given image</summary>
		/// <param name="list">The listview to send a m to</param>
		/// <param name="itemIndex">row number (0 based)</param>
		/// <param name="subItemIndex">subitem (0 is the item itself)</param>
		/// <param name="imageIndex">index into the image list</param>
		public static void SetSubItemImage(ListView list, Int32 itemIndex, Int32 subItemIndex, Int32 imageIndex)
		{
			LVITEM lvItem = new LVITEM
			{
				mask = LVIF_IMAGE,
				iItem = itemIndex,
				iSubItem = subItemIndex,
				iImage = imageIndex
			};
			SendMessageLVItem(list.Handle, LVM_SETITEM, 0, ref lvItem);
		}

		/// <summary>
		/// Setup the given column of the listview to show the given image to the right of the text.
		/// If the image index is -1, any previous image is cleared
		/// </summary>
		/// <param name="list">The listview to send a m to</param>
		/// <param name="columnIndex">Index of the column to modify</param>
		/// <param name="order"></param>
		/// <param name="imageIndex">Index into the small image list</param>
		public static void SetColumnImage(ListView list, Int32 columnIndex, SortOrder order, Int32 imageIndex)
		{
			IntPtr hdrCntl = NativeMethods.GetHeaderControl(list);
			if(hdrCntl.ToInt32() == 0)
				return;

			HDITEM item = new HDITEM
			{
				mask = HDI_FORMAT
			};
			IntPtr result = SendMessageHDItem(hdrCntl, HDM_GETITEM, columnIndex, ref item);

			item.fmt &= ~(HDF_SORTUP | HDF_SORTDOWN | HDF_IMAGE | HDF_BITMAP_ON_RIGHT);

			if(NativeMethods.HasBuiltinSortIndicators())
			{
				if(order == SortOrder.Ascending)
					item.fmt |= HDF_SORTUP;
				if(order == SortOrder.Descending)
					item.fmt |= HDF_SORTDOWN;
			} else
			{
				item.mask |= HDI_IMAGE;
				item.fmt |= (HDF_IMAGE | HDF_BITMAP_ON_RIGHT);
				item.iImage = imageIndex;
			}

			_ = SendMessageHDItem(hdrCntl, HDM_SETITEM, columnIndex, ref item);
		}

		/// <summary>Does this version of the operating system have builtin sort indicators?</summary>
		/// <returns>Are there builtin sort indicators</returns>
		/// <remarks>XP and later have these</remarks>
		public static Boolean HasBuiltinSortIndicators()
			=> OSFeature.Feature.GetVersionPresent(OSFeature.Themes) != null;

		/// <summary>Return the bounds of the update region on the given control.</summary>
		/// <remarks>The BeginPaint() system call validates the update region, effectively wiping out this information.
		/// So this call has to be made before the BeginPaint() call.</remarks>
		/// <param name="cntl">The control whose update region is be calculated</param>
		/// <returns>A rectangle</returns>
		public static Rectangle GetUpdateRect(Control cntl)
		{
			Rectangle r = new Rectangle();
			GetUpdateRectInternal(cntl.Handle, ref r, false);
			return r;
		}

		/// <summary>Validate an area of the given control. A validated area will not be repainted at the next redraw.</summary>
		/// <param name="cntl">The control to be validated</param>
		/// <param name="r">The area of the control to be validated</param>
		public static void ValidateRect(Control cntl, Rectangle r)
			=> ValidatedRectInternal(cntl.Handle, ref r);

		/// <summary>Select all rows on the given listview</summary>
		/// <param name="list">The listview whose items are to be selected</param>
		public static void SelectAllItems(ListView list)
			=> NativeMethods.SetItemState(list, -1, LVIS_SELECTED, LVIS_SELECTED);

		/// <summary>Deselect all rows on the given listview</summary>
		/// <param name="list">The listview whose items are to be deselected</param>
		public static void DeselectAllItems(ListView list)
			=> NativeMethods.SetItemState(list, -1, LVIS_SELECTED, 0);

		/// <summary>Deselect a single row</summary>
		/// <param name="list"></param>
		/// <param name="index"></param>
		public static void DeselectOneItem(ListView list, Int32 index)
			=> NativeMethods.SetItemState(list, index, LVIS_SELECTED, 0);

		/// <summary>Set the item state on the given item</summary>
		/// <param name="list">The listview whose item's state is to be changed</param>
		/// <param name="itemIndex">The index of the item to be changed</param>
		/// <param name="mask">Which bits of the value are to be set?</param>
		/// <param name="value">The value to be set</param>
		public static void SetItemState(ListView list, Int32 itemIndex, Int32 mask, Int32 value)
		{
			LVITEM lvItem = new LVITEM
			{
				stateMask = mask,
				state = value
			};
			SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
		}

		/// <summary>Scroll the given listview by the given deltas</summary>
		/// <param name="list"></param>
		/// <param name="dx"></param>
		/// <param name="dy"></param>
		/// <returns>true if the scroll succeeded</returns>
		public static Boolean Scroll(ListView list, Int32 dx, Int32 dy)
			=> SendMessage(list.Handle, LVM_SCROLL, dx, dy) != IntPtr.Zero;

		/// <summary>Return the handle to the header control on the given list</summary>
		/// <param name="list">The listview whose header control is to be returned</param>
		/// <returns>The handle to the header control</returns>
		public static IntPtr GetHeaderControl(ListView list)
			=> SendMessage(list.Handle, LVM_GETHEADER, 0, 0);

		/// <summary>Return the edges of the given column.</summary>
		/// <param name="lv"></param>
		/// <param name="columnIndex"></param>
		/// <returns>A Point holding the left and right co-ords of the column.
		/// -1 means that the sides could not be retrieved.</returns>
		public static Point GetColumnSides(ObjectListView lv, Int32 columnIndex)
		{
			IntPtr hdr = NativeMethods.GetHeaderControl(lv);
			if(hdr == IntPtr.Zero)
				return new Point(-1, -1);

			RECT r = new RECT();
			NativeMethods.SendMessageRECT(hdr, HDM_GETITEMRECT, columnIndex, ref r);
			return new Point(r.left, r.right);
		}

		/// <summary>Return the edges of the given column.</summary>
		/// <param name="lv"></param>
		/// <param name="columnIndex"></param>
		/// <returns>A Point holding the left and right co-ords of the column.
		/// -1 means that the sides could not be retrieved.</returns>
		public static Point GetScrolledColumnSides(ListView lv, Int32 columnIndex)
		{
			IntPtr hdr = NativeMethods.GetHeaderControl(lv);
			if(hdr == IntPtr.Zero)
				return new Point(-1, -1);

			RECT r = new RECT();
			IntPtr result = NativeMethods.SendMessageRECT(hdr, HDM_GETITEMRECT, columnIndex, ref r);
			Int32 scrollH = NativeMethods.GetScrollPosition(lv, true);
			return new Point(r.left - scrollH, r.right - scrollH);
		}

		/// <summary>
		/// Return the index of the column of the header that is under the given point.
		/// Return -1 if no column is under the pt
		/// </summary>
		/// <param name="handle">The list we are interested in</param>
		/// <param name="pt">The client co-ords</param>
		/// <returns>The index of the column under the point, or -1 if no column header is under that point</returns>
		public static Int32 GetColumnUnderPoint(IntPtr handle, Point pt)
		{
			const Int32 HHT_ONHEADER = 2;
			const Int32 HHT_ONDIVIDER = 4;
			return NativeMethods.HeaderControlHitTest(handle, pt, HHT_ONHEADER | HHT_ONDIVIDER);
		}

		private static Int32 HeaderControlHitTest(IntPtr handle, Point pt, Int32 flag)
		{
			HDHITTESTINFO testInfo = new HDHITTESTINFO
			{
				pt_x = pt.X,
				pt_y = pt.Y
			};
			_ = NativeMethods.SendMessageHDHITTESTINFO(handle, HDM_HITTEST, IntPtr.Zero, testInfo);
			return (testInfo.flags & flag) == 0
				? -1
				: testInfo.iItem;
		}

		/// <summary>Return the index of the divider under the given point. Return -1 if no divider is under the pt</summary>
		/// <param name="handle">The list we are interested in</param>
		/// <param name="pt">The client co-ords</param>
		/// <returns>The index of the divider under the point, or -1 if no divider is under that point</returns>
		public static Int32 GetDividerUnderPoint(IntPtr handle, Point pt)
		{
			const Int32 HHT_ONDIVIDER = 4;
			return NativeMethods.HeaderControlHitTest(handle, pt, HHT_ONDIVIDER);
		}

		/// <summary>Get the scroll position of the given scroll bar</summary>
		/// <param name="lv"></param>
		/// <param name="horizontalBar"></param>
		/// <returns></returns>
		public static Int32 GetScrollPosition(ListView lv, Boolean horizontalBar)
		{
			Int32 fnBar = (horizontalBar ? SB_HORZ : SB_VERT);

			SCROLLINFO scrollInfo = new SCROLLINFO
			{
				fMask = SIF_POS
			};

			return GetScrollInfo(lv.Handle, fnBar, scrollInfo)
				? scrollInfo.nPos
				: -1;
		}

		/// <summary>Change the z-order to the window 'toBeMoved' so it appear directly on top of 'reference'</summary>
		/// <param name="toBeMoved"></param>
		/// <param name="reference"></param>
		/// <returns></returns>
		public static Boolean ChangeZOrder(IWin32Window toBeMoved, IWin32Window reference)
			=> NativeMethods.SetWindowPos(toBeMoved.Handle, reference.Handle, 0, 0, 0, 0, SWP_ZORDERONLY);

		/// <summary>Make the given control/window a topmost window</summary>
		/// <param name="toBeMoved"></param>
		/// <returns></returns>
		public static Boolean MakeTopMost(IWin32Window toBeMoved)
		{
			IntPtr HWND_TOPMOST = (IntPtr)(-1);
			return NativeMethods.SetWindowPos(toBeMoved.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_ZORDERONLY);
		}

		/// <summary>Change the size of the window without affecting any other attributes</summary>
		/// <param name="toBeMoved"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Boolean ChangeSize(IWin32Window toBeMoved, Int32 width, Int32 height)
			=> NativeMethods.SetWindowPos(toBeMoved.Handle, IntPtr.Zero, 0, 0, width, height, SWP_SIZEONLY);

		/// <summary>Show the given window without activating it</summary>
		/// <param name="win">The window to show</param>
		static public void ShowWithoutActivate(IWin32Window win)
		{
			const Int32 SW_SHOWNA = 8;
			NativeMethods.ShowWindow(win.Handle, SW_SHOWNA);
		}

		/// <summary>Mark the given column as being selected.</summary>
		/// <param name="objectListView"></param>
		/// <param name="value">The OLVColumn or null to clear</param>
		/// <remarks>This method works, but it prevents subitems in the given column from having back colors.</remarks>
		static public void SetSelectedColumn(ListView objectListView, ColumnHeader value)
			=> NativeMethods.SendMessage(objectListView.Handle,
				LVM_SETSELECTEDCOLUMN, (value == null) ? -1 : value.Index, 0);

		static public Int32 GetTopIndex(ListView lv)
			=> (Int32)SendMessage(lv.Handle, LVM_GETTOPINDEX, 0, 0);

		static public IntPtr GetTooltipControl(ListView lv)
			=> SendMessage(lv.Handle, LVM_GETTOOLTIPS, 0, 0);

		static public IntPtr SetTooltipControl(ListView lv, ToolTipControl tooltip)
			=> SendMessage(lv.Handle, LVM_SETTOOLTIPS, 0, tooltip.Handle);

		static public Boolean HasHorizontalScrollBar(ListView lv)
		{
			const Int32 GWL_STYLE = -16;
			const Int32 WS_HSCROLL = 0x00100000;

			return (NativeMethods.GetWindowLong(lv.Handle, GWL_STYLE) & WS_HSCROLL) != 0;
		}

		public static Int32 GetWindowLong(IntPtr hWnd, Int32 nIndex)
		{
			return IntPtr.Size == 4
				? (Int32)GetWindowLong32(hWnd, nIndex)
				: (Int32)(Int64)GetWindowLongPtr64(hWnd, nIndex);
		}

		public static Int32 SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong)
			=> IntPtr.Size == 4
				? (Int32)SetWindowLongPtr32(hWnd, nIndex, dwNewLong)
				: (Int32)(Int64)SetWindowLongPtr64(hWnd, nIndex, dwNewLong);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr SetBkColor(IntPtr hDC, Int32 clr);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr SetTextColor(IntPtr hDC, Int32 crColor);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

		[DllImport("uxtheme.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr SetWindowTheme(IntPtr hWnd, String subApp, String subIdList);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern Boolean InvalidateRect(IntPtr hWnd, Int32 ignored, Boolean erase);

		[StructLayout(LayoutKind.Sequential)]
		public struct LVITEMINDEX
		{
			public Int32 iItem;
			public Int32 iGroup;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public Int32 x;
			public Int32 y;
		}

		public static Int32 GetGroupInfo(ObjectListView olv, Int32 groupId, ref LVGROUP2 group)
			=> (Int32)NativeMethods.SendMessage(olv.Handle, LVM_GETGROUPINFO, groupId, ref group);

		public static GroupState GetGroupState(ObjectListView olv, Int32 groupId, GroupState mask)
			=> (GroupState)NativeMethods.SendMessage(olv.Handle, LVM_GETGROUPSTATE, groupId, (Int32)mask);

		public static Int32 InsertGroup(ObjectListView olv, LVGROUP2 group)
			=> (Int32)NativeMethods.SendMessage(olv.Handle, LVM_INSERTGROUP, -1, ref group);

		public static Int32 SetGroupInfo(ObjectListView olv, Int32 groupId, LVGROUP2 group)
			=> (Int32)NativeMethods.SendMessage(olv.Handle, LVM_SETGROUPINFO, groupId, ref group);

		public static Int32 SetGroupMetrics(ObjectListView olv, LVGROUPMETRICS metrics)
			=> (Int32)NativeMethods.SendMessage(olv.Handle, LVM_SETGROUPMETRICS, 0, ref metrics);

		public static void ClearGroups(VirtualObjectListView virtualObjectListView)
			=> NativeMethods.SendMessage(virtualObjectListView.Handle, LVM_REMOVEALLGROUPS, 0, 0);

		public static void SetGroupImageList(ObjectListView olv, ImageList il)
		{
			const Int32 LVSIL_GROUPHEADER = 3;
			NativeMethods.SendMessage(olv.Handle, LVM_SETIMAGELIST, LVSIL_GROUPHEADER, il == null ? IntPtr.Zero : il.Handle);
		}

		public static Int32 HitTest(ObjectListView olv, ref LVHITTESTINFO hittest)
			=> (Int32)NativeMethods.SendMessage(olv.Handle, olv.View == View.Details ? LVM_SUBITEMHITTEST : LVM_HITTEST, -1, ref hittest);
	}
}