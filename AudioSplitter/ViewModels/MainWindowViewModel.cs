namespace AudioSplitter.ViewModels;

using AudioSplitter.BL;
using AudioSplitter.Interfaces;
using AudioSplitter.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    private const string TAG_DEFAULT_VALUE = "<Не выбрано>";
    private readonly AudioSplitterManager splitManager;
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

    [ObservableProperty]
    public ObservableCollection<AudioFileChunkDisplayItem> chunkItems = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearChunkItemsCommand))]
    [NotifyCanExecuteChangedFor(nameof(FillChunksFromClipboardCommand))]
    [NotifyCanExecuteChangedFor(nameof(UploadAllFilesCommand))]
    public bool hasChunkItems = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadAllFilesCommand))]
    public bool canUploadAllFiles = false;

    public MainWindowViewModel(AudioSplitterManager splitManager, IAduioTagWriter tagWriter)
    {
        this.splitManager = splitManager;
        this.tagWriter = tagWriter;
        chunkItems.CollectionChanged += (o, e) => HasChunkItems = ChunkItems.Count > 0;
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
        this.ChunkItems.Clear();

        Enumerable.Range(1, this.ChunksNumber).Select(i => new AudioFileChunkDisplayItem()
        {
            AlbumName = this.TagAlbumName,
            ArtistName = this.TagAuthorName,
            TrackNumber = i,
            TrackName = "<Без имени>"
        }).ToList().ForEach(i =>
        {
            ChunkItems.Add(i);
        });

        var lastChunk = ChunkItems.Last();
        for (int i = 0; i < ChunkItems.Count; i++)
        {
            var chunk = ChunkItems[i];
            var nextChunk = chunk != lastChunk ? ChunkItems[i + 1] : null;
            var prevChunk = i != 0 ? ChunkItems[i - 1] : null;
            chunk.PropertyChanged += (o, e) =>
            {
                CanUploadAllFiles = HasChunkItems && ChunkItems.All(i => i.Duration != TimeSpan.Zero && i.TimeEnd != TimeSpan.Zero);
                if (e.PropertyName == nameof(AudioFileChunkDisplayItem.TimeEnd) && nextChunk != null)
                {
                    nextChunk.TimeStart = chunk.TimeEnd;
                }
            };
        }
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

    [RelayCommand(CanExecute = nameof(CanUploadAllFiles))]
    public async Task UploadAllFiles()
    {
        var sw = Stopwatch.StartNew();
        var result = await splitManager.SplitFile(SelectedSourceFile, TagAuthorName, TagAlbumName, ChunkItems);
        if (result.Any())
        {
            var tagsData = new Dictionary<string, string>()
            {
                [nameof(TagAlbumName)] = TagAlbumName,
                [nameof(TagAuthorName)] = TagAuthorName,
                [nameof(TagYearOfRelease)] = TagYearOfRelease.ToString(),
            };

            foreach (var item in result)
            {
                tagsData["TrackNumber"] = item.TrackNumber.ToString();
                tagsData["TrackName"] = Path.GetFileNameWithoutExtension(item.ChunkFileName);
                this.tagWriter.SetTags(item.FileInfo.FullName, tagsData);
            }
            MessageBox.Show($"Конвертирование выполнено за {sw.Elapsed.Humanize()}", "Выполнено", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand(CanExecute = nameof(HasChunkItems))]
    public void FillChunksFromClipboard()
    {
        try
        {
            splitManager.FillTimeFromClipboard(Clipboard.GetText(), SelectedSourceFile, ChunkItems);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка парсинга данных: {ex.Message}\r\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HasChunkItems))]
    public void ClearChunkItems()
    {
        ChunkItems.Clear();
    }
}
