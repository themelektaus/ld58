using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

namespace Prototype
{
    public class Sequence : CustomYieldInstruction
    {
        public override bool keepWaiting => false;

        public class Instance : Sequence
        {
            public override bool keepWaiting => coroutine is not null;
            public bool isRunning => !keepWaiting;

            public Coroutine coroutine { get; private set; }

            readonly Queue<Action> synchronizedActions = new();

            int? nextIndex;
            public bool isPaused { get; private set; }

            public Instance(Sequence original) : base(original.owner)
            {
                routineInfos.AddRange(original.routineInfos);
            }

            public new void Start()
            {
                Stop();
                StartInternal();
            }

            void StartInternal()
            {
                nextIndex = null;
                isPaused = false;

                IEnumerator _()
                {
                    int index = 0;

                    while (index < routineInfos.Count)
                    {
                        while (isPaused)
                            yield return null;

                        yield return owner.StartCoroutine(routineInfos[index].enumerator(this));

                        if (!nextIndex.HasValue)
                        {
                            index++;
                            continue;
                        }

                        if (nextIndex.Value == -1)
                        {
                            nextIndex = null;
                            continue;
                        }

                        if (nextIndex.Value == -2)
                            break;

                        index = nextIndex.Value;
                        nextIndex = null;
                    }

                    coroutine = null;
                };

                coroutine = owner.StartCoroutine(_());
            }

            public void Stop()
            {
                if (coroutine is null)
                    return;

                owner.StopCoroutine(coroutine);
                coroutine = null;
            }

            public Task Delay(int millisecondsDelay)
            {
                return Task.Delay(millisecondsDelay);
            }

            public void Synchronize(Action action)
            {
                synchronizedActions.Enqueue(action);
            }

            public void Restart() => Goto(0);

            public void Repeat() => Goto(-1);

            public void Break() => Goto(-2);

            public void Goto(int index)
                => nextIndex = index;

            public void Goto(string name)
            {
                var index = routineInfos.FindIndex(x => x.name == name);
                if (index > -1)
                    nextIndex = index;
            }

            public void Pause()
            {
                isPaused = true;
            }

            public void Resume()
            {
                if (coroutine is null)
                {
                    StartInternal();
                    return;
                }

                isPaused = false;
            }

            public void PerformSynchronizedActions()
            {
                while (synchronizedActions.Count > 0)
                    synchronizedActions.Dequeue()();
            }
        }

        public struct EnumeratorInfo
        {
            public string name;
            public Func<Instance, IEnumerator> enumerator;

            public EnumeratorInfo(string name, Func<Instance, IEnumerator> enumerator)
            {
                this.name = name;
                this.enumerator = enumerator;
            }
        }

        readonly MonoBehaviour owner;
        readonly List<EnumeratorInfo> routineInfos = new();

        public Sequence(MonoBehaviour owner)
        {
            this.owner = owner;
        }

        Sequence Then(string name, Func<Instance, IEnumerator> enumerator)
        {
            if (this is Instance)
            {
                Debug.LogWarning("The \"Then\"-keyword inside a sequence instance has no effect");
                return this;
            }

            routineInfos.Add(new(name, enumerator));
            return this;
        }

        public Sequence Then(IEnumerator enumerator)
        {
            IEnumerator _(Instance instance)
            {
                instance.PerformSynchronizedActions();
                yield return enumerator;
            }
            return Then(null, _);
        }

        public Sequence Then(Action action) => Then(null, action);
        public Sequence Then(string name, Action action)
        {
            IEnumerator _(Instance instance)
            {
                action();
                instance.PerformSynchronizedActions();
                yield break;

            }
            return Then(name, _);
        }

        public Sequence Then(Action<Instance> action) => Then(null, action);
        public Sequence Then(string name, Action<Instance> action)
        {
            IEnumerator _(Instance instance)
            {
                action(instance);
                instance.PerformSynchronizedActions();
                yield break;

            }
            return Then(name, _);
        }

        public Sequence Then(Func<Task> task) => Then(null, task);
        public Sequence Then(string name, Func<Task> task)
        {
            IEnumerator _(Instance instance)
            {
                yield return WaitForTask(instance, Task.Run(task));
            }
            return Then(name, _);
        }

