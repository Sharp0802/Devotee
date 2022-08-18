namespace Devotee.Core.Primitives;

public class EpisodeHeader
{
    public EpisodeHeader(string identifier, string title, MangaHeader manga, DateTime uploadDate)
    {
        Identifier = identifier;
        Title = title;
        Manga = manga;
        UploadDate = uploadDate;
    }

    public string Identifier { get; }
    public string Title { get; }
    public MangaHeader Manga { get; }
    public DateTime UploadDate { get; }

    public override string ToString()
    {
        return $"Title      : {Title}\n" +
               $"Identifier : {Identifier}\n" +
               $"UploadDate : {UploadDate}\n" +
               $"Manga      : {Manga.Title}";
    }

    public string ToConsoleString()
    {
        return $"Title      : {Title}\n" +
               $"Identifier : {Identifier}\n" +
               $"UploadDate : {UploadDate}";
    }
}