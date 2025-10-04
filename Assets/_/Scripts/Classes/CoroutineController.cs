using System.Collections;

using UnityEngine;

namespace Prototype
{
    public class CoroutineController
    {
        public enum State
        {
            Ready,
            Running,
            Paused,
            Finished
        }

        public State state { get; private set; }

        public delegate void OnFinishEvent();
        public event OnFinishEvent OnFinish;

        readonly MonoBehaviour behaviour;

        Coroutine coroutine;

        public bool isBusy => coroutine is not null;

        public CoroutineController(MonoBehaviour behaviour)
        {
            this.behaviour = behaviour;
            state = State.Ready;
        }

        public Coroutine Start(IEnumerator routine)
        {
            if (coroutine is not null)
                throw new System.InvalidOperationException($"Coroutine is already running");

            coroutine = behaviour.StartCoroutine(Wrap(routine));
            return coroutine;
        }

        public bool Stop()
        {
            if (state != State.Running && state != State.Paused)
                return false;

            Finish();
            return true;
        }

        public bool Pause()
        {
            if (state != State.Running)
                return false;

            state = State.Paused;
            return true;
        }

        public bool Resume()
        {
            if (state != State.Paused)
                return false;

            state = State.Running;
            return true;
        }

        IEnumerator Wrap(IEnumerator routine)
        {
            state = State.Running;

            while (routine.MoveNext())
            {
                yield return routine.Current;

                while (state == State.Paused)
                    yield return null;

                if (state == State.Finished)
                    yield break;
            }

            Finish();
        }

        void Finish()
        {
            if (state != State.Finished)
            {
                state = State.Finished;
                OnFinish?.Invoke();
            }

            if (coroutine is not null)
            {
                behaviour.StopCoroutine(coroutine);
                coroutine = null;
            }
        }
    }
}
