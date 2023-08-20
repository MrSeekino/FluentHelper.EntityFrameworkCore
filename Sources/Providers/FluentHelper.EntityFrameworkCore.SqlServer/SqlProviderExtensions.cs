﻿using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FluentHelper.EntityFrameworkCore.SqlServer
{
    public static class SqlProviderExtensions
    {
        public static EfDbConfigBuilder WithSqlDbProvider(this EfDbConfigBuilder dbContextBuilder, string connectionString, Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            dbContextBuilder = dbContextBuilder.WithDbProviderConfiguration(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder = sqlServerOptionsAction != null ? dbContextOptionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction) : dbContextOptionsBuilder.UseSqlServer(connectionString);
            });

            return dbContextBuilder;
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, IsolationLevel isolationLevel)
        {
            if (dbContext.IsTransactionOpen())
                throw new InvalidOperationException("A transaction is already open");

            return dbContext.ExecuteOnDatabase(db => db.BeginTransaction(isolationLevel));
        }
    }
}
