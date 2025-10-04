using System;
using System.Collections.Generic;

using UnityEngine;

namespace Prototype
{
    public abstract class TimedList<T> where T : struct, ITimedListItem
    {
        public bool updateInEditor;

        public List<T> items = new();

        public abstract float GetMaxTime();
        public abstract float GetCurrentTime();

        public void Add(T item) => items.Add(item);

        public void Update(Action<bool, T, T, float> onUpdate)
        {
            if (!updateInEditor && !Application.isPlaying)
            {
                onUpdate(false, default, default, 0);
                return;
            }

            T a;
            T b;
            float t = 0;

            if (items.Count == 0)
            {
                a = default;
                b = a;
                goto Exit;
            }

            if (items.Count == 1)
            {
                a = items[0];
                b = a;
                goto Exit;
            }

            var maxTime = GetMaxTime();
            var currentTime = GetCurrentTime();

            var index = items.FindLastIndex(x => x.GetTime() <= currentTime);
            if (index == -1)
            {
                a = items[0];
                b = items[1];
                b.SetTime(b.GetTime() - maxTime);
            }
            else
            {
                a = items[index];
                if (index == items.Count - 1)
                {
                    b = items[0];
                    b.SetTime(b.GetTime() + maxTime);
                }
                else
                {
                    b = items[index + 1];
                }
                t = (currentTime - a.GetTime()) / (b.GetTime() - a.GetTime());
            }

        Exit:
            onUpdate(true, a, b, t);
        }
    }
}
