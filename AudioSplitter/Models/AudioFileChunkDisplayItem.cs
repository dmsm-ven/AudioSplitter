using CommunityToolkit.Mvvm.ComponentModel;
using Humanizer;

namespace AudioSplitter.Models;

public partial class AudioFileChunkDisplayItem : ObservableObject
{
    private readonly AudioFileChunk chunkInfo;

    [ObservableProperty]
    public string artistName;

    [ObservableProperty]
    public string albumName;

    [ObservableProperty]
    public int trackNumber;

    [ObservableProperty]
    public string chunkFileName;

    [ObservableProperty]
    public TimeSpan timeStart;

    [ObservableProperty]
    public TimeSpan timeEnd;

    public TimeSpan Duration => timeEnd - timeStart;

    public string LengthInKb => (chunkInfo.Data != null ? chunkInfo.Data.Length : 0).Bytes().Humanize();

    public AudioFileChunkDisplayItem()
    {

    }
}
