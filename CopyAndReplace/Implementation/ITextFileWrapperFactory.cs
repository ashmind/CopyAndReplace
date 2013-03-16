namespace CopyAndReplace.Implementation {
    public interface ITextFileWrapperFactory {
        ITextFileWrapper OpenFrom(string path);
    }
}