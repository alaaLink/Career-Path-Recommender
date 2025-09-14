namespace CareerPathRecommender.Web.Models;

public class PaginationInfo
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

    public IEnumerable<int> GetPageNumbers()
    {
        var startPage = Math.Max(1, CurrentPage - 2);
        var endPage = Math.Min(TotalPages, CurrentPage + 2);

        return Enumerable.Range(startPage, endPage - startPage + 1);
    }
}