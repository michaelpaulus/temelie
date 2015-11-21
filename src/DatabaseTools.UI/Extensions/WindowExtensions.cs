
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
        public static class WindowExtensions
        {

            #region Properties

            private static System.Text.Encoding _Encoding = new System.Text.UTF8Encoding();
            private static System.Text.Encoding Encoding
            {
                get
                {
                    return _Encoding;
                }
                set
                {
                    _Encoding = value;
                }
            }
            private static System.Xml.Serialization.XmlSerializer _Serializer = new System.Xml.Serialization.XmlSerializer(typeof(NativeMethods.WINDOWPLACEMENT));
            private static System.Xml.Serialization.XmlSerializer Serializer
            {
                get
                {
                    return _Serializer;
                }
                set
                {
                    _Serializer = value;
                }
            }

            #endregion

            #region Extension Methods

            public static void SetPlacement(this Window window, string placementXml)
            {
                if (!(string.IsNullOrEmpty(placementXml)))
                {
                    SetPlacement((new System.Windows.Interop.WindowInteropHelper(window)).Handle, placementXml);
                }
            }

            public static string GetPlacement(this Window window)
            {
                return GetPlacement((new System.Windows.Interop.WindowInteropHelper(window)).Handle);
            }


            #endregion

            #region Supporting Methods

            private static void SetPlacement(IntPtr windowHandle, string placementXml)
            {
                if (string.IsNullOrEmpty(placementXml))
                {
                    return;
                }

                NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
                byte[] xmlBytes = Encoding.GetBytes(placementXml);

                try
                {
                    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(xmlBytes))
                    {
                        placement = (NativeMethods.WINDOWPLACEMENT)Serializer.Deserialize(memoryStream);
                    }

                    placement.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));
                    placement.flags = 0;
                    placement.showCmd = (((placement.showCmd == NativeMethods.ShowWindowCommands.ShowMinimized) ? NativeMethods.ShowWindowCommands.Normal : placement.showCmd));
                    NativeMethods.SetWindowPlacement(windowHandle, ref placement);
                    // Parsing placement XML failed. Fail silently.
                }
                catch (InvalidOperationException ex)
                {
                    var message = ex.Message;
                }
            }

            private static string GetPlacement(IntPtr windowHandle)
            {
                NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
                NativeMethods.GetWindowPlacement(windowHandle, ref placement);

                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    using (System.Xml.XmlTextWriter xmlTextWriter = new System.Xml.XmlTextWriter(memoryStream, System.Text.Encoding.UTF8))
                    {
                        Serializer.Serialize(xmlTextWriter, placement);
                        byte[] xmlBytes = memoryStream.ToArray();
                        return Encoding.GetString(xmlBytes);
                    }
                }
            }

            #endregion

        }
    }


}