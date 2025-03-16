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
    private const string TAG_DEFAULT_VALUE = "<Не заполнять>";
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
    public string tagYearOfRelease = DateTime.Now.Year.ToString();

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

    /// <summary>
    /// Выбор файла который нужно разбить на части
    /// </summary>
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

    /// <summary>
    /// Создать элементы-шаблоны для дальнейшего заполнения. Количество = <c>ChunksNumber</c>
    /// </summary>
    /// <returns></returns>
    [RelayCommand(CanExecute = nameof(CanCreateChunks))]
    public async Task CreateChunks(int chunksNumber)
    {
        this.ChunkItems.Clear();

        Enumerable.Range(1, chunksNumber).Select(i => new AudioFileChunkDisplayItem()
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

    public bool CanCreateChunks()
    {
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

    /// <summary>
    /// Выгрузить файлы по созданному шаблону и заполнить аудио-теги
    /// </summary>
    /// <returns></returns>
    [RelayCommand(CanExecute = nameof(CanUploadAllFiles))]
    public async Task UploadAllFiles()
    {
        // Создаем файлы
        var sw = Stopwatch.StartNew();

        var result = await splitManager.SplitFile(SelectedSourceFile, TagAuthorName, TagAlbumName, ChunkItems);

        //Заполняем Аудио-теги файлов
        if (result.Any())
        {
            var tagsData = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(TagAlbumName) && TagAlbumName != TAG_DEFAULT_VALUE)
            {
                tagsData[nameof(TagAlbumName)] = TagAlbumName;
            }
            if (!string.IsNullOrWhiteSpace(TagAuthorName) && TagAuthorName != TAG_DEFAULT_VALUE)
            {
                tagsData[nameof(TagAuthorName)] = TagAuthorName;
            }
            if (!string.IsNullOrWhiteSpace(TagYearOfRelease) && uint.TryParse(TagYearOfRelease, out var year))
            {
                tagsData[nameof(TagYearOfRelease)] = year.ToString();
            }
            foreach (var item in result)
            {
                tagsData["TrackNumber"] = item.TrackNumber.ToString();
                tagsData["TrackName"] = Path.GetFileNameWithoutExtension(item.ChunkFileName);
                this.tagWriter.SetTags(item.FileInfo.FullName, tagsData);
            }

            MessageBox.Show($"Конвертирование выполнено за {sw.Elapsed.Humanize()}", "Выполнено", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Загрузить данные по названи треков и длительности из буфера обмена. Разделитель знак табуляции. 
    /// Автоматическое определение что временной отметке в зависимости от данных. Варианты: <br/>
    /// 1) Начало каждого файла: если первый элемент начинается с 00:00:00 и каждый последующий больше предыдущего  <br/>
    /// 2) Конец каждого файла: если тоже самое что #1, но первый элемент не равен 00:00:00 <br />
    /// 3) Длительность каждого файла: если все елементы не равны 00:00:00
    /// </summary>
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

    /// <summary>
    /// Очистить элементы-шаблоны
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasChunkItems))]
    public void ClearChunkItems()
    {
        ChunkItems.Clear();
    }
}
