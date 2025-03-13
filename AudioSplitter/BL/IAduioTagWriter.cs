namespace AudioSplitter.BL;

public class SimpleAduioTagWriter : IAduioTagWriter
{
    public void SetTag(string tagName, string value)
    {
        throw new NotImplementedException();
    }
}

public interface IAduioTagWriter
{
    void SetTag(string tagName, string value);
}
