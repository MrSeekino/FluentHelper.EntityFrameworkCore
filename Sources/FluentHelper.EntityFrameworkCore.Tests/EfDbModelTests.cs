using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Interfaces;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentHelper.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal class EfDbModelTests
    {
        [Test]
        public void Verify_UseSqlServer_IsCalledCorrectly()
        {
            string connStringSample = "A_Conn_String";

            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<string> logAction = (x) => { };
            Func<EventId, LogLevel, bool> logFilter = (x, y) => { return true; };

            bool funcCalled = false;
            Func<DbContextOptionsBuilder, string, DbContextOptionsBuilder> useSqlServerBehaviour = (x, cs) =>
            {
                if (connStringSample == cs)
                    funcCalled = true;

                return null;
            };

            var dbModel = new EfDbModel(connStringSample, logAction, logFilter, false, false,
                                        useSqlServerBehaviour, (x) => { return null; },
                                        (x) => { return null; });
            dbModel.Configure(mockOptionsBuilder.Object);

            Assert.True(funcCalled);
        }

        [Test]
        public void Verify_LogTo_IsCalledCorrectly()
        {
            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<string> logAction = (x) => { };
            Func<EventId, LogLevel, bool> logFilter = (x, y) => { return true; };

            var dbModel = new EfDbModel(string.Empty, logAction, logFilter, false, false,
                                        (x, y) => { return null; }, (x) => { return null; },
                                        (x) => { return null; });
            dbModel.Configure(mockOptionsBuilder.Object);

            mockOptionsBuilder.Verify(x => x.LogTo(logAction, logFilter, null), Times.Once());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Verify_EnableSensitiveDataLogging_IsCalledCorrectly(bool enableSensitivityDataLogging)
        {
            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<string> logAction = (x) => { };
            Func<EventId, LogLevel, bool> logFilter = (x, y) => { return true; };

            var dbModel = new EfDbModel(string.Empty, logAction, logFilter, enableSensitivityDataLogging, false,
                                        (x, y) => { return null; }, (x) => { return null; },
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
            var mockOptionsBuilder = new Mock<DbContextOptionsBuilder>();
            mockOptionsBuilder.Setup(x => x.IsConfigured).Returns(false);

            Action<string> logAction = (x) => { };
            Func<EventId, LogLevel, bool> logFilter = (x, y) => { return true; };

            bool funcCalled = false;
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> useLazyLoadingProxiesBehaviour = (x) =>
            {
                funcCalled = true;
                return null;
            };

            var dbModel = new EfDbModel(string.Empty, logAction, logFilter, false, enableLazyLoadingProxies,
                                        (x, y) => { return null; }, useLazyLoadingProxiesBehaviour,
                                        (x) => { return null; });

            dbModel.Configure(mockOptionsBuilder.Object);

            Assert.That(funcCalled, Is.EqualTo(enableLazyLoadingProxies));
        }

        [Test]
        public void Verify_AddMappingAssembly_WorksProperly()
        {
            var dbModel = new EfDbModel(string.Empty, (x) => { }, (x, y) => { return true; }, false, false,
                                        (x, y) => { return null; }, (x) => { return null; },
                                        (x) => { return null; });

            dbModel.AddMappingAssembly(Assembly.GetExecutingAssembly());

            Assert.That(dbModel.MappingAssemblies.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_CreateModel_WorksProperly()
        {
            var mockModelBuilder = new Mock<ModelBuilder>();
            var mockOfDbMap = new Mock<IDbMap>();
            

            Func<Type, IDbMap> getInstanceOfDbMapBehaviour = (x) =>
            {
                return mockOfDbMap.Object;
            };

            var dbModel = new EfDbModel(string.Empty, (x) => { }, (x, y) => { return true; }, false, false,
                                        (x, y) => { return null; }, (x) => { return null; },
                                        getInstanceOfDbMapBehaviour);

            dbModel.AddMappingAssembly(Assembly.GetExecutingAssembly());

            dbModel.CreateModel(mockModelBuilder.Object);

            mockOfDbMap.Verify(x => x.SetModelBuilder(mockModelBuilder.Object), Times.Once());
            mockOfDbMap.Verify(x => x.Map(), Times.Once());
        }
    }
}
