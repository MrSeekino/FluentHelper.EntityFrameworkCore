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
        public void Verify_UseSqlServer_IsCalledCorrectly()
        {
            Mock<IDbProviderConfiguration> dbProviderConfigurationMocked = new();
            dbProviderConfigurationMocked.Setup(x => x.ConfigureDbProvider(It.IsAny<DbContextOptionsBuilder>())).Verifiable();

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            var dbModel = new EfDbModel(dbProviderConfigurationMocked.Object, logAction, false, false,
                                        (x) => { return null; },
                                        (x) => { return null; });
            dbModel.Configure(mockOptionsBuilder.Object);


            dbProviderConfigurationMocked.Verify(x => x.ConfigureDbProvider(It.IsAny<DbContextOptionsBuilder>()), Times.Once());
        }

        [Test]
        public void Verify_LogTo_IsCalledCorrectly()
        {
            Mock<IDbProviderConfiguration> dbProviderConfigurationMocked = new();

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            var dbModel = new EfDbModel(dbProviderConfigurationMocked.Object, logAction, false, false,
                                        (x) => { return null; },
                                        (x) => { return null; });
            dbModel.Configure(mockOptionsBuilder.Object);

            mockOptionsBuilder.Verify(x => x.LogTo(It.IsAny<Func<EventId, LogLevel, bool>>(), It.IsAny<Action<EventData>>()), Times.Once());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Verify_EnableSensitiveDataLogging_IsCalledCorrectly(bool enableSensitivityDataLogging)
        {
            Mock<IDbProviderConfiguration> dbProviderConfigurationMocked = new();

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            var dbModel = new EfDbModel(dbProviderConfigurationMocked.Object, logAction, enableSensitivityDataLogging, false,
                                        (x) => { return null; },
                                        (x) => { return null; });
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
            Mock<IDbProviderConfiguration> dbProviderConfigurationMocked = new();

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            bool funcCalled = false;
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> useLazyLoadingProxiesBehaviour = (x) =>
            {
                funcCalled = true;
                return null;
            };

            var dbModel = new EfDbModel(dbProviderConfigurationMocked.Object, logAction, false, enableLazyLoadingProxies,
                                        useLazyLoadingProxiesBehaviour,
                                        (x) => { return null; });

            dbModel.Configure(mockOptionsBuilder.Object);

            Assert.That(funcCalled, Is.EqualTo(enableLazyLoadingProxies));
        }

        [Test]
        public void Verify_AddMappingAssembly_WorksProperly()
        {
            Mock<IDbProviderConfiguration> dbProviderConfigurationMocked = new();

            var dbModel = new EfDbModel(dbProviderConfigurationMocked.Object, (x, y, z) => { }, false, false,
                                        (x) => { return null; },
                                        (x) => { return null; });

            dbModel.AddMappingAssembly(Assembly.GetExecutingAssembly());

            Assert.That(dbModel.MappingAssemblies.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateModel_WorksProperly()
        {
            Mock<IDbProviderConfiguration> dbProviderConfigurationMocked = new();

            var mockModelBuilder = new Mock<ModelBuilder>();
            var mockOfDbMap = new Mock<IDbMap>();

            Func<Type, IDbMap> getInstanceOfDbMapBehaviour = (x) =>
            {
                return mockOfDbMap.Object;
            };

            var dbModel = new EfDbModel(dbProviderConfigurationMocked.Object, (x, y, z) => { }, false, false,
                                        (x) => { return null; },
                                        getInstanceOfDbMapBehaviour);

            dbModel.AddMappingAssembly(Assembly.GetExecutingAssembly());

            dbModel.CreateModel(mockModelBuilder.Object);

            mockOfDbMap.Verify(x => x.SetModelBuilder(mockModelBuilder.Object), Times.Once());
            mockOfDbMap.Verify(x => x.Map(), Times.Once());
        }
    }
}
