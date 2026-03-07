using Shared.Domain.Primitives;

namespace DocumentService.Domain.ValueObjects;

// StoragePath represents a location in MinIO/S3
// Format: {tenantId}/{year}/{month}/{documentId}/{fileName}
// Example: 3fa85f64/2025/01/7c9e6679/invoice.pdf
// Keeps storage organization consistent across all uploads
public sealed class StoragePath : ValueObject
{
    public string Value { get; }

    private StoragePath(string value) => Value = value;

    //for uploading new datas
    public static StoragePath Create(
        Guid tenantId,
        Guid documentId,
        string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var now = DateTime.UtcNow;
        var path = $"{tenantId}/{now.Year}/{now.Month:D2}/{documentId}/{fileName}";
        return new StoragePath(path);
    }

    // Used when rehydrating from database
    public static StoragePath FromExisting(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new StoragePath(path);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
    public static implicit operator string(StoragePath path) => path.Value;
}