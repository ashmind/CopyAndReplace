using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace CopyAndReplace.Implementation {
    public class ClipboardHelperEventHandler : IVsUIHierWinClipboardHelperEvents {
        private readonly IVsOutputWindowPane output;

        public ClipboardHelperEventHandler(IVsOutputWindowPane output) {
            this.output = output;
        }

        public int OnClear(int fDataWasCut) {
            output.OutputString("OnClear");
            return VSConstants.S_OK;
        }

        public int OnPaste(int fDataWasCut, uint dwEffects) {
            output.OutputString("OnPaste");
            return VSConstants.S_OK;
        }
    }
}
