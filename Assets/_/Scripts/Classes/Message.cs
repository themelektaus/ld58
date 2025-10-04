namespace Prototype
{
    public class Message : IMessage
    {
        public bool Validate<T>(IObserver<T> _) where T : IMessage
        {
            return true;
        }
    }

    public abstract class Message<T> : IMessage
    {
        public bool Validate<T1>(IObserver<T1> reciever) where T1 : IMessage
        {
            if (reciever is null)
                throw new($"reciever is null");

            if (reciever is not T _reciever)
                throw new($"{reciever.GetType().Name} is not {typeof(T).Name}");

            return Validate(_reciever);
        }

        public abstract bool Validate(T reciever);
    }
}
