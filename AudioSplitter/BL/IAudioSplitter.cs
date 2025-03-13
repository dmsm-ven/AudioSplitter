using AudioSplitter.Models;

namespace AudioSplitter.BL;

public class SimpleAudioSplitter : IAudioSplitter
{
    public Task<AudioFileChunk> GetChunk(string sourceFile, string chunkName, TimeSpan start, TimeSpan end)
    {
        throw new NotImplementedException();
    }
}

public interface IAudioSplitter
{
    Task<AudioFileChunk> GetChunk(string sourceFile, string chunkName, TimeSpan start, TimeSpan end);
}
