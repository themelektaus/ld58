namespace Prototype
{
    public interface IMessage
    {
        public bool Validate<T>(IObserver<T> reciever) where T : IMessage;
    }
}
