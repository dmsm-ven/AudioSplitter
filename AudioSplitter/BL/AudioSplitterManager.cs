namespace AudioSplitter.BL;

using AudioSplitter.Models;
using System.IO;
using System.Windows;

public class AudioSplitterManager
{
    private readonly IAudioSplitter audioSplitter;

    public AudioSplitterManager(IAudioSplitter audioSplitter)
    {
        this.audioSplitter = audioSplitter;
    }

    public async Task<AudioFileChunk[]> SplitFile(string sourceFileName, IList<AudioFileChunkDisplayItem> items)
    {
        bool hasErrors = false;
        List<AudioFileChunk> convertedChunks = new();

        int i = 0;
        foreach (var item in items)
        {
            TimeSpan startTime = i == 0 ? TimeSpan.Zero : items[i - 1].TimeEnd;
            item.InProgress = true;
            try
            {
                var data = await audioSplitter.GetChunk(sourceFileName, item.TrackNumber, item.TrackName, startTime, item.TimeEnd);
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
}
