namespace AudioSplitter.ViewModels;

using AudioSplitter.BL;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class MainWindowViewModelDesignTime : MainWindowViewModel
{
    public MainWindowViewModelDesignTime() : base(null, null)
    {

    }
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAudioSplitter audioSplitter;
    private readonly IAduioTagWriter tagWriter;

    [ObservableProperty]
    public string title = "Audio Splitter (VM2)";

    public MainWindowViewModel(IAudioSplitter audioSplitter, IAduioTagWriter tagWriter)
    {
        this.audioSplitter = audioSplitter;
        this.tagWriter = tagWriter;
    }
}
