namespace AudioSplitter.Interfaces;

public interface IAudioFileAnalyser
{
    TimeSpan GetFileTotalDuration(string fileName);
}
