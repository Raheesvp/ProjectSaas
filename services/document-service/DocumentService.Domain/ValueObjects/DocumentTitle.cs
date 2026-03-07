using Shared.Domain.Primitives;

namespace DocumentService.Domain.ValueObjects;

// DocumentTitle is a Value Object — not just a string
// Business rule: title cannot be empty, max 255 chars
// Two DocumentTitle("Invoice.pdf") are EQUAL — value equality
// Prevents raw string being passed where a title is expected
public sealed class DocumentTitle : ValueObject
{
    public string Value { get; }

    private DocumentTitle(string value) => Value = value;

    public static DocumentTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Document title cannot be empty");

        if (value.Length > 255)
            throw new ArgumentException("Document title cannot exceed 255 characters");

        return new DocumentTitle(value.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    // Implicit conversion — use DocumentTitle anywhere string is accepted
    public static implicit operator string(DocumentTitle title) => title.Value;
}