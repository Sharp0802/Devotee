using Devotee.Core.Interfaces;

namespace Devotee.Provider.JMana;

public class JManaSiteConfigProvider : ISiteConfigProvider
{
    public string SiteIdentifier => Config.SiteIdentifier;
    public string SiteUrl => Config.SiteBaseUrl;
}