        public Sequence Then(Func<Instance, Task> task) => Then(null, task);
        public Sequence Then(string name, Func<Instance, Task> task)
        {
            IEnumerator _(Instance instance)
            {
                yield return WaitForTask(instance, Task.Run(() => task(instance)));
            }
            return Then(name, _);
        }

        public Sequence Wait(float seconds) => Wait(null, seconds);
        public Sequence Wait(string name, float seconds)
        {
            IEnumerator _(Instance instance)
            {
                yield return new WaitForSeconds(seconds);
            }
            return Then(name, _);
        }

        public Sequence Wait(Func<float> seconds) => Wait(null, seconds);
        public Sequence Wait(string name, Func<float> seconds)
        {
            IEnumerator _(Instance instance)
            {
                yield return new WaitForSeconds(seconds());
            }
            return Then(name, _);
        }

        public Sequence WaitForFrame() => WaitForFrame(null);
        public Sequence WaitForFrame(string name)
        {
            IEnumerator _(Instance instance)
            {
                yield return new WaitForEndOfFrame();
            }
            return Then(name, _);
        }

        public Sequence WaitForFrames(int frameCount) => WaitForFrames(null, frameCount);
        public Sequence WaitForFrames(string name, int frameCount)
        {
            IEnumerator _(Instance instance)
            {
                for (int i = 0; i < frameCount; i++)
                    yield return new WaitForEndOfFrame();
            }
            return Then(name, _);
        }

        public Sequence While(Func<bool> condition) => While(null, condition);
        public Sequence While(string name, Func<bool> condition)
        {
            return Then(name, @this =>
            {
                if (condition())
                    @this.Repeat();
            });
        }

        public Sequence While(Func<bool> condition, Action action) => While(null, condition, action);
        public Sequence While(string name, Func<bool> condition, Action action)
        {
            return Then(name, @this =>
            {
                if (condition())
                {
                    action();
                    @this.Repeat();
                }
            });
        }

        public Sequence BreakIf(Func<bool> condition) => BreakIf(null, condition);
        public Sequence BreakIf(string name, Func<bool> condition)
        {
            return Then(name, @this =>
            {
                if (condition())
                    @this.Break();
            });
        }

        public Sequence StartOver() => StartOver(null);
        public Sequence StartOver(string name)
            => Then(name, @this => @this.Restart());

        public Sequence Activate(GameObject gameObject) => Activate(null, gameObject);
        public Sequence Activate(string name, GameObject gameObject)
            => Then(name, () => gameObject.SetActive(true));

        public Sequence Deactivate(GameObject gameObject) => Deactivate(null, gameObject);
        public Sequence Deactivate(string name, GameObject gameObject)
            => Then(name, () => gameObject.SetActive(false));

        public Sequence Enable(Component component) => Enable(null, component);
        public Sequence Enable(string name, Component component)
            => Then(name, () => component.Enable());

        public Sequence Disable(Component component) => Disable(null, component);
        public Sequence Disable(string name, Component component)
            => Then(name, () => component.Disable());

        public Sequence Destroy(UnityEngine.Object @object) => Destroy(null, @object);
        public Sequence Destroy(string name, UnityEngine.Object @object)
            => Then(name, () =>
            {
                if (@object)
                    (@object as GameObject).Destroy();
            });

        public Sequence DestroyOwnerGameObject() => DestroyOwnerGameObject(null);
        public Sequence DestroyOwnerGameObject(string name)
            => Then(name, owner.GetGameObject().Destroy);

        public Sequence Kill(GameObject gameObject) => Kill(null, gameObject);
        public Sequence Kill(string name, GameObject gameObject)
            => Then(name, () => gameObject.Kill());

        IEnumerator WaitForTask(Instance instance, Task task)
        {
            do
            {
                instance.PerformSynchronizedActions();
                yield return null;
            }
            while (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled);
        }

        public Instance Build(Action action) => Then(action).Build();
        public Instance Build() => new(this);

        public Instance Start(Action action) => Then(action).Start();
        public Instance Start(Action<Instance> action) => Then(action).Start();
        public Instance Start()
        {
            var instance = new Instance(this);
            instance.Start();
            return instance;
        }
    }
}
