using System.Globalization;
using AngleSharp.Html.Parser;
using Devotee.Core.Interfaces;
using Devotee.Core.Primitives;
using Devotee.Core.Web;
using IQueryProvider = Devotee.Core.Interfaces.IQueryProvider;

namespace Devotee.Provider.JMana;

public class JManaQueryProvider : IQueryProvider
{
    public string SiteIdentifier => Config.SiteIdentifier;

    public Task<IPager<MangaHeader>> QueryAsync(MangaQuery query)
    {
        return Task.Factory.StartNew(() =>
        {
            var title = query.Title ?? string.Empty;
            var author = query.Author ?? string.Empty;
            var cycle = query.PublishCycle switch
            {
                PublishCycle.Weekly        => "주간",
                PublishCycle.Biweekly      => "격주",
                PublishCycle.Monthly       => "월간",
                PublishCycle.Paperback     => "단행본",
                PublishCycle.Snippet       => "단편",
                PublishCycle.Miscellaneous => "기타",
                PublishCycle.Completed     => "완결",
                _                          => string.Empty
            };
            var genre = string.Join("::", query.Genres);
            var sortBy = query.SortBy switch
            {
                SortBy.Latest   => "최신순",
                SortBy.Rating   => "추천",
                SortBy.Bookmark => "북마크",
                _               => string.Empty
            };

            return (IPager<MangaHeader>)new JManaMangaHeaderPager(
                $"{Config.SiteBaseUrl}/comic_list_search?keyword={title}&author={author}&gubun={cycle}&tag={genre}&ordering={sortBy}");
        });
    }

    public async Task<IEnumerable<EpisodeHeader>> QueryEpisodesAsync(MangaHeader header)
    {
        var link = $"{Config.SiteBaseUrl}/comic_list_title?bookname={header.Identifier}";

        using var request = new HttpRequestMessage(HttpMethod.Get, link)
        {
            Headers =
            {
                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                { "accept-encoding", Request.AcceptEncoding },
                { "accept-language", Request.AcceptLanguage },
                { "user-agent", Request.UserAgent },
                { "dnt", Request.DoNotTrack },
                { "referer", Config.SiteBaseUrl },
                { "upgrade-insecure-requests", "1" }
            }
        };

        using var response = (await Request.Client.SendAsync(request)).EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        using var document = await new HtmlParser(new HtmlParserOptions
        {
            IsScripting = false,
            IsStrictMode = true
        }).ParseDocumentAsync(html);

        var list = new List<EpisodeHeader>();
        foreach (var element in document.QuerySelectorAll(
                     "body > div.container > div.wrap.left > div.content > div > div.lst-wrap.stl5 > ul > li"))
        {
            var title = element.QuerySelector("a.tit").TextContent;
            var id = element.QuerySelector("a.tit").GetAttribute("id");
            var dateStr = element.QuerySelector("p.date").TextContent;
            if (!DateTime.TryParseExact(dateStr, "yy-dd-MM", null, DateTimeStyles.None, out var date))
                date = DateTime.MinValue;
            list.Add(new EpisodeHeader(id, title, header, date));
        }

        return list;
    }

    public async Task<IEnumerable<IImageProvider>> QueryImagesAsync(EpisodeHeader header)
    {
        var link = $"{Config.SiteBaseUrl}/bookdetail?bookdetailid={header.Identifier}";

        using var request = new HttpRequestMessage(HttpMethod.Get, link)
        {
            Headers =
            {
                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                { "accept-encoding", Request.AcceptEncoding },
                { "accept-language", Request.AcceptLanguage },
                { "user-agent", Request.UserAgent },
                { "dnt", Request.DoNotTrack },
                { "referer", Config.SiteBaseUrl },
                { "upgrade-insecure-requests", "1" }
            }
        };

        using var response = (await Request.Client.SendAsync(request)).EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        using var document = await new HtmlParser(new HtmlParserOptions
        {
            IsScripting = false,
            IsStrictMode = true
        }).ParseDocumentAsync(html);

        return document
               .QuerySelectorAll("div.pdf-wrap > img.comicdetail")
               .Select(image => image.GetAttribute("data-src") ?? image.GetAttribute("src"))
               .Select(src => new JManaImageProvider(src));
    }
}