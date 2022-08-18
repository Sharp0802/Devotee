namespace Devotee.Core.Interfaces;

public interface ISiteConfigProvider : IProvider
{
    public string SiteUrl { get; }
}