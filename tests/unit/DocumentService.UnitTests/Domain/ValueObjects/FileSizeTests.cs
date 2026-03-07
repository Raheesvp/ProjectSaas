using DocumentService.Domain.ValueObjects;
using FluentAssertions;

namespace DocumentService.UnitTests.Domain.ValueObjects;

public class FileSizeTests
{
    [Fact]
    public void FromBytes_ValidSize_CreatesCorrectly()
    {
        var size = FileSize.FromBytes(2048);

        size.Bytes.Should().Be(2048);
    }

    [Fact]
    public void FromBytes_NegativeValue_ThrowsArgumentException()
    {
        var act = () => FileSize.FromBytes(-1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void FromBytes_ExactlyAt500MB_Succeeds()
    {
        var act = () => FileSize.FromBytes(500L * 1024 * 1024);

        act.Should().NotThrow();
    }

    [Fact]
    public void FromBytes_Above500MB_ThrowsArgumentException()
    {
        var act = () => FileSize.FromBytes(500L * 1024 * 1024 + 1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*500MB*");
    }

    [Fact]
    public void ToMegabytes_CalculatesCorrectly()
    {
        var size = FileSize.FromBytes(1024 * 1024);

        size.ToMegabytes().Should().Be(1.0);
    }

    [Fact]
    public void TwoFileSizesWithSameBytes_AreValueEqual()
    {
        var size1 = FileSize.FromBytes(4096);
        var size2 = FileSize.FromBytes(4096);

        size1.Should().Be(size2);
        (size1 == size2).Should().BeTrue();
    }

    [Fact]
    public void TwoFileSizesWithDifferentBytes_AreNotEqual()
    {
        var size1 = FileSize.FromBytes(1024);
        var size2 = FileSize.FromBytes(2048);

        size1.Should().NotBe(size2);
    }
}
