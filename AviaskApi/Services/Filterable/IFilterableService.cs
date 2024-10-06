namespace AviaskApi.Services.Filterable;

public interface IFilterableService
{
    public IEnumerable<T> Paginate<T>(IEnumerable<T> coll, int page, int pageLength = 25);
}