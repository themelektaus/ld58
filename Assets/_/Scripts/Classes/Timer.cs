using UnityEngine;

namespace Prototype
{
    public class Timer
    {
        public System.Func<float> getDeltaTime = () => Time.deltaTime;

        Vector2 interval;
        float time;

        public void Reset(bool clearTime = true)
        {
            time = 0;
            if (!clearTime)
                SetupTime();
        }

        public bool Update(float interval, bool clearTime = true)
        {
            return Update(new Vector2(interval, interval), clearTime);
        }

        public bool Update(Vector2 interval, bool clearTime = true)
        {
            if (!Utils.Approximately(this.interval, interval))
            {
                this.interval = interval;
                Reset(clearTime);
            }

            bool result = false;

            if (time <= 0)
            {
                SetupTime();
                result = true;
            }

            time -= getDeltaTime();

            return result;
        }

        void SetupTime()
        {
            if (interval.y > interval.x)
                time += Utils.RandomRange(interval);
            else
                time += interval.x;
        }
    }
}
