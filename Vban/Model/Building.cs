namespace Vban.Model
{
    public interface IFactory<T>
    {
        T Create();

        int Counter();
    }

    public interface IBuilder<T>
    {
        T Build();
    }
}