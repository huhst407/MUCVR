// A simple logger class that uses Console.WriteLine by default.
// Can also do Logger.LogMethod = Debug.Log for Unity etc.
// (this way we don't have to depend on UnityEngine)
using System;
using System.Diagnostics;

namespace kcp2k
{
    public static class Log
    {
        public static Action<string> Info = UnityEngine.Debug.Log;
        public static Action<string> Warning = UnityEngine.Debug.Log;
        public static Action<string> Error = UnityEngine.Debug.LogError;
    }
}
