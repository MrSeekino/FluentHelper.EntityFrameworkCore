using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Reflection;

namespace FluentHelper.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal class EfDbModelTests
    {
        [Test]
        public void Verify_DbProviderConfiguration_IsCalledCorrectly()
        {
            bool funcCalled = false;
            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { funcCalled = true; };

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            Mock<IDbMap> dbMapMocked = new();

            var dbModel = new EfDbModel(dbProviderConfiguration, logAction, false, false,
                                        (x) => { return x; },
                                        (x) => { return dbMapMocked.Object; });
            dbModel.Configure(mockOptionsBuilder.Object);

            Assert.True(funcCalled);
        }

        [Test]
        public void Verify_LogTo_IsCalledCorrectly()
        {
            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { };

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            Mock<IDbMap> dbMapMocked = new();

            var dbModel = new EfDbModel(dbProviderConfiguration, logAction, false, false,
                                        (x) => { return x; },
                                        (x) => { return dbMapMocked.Object; });
            dbModel.Configure(mockOptionsBuilder.Object);

            mockOptionsBuilder.Verify(x => x.LogTo(It.IsAny<Func<EventId, LogLevel, bool>>(), It.IsAny<Action<EventData>>()), Times.Once());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Verify_EnableSensitiveDataLogging_IsCalledCorrectly(bool enableSensitivityDataLogging)
        {
            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { };

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            Mock<IDbMap> dbMapMocked = new();

            var dbModel = new EfDbModel(dbProviderConfiguration, logAction, enableSensitivityDataLogging, false,
                                        (x) => { return x; },
                                        (x) => { return dbMapMocked.Object; });
            dbModel.Configure(mockOptionsBuilder.Object);

            if (enableSensitivityDataLogging)
                mockOptionsBuilder.Verify(x => x.EnableSensitiveDataLogging(true), Times.Once());
            else
                mockOptionsBuilder.Verify(x => x.EnableSensitiveDataLogging(It.IsAny<bool>()), Times.Never());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Verify_UseLazyLoadingProxies_IsCalledCorrectly(bool enableLazyLoadingProxies)
        {
            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { };

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            bool funcCalled = false;
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> useLazyLoadingProxiesBehaviour = (x) =>
            {
                funcCalled = true;
                return x;
            };

            Mock<IDbMap> dbMapMocked = new();

            var dbModel = new EfDbModel(dbProviderConfiguration, logAction, false, enableLazyLoadingProxies,
                                        useLazyLoadingProxiesBehaviour,
                                        (x) => { return dbMapMocked.Object; });

            dbModel.Configure(mockOptionsBuilder.Object);

            Assert.That(funcCalled, Is.EqualTo(enableLazyLoadingProxies));
        }

        [Test]
        public void Verify_AddMappingAssembly_WorksProperly()
        {
            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { };

            Mock<IDbMap> dbMapMocked = new();

            var dbModel = new EfDbModel(dbProviderConfiguration, (x, y, z) => { }, false, false,
                                        (x) => { return x; },
                                        (x) => { return dbMapMocked.Object; });

            dbModel.AddMappingAssembly(Assembly.GetExecutingAssembly());

            Assert.That(dbModel.MappingAssemblies.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateModel_WorksProperly()
        {
            Action<DbContextOptionsBuilder> dbProviderConfiguration = (x) => { };

            var mockModelBuilder = new Mock<ModelBuilder>();
            var mockOfDbMap = new Mock<IDbMap>();

            Func<Type, IDbMap> getInstanceOfDbMapBehaviour = (x) =>
            {
                return mockOfDbMap.Object;
            };

            var dbModel = new EfDbModel(dbProviderConfiguration, (x, y, z) => { }, false, false,
                                        (x) => { return x; },
                                        getInstanceOfDbMapBehaviour);

            dbModel.AddMappingAssembly(Assembly.GetExecutingAssembly());

            dbModel.CreateModel(mockModelBuilder.Object);

            mockOfDbMap.Verify(x => x.SetModelBuilder(mockModelBuilder.Object), Times.Once());
            mockOfDbMap.Verify(x => x.Map(), Times.Once());
        }
    }
}
