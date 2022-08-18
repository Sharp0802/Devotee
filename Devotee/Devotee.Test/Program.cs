using System.Text;
using Devotee.Core.Primitives;
using Devotee.Provider.JMana;
using IQueryProvider = Devotee.Core.Interfaces.IQueryProvider;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("Hello, World!");



IQueryProvider queryProvider = new JManaQueryProvider();



var queryResult = await queryProvider.QueryAsync(new MangaQuery(
    "귀멸의 칼날",
    null,
    null,
    Enumerable.Empty<string>(),
    SortBy.Latest));
Console.WriteLine("queried search");

var headers = (await queryResult.GetPageAt(1)).ToArray();
Console.WriteLine("queried search page 1");

foreach (var header in headers)
{
    Console.WriteLine(header.ToConsoleString());
    Console.WriteLine();
}

var episodes = (await queryProvider.QueryEpisodesAsync(headers[0])).ToArray();
Console.WriteLine("queried episode");

var imageProviders = (await queryProvider.QueryImagesAsync(episodes[0])).ToArray();
Console.WriteLine("queried images");

foreach (var imageProvider in imageProviders)
{
    Console.WriteLine(imageProvider.Hyperlink);
    await imageProvider.CopyToAsync(Stream.Null);
}
    
Console.WriteLine("Goodbye, World!");    