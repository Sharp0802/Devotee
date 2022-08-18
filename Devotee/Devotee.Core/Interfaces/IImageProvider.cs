namespace Devotee.Core.Interfaces;

public interface IImageProvider : IProvider
{
    public string Hyperlink { get; }

    public Task CopyToAsync(Stream output, string? referer = null);
}