using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace CopyAndReplace.Implementation {
    public class TextDocumentWrapperFactory : ITextFileWrapperFactory {
        private readonly IFileExtensionRegistryService fileExtensionRegistry;
        private readonly ITextDocumentFactoryService textDocumentFactory;

        public TextDocumentWrapperFactory(IFileExtensionRegistryService fileExtensionRegistry, ITextDocumentFactoryService textDocumentFactory) {
            this.fileExtensionRegistry = fileExtensionRegistry;
            this.textDocumentFactory = textDocumentFactory;
        }

        public ITextFileWrapper OpenFrom(string path) {
            var extension = Path.GetExtension(path);
            var contentType = fileExtensionRegistry.GetContentTypeForExtension(extension);
            using (var document = textDocumentFactory.CreateAndLoadTextDocument(path, contentType)) {
                return new TextFileWrapper(path, document.Encoding, document.TextBuffer.CurrentSnapshot.GetText());
            }
        }
    }
}
