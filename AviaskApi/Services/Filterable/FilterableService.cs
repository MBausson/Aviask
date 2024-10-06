namespace AviaskApi.Services.Filterable;

public class FilterableService : IFilterableService
{
    public IEnumerable<T> Paginate<T>(IEnumerable<T> coll, int page, int pageLength = 25)
    {
        return coll.Skip((page - 1) * pageLength).Take(pageLength);
    }
}