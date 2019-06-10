using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BetsData.EnumPropertyBuilderExtensions
{
    internal static class EnumPropertyBuilderExtensions
    {
        public static PropertyBuilder<T> HasStringConversion<T>(this PropertyBuilder<T> builder) where T : struct, Enum =>
            builder.HasConversion(v => v.ToString(), v => (T) Enum.Parse(typeof(T), v));

        public static PropertyBuilder<T?> HasStringConversion<T>(this PropertyBuilder<T?> builder) where T : struct, Enum =>
            builder.HasConversion(v => v == null ? null : v.ToString(), v => v == "NULL" ? (T?)null : (T) Enum.Parse(typeof(T), v));
    }
}