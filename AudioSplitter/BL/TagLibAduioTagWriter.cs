using AudioSplitter.Interfaces;
using AudioSplitter.ViewModels;
using TagLib;

namespace AudioSplitter.BL;

public class TagLibAduioTagWriter : IAduioTagWriter
{
    public void SetTag(string fileName, string tagName, string value)
    {
        var tfile = File.Create(fileName);

        if (tagName == nameof(MainWindowViewModel.TagAuthorName))
        {
            tfile.Tag.Artists = new string[] { value };
            tfile.Tag.AlbumArtists = new string[] { value };
        }
        if (tagName == nameof(MainWindowViewModel.TagAlbumName))
        {
            tfile.Tag.Album = value;
        }
        if (tagName == nameof(MainWindowViewModel.TagYearOfRelease))
        {
            tfile.Tag.Year = uint.Parse(value);
        }
        if (tagName == "TrackNumber")
        {
            tfile.Tag.Track = uint.Parse(value);
        }
        if (tagName == "TrackName")
        {
            tfile.Tag.Title = value;
        }

        tfile.Save();
    }

    public void SetTags(string fileName, IReadOnlyDictionary<string, string> tags)
    {
        foreach (var tag in tags)
        {
            SetTag(fileName, tag.Key, tag.Value);
        }
    }
}
