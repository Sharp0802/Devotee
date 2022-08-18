using System.Net;

namespace Devotee.Core.Web;

public static class Request
{
    private static Lazy<HttpClient> ClientInitializer { get; } = new(() =>
    {
        var handler = new StandardSocketsHttpHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = (DecompressionMethods) Enum.ToObject(typeof(DecompressionMethods), -1)
        };
        var client = new HttpClient(handler, true);

        AppDomain.CurrentDomain.DomainUnload += (_, _) =>
        {
            client.Dispose();
        };

        return client;
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    public static HttpClient Client => ClientInitializer.Value;

    public const string AcceptEncoding = "gzip, deflate, br";
    public const string AcceptLanguage = "en-GB,en;q=0.9,en-US;q=0.8,ko;q=0.7";
    public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.134 Safari/537.36 Edg/103.0.1264.71";
    public const string DoNotTrack = "1";
}