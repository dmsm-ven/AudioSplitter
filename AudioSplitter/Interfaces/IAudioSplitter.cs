using AudioSplitter.Models;

namespace AudioSplitter.Interfaces;

public interface IAudioSplitter
{
    Task<AudioFileChunk> GetChunk(int trackNumber, string chunkName, TimeSpan start, TimeSpan end);
    void Initialize(string sourceFile, string artist, string album);
}
