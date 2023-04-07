﻿using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace FluentHelper.EntityFramworkCore.PostgreSQL
{
    public static class PostgreProviderExtensions
    {
        public static EfDbConfigBuilder WithPostgreSQLProvider(this EfDbConfigBuilder dbContextBuilder, string connectionString, Action<NpgsqlDbContextOptionsBuilder>? npgSqlOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            dbContextBuilder = dbContextBuilder.WithDbProviderConfiguration(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = npgSqlOptionsAction != null ? dbContextOptionsBuilder.UseNpgsql(connectionString, npgSqlOptionsAction) : dbContextOptionsBuilder.UseNpgsql(connectionString);
            });

            return dbContextBuilder;
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, System.Data.IsolationLevel isolationLevel)
        {
            if (dbContext.IsTransactionOpen())
                throw new Exception("A transaction is already open");

            return dbContext.ExecuteOnDatabase(db => db.BeginTransaction(isolationLevel));
        }
    }
}
