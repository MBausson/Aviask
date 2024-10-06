namespace AviaskApi.Models.Result;

public record FilteredResult<T>(IEnumerable<T> Elements, int TotalCount) where T : class
{
    public static FilteredResult<T> Empty()
    {
        return new FilteredResult<T>(Enumerable.Empty<T>(), 0);
    }
}