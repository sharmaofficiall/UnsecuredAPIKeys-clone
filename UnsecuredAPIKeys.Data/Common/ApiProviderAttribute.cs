namespace UnsecuredAPIKeys.Data.Common;

/// <summary>
/// Enum for categorizing API providers
/// </summary>
public enum ProviderCategory
{
    AI_LLM,
    CloudInfrastructure,
    Communication,
    DatabaseBackend,
    MapsLocation,
    Monitoring,
    SourceControl,
    Other
}

[AttributeUsage(AttributeTargets.Class)]
public class ApiProviderAttribute : Attribute
{
    /// <summary>
    /// Whether this provider should be used by the Scraper bot
    /// </summary>
    public bool ScraperUse { get; set; } = true;

    /// <summary>
    /// Whether this provider should be used by the Verifier bot
    /// </summary>
    public bool VerificationUse { get; set; } = true;

    /// <summary>
    /// Category of the provider
    /// </summary>
    public ProviderCategory Category { get; set; } = ProviderCategory.Other;

    /// <summary>
    /// Whether to display this provider in the UI
    /// </summary>
    public bool DisplayInUI { get; set; } = true;

    /// <summary>
    /// Reason why this provider is hidden from UI
    /// </summary>
    public string? HiddenFromUIReason { get; set; }

    /// <summary>
    /// Reason why verification is disabled
    /// </summary>
    public string? VerificationDisabledReason { get; set; }

    /// <summary>
    /// Reason why scraper is disabled
    /// </summary>
    public string? ScraperDisabledReason { get; set; }

    /// <summary>
    /// Creates an ApiProvider attribute with default usage (enabled for both scraper and verifier)
    /// </summary>
    public ApiProviderAttribute()
    {
    }

    /// <summary>
    /// Creates an ApiProvider attribute with specific usage flags
    /// </summary>
    /// <param name="scraperUse">Enable for scraper bot</param>
    /// <param name="verificationUse">Enable for verifier bot</param>
    public ApiProviderAttribute(bool scraperUse, bool verificationUse)
    {
        ScraperUse = scraperUse;
        VerificationUse = verificationUse;
    }
}
