using Exercise.Application.Common.Models;
using FluentAssertions;

/// <summary>
/// Unit tests for the PagedResult&lt;T&gt; computed properties:
/// TotalPages, HasPreviousPage, HasNextPage.
/// </summary>
public class PagedResultTests
{
    // ── TotalPages ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(10, 5, 2)]   // exact fit
    [InlineData(11, 5, 3)]   // ceil required
    [InlineData(0,  5, 0)]   // empty result
    [InlineData(1,  20, 1)]  // single page, default size
    [InlineData(20, 20, 1)]  // exactly one page
    [InlineData(21, 20, 2)]  // overflow by 1
    public void TotalPages_IsCorrectlyCeiled(int totalCount, int pageSize, int expectedPages)
    {
        var result = new PagedResult<string>(
            new List<string>(), totalCount, pageNumber: 1, pageSize: pageSize);

        result.TotalPages.Should().Be(expectedPages);
    }

    // ── HasPreviousPage ───────────────────────────────────────────────────────

    [Fact]
    public void HasPreviousPage_IsFalse_OnFirstPage()
    {
        var result = new PagedResult<string>(new List<string>(), 50, pageNumber: 1, pageSize: 10);
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_IsTrue_OnSecondPage()
    {
        var result = new PagedResult<string>(new List<string>(), 50, pageNumber: 2, pageSize: 10);
        result.HasPreviousPage.Should().BeTrue();
    }

    // ── HasNextPage ───────────────────────────────────────────────────────────

    [Fact]
    public void HasNextPage_IsTrue_WhenNotOnLastPage()
    {
        var result = new PagedResult<string>(new List<string>(), 50, pageNumber: 1, pageSize: 10);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_IsFalse_OnLastPage()
    {
        var result = new PagedResult<string>(new List<string>(), 50, pageNumber: 5, pageSize: 10);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_IsFalse_WhenTotalCountIsZero()
    {
        var result = new PagedResult<string>(new List<string>(), 0, pageNumber: 1, pageSize: 10);
        result.HasNextPage.Should().BeFalse();
    }
}
