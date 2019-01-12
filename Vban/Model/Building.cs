namespace Vban.Model
{
    public interface IFactory<T>
    {
        int Counter { get; }
        T   Create();
    }

    public interface IBuilder<T>
    {
        T Build();
    }
}