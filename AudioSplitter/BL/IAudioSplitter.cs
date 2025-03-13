using AudioSplitter.Models;
using FFMpegCore;
using FFMpegCore.Enums;
using System.IO;

namespace AudioSplitter.BL;

public class FfmpegAudioSplitter : IAudioSplitter
{
    public async Task<AudioFileChunk> GetChunk(string sourceFile, string chunkName, TimeSpan start, TimeSpan end)
    {
        string extension = Path.GetExtension(sourceFile);
        string outputFolder = Path.GetDirectoryName(sourceFile);
        string outputFile = Path.Combine(outputFolder, chunkName + extension);

        await FFMpegArguments
            .FromFileInput(sourceFile)
            .OutputToFile(outputFile, false, options =>
            {
                options.WithAudioCodec(AudioCodec.Copy);
                options.Seek(start);
                options.WithDuration(end - start);
            })
            .ProcessAsynchronously();

        await Task.Delay(TimeSpan.FromSeconds(3));

        var chunkInfo = new AudioFileChunk()
        {
            ChunkFileName = outputFile,
            FileInfo = new FileInfo(outputFile),
            ParentFileName = sourceFile,
            TimeStart = start,
            TimeEnd = end
        };

        return chunkInfo;
    }
}

public interface IAudioSplitter
{
    Task<AudioFileChunk> GetChunk(string sourceFile, string chunkName, TimeSpan start, TimeSpan end);
}
