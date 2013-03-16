using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using CopyAndReplace.Implementation;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CopyAndReplace {
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [Guid(GuidList.PackageString)]
    public sealed class CopyAndReplacePackage : Microsoft.VisualStudio.Shell.Package {
        private Controller controller;
        private DTE dte;
        private ProjectItemsEvents csItemsEvents;
        private CommandEvents pasteEvent;
        private IDebugLogger logger;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CopyAndReplacePackage() {
            Trace.WriteLine(string.Format("Entering constructor for: {0}", this), VSPackageInfo.Name);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            Trace.WriteLine(string.Format("Entering Initialize() of: {0}", this), VSPackageInfo.Name);
            base.Initialize();

            this.dte = (DTE)GetService(typeof(DTE));
            var output = this.GetOutputPane(GuidList.OutputPane, VSPackageInfo.Name + " (Diagnostic)");
            this.logger = new DebugLogger(output, VSPackageInfo.Name);

            this.controller = new Controller(this.logger);
            
            this.pasteEvent = this.dte.Events.CommandEvents[typeof(VSConstants.VSStd97CmdID).GUID.ToString("B"), (int)VSConstants.VSStd97CmdID.Paste];
            this.pasteEvent.BeforeExecute += delegate { LogExceptions(() => controller.HandleBeforePaste()); };
            this.pasteEvent.AfterExecute += delegate { LogExceptions(() => controller.HandleAfterPaste()); };

            this.csItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("CSharpProjectItemsEvents");
            this.csItemsEvents.ItemAdded += item => LogExceptions(() => controller.HandleAddedItem(item));
        }

        private void LogExceptions(Action action) {
            try {
                action();
            }
            catch (Exception ex) {
                this.logger.WriteLine("Exception: {0}", ex);
                throw;
            }
        }
    }
}