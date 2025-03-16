using FFMpegCore;

namespace AudioSplitter.BL;

public interface IAudioFileAnalyser
{
    TimeSpan GetFileTotalDuration(string fileName);
}

public class FFProbeAudioFileAnalyser : IAudioFileAnalyser
{
    public TimeSpan GetFileTotalDuration(string fileName)
    {
        try
        {
            var data = FFProbe.Analyse(fileName);
            return data.Duration;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }
}
