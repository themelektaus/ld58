namespace Prototype
{
    public abstract class Selector<T>
    {
        public abstract bool Match(T value, bool includeTree);
        public abstract T Find();
        public abstract T[] FindAll();
    }
}
