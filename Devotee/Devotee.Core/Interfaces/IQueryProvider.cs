using Devotee.Core.Primitives;

namespace Devotee.Core.Interfaces;

public interface IQueryProvider : IProvider
{
    public Task<IPager<MangaHeader>> QueryAsync(MangaQuery query);

    public Task<IEnumerable<EpisodeHeader>> QueryEpisodesAsync(MangaHeader header);

    public Task<IEnumerable<IImageProvider>> QueryImagesAsync(EpisodeHeader header);
}