/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, 
 * Version 2.0. A copy of the license can be found in the License.html file in 
 * the current folder.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio.OLE.Interop;

// ReSharper disable CheckNamespace
namespace Microsoft.VisualStudio.Project {
// ReSharper restore CheckNamespace
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal static class DragDropHelper {
#pragma warning disable 414
        internal static readonly ushort CF_VSREFPROJECTITEMS;
        internal static readonly ushort CF_VSSTGPROJECTITEMS;
        internal static readonly ushort CF_VSPROJECTCLIPDESCRIPTOR;
#pragma warning restore 414

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DragDropHelper() {
            CF_VSREFPROJECTITEMS = UnsafeNativeMethods.RegisterClipboardFormat("CF_VSREFPROJECTITEMS");
            CF_VSSTGPROJECTITEMS = UnsafeNativeMethods.RegisterClipboardFormat("CF_VSSTGPROJECTITEMS");
            CF_VSPROJECTCLIPDESCRIPTOR = UnsafeNativeMethods.RegisterClipboardFormat("CF_PROJECTCLIPBOARDDESCRIPTOR");
        }


        public static FORMATETC CreateFormatEtc(ushort iFormat) {
            FORMATETC fmt = new FORMATETC();
            fmt.cfFormat = iFormat;
            fmt.ptd = IntPtr.Zero;
            fmt.dwAspect = (uint)DVASPECT.DVASPECT_CONTENT;
            fmt.lindex = -1;
            fmt.tymed = (uint)TYMED.TYMED_HGLOBAL;
            return fmt;
        }

        public static int QueryGetData(Microsoft.VisualStudio.OLE.Interop.IDataObject pDataObject, ref FORMATETC fmtetc) {
            int returnValue = VSConstants.E_FAIL;
            FORMATETC[] af = new FORMATETC[1];
            af[0] = fmtetc;
            try {
                int result = ErrorHandler.ThrowOnFailure(pDataObject.QueryGetData(af));
                if (result == VSConstants.S_OK) {
                    fmtetc = af[0];
                    returnValue = VSConstants.S_OK;
                }
            }
            catch (COMException e) {
                Trace.WriteLine("COMException : " + e.Message);
                returnValue = e.ErrorCode;
            }

            return returnValue;
        }

        public static STGMEDIUM GetData(Microsoft.VisualStudio.OLE.Interop.IDataObject pDataObject, ref FORMATETC fmtetc) {
            FORMATETC[] af = new FORMATETC[1];
            af[0] = fmtetc;
            STGMEDIUM[] sm = new STGMEDIUM[1];
            pDataObject.GetData(af, sm);
            fmtetc = af[0];
            return sm[0];
        }

        /// <summary>
        /// Retrives data from a VS format.
        /// </summary>
        public static List<string> GetDroppedFiles(ushort format, Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject, out DropDataType ddt) {
            ddt = DropDataType.None;
            List<string> droppedFiles = new List<string>();

            // try HDROP
            FORMATETC fmtetc = CreateFormatEtc(format);

            if (QueryGetData(dataObject, ref fmtetc) == VSConstants.S_OK) {
                STGMEDIUM stgmedium = DragDropHelper.GetData(dataObject, ref fmtetc);
                if (stgmedium.tymed == (uint)TYMED.TYMED_HGLOBAL) {
                    // We are releasing the cloned hglobal here.
                    IntPtr dropInfoHandle = stgmedium.unionmember;
                    if (dropInfoHandle != IntPtr.Zero) {
                        ddt = DropDataType.Shell;
                        try {
                            uint numFiles = UnsafeNativeMethods.DragQueryFile(dropInfoHandle, 0xFFFFFFFF, null, 0);

                            // We are a directory based project thus a projref string is placed on the clipboard.
                            // We assign the maximum length of a projref string.
                            // The format of a projref is : <Proj Guid>|<project rel path>|<file path>
                            uint lenght = (uint)Guid.Empty.ToString().Length + 2 * NativeMethods.MAX_PATH + 2;
                            char[] moniker = new char[lenght + 1];
                            for (uint fileIndex = 0; fileIndex < numFiles; fileIndex++) {
                                uint queryFileLength = UnsafeNativeMethods.DragQueryFile(dropInfoHandle, fileIndex, moniker, lenght);
                                string filename = new String(moniker, 0, (int)queryFileLength);
                                droppedFiles.Add(filename);
                            }
                        }
                        finally {
                            Marshal.FreeHGlobal(dropInfoHandle);
                        }
                    }
                }
            }

            return droppedFiles;
        }



        public static string GetSourceProjectPath(Microsoft.VisualStudio.OLE.Interop.IDataObject dataObject) {
            string projectPath = null;
            FORMATETC fmtetc = CreateFormatEtc(CF_VSPROJECTCLIPDESCRIPTOR);

            if (QueryGetData(dataObject, ref fmtetc) == VSConstants.S_OK) {
                STGMEDIUM stgmedium = DragDropHelper.GetData(dataObject, ref fmtetc);
                if (stgmedium.tymed == (uint)TYMED.TYMED_HGLOBAL) {
                    // We are releasing the cloned hglobal here.
                    IntPtr dropInfoHandle = stgmedium.unionmember;
                    if (dropInfoHandle != IntPtr.Zero) {
                        try {
                            string path = GetData(dropInfoHandle);

                            // Clone the path that we can release our memory.
                            if (!String.IsNullOrEmpty(path)) {
                                projectPath = String.Copy(path);
                            }
                        }
                        finally {
                            Marshal.FreeHGlobal(dropInfoHandle);
                        }
                    }
                }
            }

            return projectPath;
        }

        /// <summary>
        /// Returns the data packed after the DROPFILES structure.
        /// </summary>
        /// <param name="dropHandle"></param>
        /// <returns></returns>
        internal static string GetData(IntPtr dropHandle) {
            IntPtr data = UnsafeNativeMethods.GlobalLock(dropHandle);
            try {
                _DROPFILES df = (_DROPFILES)Marshal.PtrToStructure(data, typeof(_DROPFILES));
                if (df.fWide != 0) {
                    IntPtr pdata = new IntPtr((long)data + df.pFiles);
                    return Marshal.PtrToStringUni(pdata);
                }
            }
            finally {
                if (data != null) {
                    UnsafeNativeMethods.GlobalUnLock(data);
                }
            }

            return null;
        }

        internal static IntPtr CopyHGlobal(IntPtr data) {
            IntPtr src = UnsafeNativeMethods.GlobalLock(data);
            int size = UnsafeNativeMethods.GlobalSize(data);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            IntPtr buffer = UnsafeNativeMethods.GlobalLock(ptr);

            try {
                for (int i = 0; i < size; i++) {
                    byte val = Marshal.ReadByte(new IntPtr((long)src + i));

                    Marshal.WriteByte(new IntPtr((long)buffer + i), val);
                }
            }
            finally {
                if (buffer != IntPtr.Zero) {
                    UnsafeNativeMethods.GlobalUnLock(buffer);
                }

                if (src != IntPtr.Zero) {
                    UnsafeNativeMethods.GlobalUnLock(src);
                }
            }
            return ptr;
        }

        internal static void CopyStringToHGlobal(string s, IntPtr data, int bufferSize) {
            Int16 nullTerminator = 0;
            int dwSize = Marshal.SizeOf(nullTerminator);

            if ((s.Length + 1) * Marshal.SizeOf(s[0]) > bufferSize)
                throw new System.IO.InternalBufferOverflowException();
            // IntPtr memory already locked...
            for (int i = 0, len = s.Length; i < len; i++) {
                Marshal.WriteInt16(data, i * dwSize, s[i]);
            }
            // NULL terminate it
            Marshal.WriteInt16(new IntPtr((long)data + (s.Length * dwSize)), nullTerminator);
        }

    } // end of dragdrophelper
}
