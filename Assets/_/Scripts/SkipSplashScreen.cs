#if !UNITY_EDITOR
#if UNITY_STANDALONE_WIN
using System.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.Rendering;
public class SkipSplashScreen
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void BeforeSplashScreen()
    {
#if UNITY_STANDALONE_WIN
        Task.Run(() =>
        {
#endif
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
#if UNITY_STANDALONE_WIN
        });
#endif
    }
}
#endif