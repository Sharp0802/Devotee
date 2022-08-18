using System.Text;
using System.Web;
using AngleSharp.Html.Parser;
using Devotee.Core.Interfaces;
using Devotee.Core.Primitives;
using Devotee.Core.Web;

namespace Devotee.Provider.JMana;

public class JManaMangaHeaderPager : IPager<MangaHeader>
{
    public JManaMangaHeaderPager(string baseLink)
    {
        BaseLink = baseLink;
    }

    private string BaseLink { get; }
    
    public async Task<IEnumerable<MangaHeader>> GetPageAt(int page)
    {
        var link = $"{BaseLink}&page={page - 1}";

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
                {"upgrade-insecure-requests", "1"}
            }
        };

        using var response = (await Request.Client.SendAsync(request)).EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        using var document = await new HtmlParser(new HtmlParserOptions
        {
            IsScripting = false,
            IsStrictMode = true
        }).ParseDocumentAsync(html);

        var elements = document.QuerySelectorAll("body > div.container > div.wrap.left > div.content > div.search-result-wrap > div.img-lst-wrap.col6.stl1 > ul > li");

        return (from element in elements
            let thumbnailElement = element.QuerySelector("img.main_img")!
            let id = HttpUtility.UrlDecode(thumbnailElement.GetAttribute("data-id"), Encoding.UTF8)
            let title = element.QuerySelector("a.tit")!.TextContent
            let author = element.QuerySelector("p.p_author")!.TextContent
            let cycle = element.ClassName switch
            {
                "weekly" => PublishCycle.Weekly,
                "weekly2" => PublishCycle.Biweekly,
                "monthly" => PublishCycle.Monthly,
                "one" => PublishCycle.Paperback,
                "dan" => PublishCycle.Snippet,
                "finish" => PublishCycle.Completed,
                "etc" or _ => PublishCycle.Miscellaneous
            }
            let genre = element.QuerySelector("a.genre")!.TextContent.Split(',')
            let thumbnail = new JManaImageProvider(Config.SiteBaseUrl + element.QuerySelector("img.main_img")!.GetAttribute("src"))
            select new MangaHeader(id, title, author, cycle, genre, thumbnail)).ToList();
    }
}