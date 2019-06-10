using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace BetsData.Migrations
{
    internal static class MigrationBuilderExtensions
    {
        public static OperationBuilder<SqlOperation> RunSqlScript(this MigrationBuilder builder, string scriptName) =>
            builder.Sql(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts", scriptName)));
    }
}