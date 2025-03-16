namespace AudioSplitter.Interfaces;

public interface IAduioTagWriter
{
    void SetTags(string fileName, IReadOnlyDictionary<string, string> tags);
    void SetTag(string fileName, string tagName, string value);
}
