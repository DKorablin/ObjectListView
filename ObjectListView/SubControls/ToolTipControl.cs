/*
 * ToolTipControl - A limited wrapper around a Windows tooltip control
 *
 * For some reason, the ToolTip class in the .NET framework is implemented in a significantly
 * different manner to other controls. For our purposes, the worst of these problems
 * is that we cannot get the Handle, so we cannot send Windows level messages to the control.
 * 
 * Author: Phillip Piper
 * Date: 2009-05-17 7:22PM 
 *
 * Change log:
 * v2.3
 * 2009-06-13  JPP  - Moved ToolTipShowingEventArgs to Events.cs
 * v2.2
 * 2009-06-06  JPP  - Fixed some Vista specific problems
 * 2009-05-17  JPP  - Initial version
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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security.Permissions;

namespace BrightIdeasSoftware
{
	/// <summary>A limited wrapper around a Windows tooltip window.</summary>
	public class ToolTipControl : NativeWindow
	{
		#region Constants

		/// <summary>These are the standard icons that a tooltip can display.</summary>
		public enum StandardIcons
		{
			/// <summary>No icon</summary>
			None = 0,

			/// <summary>Info</summary>
			Info = 1,

			/// <summary>Warning</summary>
			Warning = 2,

			/// <summary>Error</summary>
			Error = 3,

			/// <summary>Large info (Vista and later only)</summary>
			InfoLarge = 4,

			/// <summary>Large warning (Vista and later only)</summary>
			WarningLarge = 5,

			/// <summary>Large error (Vista and later only)</summary>
			ErrorLarge = 6
		}

		const Int32 GWL_STYLE = -16;
		const Int32 WM_GETFONT = 0x31;
		const Int32 WM_SETFONT = 0x30;
		const Int32 WS_BORDER = 0x800000;
		const Int32 WS_EX_TOPMOST = 8;

		const Int32 TTM_ADDTOOL = 0x432;
		const Int32 TTM_ADJUSTRECT = 0x400 + 31;
		const Int32 TTM_DELTOOL = 0x433;
		const Int32 TTM_GETBUBBLESIZE = 0x400 + 30;
		const Int32 TTM_GETCURRENTTOOL = 0x400 + 59;
		const Int32 TTM_GETTIPBKCOLOR = 0x400 + 22;
		const Int32 TTM_GETTIPTEXTCOLOR = 0x400 + 23;
		const Int32 TTM_GETDELAYTIME = 0x400 + 21;
		const Int32 TTM_NEWTOOLRECT = 0x400 + 52;
		const Int32 TTM_POP = 0x41c;
		const Int32 TTM_SETDELAYTIME = 0x400 + 3;
		const Int32 TTM_SETMAXTIPWIDTH = 0x400 + 24;
		const Int32 TTM_SETTIPBKCOLOR = 0x400 + 19;
		const Int32 TTM_SETTIPTEXTCOLOR = 0x400 + 20;
		const Int32 TTM_SETTITLE = 0x400 + 33;
		const Int32 TTM_SETTOOLINFO = 0x400 + 54;

		const Int32 TTF_IDISHWND = 1;
		//const Int32 TTF_ABSOLUTE = 0x80;
		const Int32 TTF_CENTERTIP = 2;
		const Int32 TTF_RTLREADING = 4;
		const Int32 TTF_SUBCLASS = 0x10;
		//const Int32 TTF_TRACK = 0x20;
		//const Int32 TTF_TRANSPARENT = 0x100;
		const Int32 TTF_PARSELINKS = 0x1000;

		const Int32 TTS_NOPREFIX = 2;
		const Int32 TTS_BALLOON = 0x40;
		const Int32 TTS_USEVISUALSTYLE = 0x100;

		const Int32 TTN_FIRST = -520;

		/// <summary></summary>
		public const Int32 TTN_SHOW = (TTN_FIRST - 1);

		/// <summary></summary>
		public const Int32 TTN_POP = (TTN_FIRST - 2);

		/// <summary></summary>
		public const Int32 TTN_LINKCLICK = (TTN_FIRST - 3);

		/// <summary></summary>
		public const Int32 TTN_GETDISPINFO = (TTN_FIRST - 10);

		const Int32 TTDT_AUTOMATIC = 0;
		const Int32 TTDT_RESHOW = 1;
		const Int32 TTDT_AUTOPOP = 2;
		const Int32 TTDT_INITIAL = 3;

		#endregion

		#region Properties

		/// <summary>Get or set if the style of the tooltip control</summary>
		internal Int32 WindowStyle
		{
			get => (Int32)NativeMethods.GetWindowLong(this.Handle, GWL_STYLE);
			set => NativeMethods.SetWindowLong(this.Handle, GWL_STYLE, value);
		}

		/// <summary>Get or set if the tooltip should be shown as a balloon</summary>
		public Boolean IsBalloon
		{
			get => (this.WindowStyle & TTS_BALLOON) == TTS_BALLOON;
			set
			{
				if(this.IsBalloon == value)
					return;

				Int32 windowStyle = this.WindowStyle;
				if(value)
				{
					windowStyle |= (TTS_BALLOON | TTS_USEVISUALSTYLE);
					// On XP, a border makes the balloon look wrong
					if(!ObjectListView.IsVistaOrLater)
						windowStyle &= ~WS_BORDER;
				} else
				{
					windowStyle &= ~(TTS_BALLOON | TTS_USEVISUALSTYLE);
					if(!ObjectListView.IsVistaOrLater)
					{
						if(this._hasBorder)
							windowStyle |= WS_BORDER;
						else
							windowStyle &= ~WS_BORDER;
					}
				}
				this.WindowStyle = windowStyle;
			}
		}

		/// <summary>Get or set if the tooltip should be shown as a balloon</summary>
		public Boolean HasBorder
		{
			get => this._hasBorder;
			set
			{
				if(this._hasBorder == value)
					return;

				if(value)
					this.WindowStyle |= WS_BORDER;
				else
					this.WindowStyle &= ~WS_BORDER;
			}
		}
		private Boolean _hasBorder = true;

		/// <summary>Get or set the background color of the tooltip</summary>
		public Color BackColor
		{
			get
			{
				Int32 color = (Int32)NativeMethods.SendMessage(this.Handle, TTM_GETTIPBKCOLOR, 0, 0);
				return ColorTranslator.FromWin32(color);
			}
			set
			{
				// For some reason, setting the color fails on Vista and messes up later ops.
				// So we don't even try to set it.
				if(!ObjectListView.IsVistaOrLater)
				{
					Int32 color = ColorTranslator.ToWin32(value);
					NativeMethods.SendMessage(this.Handle, TTM_SETTIPBKCOLOR, color, 0);
					//Int32 x2 = Marshal.GetLastWin32Error();
				}
			}
		}

		/// <summary>Get or set the color of the text and border on the tooltip.</summary>
		public Color ForeColor
		{
			get
			{
				Int32 color = (Int32)NativeMethods.SendMessage(this.Handle, TTM_GETTIPTEXTCOLOR, 0, 0);
				return ColorTranslator.FromWin32(color);
			}
			set
			{
				// For some reason, setting the color fails on Vista and messes up later ops.
				// So we don't even try to set it.
				if(!ObjectListView.IsVistaOrLater)
				{
					Int32 color = ColorTranslator.ToWin32(value);
					NativeMethods.SendMessage(this.Handle, TTM_SETTIPTEXTCOLOR, color, 0);
				}
			}
		}

		/// <summary>Get or set the title that will be shown on the tooltip.</summary>
		public String Title
		{
			get => this.title;
			set
			{
				if(String.IsNullOrEmpty(value))
					this.title = String.Empty;
				else
					this.title = value.Length >= 100
						? value.Substring(0, 99)
						: value;
				NativeMethods.SendMessageString(this.Handle, TTM_SETTITLE, (Int32)this._standardIcon, this.title);
			}
		}
		private String title;

		/// <summary>Get or set the icon that will be shown on the tooltip.</summary>
		public StandardIcons StandardIcon
		{
			get => this._standardIcon;
			set
			{
				this._standardIcon = value;
				NativeMethods.SendMessageString(this.Handle, TTM_SETTITLE, (Int32)this._standardIcon, this.title);
			}
		}
		private StandardIcons _standardIcon;

		/// <summary>Gets or sets the font that will be used to draw this control. is still.</summary>
		/// <remarks>Setting this to null reverts to the default font.</remarks>
		public Font Font
		{
			get
			{
				IntPtr hfont = NativeMethods.SendMessage(this.Handle, WM_GETFONT, 0, 0);
				return hfont == IntPtr.Zero
					? Control.DefaultFont
					: Font.FromHfont(hfont);
			}
			set
			{
				Font newFont = value ?? Control.DefaultFont;
				if(newFont == this._font)
					return;

				this._font = newFont;
				IntPtr hfont = this._font.ToHfont(); // THINK: When should we delete this hfont?
				NativeMethods.SendMessage(this.Handle, WM_SETFONT, hfont, 0);
			}
		}
		private Font _font;

		/// <summary>Gets or sets how many milliseconds the tooltip will remain visible while the mouse is still.</summary>
		public Int32 AutoPopDelay
		{
			get => this.GetDelayTime(TTDT_AUTOPOP);
			set => this.SetDelayTime(TTDT_AUTOPOP, value);
		}

		/// <summary>Gets or sets how many milliseconds the mouse must be still before the tooltip is shown.</summary>
		public Int32 InitialDelay
		{
			get => this.GetDelayTime(TTDT_INITIAL);
			set => this.SetDelayTime(TTDT_INITIAL, value);
		}

		/// <summary>Gets or sets how many milliseconds the mouse must be still before the tooltip is shown again.</summary>
		public Int32 ReshowDelay
		{
			get => this.GetDelayTime(TTDT_RESHOW);
			set => this.SetDelayTime(TTDT_RESHOW, value);
		}

		private Int32 GetDelayTime(Int32 which)
			=> (Int32)NativeMethods.SendMessage(this.Handle, TTM_GETDELAYTIME, which, 0);

		private void SetDelayTime(Int32 which, Int32 value)
			=> NativeMethods.SendMessage(this.Handle, TTM_SETDELAYTIME, which, value);

		#endregion

		#region Commands

		/// <summary>Create the underlying control.</summary>
		/// <param name="parentHandle">The parent of the tooltip</param>
		/// <remarks>This does nothing if the control has already been created</remarks>
		public void Create(IntPtr parentHandle)
		{
			if(this.Handle != IntPtr.Zero)
				return;

			CreateParams cp = new CreateParams
			{
				ClassName = "tooltips_class32",
				Style = TTS_NOPREFIX,
				ExStyle = WS_EX_TOPMOST,
				Parent = parentHandle
			};
			this.CreateHandle(cp);

			// Ensure that multiline tooltips work correctly
			this.SetMaxWidth();
		}

		/// <summary>Take a copy of the current settings and restore them when the tooltip is popped.</summary>
		/// <remarks>This call cannot be nested. Subsequent calls to this method will be ignored until PopSettings() is called.</remarks>
		public void PushSettings()
		{
			// Ignore any nested calls
			if(this._settings != null)
				return;
			this._settings = new Hashtable
			{
				["IsBalloon"] = this.IsBalloon,
				["HasBorder"] = this.HasBorder,
				["BackColor"] = this.BackColor,
				["ForeColor"] = this.ForeColor,
				["Title"] = this.Title,
				["StandardIcon"] = this.StandardIcon,
				["AutoPopDelay"] = this.AutoPopDelay,
				["InitialDelay"] = this.InitialDelay,
				["ReshowDelay"] = this.ReshowDelay,
				["Font"] = this.Font
			};
		}
		private Hashtable _settings;

		/// <summary>Restore the settings of the tooltip as they were when PushSettings() was last called.</summary>
		public void PopSettings()
		{
			if(this._settings == null)
				return;

			this.IsBalloon = (Boolean)this._settings["IsBalloon"];
			this.HasBorder = (Boolean)this._settings["HasBorder"];
			this.BackColor = (Color)this._settings["BackColor"];
			this.ForeColor = (Color)this._settings["ForeColor"];
			this.Title = (String)this._settings["Title"];
			this.StandardIcon = (StandardIcons)this._settings["StandardIcon"];
			this.AutoPopDelay = (Int32)this._settings["AutoPopDelay"];
			this.InitialDelay = (Int32)this._settings["InitialDelay"];
			this.ReshowDelay = (Int32)this._settings["ReshowDelay"];
			this.Font = (Font)this._settings["Font"];

			this._settings = null;
		}

		/// <summary>Add the given window to those for whom this tooltip will show tips</summary>
		/// <param name="window">The window</param>
		public void AddTool(IWin32Window window)
		{
			NativeMethods.TOOLINFO lParam = this.MakeToolInfoStruct(window);
			NativeMethods.SendMessageTOOLINFO(this.Handle, TTM_ADDTOOL, 0, lParam);
		}

		/// <summary>Hide any currently visible tooltip</summary>
		/// <param name="window"></param>
		public void PopToolTip(IWin32Window window)
			=> NativeMethods.SendMessage(this.Handle, TTM_POP, 0, 0);

		//public void Munge() {
		//    NativeMethods.TOOLINFO tool = new NativeMethods.TOOLINFO();
		//    IntPtr result = NativeMethods.SendMessageTOOLINFO(this.Handle, TTM_GETCURRENTTOOL, 0, tool);
		//    System.Diagnostics.Trace.WriteLine("-");
		//    System.Diagnostics.Trace.WriteLine(result);
		//    result = NativeMethods.SendMessageTOOLINFO(this.Handle, TTM_GETBUBBLESIZE, 0, tool);
		//    System.Diagnostics.Trace.WriteLine(String.Format("{0} {1}", result.ToInt32() >> 16, result.ToInt32() & 0xFFFF));
		//    NativeMethods.ChangeSize(this, result.ToInt32() & 0xFFFF, result.ToInt32() >> 16);
		//    //NativeMethods.RECT r = new NativeMethods.RECT();
		//    //r.right 
		//    //IntPtr x = NativeMethods.SendMessageRECT(this.Handle, TTM_ADJUSTRECT, true, ref r);

		//    //System.Diagnostics.Trace.WriteLine(String.Format("{0} {1} {2} {3}", r.left, r.top, r.right, r.bottom));
		//}

		/// <summary>Remove the given window from those managed by this tooltip</summary>
		/// <param name="window"></param>
		public void RemoveToolTip(IWin32Window window)
		{
			NativeMethods.TOOLINFO lParam = this.MakeToolInfoStruct(window);
			NativeMethods.SendMessageTOOLINFO(this.Handle, TTM_DELTOOL, 0, lParam);
		}

		/// <summary>Set the maximum width of a tooltip String.</summary>
		public void SetMaxWidth()
			=> this.SetMaxWidth(SystemInformation.MaxWindowTrackSize.Width);

		/// <summary>Set the maximum width of a tooltip String.</summary>
		/// <remarks>Setting this ensures that line breaks in the tooltip are honoured.</remarks>
		public void SetMaxWidth(Int32 maxWidth)
			=> NativeMethods.SendMessage(this.Handle, TTM_SETMAXTIPWIDTH, 0, maxWidth);

		#endregion

		#region Implementation

		/// <summary>Make a TOOLINFO structure for the given window</summary>
		/// <param name="window"></param>
		/// <returns>A filled in TOOLINFO</returns>
		private NativeMethods.TOOLINFO MakeToolInfoStruct(IWin32Window window)
		{

			NativeMethods.TOOLINFO toolinfo_tooltip = new NativeMethods.TOOLINFO
			{
				hwnd = window.Handle,
				uFlags = TTF_IDISHWND | TTF_SUBCLASS,
				uId = window.Handle,
				lpszText = (IntPtr)(-1) // LPSTR_TEXTCALLBACK
			};

			return toolinfo_tooltip;
		}

		/// <summary>Handle a WmNotify message</summary>
		/// <param name="msg">The msg</param>
		/// <returns>True if the message has been handled</returns>
		protected virtual Boolean HandleNotify(ref Message msg)
		{

			//THINK: What do we have to do here? Nothing it seems :)

			//NativeMethods.NMHEADER nmheader = (NativeMethods.NMHEADER)msg.GetLParam(typeof(NativeMethods.NMHEADER));
			//System.Diagnostics.Trace.WriteLine("HandleNotify");
			//System.Diagnostics.Trace.WriteLine(nmheader.nhdr.code);

			//switch (nmheader.nhdr.code) {
			//}

			return false;
		}

		/// <summary>Handle a get display info message</summary>
		/// <param name="msg">The msg</param>
		/// <returns>True if the message has been handled</returns>
		public virtual Boolean HandleGetDispInfo(ref Message msg)
		{
			//System.Diagnostics.Trace.WriteLine("HandleGetDispInfo");
			this.SetMaxWidth();
			ToolTipShowingEventArgs args = new ToolTipShowingEventArgs
			{
				ToolTipControl = this
			};
			this.OnShowing(args);
			if(String.IsNullOrEmpty(args.Text))
				return false;

			this.ApplyEventFormatting(args);

			NativeMethods.NMTTDISPINFO dispInfo = (NativeMethods.NMTTDISPINFO)msg.GetLParam(typeof(NativeMethods.NMTTDISPINFO));
			dispInfo.lpszText = args.Text;
			dispInfo.hinst = IntPtr.Zero;
			if(args.RightToLeft == RightToLeft.Yes)
				dispInfo.uFlags |= TTF_RTLREADING;
			Marshal.StructureToPtr(dispInfo, msg.LParam, false);

			return true;
		}

		private void ApplyEventFormatting(ToolTipShowingEventArgs args)
		{
			if(!args.IsBalloon.HasValue &&
				!args.BackColor.HasValue &&
				!args.ForeColor.HasValue &&
				args.Title == null &&
				!args.StandardIcon.HasValue &&
				!args.AutoPopDelay.HasValue &&
				args.Font == null)
				return;

			this.PushSettings();
			if(args.IsBalloon.HasValue)
				this.IsBalloon = args.IsBalloon.Value;
			if(args.BackColor.HasValue)
				this.BackColor = args.BackColor.Value;
			if(args.ForeColor.HasValue)
				this.ForeColor = args.ForeColor.Value;
			if(args.StandardIcon.HasValue)
				this.StandardIcon = args.StandardIcon.Value;
			if(args.AutoPopDelay.HasValue)
				this.AutoPopDelay = args.AutoPopDelay.Value;
			if(args.Font != null)
				this.Font = args.Font;
			if(args.Title != null)
				this.Title = args.Title;
		}

		/// <summary>Handle a TTN_LINKCLICK message</summary>
		/// <param name="msg">The msg</param>
		/// <returns>True if the message has been handled</returns>
		/// <remarks>This cannot call base.WndProc() since the msg may have come from another control.</remarks>
		public virtual Boolean HandleLinkClick(ref Message msg)
			=> false;//System.Diagnostics.Trace.WriteLine("HandleLinkClick");

		/// <summary>Handle a TTN_POP message</summary>
		/// <param name="msg">The msg</param>
		/// <returns>True if the message has been handled</returns>
		/// <remarks>This cannot call base.WndProc() since the msg may have come from another control.</remarks>
		public virtual Boolean HandlePop(ref Message msg)
		{
			//System.Diagnostics.Trace.WriteLine("HandlePop");
			this.PopSettings();
			return true;
		}

		/// <summary>Handle a TTN_SHOW message</summary>
		/// <param name="msg">The msg</param>
		/// <returns>True if the message has been handled</returns>
		/// <remarks>This cannot call base.WndProc() since the msg may have come from another control.</remarks>
		public virtual Boolean HandleShow(ref Message msg)
			=> false;//System.Diagnostics.Trace.WriteLine("HandleShow");

		/// <summary>Handle a reflected notify message</summary>
		/// <param name="msg">The msg</param>
		/// <returns>True if the message has been handled</returns>
		protected virtual Boolean HandleReflectNotify(ref Message msg)
		{

			NativeMethods.NMHEADER nmheader = (NativeMethods.NMHEADER)msg.GetLParam(typeof(NativeMethods.NMHEADER));
			switch(nmheader.nhdr.code)
			{
			case TTN_SHOW:
				//System.Diagnostics.Trace.WriteLine("reflect TTN_SHOW");
				if(this.HandleShow(ref msg))
					return true;
				break;
			case TTN_POP:
				//System.Diagnostics.Trace.WriteLine("reflect TTN_POP");
				if(this.HandlePop(ref msg))
					return true;
				break;
			case TTN_LINKCLICK:
				//System.Diagnostics.Trace.WriteLine("reflect TTN_LINKCLICK");
				if(this.HandleLinkClick(ref msg))
					return true;
				break;
			case TTN_GETDISPINFO:
				//System.Diagnostics.Trace.WriteLine("reflect TTN_GETDISPINFO");
				if(this.HandleGetDispInfo(ref msg))
					return true;
				break;
			}

			return false;
		}

		/// <summary>Mess with the basic message pump of the tooltip</summary>
		/// <param name="msg"></param>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		override protected void WndProc(ref Message msg)
		{
			//System.Diagnostics.Trace.WriteLine(String.Format("xx {0:x}", msg.Msg));
			switch(msg.Msg)
			{
			case 0x4E: // WM_NOTIFY
				if(!this.HandleNotify(ref msg))
					return;
				break;

			case 0x204E: // WM_REFLECT_NOTIFY
				if(!this.HandleReflectNotify(ref msg))
					return;
				break;
			}

			base.WndProc(ref msg);
		}

		#endregion

		#region Events

		/// <summary>Tell the world that a tooltip is about to show</summary>
		public event EventHandler<ToolTipShowingEventArgs> Showing;

		/// <summary>Tell the world that a tooltip is about to disappear</summary>
		public event EventHandler<EventArgs> Pop;

		/// <summary></summary>
		/// <param name="e"></param>
		protected virtual void OnShowing(ToolTipShowingEventArgs e)
			=> this.Showing?.Invoke(this, e);

		/// <summary></summary>
		/// <param name="e"></param>
		protected virtual void OnPop(EventArgs e)
			=> this.Pop?.Invoke(this, e);

		#endregion
	}
}