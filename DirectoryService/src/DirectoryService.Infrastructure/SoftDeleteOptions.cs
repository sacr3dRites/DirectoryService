using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure;

public class SoftDeleteOptions
{
    public const string SectionName = "SoftDeleteOptions";

    public TimeSpan SoftDeleteInterval { get; set; }

    public TimeSpan AgeOfRecords { get; set; }

    public int BatchSize { get; set; }
}