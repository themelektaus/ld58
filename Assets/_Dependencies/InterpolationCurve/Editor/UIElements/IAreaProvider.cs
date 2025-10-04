using UnityEngine;

namespace InterpolationCurve.Editor.UIElements
{
    public interface IAreaProvider
    {
        Vector2 GetAbsolute(Vector2 relative);
        Vector2 GetRelative(Vector2 absolute);
    }
}
