using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using AshMind.Extensions;
using CopyAndReplace.UI;
using EnvDTE;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;

namespace CopyAndReplace {
    public class CopyAndReplaceController {
        private const string CopyOf = "Copy of ";

        //private HashSet<string> lastReplaceFileList;
        private ReplacementViewModel lastReplaceViewModel;
        private readonly Stopwatch timeSinceLastReplace = new Stopwatch(); // TEMPHACK

        public void ProcessPaste(ProjectItem item) {
            // HACK :)
            if (!item.Name.StartsWith(CopyOf))
                return;

            Trace.WriteLine("Pasting '" + item.Name + "'.", TraceCategory.Name);
            var viewModel = this.GetReplacementViewModel(item);
            if (viewModel == null) // cancelled
                return;

            RenameAndReplace(item, viewModel.Pattern, viewModel.Replacement);
            timeSinceLastReplace.Reset();
            timeSinceLastReplace.Start();
        }

        //private IEnumerable<string> GetFilesInClipboard() {
        //    if (Clipboard.ContainsFileDropList())
        //        return Clipboard.GetFileDropList().Cast<string>();

        //    var oleDataObject = new OleDataObject(System.Windows.Forms.Clipboard.GetDataObject());
        //    DropDataType dropDataType;
        //    var stgItems = DragDropHelper.GetDroppedFiles(DragDropHelper.CF_VSSTGPROJECTITEMS, oleDataObject, out dropDataType);
        //    if (stgItems.Count > 0)
        //        return stgItems.Select(item => item.Split('|')).Select(parts => parts[2]);

        //    return Enumerable.Empty<string>();
        //}

        private ReplacementViewModel GetReplacementViewModel(ProjectItem item) {
            var originalName = GetNameBeforeCopy(item);
            //var files = GetFilesInClipboard()
            //                .Select(Path.GetFileName)
            //                .ToSet(StringComparer.InvariantCultureIgnoreCase);

            //if (lastReplaceFileList != null && lastReplaceFileList.SetEquals(files) && lastReplaceFileList.Contains(originalName) && ) {
            //    Trace.WriteLine(originalName + "is in the same clipboard file list as previous file, using previous replacement settings.", TraceCategory.Name);
            //    return lastReplaceViewModel;
            //}

            //lastReplaceFileList = files;

            // TEMPHACK
            if (lastReplaceViewModel != null && timeSinceLastReplace.ElapsedMilliseconds < 1000 && originalName.Contains(lastReplaceViewModel.Pattern))
                return lastReplaceViewModel;

            var initialText = Path.GetFileNameWithoutExtension(originalName);

            var dialog = new ReplaceDialog {
                ViewModel = new ReplacementViewModel {
                    Pattern = initialText,
                    Replacement = initialText
                }
            };
            var shouldReplace = dialog.ShowModal() ?? false;
            if (!shouldReplace)
                return null;

            var viewModel = dialog.ViewModel;
            lastReplaceViewModel = viewModel;
            return viewModel;
        }

        private string GetNameBeforeCopy(ProjectItem item) {
            return item.Name.Substring(CopyOf.Length);
        }

        private void RenameAndReplace(ProjectItem item, string pattern, string replacement) {
            Trace.WriteLine(string.Format("Renaming/replacing '{0}': '{1}' to '{2}'.", item.Name, pattern, replacement), TraceCategory.Name);
            var nameBeforeCopy = GetNameBeforeCopy(item);
            var renamed = nameBeforeCopy.Replace(pattern, replacement);

            if (renamed != nameBeforeCopy)
                item.Name = renamed;

            var fullPath = (string)item.Properties.Item("FullPath").Value;
            var content = File.ReadAllText(fullPath);
            File.WriteAllText(fullPath, content.Replace(pattern, replacement));
        }
    }
}
