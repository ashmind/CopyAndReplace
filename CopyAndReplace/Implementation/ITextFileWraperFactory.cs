namespace CopyAndReplace.Implementation {
    public interface ITextFileWraperFactory {
        ITextFileWrapper OpenFrom(string path);
    }
}