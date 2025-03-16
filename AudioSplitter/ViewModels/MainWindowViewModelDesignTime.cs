using AudioSplitter.Models;

namespace AudioSplitter.ViewModels;

public partial class MainWindowViewModelDesignTime : MainWindowViewModel
{
    public MainWindowViewModelDesignTime() : base(null, null)
    {
        Enumerable.Range(1, 4).Select(i => new AudioFileChunkDisplayItem()
        {
            AlbumName = $"Test album name",
            ArtistName = $"Test artist name",
            TrackName = $"DESIGN TIME NAME {i}",
            TimeStart = TimeSpan.FromSeconds(0 + (i - 1) * 60),
            TimeEnd = TimeSpan.FromSeconds(60 + (i - 1) * 60),
            TrackNumber = i,
            LengthInBytes = 1_000_000 * i
        }).ToList().ForEach(item =>
        {
            this.ChunkItems.Add(item);
        });
    }
}
