using FluentHelper.EntityFrameworkCore.Common;
using FluentHelper.EntityFrameworkCore.Tests.Support;
using NUnit.Framework;

namespace FluentHelper.EntityFrameworkCore.Tests.Core
{
    [TestFixture]
    internal class EfDbConfigBuilderTests
    {
        [Test]
        public void Verify_EfDbConfigBuilder_WithDbConfiguration_Works()
        {
            var efDbConfig = new EfDbConfigBuilder()
                                .WithDbConfiguration(x => { })
                                .Build();

            Assert.That(efDbConfig, Is.Not.Null);
            Assert.That(efDbConfig.DbConfiguration, Is.Not.Null);
        }

        [Test]
        public void Verify_EfDbConfigBuilder_WithDbProvider_Works()
        {
            var efDbConfig = new EfDbConfigBuilder()
                                .WithDbProvider(x => { })
                                .Build();

            Assert.That(efDbConfig, Is.Not.Null);
            Assert.That(efDbConfig.DbProvider, Is.Not.Null);
        }

        [Test]
        public void Verify_EfDbConfigBuilder_WithLazyLoadingProxies_Works()
        {
            var efDbConfig = new EfDbConfigBuilder()
                                .WithLazyLoadingProxies()
                                .Build();

            Assert.That(efDbConfig, Is.Not.Null);
            Assert.That(efDbConfig.EnableLazyLoadingProxies, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Verify_EfDbConfigBuilder_WithLogAction_Works(bool enableSensitiveDataLogging)
        {
            var efDbConfig = new EfDbConfigBuilder()
                                .WithLogAction((l, e, m) => { }, enableSensitiveDataLogging)
                                .Build();

            Assert.That(efDbConfig, Is.Not.Null);
            Assert.That(efDbConfig.EnableSensitiveDataLogging, Is.EqualTo(enableSensitiveDataLogging));
            Assert.That(efDbConfig.LogAction, Is.Not.Null);
        }

        [Test]
        public void Verify_EfDbConfigBuilder_WithMappingFromAssemblyOf_Works()
        {
            var efDbConfig = new EfDbConfigBuilder()
                                .WithMappingFromAssemblyOf<TestEntityMap>()
                                .Build();

            Assert.That(efDbConfig, Is.Not.Null);
            Assert.That(efDbConfig.MappingAssemblies.Count, Is.EqualTo(1));
        }
    }
}
