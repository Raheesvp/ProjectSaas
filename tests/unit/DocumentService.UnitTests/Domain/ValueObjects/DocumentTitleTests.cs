using DocumentService.Domain.ValueObjects;
using FluentAssertions;

namespace DocumentService.UnitTests.Domain.ValueObjects;

public class DocumentTitleTests
{
    [Fact]
    public void Create_ValidTitle_CreatesSuccessfully()
    {
        var title = DocumentTitle.Create("Invoice Q1 2025.pdf");

        title.Value.Should().Be("Invoice Q1 2025.pdf");
    }

    [Fact]
    public void Create_EmptyString_ThrowsArgumentException()
    {
        var act = () => DocumentTitle.Create(string.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public void Create_WhitespaceOnly_ThrowsArgumentException()
    {
        var act = () => DocumentTitle.Create("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_TitleWith256Chars_ThrowsArgumentException()
    {
        var longTitle = new string('a', 256);
        var act = () => DocumentTitle.Create(longTitle);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*255*");
    }

    [Fact]
    public void Create_TitleWithLeadingSpaces_TrimsAutomatically()
    {
        var title = DocumentTitle.Create("  Invoice.pdf  ");

        title.Value.Should().Be("Invoice.pdf");
    }

    [Fact]
    public void TwoTitlesWithSameValue_AreValueEqual()
    {
        var title1 = DocumentTitle.Create("Contract.pdf");
        var title2 = DocumentTitle.Create("Contract.pdf");

        title1.Should().Be(title2);
    }
}
