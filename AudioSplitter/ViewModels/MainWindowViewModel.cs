namespace AudioSplitter.ViewModels;

using AudioSplitter.BL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;

public partial class MainWindowViewModelDesignTime : MainWindowViewModel
{
    public MainWindowViewModelDesignTime() : base(null, null)
    {

    }
}

public partial class MainWindowViewModel : ObservableObject
{
    private const string TAG_DEFAULT_VALUE = "<Не выбрано>";

    private readonly IAudioSplitter audioSplitter;
    private readonly IAduioTagWriter tagWriter;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateChunksCommand))]
    public string selectedSourceFile = @"C:\users\user\Desktop\123.m4a";

    [ObservableProperty]
    public string title = "Audio Splitter";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateChunksCommand))]
    public int chunksNumber = 8;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateChunksCommand))]
    public string tagAuthorName = TAG_DEFAULT_VALUE;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateChunksCommand))]
    public string tagAlbumName = TAG_DEFAULT_VALUE;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateChunksCommand))]
    public int tagYearOfRelease = DateTime.Now.Year;

    public MainWindowViewModel(IAudioSplitter audioSplitter, IAduioTagWriter tagWriter)
    {
        this.audioSplitter = audioSplitter;
        this.tagWriter = tagWriter;
    }

    [RelayCommand]
    public void SelectSourceFile()
    {
        var ofd = new OpenFileDialog()
        {
            Title = "Выберите файл который нужно разрезать на части",
            Filter = "M4A|*.m4a|MP3|*.mp3"
        };
        if (ofd.ShowDialog() == true)
        {
            SelectedSourceFile = ofd.FileName;
        }
    }

    [RelayCommand(CanExecute = nameof(CreateChunksCanExecute))]
    public async Task CreateChunks()
    {
        MessageBox.Show("OK");
    }

    public bool CreateChunksCanExecute()
    {
        if (TagAuthorName == TAG_DEFAULT_VALUE)
        {
            return false;
        }
        if (TagAlbumName == TAG_DEFAULT_VALUE)
        {
            return false;
        }
        if (TagYearOfRelease == 0)
        {
            return false;
        }
        if (ChunksNumber <= 1)
        {
            return false;
        }
        if (!File.Exists(SelectedSourceFile))
        {
            return false;
        }

        return true;
    }
}
