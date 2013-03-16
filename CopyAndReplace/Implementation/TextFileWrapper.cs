using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CopyAndReplace.Implementation {
    public class TextFileWrapper : ITextFileWrapper {
        private readonly string path;
        private readonly Encoding encoding;
        private string cachedText;

        public TextFileWrapper(string path, Encoding encoding, string text = null) {
            this.path = path;
            this.encoding = encoding;
            this.cachedText = text;
        }

        public string ReadAllText(bool allowCached = false) {
            if (allowCached && this.cachedText != null)
                return this.cachedText;

            var text = File.ReadAllText(this.path, this.encoding);
            this.cachedText = text;
            return text;
        }

        public void WriteAllText(string text) {
            this.cachedText = text;
            File.WriteAllText(this.path, text, this.encoding);
        }
        
        #region IDisposable Members

        void IDisposable.Dispose() {
        }

        #endregion
    }
}
