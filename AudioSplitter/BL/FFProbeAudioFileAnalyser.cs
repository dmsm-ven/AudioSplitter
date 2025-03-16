using AudioSplitter.Interfaces;
using FFMpegCore;

namespace AudioSplitter.BL;

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
