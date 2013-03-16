using System;

namespace CopyAndReplace.Implementation {
    public class ReplacementUsedEventArgs : EventArgs {
        public ReplacementUsedEventArgs(string match, string replacement) {
            this.Match = match;
            this.Replacement = replacement;
        }

        public string Match { get; private set; }
        public string Replacement { get; private set; }
    }
}