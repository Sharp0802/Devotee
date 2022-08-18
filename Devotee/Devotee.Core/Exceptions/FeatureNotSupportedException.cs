namespace Devotee.Core.Exceptions;

public class FeatureNotSupportedException : InvalidOperationException
{
    public FeatureNotSupportedException(
        string siteIdentifier,
        string message) : base(message)
    {
        SiteIdentifier = siteIdentifier;
    }
    
    public string SiteIdentifier { get; }
}