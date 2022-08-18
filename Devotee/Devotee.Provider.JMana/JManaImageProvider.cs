using Devotee.Core.Interfaces;
using Devotee.Core.Web;

namespace Devotee.Provider.JMana;

public class JManaImageProvider : IImageProvider
{
    public JManaImageProvider(string hyperlink)
    {
        Hyperlink = hyperlink;
    }

    public string SiteIdentifier => Config.SiteIdentifier;
    
    public string Hyperlink { get; }
    
    public async Task CopyToAsync(Stream output, string? referer = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Hyperlink)
        {
            Headers =
            {
                { "accept", "image/*,*/*;q=0.8" },
                { "accept-encoding", Request.AcceptEncoding },
                { "accept-language", Request.AcceptLanguage },
                { "user-agent", Request.UserAgent },
                { "dnt", Request.DoNotTrack },
                { "referer", $"{Config.SiteBaseUrl}{referer}" }
            }
        };

        using var response = (await Request.Client.SendAsync(request)).EnsureSuccessStatusCode();

        await (await response.Content.ReadAsStreamAsync()).CopyToAsync(output);
    }
}