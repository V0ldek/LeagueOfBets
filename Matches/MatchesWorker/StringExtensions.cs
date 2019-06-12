using System;
using System.Collections.Generic;
using System.Text;

namespace MatchesWorker
{
    internal static class StringExtensions
    {
        public static string AddTimestamp(this string @string) => $"{DateTime.Now}: {@string}";
    }
}
