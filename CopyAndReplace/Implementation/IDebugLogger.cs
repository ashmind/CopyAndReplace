namespace CopyAndReplace.Implementation {
    public interface IDebugLogger {
        void WriteLine(string message);
        void WriteLine(string format, params object[] args);
    }
}