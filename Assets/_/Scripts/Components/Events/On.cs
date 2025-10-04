using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Prototype
{
    public interface IEventInfo<TEvent, TAction> where TEvent : UnityEventBase
    {
        public float delay { get; set; }
        public bool delayByFrameCount { get; set; }
        public TEvent @event { get; set; }

        public abstract void OnAddListener(TAction call);
        public abstract void OnRemoveListener(TAction call);
    }

    public abstract class On<TEvent, TAction> : MonoBehaviour where TEvent : UnityEventBase
    {
        protected enum CallState { Always, Never, Build, Editor }
        [SerializeField] protected CallState callState = CallState.Always;

        protected enum ApplicationType { Always, Never, Application, Headless, Web }
        [SerializeField] protected ApplicationType applicationType = ApplicationType.Always;

        protected virtual bool offloadCoroutine => false;
        protected abstract IEventInfo<TEvent, TAction> CreateEventInfo();
        protected abstract void OnAddEventInfo(IEventInfo<TEvent, TAction> eventInfo);

        protected abstract IEnumerable<IEventInfo<TEvent, TAction>> eventInfos { get; }

        protected void Invoke(Action<IEventInfo<TEvent, TAction>> action)
        {
#if UNITY_EDITOR
            if (!eventInfos.Any())
                this.LogWarning("No Event Infos");
#endif
            if (callState == CallState.Never)
                return;

            if (callState != CallState.Always)
            {
#if UNITY_EDITOR
                if (callState != CallState.Editor)
                    return;
#else
                if (callState != CallState.Build)
                    return;
#endif
            }

            if (applicationType == ApplicationType.Never)
                return;

#if !ENABLE_IL2CPP
            if (applicationType == ApplicationType.Web)
                return;
#endif

            if (applicationType != ApplicationType.Always)
            {
                var @null = UnityEngine.Rendering.GraphicsDeviceType.Null;
                var a = SystemInfo.graphicsDeviceType == @null;
                var b = applicationType == ApplicationType.Headless;
                if (a != b)
                    return;
            }

            foreach (var eventInfo in eventInfos)
            {
                var @event = eventInfo.@event;
                if (@event is null)
                    continue;

                var delay = eventInfo.delay;
                var delayByFrameCount = eventInfo.delayByFrameCount;

                if (delay > 0)
                {
                    if (delayByFrameCount)
                        this.Log($"{name} ({GetType().Name}) starts by skipping frames", (int) delay);
                    //else
                    //    this.Log($"{name} ({GetType().Name}) starts delayed", delay, "#666666");

                    if (offloadCoroutine)
                    {
                        var dummy = Dummy.Create("Temp Game Object for Events");

                        (
                            delayByFrameCount
                                ? dummy.WaitForFrames(Mathf.CeilToInt(delay))
                                : dummy.Wait(delay)
                        )
                            .Then(() => action(eventInfo))
                            .Destroy(dummy.gameObject)
                            .Start();

                        continue;
                    }

                    (
                        delayByFrameCount
                            ? this.WaitForFrames(Mathf.CeilToInt(delay))
                            : this.Wait(delay)
                    ).Start(() => action(eventInfo));

                    continue;
                }

                action(eventInfo);
            }
        }

        public void AddListener(TAction call) => AddListener(0, call);

        public void AddListener(float delay, TAction call) => AddListener(delay, false, call);

        public void AddListener(float delay, bool delayByFrameCount, TAction call)
        {
            var eventInfo = CreateEventInfo();
            eventInfo.delay = delay;
            eventInfo.delayByFrameCount = delayByFrameCount;
            eventInfo.OnAddListener(call);
            OnAddEventInfo(eventInfo);
        }

        public void RemoveListener(TAction call)
        {
            foreach (var eventInfo in eventInfos)
                eventInfo.OnRemoveListener(call);
        }

        Transform parent;

        public void UseParent(Transform parent)
        {
            this.parent = parent;
        }

        public void Instantiate(GameObject gameObject)
        {
            gameObject.Instantiate(parent);
            parent = null;
        }

        public void DestroyGameObject()
        {
            gameObject.Destroy();
        }

        public void DestroyUnityObject(UnityEngine.Object @object)
        {
            Destroy(@object);
        }

        public void LoadScene(string sceneName)
        {
            var dummy = Dummy.Create();
            var process = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            process.completed += _ => dummy.gameObject.Destroy();
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        public void CreateEventSystem()
        {
            if (!EventSystem.current)
            {
                var gameObject = new GameObject("Event System");
                gameObject.AddComponent<EventSystem>();
                gameObject.AddComponent<StandaloneInputModule>();
                gameObject.AddComponent<BaseInput>();
            }
        }
    }

    public abstract class On : On<UnityEvent, UnityAction>
    {
        [Serializable]
        public struct EventInfo : IEventInfo<UnityEvent, UnityAction>
        {
            [SerializeField] float _delay;
            public float delay
            {
                get => _delay;
                set => _delay = value;
            }

            [SerializeField] bool _delayByFrameCount;
            public bool delayByFrameCount
            {
                get => _delayByFrameCount;
                set => _delayByFrameCount = value;
            }

            [SerializeField] UnityEvent _event;
            public UnityEvent @event
            {
                get => _event;
                set => _event = value;
            }

            public void OnAddListener(UnityAction call) => _event.AddListener(call);
            public void OnRemoveListener(UnityAction call) => _event.RemoveListener(call);
        }

        protected override IEventInfo<UnityEvent, UnityAction> CreateEventInfo()
            => new EventInfo { @event = new() };

        [SerializeField] List<EventInfo> _eventInfos = new();
        protected override IEnumerable<IEventInfo<UnityEvent, UnityAction>> eventInfos
            => _eventInfos.Select(x => x as IEventInfo<UnityEvent, UnityAction>);

        public void ModifyEventInfo(int index, Func<EventInfo, EventInfo> action)
            => _eventInfos[index] = action(_eventInfos[index]);

        protected override void OnAddEventInfo(IEventInfo<UnityEvent, UnityAction> eventInfo)
            => _eventInfos.Add((EventInfo) eventInfo);

        bool skipNextTime;
        public void SkipNextTime() => skipNextTime = true;

        public void Invoke()
        {
            if (skipNextTime)
            {
                skipNextTime = false;
                return;
            }

            Invoke(x =>
            {
                var @event = x.@event;
                x.@event.Invoke();
            });
        }
    }

    public abstract class On<T0> : On<UnityEvent<T0>, UnityAction<T0>>
    {
        [Serializable]
        public struct EventInfo : IEventInfo<UnityEvent<T0>, UnityAction<T0>>
        {
            [SerializeField] float _delay;
            public float delay
            {
                get => _delay;
                set => _delay = value;
            }

            [SerializeField] bool _delayByFrameCount;
            public bool delayByFrameCount
            {
                get => _delayByFrameCount;
                set => _delayByFrameCount = value;
            }

            [SerializeField] UnityEvent<T0> _event;
            public UnityEvent<T0> @event
            {
                get => _event;
                set => _event = value;
            }

            public void OnAddListener(UnityAction<T0> call) => _event.AddListener(call);
            public void OnRemoveListener(UnityAction<T0> call) => _event.RemoveListener(call);
        }

        protected override IEventInfo<UnityEvent<T0>, UnityAction<T0>> CreateEventInfo()
            => new EventInfo { @event = new() };

        [SerializeField] List<EventInfo> _eventInfos = new();
        protected override IEnumerable<IEventInfo<UnityEvent<T0>, UnityAction<T0>>> eventInfos
            => _eventInfos.Select(x => x as IEventInfo<UnityEvent<T0>, UnityAction<T0>>);

        protected override void OnAddEventInfo(IEventInfo<UnityEvent<T0>, UnityAction<T0>> eventInfo)
            => _eventInfos.Add((EventInfo) eventInfo);

        bool skipNextTime;
        public void SkipNextTime() => skipNextTime = true;

        public void Invoke(T0 arg0)
        {
            if (skipNextTime)
            {
                skipNextTime = false;
                return;
            }
            Invoke(x => x.@event.Invoke(arg0));
        }
    }
}
