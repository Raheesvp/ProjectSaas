using Shared.Domain.Primitives;

namespace DocumentService.Domain.ValueObjects;

// FileSize typed value — prevents mixing bytes with MB accidentally
// FileSize.FromBytes(1024).ToMegabytes() → clean API
public sealed class FileSize : ValueObject
{
    public long Bytes { get; }

    private FileSize(long bytes) => Bytes = bytes;

    public static FileSize FromBytes(long bytes)
    {
        if (bytes < 0)
            throw new ArgumentException("File size cannot be negative");

        // Max 500MB per file — business rule enforced in domain
        if (bytes > 500L * 1024 * 1024)
            throw new ArgumentException("File size cannot exceed 500MB");

        return new FileSize(bytes);
    }

    public double ToMegabytes() => Math.Round(Bytes / (1024.0 * 1024.0), 2);
    public double ToKilobytes() => Math.Round(Bytes / 1024.0, 2);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Bytes;
    }

    public override string ToString() => $"{ToMegabytes()} MB";
}