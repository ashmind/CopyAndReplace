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
using System.Linq;
using System.Runtime.InteropServices;

// ReSharper disable CheckNamespace
namespace Microsoft.VisualStudio.Project {
// ReSharper restore CheckNamespace

    /// <summary>
    /// Defines drop types
    /// </summary>
    internal enum DropDataType {
        None,
        Shell,
        VsStg,
        VsRef
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct _DROPFILES {
        public Int32 pFiles;
        public Int32 X;
        public Int32 Y;
        public Int32 fNC;
        public Int32 fWide;
    }
}
