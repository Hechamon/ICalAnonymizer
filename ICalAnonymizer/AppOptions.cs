namespace ICalAnonymizer;

public sealed record AppOptions
{
    public required string ApiKey { get; init; }
    public required string ConfigPath { get; init; }
}