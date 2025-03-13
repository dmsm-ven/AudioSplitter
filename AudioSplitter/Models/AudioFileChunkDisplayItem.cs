using CommunityToolkit.Mvvm.ComponentModel;
using Humanizer;

namespace AudioSplitter.Models;

public partial class AudioFileChunkDisplayItem : ObservableObject
{
    [ObservableProperty]
    public string artistName;

    [ObservableProperty]
    public string albumName;

    [ObservableProperty]
    public int trackNumber;

    [ObservableProperty]
    public string trackName;

    [ObservableProperty]
    public TimeSpan timeStart = TimeSpan.Zero;

    [ObservableProperty]
    public TimeSpan timeEnd = TimeSpan.Zero;

    public TimeSpan Duration => timeEnd - timeStart;

    public string LengthInKb => 0.Bytes().Humanize();

    public AudioFileChunkDisplayItem()
    {

    }
}
