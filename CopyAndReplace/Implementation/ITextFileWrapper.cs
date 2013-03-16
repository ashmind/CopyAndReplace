using System;

namespace CopyAndReplace.Implementation {
    public interface ITextFileWrapper : IDisposable {
        string ReadAllText(bool allowCached = false);
        void WriteAllText(string text);
    }
}