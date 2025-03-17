namespace AudioSplitter.BL;

using AudioSplitter.Interfaces;
using AudioSplitter.Models;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

public class AudioSplitterManager
{
    private readonly IAudioSplitter audioSplitter;
    private readonly IAudioFileAnalyser analyzer;

    public AudioSplitterManager(IAudioSplitter audioSplitter, IAudioFileAnalyser analyzer)
    {
        this.audioSplitter = audioSplitter;
        this.analyzer = analyzer;
    }

    public async Task<AudioFileChunk[]> SplitFile(string sourceFileName, string artist, string album, IList<AudioFileChunkDisplayItem> items)
    {
        bool hasErrors = false;
        List<AudioFileChunk> convertedChunks = new();

        audioSplitter.Initialize(sourceFileName, artist, album);

        int i = 0;
        foreach (var item in items)
        {
            TimeSpan startTime = i == 0 ? TimeSpan.Zero : items[i - 1].TimeEnd;
            item.InProgress = true;


            try
            {
                var data = await audioSplitter.GetChunk(item.TrackNumber, item.TrackName, startTime, item.TimeEnd);
                convertedChunks.Add(data);
                item.LengthInBytes = data.FileInfo.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обрезки файла: {ex.Message}. \r\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                hasErrors = true;
                break;
            }
            finally
            {
                item.InProgress = false;
            }
            i++;
        }

        if (hasErrors)
        {
            foreach (var file in convertedChunks)
            {
                File.Delete(file.FileInfo.FullName);
            }
            return Enumerable.Empty<AudioFileChunk>().ToArray();
        }

        return convertedChunks.ToArray();
    }

    public void FillTimeFromClipboard(string clipboardText, string sourceFile, IList<AudioFileChunkDisplayItem> items)
    {
        //Добавляем часы для парсинга TimeSpan 13:45 => 00:13:45

        clipboardText = Regex.Replace(clipboardText, @"(\d{2}):(\d{2})", "00:$1:$2");

        var rawData = clipboardText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split('\t'))
            .ToArray();

        int timeDataIndex = TimeSpan.TryParse(rawData[0][0], out _) ? 0 :
                            TimeSpan.TryParse(rawData[0][1], out _) ? 1 :
                            throw new ArgumentOutOfRangeException("В строке нет данных о длительности");
        int nameDataIndex = (timeDataIndex + 1) % 2;

        var data = rawData
                .Select(line => new BufferParseItem(line[nameDataIndex].Trim(), TimeSpan.Parse(line[timeDataIndex])))
                .ToArray();

        if (data.Length != items.Count)
        {
            MessageBox.Show("Количество элементов не совпадает");
            return;
        }

        TimeParseMode mode = GetParseMode(data);

        int i = 0;
        foreach (var item in items)
        {
            item.TrackName = data[i].TrackName;
            if (mode == TimeParseMode.Duration)
            {
                item.Duration = data[i].Time;
            }
            else if (mode == TimeParseMode.TimeStart)
            {
                item.TimeStart = data[i].Time;
                if (i > 0)
                {
                    items[i - 1].TimeEnd = item.TimeStart;
                }
                if (i == data.Length - 1)
                {
                    TimeSpan fullDuration = analyzer.GetFileTotalDuration(sourceFile);
                    item.TimeEnd = fullDuration;
                }
            }
            else if (mode == TimeParseMode.TimeEnd)
            {
                item.TimeEnd = data[i].Time;
            }
            i++;
        }
    }

    private TimeParseMode GetParseMode(BufferParseItem[] data)
    {
        if (data == null || data.Length <= 1)
        {
            throw new ArgumentOutOfRangeException("Необходимо 2 или более треков");
        }

        TimeParseMode mode = TimeParseMode.None;
        BufferParseItem firstItem = data[0];
        BufferParseItem[] sourceAllExceptLast = data.Take(data.Length - 1).ToArray();

        bool isIncresing = false;
        for (int i = 0; i < sourceAllExceptLast.Length; i++)
        {
            isIncresing = data[i].Time < data[i + 1].Time;
        }

        if (firstItem.Time == TimeSpan.Zero && isIncresing)
        {
            mode = TimeParseMode.TimeStart;
        }
        else if (firstItem.Time != TimeSpan.Zero && isIncresing)
        {
            mode = TimeParseMode.TimeStart;
        }
        else if (data.All(d => d.Time != TimeSpan.Zero))
        {
            mode = TimeParseMode.Duration;
        }

        if (mode == TimeParseMode.None)
        {
            throw new Exception("Ошибка парсинга времени данных");
        }

        return mode;
    }
}
