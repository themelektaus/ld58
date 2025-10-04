using System.Collections.Generic;

using UnityEngine;

public static class ExtensionMethods
{
    public static void Log(this Object context, string message)
    {
#if UNITY_EDITOR
        Debug.Log(GetFormattedText(context.name, message, "white"), context);
#endif
    }

    public static void Log(this Object context, string message, int value, string color = "white")
    {
#if UNITY_EDITOR
        Debug.Log($"<b>{context.name}</b> <color={color}>{message}: <b>{value}</b></color>");
#endif
    }

    public static void Log(this Object context, string message, float value, string color = "white")
    {
#if UNITY_EDITOR
        Debug.Log($"<b>{context.name}</b> <color={color}>{message}: <b>{value.ToString("0.000").Replace(',', '.')}</b></color>");
#endif
    }

    public static void Log(this Object context, string message, Vector2 value, string color = "white")
    {
#if UNITY_EDITOR
        var x = value.x.ToString("0.000").Replace(',', '.');
        var y = value.y.ToString("0.000").Replace(',', '.');
        Debug.Log($"<b>{context.name}</b> <color={color}>{message}: <b>{x}</b>, <b>{y}</b></color>");
#endif
    }

    public static void LogWarning(this System.Type context, string message)
    {
        Debug.LogWarning(GetFormattedText(context.Name, message, "yellow"), null);
    }

    public static void LogWarning(this Object context, string message)
    {
        Debug.LogWarning(GetFormattedText(context.name, message, "yellow"), context);
    }

    public static void LogError(this Object context, string message)
    {
        Debug.LogError(GetFormattedText(context.name, message, "orange"), context);
    }

    static string GetFormattedText(string name, object message, string color)
    {
        return $"<b><color=white>{name}</color></b> <color={color}>{message}</color>";
    }

    static readonly Dictionary<string, float> heatTimes = new();

    public static void LogHeat(this Object context, string message, float timeThreshold)
    {
        var time = Time.unscaledTime;

        message = GetFormattedText(context.name, message, "orange");

        if (!heatTimes.ContainsKey(message))
            heatTimes.Add(message, time - timeThreshold - 1);

        if (time - heatTimes[message] < timeThreshold)
            Debug.LogWarning($"🔥 {message}", context);

        heatTimes[message] = time;
    }
}
