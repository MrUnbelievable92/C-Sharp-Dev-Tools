using System;

namespace DevTools
{
    public static class GenericExtensions
    {
        /// <summary>   Logs a message of type '<typeparamref name="T"/>' to the console.   </summary>
        public static void Log<T>(this T obj)
        {
#if DEBUG
    #if UNITY_EDITOR
            UnityEngine.Debug.Log(obj);
    #else
            Console.WriteLine(obj);
    #endif
#endif
        }

        /// <summary>   Logs a message of type '<typeparamref name="T"/>' to the console.   </summary>
        public static void Log<T>(this T obj, string format, IFormatProvider formatProvider = null)
            where T : IFormattable
        {
            Log(obj.ToString(format, formatProvider));
        }
    }
}
