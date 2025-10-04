namespace Prototype
{
    public interface IObservable<T> where T : IMessage
    {
        public void Register(IObserver<T> observer);
        public void Unregister(IObserver<T> observer);
        public void UnregisterAll();
    }
}
