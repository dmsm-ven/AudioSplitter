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
    [NotifyPropertyChangedFor(nameof(Duration))]
    public TimeSpan timeEnd = TimeSpan.Zero;

    [ObservableProperty]
    public bool inProgress;

    public TimeSpan Duration => TimeEnd - TimeStart;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSize))]
    public long lengthInBytes;

    public string FileSize => LengthInBytes.Bytes().Humanize();


    public AudioFileChunkDisplayItem()
    {

    }
}
