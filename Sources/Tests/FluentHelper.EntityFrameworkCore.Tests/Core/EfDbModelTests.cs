using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FluentHelper.EntityFrameworkCore.Tests.Core
{
    [TestFixture]
    internal class EfDbModelTests
    {
        [Test]
        public void Verify_DbConfiguration_IsCalledCorrectly()
        {
            bool funcCalled = false;

            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbProvider.Returns(x => { });
            dbConfig.DbConfiguration.Returns(x => { funcCalled = true; });

            var contextOptBuilder = Substitute.For<DbContextOptionsBuilder>();
            contextOptBuilder.IsConfigured.Returns(false);

            var dbModel = new EfDbModel(dbConfig, new List<IDbMap>());
            dbModel.Configure(contextOptBuilder);

            Assert.True(funcCalled);
        }

        [Test]
        public void Verify_DbProvider_IsCalledCorrectly()
        {
            bool funcCalled = false;

            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbProvider.Returns(x => { }).AndDoes(x =>
            {
                funcCalled = true;
            });

            var contextOptBuilder = Substitute.For<DbContextOptionsBuilder>();
            contextOptBuilder.IsConfigured.Returns(false);

            var dbModel = new EfDbModel(dbConfig, new List<IDbMap>());
            dbModel.Configure(contextOptBuilder);

            Assert.True(funcCalled);
        }

        [Test]
        public void Verify_LogTo_IsCalledCorrectly()
        {
            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbProvider.Returns(x => { });
            dbConfig.LogAction.Returns(logAction);

            var contextOptBuilder = Substitute.For<DbContextOptionsBuilder>();
            contextOptBuilder.IsConfigured.Returns(false);

            var dbModel = new EfDbModel(dbConfig, new List<IDbMap>());
            dbModel.Configure(contextOptBuilder);

            contextOptBuilder.Received(1).LogTo(Arg.Any<Func<EventId, LogLevel, bool>>(), Arg.Any<Action<EventData>>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Verify_EnableSensitiveDataLogging_IsCalledCorrectly(bool enableSensitivityDataLogging)
        {
            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbProvider.Returns(x => { });
            dbConfig.EnableSensitiveDataLogging.Returns(enableSensitivityDataLogging);

            var contextOptBuilder = Substitute.For<DbContextOptionsBuilder>();
            contextOptBuilder.IsConfigured.Returns(false);

            var dbModel = new EfDbModel(dbConfig, new List<IDbMap>());
            dbModel.Configure(contextOptBuilder);

            if (enableSensitivityDataLogging)
                contextOptBuilder.Received(1).EnableSensitiveDataLogging(true);
            else
                contextOptBuilder.DidNotReceive().EnableSensitiveDataLogging(true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Verify_UseLazyLoadingProxies_IsCalledCorrectly(bool enableLazyLoadingProxies) // to be verified
        {
            bool funcCalled = false;

            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbProvider.Returns(x => { });
            dbConfig.EnableLazyLoadingProxies.Returns(enableLazyLoadingProxies);

            var contextOptBuilder = Substitute.For<DbContextOptionsBuilder>();
            contextOptBuilder.IsConfigured.Returns(false);

            var dbModel = new EfDbModel(dbConfig, new List<IDbMap>(), x =>
            {
                funcCalled = true;
            });
            dbModel.Configure(contextOptBuilder);

            Assert.That(funcCalled, Is.EqualTo(enableLazyLoadingProxies));
        }

        [Test]
        public void Verify_CreateModel_WorksProperly()
        {
            var modelBuilder = Substitute.For<ModelBuilder>();

            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbProvider.Returns(x => { });

            var contextOptBuilder = Substitute.For<DbContextOptionsBuilder>();
            contextOptBuilder.IsConfigured.Returns(false);

            var dbMap = Substitute.For<IDbMap>();
            dbMap.SetModelBuilder(Arg.Any<ModelBuilder>());

            var dbModel = new EfDbModel(dbConfig, new List<IDbMap>() { dbMap });
            dbModel.Configure(contextOptBuilder);
            dbModel.CreateModel(modelBuilder);

            dbMap.Received(1).SetModelBuilder(modelBuilder);
            dbMap.Received(1).Map();
        }

        [Test]
        public void Verify_CreateModel_Throws_Without_DbProviderConfiguration()
        {
            var moedlBuilder = Substitute.For<ModelBuilder>();

            var dbConfig = Substitute.For<IDbConfig>();
            dbConfig.DbProvider.Returns(null);

            var contextOptBuilder = Substitute.For<DbContextOptionsBuilder>();
            contextOptBuilder.IsConfigured.Returns(false);

            var mockDbMap = Substitute.For<IDbMap>();

            var dbModel = new EfDbModel(dbConfig, new List<IDbMap>() { mockDbMap });
            Assert.Throws<NullReferenceException>(() => dbModel.Configure(contextOptBuilder));
        }
    }
}
