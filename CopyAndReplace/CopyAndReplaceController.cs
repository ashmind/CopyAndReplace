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

        private bool inPaste = false;
        private readonly IList<ProjectItem> pastedItems = new List<ProjectItem>();
        
        public void HandleBeforePaste() {
            Trace.WriteLine("Before paste.", TraceCategory.Name);
            inPaste = true;
            pastedItems.Clear();
        }

        public void HandleAddedItem(ProjectItem item) {
            if (!inPaste)
                return;

            Trace.WriteLine("Pasted: " + item.Name, TraceCategory.Name);
            pastedItems.Add(item);
        }
        
        public void HandleAfterPaste() {
            Trace.WriteLine("After paste: " + pastedItems.Count + " items to process.", TraceCategory.Name);
            inPaste = false;

            if (pastedItems.Count == 0)
                return;
            
            var viewModel = this.GetReplacementViewModel(pastedItems[0]);
            if (viewModel == null) {
                Trace.WriteLine("  Processing cancelled by user.", TraceCategory.Name);
                return;
            }

            foreach (var item in pastedItems) {
                RenameAndReplace(item, viewModel.Pattern, viewModel.Replacement);                
            }
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

            return dialog.ViewModel;
        }

        private string GetNameBeforeCopy(ProjectItem item) {
            return item.Name.Substring(CopyOf.Length);
        }

        private void RenameAndReplace(ProjectItem item, string pattern, string replacement) {
            Trace.WriteLine(string.Format("  {0}: \"{1}\" -> \"{2}\".", item.Name, pattern, replacement), TraceCategory.Name);
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
