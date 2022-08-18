using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Devotee.Core.Primitives;
using Devotee.Helper;
using IQueryProvider = Devotee.Core.Interfaces.IQueryProvider;

namespace Devotee.UI.Console;

using Console = System.Console;

public class ProgramContext
{
    private static Dictionary<string, IQueryProvider> AllQueryProviders { get; } =
        QueryProvider.QueryProviders.ToDictionary(qp => qp.SiteIdentifier);

    private IQueryProvider? CurrentQueryProvider { get; set; }

    [MemberNotNull(nameof(CurrentQueryProvider))]
    private void EnsureQueryProvider()
    {
        if (CurrentQueryProvider is not null) return;
        throw new InvalidOperationException("Query provider must be set.");
    }
    
    private void ListProviders()
    {
        foreach (var identifier in AllQueryProviders.Keys.OrderBy(key => key))
            Console.WriteLine($"{(CurrentQueryProvider?.SiteIdentifier == identifier ? '*' : ' ')} {identifier}");
        if (AllQueryProviders.Keys.Count == 0)
            Console.WriteLine("No query provider has detected. Please contacts to developer.");
    }

    private void SetProvider(string? providerIdentifier)
    {
        if (providerIdentifier is null)
        {
            Console.WriteLine("identifier cannot be null.");
            return;
        }
        
        if (AllQueryProviders.ContainsKey(providerIdentifier))
        {
            CurrentQueryProvider = AllQueryProviders[providerIdentifier];
            Console.WriteLine($"Query provider is set to \"{providerIdentifier}\".");
        }
        else
        {
            Console.WriteLine(
                $"There is no query provider matched with the identifier: \"{providerIdentifier}\".\nNothing has changed.");
        }
    }

    private void CurrentProvider()
    {
        Console.WriteLine(CurrentQueryProvider?.SiteIdentifier ?? "Value not set.");
    }

