using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchesData.EnumPropertyBuilderExtensions
{
    internal static class EnumPropertyBuilderExtensions
    {
        public static PropertyBuilder<T> HasStringConversion<T>(this PropertyBuilder<T> builder) where T : Enum =>
            builder.HasConversion(v => v.ToString(), v => (T) Enum.Parse(typeof(T), v));
    }
}