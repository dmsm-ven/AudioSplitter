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
    [NotifyPropertyChangedFor(nameof(Duration))]
    public TimeSpan timeStart = TimeSpan.Zero;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Duration))]
    public TimeSpan timeEnd = TimeSpan.Zero;

    [ObservableProperty]
    public bool inProgress;

    private readonly TimeSpan duration;
    public TimeSpan Duration
    {
        get => TimeEnd - TimeStart;
        set
        {
            if (value != duration)
            {
                TimeEnd = TimeStart + value;
                this.OnPropertyChanged(nameof(Duration));
            }
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSize))]
    public long lengthInBytes;

    public string FileSize => LengthInBytes.Bytes().Humanize();


    public AudioFileChunkDisplayItem()
    {

    }
}
