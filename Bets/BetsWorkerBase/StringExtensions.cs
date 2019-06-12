using System;

namespace BetsWorkerBase
{
    public static class StringExtensions
    {
        public static string AddTimestamp(this string @string) => $"{DateTime.Now}: {@string}";
    }
}