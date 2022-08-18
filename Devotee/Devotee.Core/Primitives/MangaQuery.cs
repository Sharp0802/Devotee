namespace Devotee.Core.Primitives;

public class MangaQuery
{
    public MangaQuery(
        string? title, 
        string? author, 
        PublishCycle? publishCycle, 
        IEnumerable<string> genres, 
        SortBy sortBy)
    {
        Title = title;
        Author = author;
        PublishCycle = publishCycle;
        Genres = genres;
        SortBy = sortBy;
    }

    public string? Title { get; }
    public string? Author { get; }
    public PublishCycle? PublishCycle { get; }
    public IEnumerable<string> Genres { get; }
    public SortBy SortBy { get; }

    public override string ToString()
    {
        return 
            $"Title        : {Title ?? "<null>"}\n" +
            $"Author       : {Author ?? "<null>"}\n" +
            $"PublishCycle : {PublishCycle?.ToString() ?? "<null>"}\n" +
            $"Genres       : {string.Join(", ", Genres)}\n" +
            $"SortBy       : {SortBy}";
    }
}