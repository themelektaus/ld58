using System.Collections.Generic;
using System.Linq;

namespace Prototype
{
    public class Observable<T> : IObservable<T> where T : IMessage
    {
        struct ObserverInfo
        {
            public IObserver<T> observer;
        }

        readonly List<ObserverInfo> observerInfos = new();

        public void Register(IObserver<T> observer)
        {
            observerInfos.Add(new() { observer = observer });
        }

        public void Unregister(IObserver<T> observer)
        {
            observerInfos.RemoveAll(x => x.observer == observer);
        }

        public void UnregisterAll()
        {
            observerInfos.Clear();
        }

        public void Notify(T message)
        {
            foreach (var observerInfo in observerInfos.ToList())
                if (message.Validate(observerInfo.observer))
                    observerInfo.observer.ReceiveNotification(message);
        }

        public bool HasAnyReceiver(T message)
        {
            foreach (var observerInfo in observerInfos.ToList())
                if (message.Validate(observerInfo.observer))
                    return true;
            return false;
        }
    }
}
