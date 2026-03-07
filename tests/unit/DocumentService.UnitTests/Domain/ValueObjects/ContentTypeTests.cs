using DocumentService.Domain.Enums;
using DocumentService.Domain.ValueObjects;
using FluentAssertions;

namespace DocumentService.UnitTests.Domain.ValueObjects;

public class ContentTypeTests
{
    [Theory]
    [InlineData("application/pdf", DocumentType.Pdf)]
    [InlineData("application/msword", DocumentType.Word)]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", DocumentType.Word)]
    [InlineData("application/vnd.ms-excel", DocumentType.Excel)]
    [InlineData("image/jpeg", DocumentType.Image)]
    [InlineData("image/png", DocumentType.Image)]
    [InlineData("text/plain", DocumentType.Text)]
    [InlineData("application/zip", DocumentType.Other)]
    public void Create_KnownMimeType_MapsToCorrectDocumentType(
        string mimeType,
        DocumentType expectedType)
    {
        var contentType = ContentType.Create(mimeType);

        contentType.DocumentType.Should().Be(expectedType);
        contentType.MimeType.Should().Be(mimeType);
    }

    [Fact]
    public void Create_EmptyMimeType_ThrowsArgumentException()
    {
        var act = () => ContentType.Create(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoContentTypesWithSameMime_AreValueEqual()
    {
        var ct1 = ContentType.Create("application/pdf");
        var ct2 = ContentType.Create("application/pdf");

        ct1.Should().Be(ct2);
    }
}
