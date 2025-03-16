using AudioSplitter.Interfaces;
using AudioSplitter.Models;
using FFMpegCore;
using FFMpegCore.Enums;
using System.IO;

namespace AudioSplitter.BL;

public class FfmpegAudioSplitter : IAudioSplitter
{
    public string SourceFilePath { get; private set; }
    public string Artist { get; private set; }
    public string Album { get; private set; }

    private bool isInitialized = false;

    public async Task<AudioFileChunk> GetChunk(int trackNumber,
        string chunkName,
        TimeSpan start,
        TimeSpan end)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("Необходимо инициализировать с начальными данными");
        }

        string extension = Path.GetExtension(SourceFilePath);
        string outputFolder = Path.Combine(Path.GetDirectoryName(SourceFilePath), this.Artist, this.Album);
        string outputFile = Path.Combine(outputFolder, chunkName + extension);

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        await FFMpegArguments
            .FromFileInput(SourceFilePath)
            .OutputToFile(outputFile, false, options =>
            {
                options.WithAudioCodec(AudioCodec.Copy);
                options.Seek(start);
                options.WithDuration(end - start);
            })
            .ProcessAsynchronously();

        var chunkInfo = new AudioFileChunk()
        {
            ChunkFileName = outputFile,
            FileInfo = new FileInfo(outputFile),
            ParentFileName = SourceFilePath,
            TimeStart = start,
            TrackNumber = trackNumber,
            TimeEnd = end
        };

        return chunkInfo;
    }

    public void Initialize(string sourceFile, string artist, string album)
    {
        this.SourceFilePath = sourceFile;
        this.Artist = artist;
        this.Album = album;

        isInitialized = true;
    }
}
