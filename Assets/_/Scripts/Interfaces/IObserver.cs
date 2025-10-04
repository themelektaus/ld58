namespace Prototype
{
    public interface IObserver<T> where T : IMessage
    {
        public void ReceiveNotification(T message);
    }
}
