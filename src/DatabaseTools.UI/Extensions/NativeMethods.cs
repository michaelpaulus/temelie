
using System.Linq;
using System.Xml.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DatabaseTools
{
	namespace Extensions
	{
		public class NativeMethods
		{

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			public extern static bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			public extern static bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

			// RECT structure required by WINDOWPLACEMENT structure
			[Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
			public struct RECT
			{
				public int Left;
				public int Top;
				public int Right;
				public int Bottom;

				public RECT(int left, int top, int right, int bottom) : this()
				{
					this.Left = left;
					this.Top = top;
					this.Right = right;
					this.Bottom = bottom;
				}
			}

			// POINT structure required by WINDOWPLACEMENT structure
			[Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
			public struct POINT
			{
				public int X;
				public int Y;

				public POINT(int x, int y) : this()
				{
					this.X = x;
					this.Y = y;
				}
			}

			// WINDOWPLACEMENT stores the position, size, and state of a window
			[Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
			public struct WINDOWPLACEMENT
			{
				public int length;
				public int flags;
				public ShowWindowCommands showCmd;
				public POINT minPosition;
				public POINT maxPosition;
				public RECT normalPosition;
			}

			public enum ShowWindowCommands: int
			{
				Hide = 0,
				Normal = 1,
				ShowMinimized = 2,
				Maximize = 3,
				ShowMaximized = 3,
				ShowNoActivate = 4,
				Show = 5,
				Minimize = 6,
				ShowMinNoActive = 7,
				ShowNA = 8,
				Restore = 9,
				ShowDefault = 10,
				ForceMinimize = 11
			}

		}
	}


}