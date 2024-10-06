using Bogus;

namespace AviaskApiTest.Factories;

public abstract class Factory<T> where T : class
{
    protected abstract Faker<T> Faker { get; }

    public T GetOne()
    {
        return Faker.Generate();
    }

    public IEnumerable<T> GetMany(int count)
    {
        return Faker.Generate(count);
    }
}