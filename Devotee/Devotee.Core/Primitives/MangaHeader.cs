using Devotee.Core.Interfaces;

namespace Devotee.Core.Primitives;

public class MangaHeader
{
    public MangaHeader(
        string identifier,
        string title, 
        string author, 
        PublishCycle publishCycle, 
        IEnumerable<string> genres, 
        IImageProvider thumbnail)
    {
        Identifier = identifier;
        Title = title;
        Author = author;
        PublishCycle = publishCycle;
        Genres = genres;
        Thumbnail = thumbnail;
    }
    
    public string Identifier { get; }
    public string Title { get; }
    public string Author { get; }
    public PublishCycle PublishCycle { get; }
    public IEnumerable<string> Genres { get; }
    public IImageProvider Thumbnail { get; }

    public override string ToString()
    {
        return $"Title        : {Title}\n" +
               $"Identifier   : {Identifier}\n" +
               $"Author       : {Author}\n" +
               $"Genres       : {string.Join(", ", Genres)}\n" +
               $"PublishCycle : {PublishCycle}\n" +
               $"Thumbnail    : {Thumbnail.Hyperlink}";
    }
    
    public string ToConsoleString()
    {
        return $"Title        : {Title}\n" +
               $"Identifier   : {Identifier}\n" +
               $"Author       : {Author}\n" +
               $"Genres       : {string.Join(", ", Genres)}\n" +
               $"PublishCycle : {PublishCycle}\n";
    }
}