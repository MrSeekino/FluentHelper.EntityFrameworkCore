using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FluentHelper.EntityFrameworkCore.Tests.Core
{
    [TestFixture]
    internal class EfDbModelTests
    {
        [Test]
        public void Verify_DbProviderConfiguration_IsCalledCorrectly()
        {
            bool funcCalled = false;

            var mockDbConfig = new Mock<IDbConfig>();
            mockDbConfig.Setup(x => x.DbProviderConfiguration).Returns(x =>
            {
                funcCalled = true;
            });

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            var dbModel = new EfDbModel(mockDbConfig.Object, new List<IDbMap>());
            dbModel.Configure(mockOptionsBuilder.Object);

            Assert.True(funcCalled);
        }

        [Test]
        public void Verify_LogTo_IsCalledCorrectly()
        {
            Action<LogLevel, EventId, string> logAction = (x, y, z) => { };

            var mockDbConfig = new Mock<IDbConfig>();
            mockDbConfig.Setup(x => x.DbProviderConfiguration).Returns(x => { });
            mockDbConfig.Setup(x => x.LogAction).Returns(logAction);

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            var dbModel = new EfDbModel(mockDbConfig.Object, new List<IDbMap>());
            dbModel.Configure(mockOptionsBuilder.Object);

            mockOptionsBuilder.Verify(x => x.LogTo(It.IsAny<Func<EventId, LogLevel, bool>>(), It.IsAny<Action<EventData>>()), Times.Once());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Verify_EnableSensitiveDataLogging_IsCalledCorrectly(bool enableSensitivityDataLogging)
        {
            var mockDbConfig = new Mock<IDbConfig>();
            mockDbConfig.Setup(x => x.DbProviderConfiguration).Returns(x => { });
            mockDbConfig.Setup(x => x.EnableSensitiveDataLogging).Returns(enableSensitivityDataLogging);

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            var dbModel = new EfDbModel(mockDbConfig.Object, new List<IDbMap>());
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
            bool funcCalled = false;

            var mockDbConfig = new Mock<IDbConfig>();
            mockDbConfig.Setup(x => x.DbProviderConfiguration).Returns(x => { });
            mockDbConfig.Setup(x => x.EnableLazyLoadingProxies).Returns(enableLazyLoadingProxies);

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            var dbModel = new EfDbModel(mockDbConfig.Object, new List<IDbMap>(), x =>
            {
                funcCalled = true;
            });
            dbModel.Configure(mockOptionsBuilder.Object);

            Assert.That(funcCalled, Is.EqualTo(enableLazyLoadingProxies));
        }

        [Test]
        public void Verify_CreateModel_WorksProperly()
        {
            var mockModelBuilder = new Mock<ModelBuilder>();

            var mockDbConfig = new Mock<IDbConfig>();
            mockDbConfig.Setup(x => x.DbProviderConfiguration).Returns(x => { });

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            var mockDbMap = new Mock<IDbMap>();
            mockDbMap.Setup(x => x.SetModelBuilder(It.IsAny<ModelBuilder>())).Verifiable();
            mockDbMap.Setup(x => x.Map()).Verifiable();

            var dbModel = new EfDbModel(mockDbConfig.Object, new List<IDbMap>() { mockDbMap.Object });
            dbModel.Configure(mockOptionsBuilder.Object);
            dbModel.CreateModel(mockModelBuilder.Object);

            mockDbMap.Verify(x => x.SetModelBuilder(mockModelBuilder.Object), Times.Once());
            mockDbMap.Verify(x => x.Map(), Times.Once());
        }

        [Test]
        public void Verify_CreateModel_Throws_Without_DbProviderConfiguration()
        {
            var mockModelBuilder = new Mock<ModelBuilder>();

            var mockDbConfig = new Mock<IDbConfig>();

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            var mockDbMap = new Mock<IDbMap>();
            mockDbMap.Setup(x => x.SetModelBuilder(It.IsAny<ModelBuilder>())).Verifiable();
            mockDbMap.Setup(x => x.Map()).Verifiable();

            var dbModel = new EfDbModel(mockDbConfig.Object, new List<IDbMap>() { mockDbMap.Object });
            Assert.Throws<NullReferenceException>(() => dbModel.Configure(mockOptionsBuilder.Object));
        }
    }
}