    private async Task SearchManga(string? title, string? author, PublishCycle? publishCycle, IEnumerable<string> genres, SortBy sortBy)
    {
        EnsureQueryProvider();
        
        var query = new MangaQuery(title, author, publishCycle, genres, sortBy);

        var pager = await CurrentQueryProvider.QueryAsync(query);

        var exitRequested = false;
        var index = 0;
        while (!exitRequested)
        {
            if (index <= 0)
                index = 0;
            
            Console.WriteLine($"Page {index + 1}");
            Console.WriteLine();

            var mangas = (await pager.GetPageAt(index + 1)).ToArray();
            for (var i = 0; i < mangas.Length; i += 2)
            {
                ConsoleHelper.CombinePrint(
                    mangas[i].ToConsoleString(),
                    mangas.ElementAtOrDefault(i + 1)?.ToConsoleString() ?? string.Empty);
                Console.WriteLine();
            }

            if (mangas.Length == 0)
            {
                Console.WriteLine("There is no manga.");
                Console.WriteLine();
            }

            Console.WriteLine("1) Press right/left arrow key to go to next/prev page");
            Console.WriteLine("2) Press 'q' key to exit");
            Console.WriteLine();
            
            ReceiveAction:
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    index--;
                    break;
                case ConsoleKey.RightArrow:
                    index++;
                    break;
                case ConsoleKey.Q:
                    exitRequested = true;
                    break;
                default:
                    goto ReceiveAction;
            }
        }
    }

    private async Task QueryEpisodes(string? id)
    {
        EnsureQueryProvider();

        if (id is null)
        {
            Console.WriteLine("Identifier cannot be null or empty.");
            return;
        }
        
        var episodes = (await CurrentQueryProvider.QueryEpisodesAsync(new MangaHeader(
            id,
            null!,
            null!,
            (PublishCycle) Enum.ToObject(typeof(PublishCycle), -1),
            Enumerable.Empty<string>(),
            null!)))
                       .Reverse()
                       .ToArray();

        for (var i = 0; i < episodes.Length; i += 2)
        {
            ConsoleHelper.CombinePrint(
                episodes[i].ToConsoleString(),
                episodes.ElementAtOrDefault(i + 1)?.ToConsoleString() ?? string.Empty);
            Console.WriteLine();
        }

        if (episodes.Length == 0)
        {
            Console.WriteLine("There is no episodes.");
            Console.WriteLine();
        }
    }

    private async Task DownloadEpisode(string? id, DirectoryInfo? outputPath)
    {
        EnsureQueryProvider();
        
        if (id is null)
        {
            Console.WriteLine("Identifier cannot be null or empty");
            return;
        }

        if (outputPath is null)
        {
            var pre = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Output path is null. Images will not be downloaded.");
            Console.ForegroundColor = pre;
        }

        var images = (await CurrentQueryProvider.QueryImagesAsync(new EpisodeHeader(
            id, 
            null!, 
            null!, 
            DateTime.MinValue)))
            .ToArray();

        for (var i = 0; i < images.Length; ++i)
        {
            Console.WriteLine($"{i + 1,4}:{images[i].Hyperlink}");
        }

        if (outputPath is not null)
        {
            if (!outputPath.Exists)
                outputPath.Create();
            for (var i = 0; i < images.Length; ++i)
            {
                var path = Path.Combine(outputPath.FullName, $"{id}_{i}.jpg");
                await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                await images[i].CopyToAsync(stream);
            }
        }
    }

    public async Task Main()
    {
        var exitRequested = false;
        while (!exitRequested)
        {
            Console.Write("> ");

            var command = Console.ReadLine() ?? string.Empty;

            var root = new RootCommand("Devotee : a universal manga query provider");

            {
                var idOption = new Option<string>(
                    new[] { "--identifier", "--id" },
                    "The identifier of specific episode to query its images.");
                var outputPathOption = new Option<DirectoryInfo>(
                    new[] { "--output-path", "-o" },
                    "The output path to save images of specific episode");
                var downloadCommand = new Command(
                    "download",
                    "Downloads images of specific episode with its identifier");
                downloadCommand.AddOption(idOption);
                downloadCommand.AddOption(outputPathOption);
                downloadCommand.SetHandler(DownloadEpisode, idOption, outputPathOption);
                root.AddCommand(downloadCommand);
            }
            
            {
                var idOption = new Option<string>(
                    new[] { "--identifier", "--id" },
                    "The identifier of specific manga to query its episodes.");
                var queryCommand = new Command(
                    "query",
                    "Queries episodes of specific manga with its identifier.");
                queryCommand.AddOption(idOption);
                queryCommand.SetHandler(QueryEpisodes, idOption);
                root.AddCommand(queryCommand);
            }
            
            {
                var titleOption = new Option<string>(
                    new[] { "--title", "-t" },
                    "The title of manga to search.");
                var authorOption = new Option<string>(
                    new[] { "--author", "-a" },
                    "The author of manga to search.");
                var publishCycleOption = new Option<PublishCycle?>(
                    new[] { "--publish-cycle", "-p" },
                    "The publish cycle of manga to search.");
                var genresOption = new Option<IEnumerable<string>>(
                    new[] { "--genre", "-g" },
                    "The genres of manga to search.")
                {
                    AllowMultipleArgumentsPerToken = true
                };
                var sortByOption = new Option<SortBy>(
                    new[] { "--sort-by", "-s" },
                    () => SortBy.Latest,
                    "The sort-by of searched mangas.");
                var searchCommand = new Command(
                    "search",
                    "Searches for mangas that match options.");
                searchCommand.AddOption(titleOption);
                searchCommand.AddOption(authorOption);
                searchCommand.AddOption(publishCycleOption);
                searchCommand.AddOption(genresOption);
                searchCommand.AddOption(sortByOption);
                searchCommand.SetHandler(SearchManga, titleOption, authorOption, publishCycleOption, genresOption, sortByOption);
                root.AddCommand(searchCommand);
            }
            
            {
                var listProviderCommand = new Command(
                    "list-providers",
                    "Lists all available manga providers.");
                listProviderCommand.SetHandler(ListProviders);
                root.AddCommand(listProviderCommand);
            }

            {
                var providerIdOption = new Option<string>(
                    new[] { "--identifier", "--id" },
                    "The identifier of a manga provider to use.");
                var setProviderCommand = new Command(
                    "set-provider",
                    "Sets specific manga provider to use.");
                setProviderCommand.AddOption(providerIdOption);
                setProviderCommand.SetHandler(SetProvider, providerIdOption);
                root.AddCommand(setProviderCommand);
            }

            {
                var currentProviderCommand = new Command(
                    "current-provider",
                    "Gets the current manga provider in use");
                currentProviderCommand.SetHandler(CurrentProvider);
                root.AddCommand(currentProviderCommand);
            }

            {
                var exitCommand = new Command(
                    "exit",
                    "Exits the program");
                exitCommand.SetHandler(() => exitRequested = true);
                root.AddCommand(exitCommand);
            }

            await root.InvokeAsync(command);
        }
    }
}