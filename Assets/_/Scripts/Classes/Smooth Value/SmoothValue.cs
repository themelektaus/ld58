using System;

namespace Prototype
{
    public abstract class SmoothValue<T>
    {
        T _current;

        readonly Func<T> _getCurrent;
        readonly Action<T> _setCurrent;

        public T target;

        public SmoothValue() : this(default) { }

        public SmoothValue(T value)
        {
            _getCurrent = () => _current;
            _setCurrent = x => _current = x;
            _current = value;
            target = value;
        }

        public SmoothValue(Func<T> getCurrent, Action<T> setCurrent)
        {
            _getCurrent = getCurrent;
            _setCurrent = setCurrent;
            _current = getCurrent();
            target = getCurrent();
        }

        public static implicit operator bool(SmoothValue<T> smoothValue)
        {
            return smoothValue is not null;
        }

        public static implicit operator T(SmoothValue<T> smoothValue)
        {
            return smoothValue ? smoothValue.current : default;
        }

        public virtual void Update()
        {
            _setCurrent(Update(_getCurrent(), target));
        }

        protected abstract T Update(T current, T target);

        public T current { get { return _getCurrent(); } }
        public T value { set { _setCurrent(value); target = value; } }
    }
}
