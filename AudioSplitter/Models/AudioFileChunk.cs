using System.IO;

namespace AudioSplitter.Models;

public class AudioFileChunk
{
    public string ParentFileName { get; init; } = string.Empty;
    public string ChunkFileName { get; init; } = string.Empty;
    public TimeSpan TimeStart { get; init; } = TimeSpan.Zero;
    public TimeSpan TimeEnd { get; init; } = TimeSpan.Zero;
    public int TrackNumber { get; init; } = 0;
    public FileInfo FileInfo { get; init; }
}
