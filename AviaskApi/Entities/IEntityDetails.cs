namespace AviaskApi.Entities;

public interface IEntityDetails<TDetails>
{
    public TDetails GetDetails();
}