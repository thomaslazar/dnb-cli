// tests/DnbCli.Tests/Dnb/CqlBuilderTests.cs
using DnbCli.Dnb;

namespace DnbCli.Tests.Dnb;

public class CqlBuilderTests
{
    [Fact]
    public void ForIsbn_strips_hyphens_and_uses_isbn_index()
    {
        Assert.Equal("isbn=9783753931104", CqlBuilder.ForIsbn("978-3-7539-3110-4"));
    }

    [Fact]
    public void ForIsbn_preserves_trailing_X_for_isbn10()
    {
        Assert.Equal("isbn=395956175X", CqlBuilder.ForIsbn("3-95956-175-X"));
    }

    [Fact]
    public void ForDnbId_uses_IDN_index()
    {
        Assert.Equal("IDN=1356869467", CqlBuilder.ForDnbId("1356869467"));
    }

    [Fact]
    public void ForSearch_emits_single_clause_when_only_title_set()
    {
        var cql = CqlBuilder.ForSearch(title: "Blendwerk*");
        Assert.Equal("TIT=Blendwerk*", cql);
    }

    [Fact]
    public void ForSearch_combines_multiple_flags_with_and()
    {
        var cql = CqlBuilder.ForSearch(title: "Blendwerk*", contributor: "Butcher", year: "2024");
        Assert.Equal("TIT=Blendwerk* and PER=Butcher and JHR=2024", cql);
    }

    [Fact]
    public void ForSearch_uses_WOE_for_series_and_any()
    {
        Assert.Equal("WOE=Buddenbrooks", CqlBuilder.ForSearch(series: "Buddenbrooks"));
        Assert.Equal("WOE=Roman", CqlBuilder.ForSearch(any: "Roman"));
    }

    [Fact]
    public void ForSearch_quotes_values_with_spaces()
    {
        var cql = CqlBuilder.ForSearch(title: "Don't Toy");
        Assert.Equal("TIT=\"Don't Toy\"", cql);
    }

    [Fact]
    public void ForSearch_throws_when_no_flag_supplied()
    {
        Assert.Throws<ArgumentException>(() => CqlBuilder.ForSearch());
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("1234567890", true)]
    [InlineData("978-3-7539-3110-4", true)]
    [InlineData("9783753931104", true)]
    [InlineData("395956175X", true)]
    [InlineData("not-an-isbn", false)]
    [InlineData("", false)]
    [InlineData("12345678901234", false)]
    public void IsValidIsbnShape_accepts_10_or_13_digits_with_optional_hyphens_and_X(string input, bool expected)
    {
        Assert.Equal(expected, CqlBuilder.IsValidIsbnShape(input));
    }
}
